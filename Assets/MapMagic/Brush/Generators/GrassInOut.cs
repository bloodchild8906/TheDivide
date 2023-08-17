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
		name = "Grass", 
		section=2, 
		colorType = typeof(MatrixSet), 
		iconName="GeneratorIcons/BrushGrassIn",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/GrassInOut")]
	public class  BrushReadGrassSet : Generator, IOutlet<MatrixSet>,  IFnEnter<MatrixSet>, IBrushRead
	{
		public string Name { get => "Grass"; set {} } //as function portal

		public override void Generate (TileData data, StopToken stop) { }

		public void ReadTerrains (TerrainCache[] terrainCaches, TileData tileData)
		{
			MatrixSet matrixSet = new MatrixSet(tileData.area.full.rect, tileData.area.full.worldPos, tileData.area.full.worldSize, tileData.globals.height);

			foreach (TerrainCache terrainCache in terrainCaches)
				terrainCache.ReadGrass(matrixSet);

			tileData.StoreProduct(this, matrixSet);
		}


		private static void PerformRead (Terrain terrain,  MatrixSet matrixSet)
		/// Calls ReadSplats, but prepares per-channel matrices beforehand
		/// Rect needed to call with empty tlayeyrs-matrices dict
		{
			TerrainData terrainData =  terrain.terrainData;
			int resolution = terrainData.detailResolution;

			CoordRect terrainRect = terrain.PixelRect(resolution);
			CoordRect intersection = CoordRect.Intersected(terrainRect, matrixSet.rect);
			if (intersection.size.x<=0 || intersection.size.z<=0) return;

			DetailPrototype[] detLayers = terrainData.detailPrototypes;
			UnityEngine.Object[] textures = detLayers.Select(p => p.Object());

			for (int i=0; i<detLayers.Length; i++)
			{
				MatrixSet.Prototype prototype = new MatrixSet.Prototype(detLayers, i);

				if (!matrixSet.TryGetValue(prototype, out Matrix matrix)) //adding new matrix if it's not in set yet
				{
					matrix = new Matrix(matrixSet.rect);
					matrixSet[prototype] = matrix;
				}

				int[,] layer = terrainData.GetDetailLayer(
					intersection.offset.x-terrainRect.offset.x, 
					intersection.offset.z-terrainRect.offset.z, 
					intersection.size.x, 
					intersection.size.z,
					i);

				matrix.ImportDetail(layer, intersection.offset, density:1);
			}
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Brush/Output", 
		name = "Grass", 
		section=2, 
		colorType = typeof(MatrixSet), 
		iconName="GeneratorIcons/BrushGrassOut",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/GrassInOut")]
	public class  BrushWriteGrassSet : Generator, IInlet<MatrixSet>,  IFnExit<MatrixSet>, IBrushWrite, IRelevant
	{
		public string Name { get => "Grass"; set {} } //as function portal


		public override void Generate (TileData data, StopToken stop) { }


		public void WriteTerrains (TerrainCache[] terrainCaches, CacheChange change, TileData tileData)
		{
			MatrixSet matrixSet = tileData.ReadInletProduct(this);

			foreach (TerrainCache terrainCache in terrainCaches)
				terrainCache.WriteGrass(matrixSet);

			change.AddRect(matrixSet.rect);
			change.AddFlag(CacheChange.Type.Grass);
		}


		public static void PerformWrite (Terrain terrain, MatrixSet matrixSet, Noise random)
		{
			DetailPrototype[] originalDetLayers = terrain.terrainData.detailPrototypes;

			//adding to terrain prototypes that are in set, but not yet present on terrain
			DetailPrototype[] detLayers = originalDetLayers;
			foreach (MatrixSet.Prototype prototype in matrixSet.Prototypes)
				detLayers = prototype.CheckAppendLayers(detLayers);

			//finding intersection
			int resolution = terrain.terrainData.detailResolution;
			if (originalDetLayers.Length==0)  //no grass of any type added	
			{
				resolution = terrain.terrainData.heightmapResolution;
				terrain.terrainData.SetDetailResolution(terrain.terrainData.heightmapResolution, 16); 
			}
			CoordRect terrainRect = terrain.PixelRect(resolution);

			CoordRect intersection = CoordRect.Intersected(terrainRect, matrixSet.rect);
			intersection = CoordRect.Intersected(intersection, matrixSet.rect); 
			if (intersection.size.x<=0 || intersection.size.z<=0) return;

			//setting prototypes
			if (originalDetLayers != detLayers) terrain.terrainData.detailPrototypes = detLayers;

			//setting arrays
			for (int p=0; p<detLayers.Length; p++)
			{
				MatrixSet.Prototype prototype = new MatrixSet.Prototype(detLayers, p);

				Matrix matrix;
				bool isInSet = matrixSet.TryGetValue(prototype, out matrix);


				if (!isInSet) //clearing channel if it has no matrix layer in output
					continue; //not sure, it might require clearing something
				
				else
				{
					int[,] detailLayer = new int[intersection.size.z, intersection.size.x];
					matrix.ExportDetail(detailLayer, p, random, density:1);
					terrain.terrainData.SetDetailLayer(
						intersection.offset.x-terrainRect.offset.x, 
						intersection.offset.z-terrainRect.offset.z, 
						p,
						detailLayer);
				}
			}
		}

	}

}