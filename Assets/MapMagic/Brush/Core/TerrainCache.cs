using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Nodes;
using MapMagic.Terrains;
using MapMagic.Products;
using MapMagic.Expose;
using System.Threading;
using System;

namespace MapMagic.Brush
{
	public class CacheChange
	{
		public CoordRect changeRect;

		[Flags] public enum Type { Height=1, Splats=2, Grass=4, Objects=8, Trees=16 };
		public Type type;

		public bool HasFlag (Type f) => (type & f) == f;
		public void AddFlag (Type f) => type = type | f;
		public bool HasAnyFlag => type!=0;

		public void AddRect (CoordRect rect)
		{
			if (changeRect.size.x==0  &&  changeRect.size.z==0) changeRect = rect;
			else changeRect = CoordRect.Combined(rect, changeRect);
		}

		public void Clear ()
		{
			type = 0;
			changeRect = new CoordRect(0,0,0,0);
		}
	}


	public class TerrainCache
	{
		public Terrain terrain;

		public CoordRect rect;
		public Vector3 worldPos;
		public Vector3 worldSize;

		public Vector2D PixelSize => new Vector2D(worldSize.x/(rect.size.x-1), worldSize.z/(rect.size.z-1));

		private bool drawInstanced;


		public TerrainCache (Terrain terrain)
		{
			this.terrain = terrain;
			UpdateCache();
		}

		public void UpdateCache ()
		{
			TerrainData terrainData = terrain.terrainData;

			worldPos = terrain.transform.position;
			worldSize = terrainData.size;

			int heightRes = terrainData.heightmapResolution;
			float terrainPixelSize = worldSize.x / (heightRes-1);

			rect = new CoordRect(
				Mathf.RoundToInt(worldPos.x/terrainPixelSize),
				Mathf.RoundToInt(worldPos.z/terrainPixelSize),
				heightRes,
				heightRes);

			drawInstanced = terrain.drawInstanced;

			LoadHeights();
			LoadTextures();
			LoadGrass();

			#if MM_DEBUG
			Debug.Log("Cache Updated");
			#endif
		}

		public void ApplyChanges (CacheChange change)
		/// Saves all the changes to terrain
		/// This should be called before rendering the new frame, not after each stamp (we can have several stamps per frame)
		{
			if (!change.HasAnyFlag  ||  change.changeRect.size.x==0  || change.changeRect.size.z==0)
				return;

			if (!CoordRect.IsIntersecting(change.changeRect, rect))
				return;

			using (Log.Group("ApplyChanges"))
			{
				using (Log.Group("ApplyHeights"))
				if (change.HasFlag(CacheChange.Type.Height)) ApplyHeights(change.changeRect);

				using (Log.Group("ApplyTextures"))
				if (change.HasFlag(CacheChange.Type.Splats)) ApplyTextures(change.changeRect);

				using (Log.Group("ApplyGrass"))
				if (change.HasFlag(CacheChange.Type.Grass)) ApplyGrass(change.changeRect);
			}
		}

		public void EndTrace ()
		{
			terrain.drawInstanced = drawInstanced; //this is changed to false while applying heightmap to refresh visual only
		}

		#region Heights

			public Matrix heights;
			private byte[] heightBytes;
			private Texture2D heightTex;
			private RenderTexture heightRenTex;

			public void LoadHeights ()
			/// Loads height data from whole terrain
			{
				float[,] heightsArr = terrain.terrainData.GetHeights(0,0,rect.size.x,rect.size.z);
				heights = new Matrix(rect);
				heights.ImportHeights(heightsArr);
			}

			public void ReadHeights (Matrix matrix)  =>  Matrix.CopyIntersected(heights, matrix);
			/// Fills matrix with loaded whole terrain data

			public void WriteHeights (Matrix matrix) 
			/// Copies matrix data to loaded whole terrain data and marks changed rect
			{
				matrix.Clamp01();
				Matrix.CopyIntersected(matrix, heights);
			}

