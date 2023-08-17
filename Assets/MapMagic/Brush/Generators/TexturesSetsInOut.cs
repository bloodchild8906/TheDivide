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
		name = "Textures", 
		section=2, 
		colorType = typeof(MatrixSet), 
		iconName="GeneratorIcons/BrushTexturesIn",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/HeightInOut")]
	public class  BrushReadTextureSet : Generator, IOutlet<MatrixSet>,  IFnEnter<MatrixSet>, IBrushRead
	{
		public string Name { get => "Maps Set"; set {} } //as function portal

		public override void Generate (TileData data, StopToken stop) { }

		public void ReadTerrains (TerrainCache[] terrainCaches, TileData tileData)
		{
			MatrixSet matrixSet = new MatrixSet(tileData.area.full.rect, tileData.area.full.worldPos, tileData.area.full.worldSize, tileData.globals.height);

			foreach (TerrainCache terrainCache in terrainCaches)
				terrainCache.ReadTextures(matrixSet);

			tileData.StoreProduct(this, matrixSet);
		}

		private static void PerformRead (Terrain terrain, MatrixSet matrixSet)
		/// Calls ReadSplats, but prepares per-channel matrices beforehand
		/// Rect needed to call with empty tlayeyrs-matrices dict
		{
			TerrainData terrainData =  terrain.terrainData;
			int resolution = terrainData.alphamapResolution;

			CoordRect terrainRect = terrain.PixelRect(resolution);
			CoordRect intersection = CoordRect.Intersected(terrainRect, matrixSet.rect);
			if (intersection.size.x<=0 || intersection.size.z<=0) return;

			TerrainLayer[] layers = terrainData.terrainLayers;
			float[,,] splats = terrainData.GetAlphamaps(
				intersection.offset.x-terrainRect.offset.x, intersection.offset.z-terrainRect.offset.z, intersection.size.x, intersection.size.z);

			for (int p=0; p<layers.Length; p++)
			{
				MatrixSet.Prototype prototype = new MatrixSet.Prototype(layers, p);

				if (!matrixSet.TryGetValue(prototype, out Matrix matrix))
				{
					matrix = new Matrix(matrixSet.rect);
					matrixSet[prototype] = matrix;
				}

				matrix.ImportSplats(splats, intersection.offset, p);
			}
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Brush/Output", 
		name = "Textures", 
		section=2, 
		colorType = typeof(MatrixSet), 
		iconName="GeneratorIcons/BrushTexturesOut",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/HeightInOut")]
	public class  BrushWriteTextureSet : Generator, IInlet<MatrixSet>,  IFnExit<MatrixSet>, IBrushWrite, IRelevant
	{
		public string Name { get => "Maps Set"; set {} } //as function portal

		public override void Generate (TileData data, StopToken stop) { }


		public void WriteTerrains (TerrainCache[] terrainCaches, CacheChange change, TileData tileData)
		{
			MatrixSet matrixSet = tileData.ReadInletProduct(this);

			foreach (TerrainCache terrainCache in terrainCaches)
				terrainCache.WriteTextures(matrixSet);

			change.AddRect(matrixSet.rect);
			change.AddFlag(CacheChange.Type.Splats);
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
	}
}