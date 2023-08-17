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

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MapMagic.Tests.Editor")]

namespace MapMagic.Brush.Undo
{
	public class TerrainUndoData
	{
		const int numChunksPerTerrain = 8;

		Matrix2D<float[,]> heightsChunks;
		Matrix2D<float[,,]> splatsChunks;
		TerrainLayer[] splatPrototypes;
		Matrix2D<int[][,]> grassChunks;
		DetailPrototype[] grassPrototypes;
		TreeInstance[] treeInstances;
		TreePrototype[] treePrototypes;
		//objects changes are recordered via UnityEditor.Undo in outputs

		public void Read (Terrain terrain, Vector2D worldPos, Vector2D worldSize,
			bool readHeight, bool readSplats, bool readGrass, bool readTrees)
		{
			TerrainData terrainData = terrain.terrainData;

			Vector2D terrainPos = (Vector2D)terrain.transform.position;
			Vector2D terrainSize = (Vector2D)terrain.terrainData.size;

			if (!Vector2D.Intersects(worldPos, worldSize, terrainPos, terrainSize))
				return;

			Coord chunksMin = (Coord)((worldPos-terrainPos) / terrainSize * numChunksPerTerrain);
			Coord chunksMax = (Coord)((worldPos+worldSize-terrainPos) / terrainSize * numChunksPerTerrain + 1);
			chunksMin.ClampByRect(new CoordRect(0,0,numChunksPerTerrain,numChunksPerTerrain));
			chunksMax.ClampByRect(new CoordRect(0,0,numChunksPerTerrain+1,numChunksPerTerrain+1));

			for (int x=chunksMin.x; x<chunksMax.x; x++)
				for (int z=chunksMin.z; z<chunksMax.z; z++)
				{
					Coord coord = new Coord(x,z);
						
					if (readHeight  &&  heightsChunks?[x,z] == null)
					{
						if (heightsChunks==null) heightsChunks = new Matrix2D<float[,]>(numChunksPerTerrain, numChunksPerTerrain);
						CoordRect pixRect = CoordToPixels(coord, terrainData.heightmapResolution, numChunksPerTerrain); //has different size for height and splats
						float[,] heightData = terrainData.GetHeights(pixRect.offset.x, pixRect.offset.z, pixRect.size.x, pixRect.size.z);
						heightsChunks[x,z] = heightData;
					}

					if (readSplats  &&  splatPrototypes == null)
						splatPrototypes = terrainData.terrainLayers;

					if (readSplats  &&  splatsChunks?[x,z] == null)
					{
						if (splatsChunks==null) splatsChunks = new Matrix2D<float[,,]>(numChunksPerTerrain, numChunksPerTerrain);
						CoordRect pixRect = CoordToPixels(coord, terrainData.alphamapResolution, numChunksPerTerrain);
						float[,,] splatsData = terrainData.GetAlphamaps(pixRect.offset.x, pixRect.offset.z, pixRect.size.x, pixRect.size.z);
						splatsChunks[x,z] = splatsData;
					}

					if (readGrass  &&  grassPrototypes == null)
						grassPrototypes = terrainData.detailPrototypes;

					if (readGrass  &&  grassChunks?[x,z] == null)
					{
						if (grassChunks==null) grassChunks = new Matrix2D<int[][,]>(numChunksPerTerrain, numChunksPerTerrain);
						CoordRect pixRect = CoordToPixels(coord, terrainData.detailResolution, numChunksPerTerrain);

						int layersCount = grassPrototypes.Length; //grassPrototypes already assigned at this moment
						int[][,] grassData = new int[layersCount][,];
						for (int i=0; i<layersCount; i++)
							grassData[i] = terrainData.GetDetailLayer(pixRect.offset.x, pixRect.offset.z, pixRect.size.x, pixRect.size.z, i);

						grassChunks[x,z] = grassData;
					}

					if (readTrees  &&  treeInstances == null)
						treeInstances = terrainData.treeInstances;

					if (readTrees  &&  treePrototypes == null)
						treePrototypes = terrainData.treePrototypes;
				}
		}