			private void ApplyHeights (CoordRect applyRect)
			/// Copies pixels within given applyRect from heights matrix to terrain on Apply Changes
			{
				TerrainData terrainData = terrain.terrainData;

				CoordRect relApplyRect = new CoordRect(applyRect.offset - rect.offset, applyRect.size); //from here and  below: terrainheight related coords
				CoordRect relFullRect = new CoordRect(Coord.zero, rect.size);

				CoordRect intersection = CoordRect.Intersected(relApplyRect, relFullRect);
				if (intersection.size.x<=0 || intersection.size.z<=0) return;

				//matrix to bytes
				if (heightBytes == null  ||  heightBytes.Length != intersection.size.x*intersection.size.z*4)
					heightBytes = new byte[intersection.size.x*intersection.size.z*4];

				float ushortEpsilon = 1f / 65535; //since setheights is using not full ushort range, but range-1
				heights.ExportRawFloat(heightBytes, intersection.offset+heights.rect.offset, intersection.size, mult:0.5f-ushortEpsilon);

				//bytes to texture2D
				if (heightTex == null ||  heightTex.width != intersection.size.x  ||  heightTex.height != intersection.size.z)
					heightTex = new Texture2D(intersection.size.x, intersection.size.z, TextureFormat.RFloat, mipChain:false, linear:true);

				heightTex.LoadRawTextureData(heightBytes);
				heightTex.Apply(updateMipmaps:false);

				//texture2D to renderTexture
				if (heightRenTex == null ||  heightRenTex.width != intersection.size.x  ||  heightRenTex.height != intersection.size.z)
				#if UNITY_2019_2_OR_NEWER
					heightRenTex = new RenderTexture(intersection.size.x, intersection.size.z, 32, RenderTextureFormat.RFloat, mipCount:0);
				#else
					heightRenTex = new RenderTexture(intersection.size.x, intersection.size.z, 32, RenderTextureFormat.RFloat);
				#endif
				Graphics.Blit(heightTex, heightRenTex);

				//renderTexture to terrain
				RenderTexture bacRenTex = RenderTexture.active;
				RenderTexture.active = heightRenTex;

				RectInt texRect = new RectInt(0,0, intersection.size.x, intersection.size.z);
				terrain.drawInstanced = true;
				terrainData.CopyActiveRenderTextureToHeightmap(texRect, new Vector2Int(intersection.offset.x, intersection.offset.z), TerrainHeightmapSyncControl.None);
				//terrainData.DirtyHeightmapRegion(new RectInt(intersection.offset.x, intersection.offset.z, intersection.size.x, intersection.size.z), TerrainHeightmapSyncControl.HeightAndLod); //or seems a bit faster on re-setting height to already existing terrains. IDK
				//terrainData.DirtyTextureRegion("_Splat0", new RectInt(intersection.offset.x, intersection.offset.z, intersection.size.x, intersection.size.z), true); //or seems a bit faster on re-setting height to already existing terrains. IDK
				//terrainData.SetBaseMapDirty();
				//terrainData.UpdateDirtyRegion(intersection.offset.x, intersection.offset.z, intersection.size.x, intersection.size.z,);
				//terrain.GetComponent<TerrainCollider>().enabled = false;
				//terrainData.SyncHeightmap(); //doesn't seems to make difference with DirtyHeightmapRegion, waiting for readback anyways
				//terrain.GetComponent<TerrainCollider>().enabled = true;
				//terrainData.UpdateDirtyRegion(intersection.offset.x, intersection.offset.z, intersection.size.x, intersection.size.z, true);
				//terrainData.DirtyTextureRegion(

				//terrain.heightmapPixelError = 1;

				RenderTexture.active = bacRenTex;
			}

