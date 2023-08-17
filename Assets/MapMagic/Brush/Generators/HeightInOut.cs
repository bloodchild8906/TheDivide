using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Terrains;
using MapMagic.Nodes;
using MapMagic.Nodes.MatrixGenerators;

using UnityEngine.Profiling;

namespace MapMagic.Brush
{
	[Serializable]
	[GeneratorMenu(
		menu = "Brush/Input", 
		name = "Height", 
		section=2, 
		colorType = typeof(MatrixWorld), 
		iconName="GeneratorIcons/BrushHeightIn",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/HeightInOut")]
	public class  BrushReadHeight206 : Generator, IOutlet<MatrixWorld>, IFnEnter<MatrixWorld>, IBrushRead
	{
		public string Name { get => "Height"; set {} } //as function portal

		public override void Generate (TileData data, StopToken stop) { }

		public void ReadTerrains (TerrainCache[] terrainCaches, TileData tileData)
		{
			MatrixWorld matrix = new MatrixWorld(
				tileData.area.full.rect, 
				tileData.area.full.worldPos, 
				tileData.area.full.worldSize, 
				tileData.globals.height);
	
			foreach (TerrainCache terrainCache in terrainCaches)
				terrainCache.ReadHeights(matrix);

			tileData.StoreProduct(this, matrix);
		}

		public static void ReadHeights (Terrain terrain, Matrix matrix)
		/// Reads height data from terrain to matrix within given pixel rect (origin/non-terrain rect)
		/// Saves in readData as well (if any)
		{
			TerrainData terrainData =  terrain.terrainData;
			int resolution = terrainData.heightmapResolution;

			CoordRect terrainRect = terrain.PixelRect(resolution);
			CoordRect intersection = CoordRect.Intersected(terrainRect, matrix.rect);
			if (intersection.size.x<=0 || intersection.size.z<=0) return;

			CoordRect heightsRect = intersection;  //terrain-relative rect (terrain zero is 0,0)
			heightsRect.offset -= terrainRect.offset;

			float[,] heights = terrainData.GetHeights(heightsRect.offset.x, heightsRect.offset.z, heightsRect.size.x, heightsRect.size.z);
				
			matrix.ImportHeights(heights, intersection.offset);
		}

		public static void ReadTerrainTexture (Terrain terrain, Matrix matrix, Texture2D tempTex=null, RenderTexture renTex=null)
		{
			TerrainData terrainData =  terrain.terrainData;
			int resolution = terrainData.heightmapResolution;

			CoordRect terrainRect = terrain.PixelRect(resolution);
			CoordRect intersection = CoordRect.Intersected(terrainRect, matrix.rect);
			if (intersection.size.x<=0 || intersection.size.z<=0) return;

			CoordRect heightsRect = intersection;  //terrain-relative rect (terrain zero is 0,0)
			heightsRect.offset -= terrainRect.offset;

			RenderTexture terrainTex = terrain.terrainData.heightmapTexture;

			if (tempTex == null ||  tempTex.width != terrainTex.width  ||  tempTex.height != terrainTex.height)
				tempTex = new Texture2D(terrainTex.width,  terrainTex.height, TextureFormat.R16, mipChain:false, linear:true);

			//Graphics.CopyTexture(terrainTex, tempTex);
			RenderTexture bacRenTex = RenderTexture.active;
			RenderTexture.active = terrainTex;
			using (Log.Group("Read Pixels"))
			tempTex.ReadPixels(new Rect(0, 0, tempTex.width, tempTex.height), 0, 0);
			tempTex.Apply();
			RenderTexture.active = bacRenTex;

			byte[] bytes = tempTex.GetRawTextureData();
			//float ushortEpsilon = 1f / 65535; //since setheights is using not full ushort range, but range-1
			//matrix.ImportRawFloat(bytes, intersection.offset, intersection.size, mult:0.5f-ushortEpsilon);
			matrix.ImportRaw16(bytes, new Coord(0,0), new Coord(tempTex.width, tempTex.height));
			matrix.Multiply(2);

		//	Color[] colors = tempTex.GetPixels();
		//	matrix.ImportColors(colors, new Coord(0,0), new Coord(tempTex.width, tempTex.height), channel:0);
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Brush/Output", 
		name = "Height", 
		section=2, 
		colorType = typeof(MatrixWorld), 
		iconName="GeneratorIcons/BrushHeightOut",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/HeightInOut")]
	public class  BrushWriteHeight206 : Generator, IInlet<MatrixWorld>, IFnExit<MatrixWorld>, IBrushWrite, IRelevant
	{
		public string Name { get => "Height"; set {} } //as function portal

		public override void Generate (TileData data, StopToken stop) 
		{ 
			MatrixWorld matrix = data.ReadInletProduct(this);
			data.heights = matrix; //will need this while placing objects
		}


		public void WriteTerrains (TerrainCache[] terrainCaches, CacheChange change, TileData tileData)
		{
			MatrixWorld matrix = tileData.heights;

			foreach (TerrainCache terrainCache in terrainCaches)
				terrainCache.WriteHeights(matrix);

			change.AddRect(matrix.rect);
			change.AddFlag(CacheChange.Type.Height);
		}


		public static void WriteTerrain (Terrain terrain, Matrix matrix)
		{
			CoordRect terrainRect = BrushOps.GetHeightPixelRect(terrain);

			CoordRect intersection = CoordRect.Intersected(terrainRect, matrix.rect);
			if (intersection.size.x<=0 || intersection.size.z<=0) return;

			float[,] heights = new float[intersection.size.x, intersection.size.z];
			matrix.ExportHeights(heights, intersection.offset);

			terrain.terrainData.SetHeightsDelayLOD(intersection.offset.x-terrainRect.offset.x, intersection.offset.z-terrainRect.offset.z, heights);
		}

		
		public static void WriteTerrainTexture (Terrain terrain, Matrix matrix, Texture2D tempTex=null, RenderTexture renTex=null)
		{
			TerrainData data = terrain.terrainData;

			CoordRect terrainRect = BrushOps.GetHeightPixelRect(terrain);

			CoordRect intersection = CoordRect.Intersected(terrainRect, matrix.rect);
			if (intersection.size.x<=0 || intersection.size.z<=0) return;

			byte[] bytes = new byte[intersection.size.x*intersection.size.z*4];
			float ushortEpsilon = 1f / 65535; //since setheights is using not full ushort range, but range-1
			matrix.ExportRawFloat(bytes, intersection.offset, intersection.size, mult:0.5f-ushortEpsilon);

			if (tempTex == null ||  tempTex.width != intersection.size.x  ||  tempTex.height != intersection.size.z)
				tempTex = new Texture2D(intersection.size.x, intersection.size.z, TextureFormat.RFloat, mipChain:false, linear:true);
			tempTex.LoadRawTextureData(bytes);
			tempTex.Apply(updateMipmaps:false);

			if (renTex == null ||  tempTex.width != intersection.size.x  ||  tempTex.height != intersection.size.z)
			#if UNITY_2019_2_OR_NEWER
				renTex = new RenderTexture(intersection.size.x, intersection.size.z, 32, RenderTextureFormat.RFloat, mipCount:0);
			#else
				renTex = new RenderTexture(intersection.size.x, intersection.size.z, 32, RenderTextureFormat.RFloat);
			#endif
			Graphics.Blit(tempTex, renTex);

			RenderTexture bacRenTex = RenderTexture.active;
			RenderTexture.active = renTex;

			RectInt texRect = new RectInt(0,0, intersection.size.x, intersection.size.z);
			data.CopyActiveRenderTextureToHeightmap(texRect, new Vector2Int(intersection.offset.x, intersection.offset.z), TerrainHeightmapSyncControl.None);
			//data.DirtyHeightmapRegion(texRect, TerrainHeightmapSyncControl.HeightAndLod); //or seems a bit faster on re-setting height to already existing terrains. IDK
			//data.SyncHeightmap(); //doesn't seems to make difference with DirtyHeightmapRegion, waiting for readback anyways

			RenderTexture.active = bacRenTex;
		}
	}

}