		public void Write (Terrain terrain)
		{
			TerrainData terrainData = terrain.terrainData;

			if (splatPrototypes != null)
				terrainData.terrainLayers = splatPrototypes;

			if (grassPrototypes != null)
				terrainData.detailPrototypes = grassPrototypes;

			for (int x=0; x<numChunksPerTerrain; x++)
				for (int z=0; z<numChunksPerTerrain; z++)
				{
					Coord coord = new Coord(x,z);

					if (heightsChunks != null  &&  heightsChunks[x,z] != null)
					{
						CoordRect pixRect = CoordToPixels(coord, terrainData.heightmapResolution, numChunksPerTerrain);
						terrainData.SetHeights(pixRect.offset.x, pixRect.offset.z, heightsChunks[x,z]);
					}
					
					if (splatsChunks != null  &&  splatsChunks[x,z] != null)
					{
						CoordRect pixRect = CoordToPixels(coord, terrainData.alphamapResolution, numChunksPerTerrain);
						terrainData.SetAlphamaps(pixRect.offset.x, pixRect.offset.z, splatsChunks[x,z]);
					}

					if (grassChunks != null  &&  grassChunks[x,z] != null)
					{
						CoordRect pixRect = CoordToPixels(coord, terrainData.detailResolution, numChunksPerTerrain);
						int[][,] grassData = grassChunks[x,z];
						for (int i=0; i<grassData.Length; i++)
							terrainData.SetDetailLayer(pixRect.offset.x, pixRect.offset.z, i,  grassData[i]);
					}
				}

			if (treePrototypes != null)
				terrainData.treePrototypes = treePrototypes;

			if (treeInstances != null)
				terrainData.treeInstances = treeInstances;
		}


		internal static CoordRect CoordToPixels (Coord coord, int resolution, int numChunks)
		//Converts chunk coord to pixel rect
		//Note that we don't need exact number of pixels in each chunk, come of them might have 64 pixels, some 65 if res==513
		//Tested (MapMagicTester)
		{
			Vector2D minPixelFloat = (1f * resolution / numChunks) * (Vector2D)coord;
			Coord minPixel = (Coord)minPixelFloat;

			Vector2D maxPixelFloat = (1f * resolution / numChunks) * (Vector2D)(coord+1);
			Coord maxPixel = (Coord)maxPixelFloat;
			if (coord.x == numChunks-1) maxPixel.x = resolution;
			if (coord.z == numChunks-1) maxPixel.z = resolution;

			return new CoordRect(minPixel, maxPixel-minPixel);
		}
	}


	public class Undo
	{
		public const string undoName = "Brush Stroke";
		public string lastUndoName;  //the last undo group name (kept to know it on UndoRedoPerformed)

		//private Stack<Set> sets = new Stack<Set>();  //we've got to remove first items from it, so using list instead
		[SerializeField] private List< Dictionary<Terrain,TerrainUndoData> > sets = new List< Dictionary<Terrain,TerrainUndoData> >();

		Terrain testTesrrain = null;

		#if UNITY_EDITOR
		///Calling Undo
		///Clone of Tools.GUI.Undo, except it's working with brush rather than gui
			public Undo ()
			{
				UnityEditor.Undo.undoRedoPerformed -= OnUndoRedoPerformed;
				UnityEditor.Undo.undoRedoPerformed += OnUndoRedoPerformed;
			}

			public void OnUndoRedoPerformed ()
			{
				string currGroupName = UnityEditor.Undo.GetCurrentGroupName();

				if (currGroupName == undoName || currGroupName == lastUndoName)
				// a bit hacky here. On undoRedoPerformed there is already no current group in stack, and no way to get current group name
				// so we store previous (before mm change) name and performing undo if this name is first in stack
				// TODO: use undo from MapMagicBrush with ids, it's more stable
				{
					Perform();

					if (currGroupName == lastUndoName)
						lastUndoName = null;
				}
			}
		#endif

		public void NewGroup (MapMagicBrush brush)
		///Registering new undo (at the start of each stroke)
		{
			/*if (sets.Count > 10)
			{
				List<Set> last10 = new List<Set>(sets);
			}*/

			sets.Add( new Dictionary<Terrain,TerrainUndoData>() );

			#if UNITY_EDITOR
				string currGroupName = UnityEditor.Undo.GetCurrentGroupName();
				if (currGroupName != undoName)
					lastUndoName = currGroupName;

				UnityEditor.Undo.RecordObject(brush, undoName);
				brush.temp = !brush.temp;
			#endif
		}