			[Obsolete] private void ApplyHeights (Matrix matrix)
			/// Copies matrix to terrain, while the matrix size is smalle than terrain
			/// Tested and working, however not used. Just keeping as a reference
			{
				//saving to cache
				Matrix.CopyIntersected(matrix, heights);

				//applying to terrain
				CoordRect intersection = CoordRect.Intersected(rect, matrix.rect);
				if (intersection.size.x<=0 || intersection.size.z<=0) return;

				//matrix to bytes
				if (heightBytes == null  ||  heightBytes.Length != intersection.size.x*intersection.size.z*4)
					heightBytes = new byte[intersection.size.x*intersection.size.z*4];

				float ushortEpsilon = 1f / 65535; //since setheights is using not full ushort range, but range-1
				matrix.ExportRawFloat(heightBytes, intersection.offset, intersection.size, mult:0.5f-ushortEpsilon);

				//bytes to texture2D
				if (heightTex == null ||  heightTex.width != intersection.size.x  ||  heightTex.height != intersection.size.z)
					heightTex = new Texture2D(intersection.size.x, intersection.size.z, TextureFormat.RFloat, mipChain:false, linear:true);

				heightTex.LoadRawTextureData(heightBytes);
				heightTex.Apply(updateMipmaps:false);

				//texture2D to renderTexture
				if (heightRenTex == null ||  heightRenTex.width != intersection.size.x  ||  heightRenTex.height != intersection.size.z)
				#if UNITY_2019_2_OR_NEWER
					heightRenTex = new RenderTexture(intersection.size.x, intersection.size.z, 32, RenderTextureFormat.RFloat, mipCount:0);
				#else
					heightRenTex = new RenderTexture(intersection.size.x, intersection.size.z, 32, RenderTextureFormat.RFloat);
				#endif
				Graphics.Blit(heightTex, heightRenTex);

				//renderTexture to terrain
				RenderTexture bacRenTex = RenderTexture.active;
				RenderTexture.active = heightRenTex;

				RectInt texRect = new RectInt(0,0, intersection.size.x, intersection.size.z);
				terrain.terrainData.CopyActiveRenderTextureToHeightmap(texRect, new Vector2Int(intersection.offset.x, intersection.offset.z), TerrainHeightmapSyncControl.None);
				//data.DirtyHeightmapRegion(texRect, TerrainHeightmapSyncControl.HeightAndLod); //or seems a bit faster on re-setting height to already existing terrains. IDK
				//data.SyncHeightmap(); //doesn't seems to make difference with DirtyHeightmapRegion, waiting for readback anyways

				RenderTexture.active = bacRenTex;
			}

		#endregion


		#region Textures

			//public MatrixSet origTexMaps;
			public MatrixSet scaledTexMaps;

			//private MatrixSet partialScaledTexMaps; //for apply
			//private MatrixSet partialOrigTexMaps;
			//private const int partialScaleMargins = 5; //rescaling partial rect if it's more or less rect+margins

			public void LoadTextures ()
			/// Reads whole terrain textures and resizes them to fit current Rect
			{
				TerrainData terrainData =  terrain.terrainData;
				int splatRes = terrainData.alphamapResolution;

				//loading
				TerrainLayer[] layers = terrainData.terrainLayers;
				float[,,] splats = terrainData.GetAlphamaps(0, 0, splatRes, splatRes);

				CoordRect origSplatRect = new CoordRect(rect.offset.x, rect.offset.z, splatRes, splatRes);
				MatrixSet origTexMaps = new MatrixSet(origSplatRect, worldPos, worldSize);
				for (int p=0; p<layers.Length; p++)
				{
					MatrixSet.Prototype prototype = new MatrixSet.Prototype(layers, p);
					if (prototype.Object == null) 
						continue;

					if (!origTexMaps.TryGetValue(prototype, out Matrix matrix)) //adding if not contains
					{
						matrix = new Matrix(origSplatRect);
						origTexMaps[prototype] = matrix;
					}

					matrix.ImportSplats(splats, p);
				}

				//scaling
				if (splatRes != rect.size.x  ||  splatRes != rect.size.z)
				{
					scaledTexMaps = new MatrixSet(rect, worldPos, worldSize);
					MatrixSet.Resize(origTexMaps, scaledTexMaps, interpolate:true);
				}
				else
					scaledTexMaps = origTexMaps;
			}


			public void ReadTextures (MatrixSet matrixSet)  =>  MatrixSet.CopyIntersected(scaledTexMaps, matrixSet);
			/// Fills matrixSet with loaded terrain data

			public void WriteTextures (MatrixSet matrixSet)  =>  MatrixSet.CopyIntersected(matrixSet, scaledTexMaps);
			/// Copies matrix data to loaded whole terrain data and marks changed rect


			private static MatrixSet splatsApply;
			private static float[,,] splats;

			private void ApplyTextures (CoordRect applyRect)
			/// Copies pixels within given applyRect from heights matrix to terrain on Apply Changes
			{
				TerrainData terrainData =  terrain.terrainData;

				if (terrainData.alphamapLayers == 0)
				{ 
					if (scaledTexMaps.Count == 0) return;
					FillAll( MatrixSet.Prototype.NewLayer<TerrainLayer>(scaledTexMaps.GetPrototypeByNum(0)) );
					return;
				}

				int heightRes = terrainData.heightmapResolution;
				int splatsRes = terrainData.alphamapResolution;

				CoordRect relApplyRect = new CoordRect(applyRect.offset - rect.offset, applyRect.size); //from here and  below: terrainheight related coords

				//applyRect in splat coordinates
				CoordRect splatsApplyRect = new CoordRect();
				float scaleRatio = 1f*(splatsRes-1)/(heightRes-1); //using non-height alignedratio since interpolating NN anyways
				splatsApplyRect.Min = Coord.Floor(relApplyRect.offset.x*scaleRatio, relApplyRect.offset.z*scaleRatio);
				splatsApplyRect.Max = Coord.Ceil(relApplyRect.Max.x*scaleRatio, relApplyRect.Max.z*scaleRatio);

				//MatrixSet splatsApply = new MatrixSet(splatsApplyRect, Vector3.zero, Vector3.zero);
				//trying to re-use splatsApply to avoid constantly creating it
				//on average creates new matrix 1 times of 10
				if (splatsApply == null  ||  splatsApply.Count != scaledTexMaps.Count ||
					splatsApply.rect.size.x < splatsApplyRect.size.x  ||  splatsApply.rect.size.x > splatsApplyRect.size.x+3  ||
					splatsApply.rect.size.z < splatsApplyRect.size.z  ||  splatsApply.rect.size.z > splatsApplyRect.size.z+3)
						splatsApply = new MatrixSet(splatsApplyRect, Vector3.zero, Vector3.zero);
				splatsApply.SetOffset(splatsApplyRect.offset);

				//resizing from scaledTexMaps to splatsApply
				//using (Log.Group("CopyResized size:" + splatsApply.rect.size + " count:" + splatsApply.Count))
				MatrixSet.CopyResized(src:scaledTexMaps, dst:splatsApply,
					srcRectPos: (Vector2D)splatsApply.rect.offset / scaleRatio + (Vector2D)scaledTexMaps.rect.offset, //splatsApplyRect is relative to terrain 0, so returning back offset since it's present in scaledTexMaps
					srcRectSize: (Vector2D)splatsApply.rect.size / scaleRatio,
					dstRectPos: splatsApply.rect.offset,
					dstRectSize: splatsApply.rect.size);

				//adding to terrain layers prototypes prototypes that are in set, but not yet present on terrain
				TerrainLayer[] originalTerrainLayers = terrainData.terrainLayers;

				TerrainLayer[] terrainLayers = originalTerrainLayers;
				foreach (MatrixSet.Prototype prototype in splatsApply.Prototypes)
					terrainLayers = prototype.CheckAppendLayers(terrainLayers);

				if (terrainLayers != originalTerrainLayers) 
					terrain.terrainData.terrainLayers = terrainLayers;

				//finding splats intersected rect
				CoordRect intersection = CoordRect.Intersected(new CoordRect(0,0,splatsRes,splatsRes), splatsApply.rect);
				if (intersection.size.x<=0 || intersection.size.z<=0) return;

				//trying to re-use 3d array
				//float[,,] splats = new float[intersection.size.z, intersection.size.x, terrainLayers.Length]; //x and z sizes are swapped
				if (splats == null  ||  splats.GetLength(2) != terrainLayers.Length  ||
					splats.GetLength(0) != intersection.size.z  ||  splats.GetLength(1) != intersection.size.x)
						splats = new float[intersection.size.z, intersection.size.x, terrainLayers.Length]; 

				//setting
				using (Log.Group("Exporting"))
					for (int p=0; p<terrainLayers.Length; p++)
					{
						MatrixSet.Prototype prototype = new MatrixSet.Prototype(terrainLayers, p);

						Matrix matrix;
						bool isInSet = splatsApply.TryGetValue(prototype, out matrix);

						if (!isInSet) //clearing channel if it has no matrix layer in output
							ClearSplats(splatsApply.rect, splats, p);
				
						else
							matrix.ExportSplats(splats, intersection.offset, p);
					}

				using (Log.Group("Setting x:" + splats.GetLength(0) + " z:" + splats.GetLength(1) + " count:" + splats.GetLength(2) ))
				{
					Vector2D relativeOffset = (Vector2D)relApplyRect.offset / heightRes;
					Coord splatOffset = Coord.Round(relativeOffset * splatsRes);
					terrainData.SetAlphamaps(intersection.offset.x, intersection.offset.z, splats);
				}

				//re-importing cached matrixset on texture layers change
				if (terrainLayers != originalTerrainLayers) 
					LoadTextures();
			}