		public void Append (Terrain[] terrains, Vector2D worldPos, Vector2D worldSize,
			bool readHeight=false, bool readSplats=false, bool readGrass=false, bool readTrees=false)
		{
			Dictionary<Terrain,TerrainUndoData> topSet = sets[sets.Count-1];

			foreach (Terrain terrain in terrains)
			{
				Vector2D terrainPos = (Vector2D)terrain.transform.position;
				Vector2D terrainSize = (Vector2D)terrain.terrainData.size;
				if (!Vector2D.Intersects(worldPos, worldSize, terrainPos, terrainSize))
					continue;

				TerrainUndoData undoData;
				if (!topSet.TryGetValue(terrain, out undoData))
				{
					undoData = new TerrainUndoData();
					topSet.Add(terrain, undoData);
				}

				undoData.Read(terrain, worldPos, worldSize, 
					readHeight, readSplats, readGrass, readTrees);
			}

			testTesrrain = terrains[0];
		}

		
		public void Perform ()
		{
			if (sets.Count == 0)
				return; //when two Brush components are used in scene - one will have no undo stack (but undo will be called)

			Dictionary<Terrain,TerrainUndoData> topSet = sets[sets.Count-1];
			sets.RemoveAfter(sets.Count-2);

			foreach (var kvp in topSet)
			{
				Terrain terrain = kvp.Key;
				TerrainUndoData undoData = kvp.Value;

				undoData.Write(terrain);
			}

			foreach (MapMagicBrush brush in GameObject.FindObjectsOfType<MapMagicBrush>())
				brush.UpdateCaches();
		}
	}


	public class UndoBac
	{
		private enum DataType { Height=0, Splats=1, Grass=2, Trees=3, Objects=4 };

		private class Set
		{
			private struct TerrainCoordType
			{
				public Terrain terrain;
				public Coord coord;
				public DataType type;

				public TerrainCoordType (Terrain terrain, Coord coord, DataType type)
				{
					this.terrain = terrain;
					this.coord = coord;
					this.type = type;
				}

				public override int GetHashCode () => terrain.GetHashCode() ^ (coord.GetHashCode() * 10 + (int)type);
			}

			Dictionary<TerrainCoordType,object> dict = new Dictionary<TerrainCoordType, object>();

			public object this[Terrain terrain, Coord coord, DataType type]
			{
				get
				{
					TerrainCoordType tct = new TerrainCoordType(terrain, coord, type);

					if (dict.TryGetValue(tct, out object obj))
						return obj;
					else
						return null;
				}

				set
				{
					TerrainCoordType tct = new TerrainCoordType(terrain, coord, type);
					if (dict.ContainsKey(tct))
						dict[tct] = value;
					else
						dict.Add(tct, value);
				}
			}

			public IEnumerable<(Terrain,Coord,DataType,object)> Datas()
			{
				foreach (var kvp in dict)
				{
					TerrainCoordType tct = kvp.Key;
					yield return (tct.terrain, tct.coord, tct.type, kvp.Value);
				}
			}
		}



		const int numChunksPerTerrain = 8; //number*number of chunks in terrain
		const string undoName = "Brush Stroke";
		public string lastUndoName;  //the last undo group name (kept to know it on UndoRedoPerformed)

		//private Stack<Set> sets = new Stack<Set>();  //we've got to remove first items from it, so using list instead
		[SerializeField] private List<Set> sets = new List<Set>();

		#if UNITY_EDITOR
		///Calling Undo
		///Clone of Tools.GUI.Undo, except it's working with brush rather than gui
			public UndoBac ()
			{
				UnityEditor.Undo.undoRedoPerformed -= OnUndoRedoPerformed;
				UnityEditor.Undo.undoRedoPerformed += OnUndoRedoPerformed;
			}

			public void OnUndoRedoPerformed ()
			{
				string currGroupName = UnityEditor.Undo.GetCurrentGroupName();

				if (currGroupName == undoName || currGroupName == lastUndoName)
				// a bit hacky here. On undoRedoPerformed there is already no current group in stack, and no way to get current group name
				// so we store previous (before mm change) name and performing undo if this name is first in stack
				// TODO: use undo from MapMagicBrush with ids, it's more stable
				{
					Perform();

					if (currGroupName == lastUndoName)
						lastUndoName = null;
				}
			}
		#endif

		public void NewGroup (MapMagicBrush brush)
		///Registering new undo (at the start of each stroke)
		{
			/*if (sets.Count > 10)
			{
				List<Set> last10 = new List<Set>(sets);
			}*/

			Set newSet = new Set();
			sets.Add(newSet);

			#if UNITY_EDITOR
				string currGroupName = UnityEditor.Undo.GetCurrentGroupName();
				if (currGroupName != undoName)
					lastUndoName = currGroupName;

				UnityEditor.Undo.RecordObject(brush, undoName);
				brush.temp = !brush.temp;
			#endif
		}


		public void Append (Terrain[] terrains, Vector2D worldPos, Vector2D worldSize,
			bool readHeight=false, bool readSplats=false)
		{
			foreach (Terrain terrain in terrains)
				using (Log.Group("Read All"))
			{
				TerrainData terrainData = terrain.terrainData;

				Vector2D terrainPos = (Vector2D)terrain.transform.position;
				Vector2D terrainSize = (Vector2D)terrain.terrainData.size;

				if (!Vector2D.Intersects(worldPos, worldSize, terrainPos, terrainSize))
					continue;

				CoordRect intersectionChunks = terrain.PixelRect(worldPos, worldSize, numChunksPerTerrain);
				Coord chunksMin = intersectionChunks.Min; Coord chunksMax = intersectionChunks.Max;

				Set topSet = sets[sets.Count-1];

				for (int x=chunksMin.x; x<chunksMax.x; x++)
					for (int z=chunksMin.z; z<chunksMax.z; z++)
					{
						Coord coord = new Coord(x,z);
						
						if (readHeight  &&  topSet[terrain, coord, DataType.Height] == null)
						{
							CoordRect heightPixels = CoordToPixels(coord, terrainData.heightmapResolution, numChunksPerTerrain);
							float[,] heightData;
							using (Log.Group("Read heights"))
								heightData = terrainData.GetHeights(heightPixels.offset.x, heightPixels.offset.z, heightPixels.size.x, heightPixels.size.z);
							//using (new Log.Timer("Read height texture"))
							//	{ RenderTexture htex = terrainData.heightmapTexture; }
							topSet[terrain, coord, DataType.Height] = heightData;
						}

						if (readSplats  &&  topSet[terrain, coord, DataType.Splats] == null)
						{
							CoordRect splatsPixels = CoordToPixels(coord, terrainData.alphamapResolution, numChunksPerTerrain);
							float[,,] splatsData;
							using (Log.Group("Read splats"))
								splatsData = terrainData.GetAlphamaps(splatsPixels.offset.x, splatsPixels.offset.z, splatsPixels.size.x, splatsPixels.size.z);
							//using (new Log.Timer("Read alpha texture"))
							//	{ Texture2D[] atex = terrainData.alphamapTextures; }
							topSet[terrain, coord, DataType.Splats] = splatsData;
						}
					}
			}
		}

		internal static CoordRect CoordToPixels (Coord coord, int resolution, int numChunks)
		//Converts chunk coord to pixel rect
		//Note that we don't need exact number of pixels in each chunk, come of them might have 64 pixels, some 65 if res==513
		//Tested (MapMagicTester)
		{
			Vector2D minPixelFloat = (1f * resolution / numChunks) * (Vector2D)coord;
			Coord minPixel = (Coord)minPixelFloat;

			Vector2D maxPixelFloat = (1f * resolution / numChunks) * (Vector2D)(coord+1);
			Coord maxPixel = (Coord)maxPixelFloat;
			if (coord.x == numChunks-1) maxPixel.x = resolution;
			if (coord.z == numChunks-1) maxPixel.z = resolution;

			return new CoordRect(minPixel, maxPixel-minPixel);
		}



		public void Perform ()
		{
			Set topSet = sets[sets.Count-1];
			sets.RemoveAfter(sets.Count-2);

			foreach ((Terrain terrain, Coord coord, DataType dataType, object data) in topSet.Datas())
			{
				TerrainData terrainData = terrain.terrainData;

				switch (dataType)
				{
					case DataType.Height:
						CoordRect heightPixels = CoordToPixels(coord, terrainData.heightmapResolution, numChunksPerTerrain);
						terrainData.SetHeights(heightPixels.offset.x, heightPixels.offset.z, (float[,])data);
						break;
					case DataType.Splats:
						CoordRect splatsPixels = CoordToPixels(coord, terrainData.alphamapResolution, numChunksPerTerrain);
						terrainData.SetAlphamaps(splatsPixels.offset.x, splatsPixels.offset.z, (float[,,])data);
						break;
				}
			}
		}

		//public static CoordRect RectByCoord
	}
}