			private static void ClearSplats (CoordRect matrixRect, float[,,] splats, int channel) => ClearSplats(matrixRect, splats, matrixRect.offset, channel);
			private static void ClearSplats (CoordRect matrixRect, float[,,] splats, Coord splatsOffset, int channel)
			///Same as Matrix.ExportSplats(ch), but just clearing one splats channel
			{
				Coord splatsSize = new Coord(splats.GetLength(1), splats.GetLength(0));  //x and z swapped
				CoordRect splatsRect = new CoordRect(splatsOffset, splatsSize);
				
				CoordRect intersection = CoordRect.Intersected(matrixRect, splatsRect);
				Coord min = intersection.Min; Coord max = intersection.Max;

				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
					{
						int matrixPos = (z-matrixRect.offset.z)*matrixRect.size.x + x - matrixRect.offset.x;
						int heightsPosZ = x - splatsRect.offset.x;
						int heightsPosX = z - splatsRect.offset.z;

						splats[heightsPosX, heightsPosZ, channel] = 0;
					}
			}


			private void FillAll (TerrainLayer layer)
			{
				TerrainData terrainData =  terrain.terrainData;
				int heightRes = terrainData.heightmapResolution;
				terrainData.alphamapResolution = heightRes;

				float[,,] splats = new float[heightRes, heightRes, 1];
				for (int x=0; x<heightRes; x++)
					for (int z=0; z<heightRes; z++)
						splats[x,z,0] = 1;

				terrainData.terrainLayers = new TerrainLayer[] {layer};
				terrainData.SetAlphamaps(0,0,splats);

				LoadTextures(); //since changed layers
			}

		#endregion


		#region Grass

			//public MatrixSet origGrassMaps;
			public MatrixSet scaledGrassMaps;

			//private MatrixSet partialScaledTexMaps; //for apply
			//private MatrixSet partialOrigTexMaps;
			//private const int partialScaleMargins = 5; //rescaling partial rect if it's more or less rect+margins

			public void LoadGrass ()
			/// Reads whole terrain textures and resizes them to fit current Rect
			{
				TerrainData terrainData =  terrain.terrainData;
				int grassRes = terrainData.detailResolution;

				//loading
				DetailPrototype[] detLayers = terrainData.detailPrototypes;
				UnityEngine.Object[] textures = detLayers.Select(p => p.Object());

				CoordRect origGrassRect = new CoordRect(rect.offset.x, rect.offset.z, grassRes, grassRes);
				MatrixSet origGrassMaps = new MatrixSet(origGrassRect, worldPos, worldSize);
				
				for (int p=0; p<textures.Length; p++)
				{
					MatrixSet.Prototype prototype = new MatrixSet.Prototype(textures, p);

					if (!origGrassMaps.TryGetValue(prototype, out Matrix matrix)) //adding if not contains
					{
						matrix = new Matrix(origGrassRect);
						origGrassMaps[prototype] = matrix;
					}

					int[,] layer = terrainData.GetDetailLayer(0,0,grassRes,grassRes, p);
					matrix.ImportDetail(layer, density:origGrassMaps.PixelSize.x*origGrassMaps.PixelSize.z);
					//matrix will have values > 1
				}

				//scaling
				if (grassRes != rect.size.x  ||  grassRes != rect.size.z)
				{
					scaledGrassMaps = new MatrixSet(rect, worldPos, worldSize);
					MatrixSet.Resize(origGrassMaps, scaledGrassMaps, interpolate:false); //interpolated resize will clamp values to 1
				}
				else
					scaledGrassMaps = origGrassMaps;
			}


			public void ReadGrass (MatrixSet matrixSet)  =>  MatrixSet.CopyIntersected(scaledGrassMaps, matrixSet);
			/// Fills matrixSet with loaded terrain data

			public void WriteGrass (MatrixSet matrixSet)  =>  MatrixSet.CopyIntersected(matrixSet, scaledGrassMaps);
			/// Copies matrix data to loaded whole terrain data and marks changed rect


			private static MatrixSet grassApply;
			private static int[,] grass;
			private static Noise grassNoise;

			private void ApplyGrass (CoordRect applyRect)
			/// Copies pixels within given applyRect from heights matrix to terrain on Apply Changes
			{
				TerrainData terrainData =  terrain.terrainData;
				int heightRes = terrainData.heightmapResolution;
				int grassRes = terrainData.detailResolution;

				//adjusting grass res if grass is applied the first time
				if (terrainData.detailPrototypes.Length==0)  //no grass of any type added	
				{
					grassRes = terrain.terrainData.heightmapResolution;
					terrain.terrainData.SetDetailResolution(terrain.terrainData.heightmapResolution, 16); 
				}

				CoordRect relApplyRect = new CoordRect(applyRect.offset - rect.offset, applyRect.size); //from here and  below: terrainheight related coords

				//applyRect in grass coordinates
				CoordRect grassApplyRect = new CoordRect();
				float scaleRatio = 1f*(grassRes-1)/(heightRes-1); //using non-height alignedratio since interpolating NN anyways
				grassApplyRect.Min = Coord.Floor(relApplyRect.offset.x*scaleRatio, relApplyRect.offset.z*scaleRatio);
				grassApplyRect.Max = Coord.Ceil(relApplyRect.Max.x*scaleRatio, relApplyRect.Max.z*scaleRatio);

				//MatrixSet grassApply = new MatrixSet(grassApplyRect, Vector3.zero, Vector3.zero);
				//trying to re-use grassApply to avoid constantly creating it
				//on average creates new matrix 1 times of 10
				if (grassApply == null  ||  grassApply.Count != scaledGrassMaps.Count ||
					grassApply.rect.size.x < grassApplyRect.size.x  ||  grassApply.rect.size.x > grassApplyRect.size.x+3  ||
					grassApply.rect.size.z < grassApplyRect.size.z  ||  grassApply.rect.size.z > grassApplyRect.size.z+3)
						grassApply = new MatrixSet(grassApplyRect, Vector3.zero, Vector3.zero);
				grassApply.SetOffset(grassApplyRect.offset);

				//resizing from scaledGrassMaps to grassApply
				MatrixSet.CopyResized(src:scaledGrassMaps, dst:grassApply,
					srcRectPos: (Vector2D)grassApply.rect.offset / scaleRatio + (Vector2D)scaledGrassMaps.rect.offset, //grassApplyRect is relative to terrain 0, so returning back offset since it's present in scaledGrassMaps
					srcRectSize: (Vector2D)grassApply.rect.size / scaleRatio,
					dstRectPos: grassApply.rect.offset,
					dstRectSize: grassApply.rect.size);

				//adding to terrain layers prototypes prototypes that are in set, but not yet present on terrain
				DetailPrototype[] originalDetLayers = terrainData.detailPrototypes;
				DetailPrototype[] detLayers = originalDetLayers;
				foreach (MatrixSet.Prototype prototype in grassApply.Prototypes)
					detLayers = prototype.CheckAppendLayers(detLayers);

				if (originalDetLayers != detLayers) 
					terrain.terrainData.detailPrototypes = detLayers;

				//finding grasss intersected rect
				CoordRect intersection = CoordRect.Intersected(new CoordRect(0,0,grassRes,grassRes), grassApply.rect);
				if (intersection.size.x<=0 || intersection.size.z<=0) return;

				//trying to re-use array
				if (grass == null  ||  grass.GetLength(0) != intersection.size.z  ||  grass.GetLength(1) != intersection.size.x)
					grass = new int[intersection.size.z, intersection.size.x];

				if (grassNoise == null)
					grassNoise = new Noise(123);

				//setting
				for (int p=0; p<detLayers.Length; p++)
				{
					MatrixSet.Prototype prototype = new MatrixSet.Prototype(detLayers, p);

					Matrix matrix;
					bool isInSet = grassApply.TryGetValue(prototype, out matrix);

					if (!isInSet) //clearing channel if it has no matrix layer in output
					{
						for (int x=0; x<intersection.size.x; x++)
							for (int z=0; z<intersection.size.z; z++)
								grass[z,x] = 0;
					}

					else
					{
						float grassPixelSize = terrainData.size.x / (grassRes-1);
						matrix.ExportDetail(grass, intersection.offset, p, grassNoise, density:grassPixelSize*grassPixelSize);
					}

					terrain.terrainData.SetDetailLayer(intersection.offset.x, intersection.offset.z, p, grass);
				}
			}

		#endregion
	}
}