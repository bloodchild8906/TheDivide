using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.Profiling;

using Den.Tools;
using Den.Tools.Matrices;
using Den.Tools.GUI;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Nodes.GUI;
using MapMagic.Nodes;

namespace MapMagic.Brush
{
	public static class Editors
	{
		private static string[] terrainLayersNames;
		private static List<TerrainLayer> terrainLayers = new List<TerrainLayer>();
		private static HashSet<TerrainLayer> terrainLayersHash = new HashSet<TerrainLayer>();

		private static void RefreshTerrainLayers ()
		{
			//layers themselves
			MapMagicBrush brush = BrushInspector.lastBrush;
			if (brush != null  &&  brush.preset.graph != null  &&  brush.preset.graph == GraphWindow.current.graph  &&  
				brush.terrains != null  &&  brush.terrains.Length != 0)
				{
					terrainLayers.Clear();
					terrainLayersHash.Clear();
					//do not add brush.opTerrains[0].terrainData.terrainLayers initially, since layer one could be duplicated here

					foreach (Terrain terrain in brush.terrains)
					{
						TerrainLayer[] curLayers = terrain.terrainData.terrainLayers;
						foreach (TerrainLayer layer in curLayers)
						{
							if (!terrainLayersHash.Contains(layer))
								{ terrainLayers.Add(layer); terrainLayersHash.Add(layer); }
						}
					}
				}

			//names
			if (terrainLayersNames == null || terrainLayersNames.Length != terrainLayers.Count)
				terrainLayersNames = new string[terrainLayers.Count];

			for (int i=0; i<terrainLayers.Count; i++)
				terrainLayersNames[i] = terrainLayers[i].name;
		}


		[Draw.Editor(typeof(BrushReadObjects))]
		public static void DrawReadObjects (BrushReadObjects gen)
		{
			using (Cell.LineStd) Draw.ToggleLeft(ref gen.specificPrefabs, "Only Specific Prefabs");

			if (gen.specificPrefabs)
				using (Cell.LineStd)
					ObjectsEditors.DrawObjectPrefabs(ref gen.prefabs, true, treeIcon:true);
		}


		[Draw.Editor(typeof(BrushWriteObjects))]
		public static void DrawWriteObjects (BrushWriteObjects gen) 
		{
			using (Cell.LineStd)
				ObjectsEditors.DrawObjectPrefabs(ref gen.prefabs,multiPrefab:true, treeIcon:true);

			using (Cell.LinePx(0))
				using (Cell.Padded(2,2,0,0))
			{
				Cell.EmptyRowPx(4);

				using (Cell.LinePx(0))
					using (new Draw.FoldoutGroup(ref gen.guiProperties, "Properties"))
						if (gen.guiProperties)
						{
							using (Cell.LineStd) Draw.ToggleLeft(ref gen.instantiateClones, "As Clones"); 
						}

				Cell.EmptyRowPx(2);
				ObjectsEditors.DrawPositioningSettings(gen.posSettings, billboardRotWaring:true);
			}
		}


		[Draw.Editor(typeof(BrushTransferObjects))]
		public static void DrawTransferObjects (BrushTransferObjects gen)
		{
			using (Cell.LineStd) Draw.ToggleLeft(ref gen.specificPrefabs, "Only Specific Prefabs");

			if (gen.specificPrefabs)
				using (Cell.LineStd)
					ObjectsEditors.DrawObjectPrefabs(ref gen.prefabs, true, treeIcon:true);

			using (Cell.LinePx(0))
				using (Cell.Padded(2,2,0,0))
			{
				Cell.EmptyRowPx(2);
				ObjectsEditors.DrawPositioningSettings(gen.posSettings, billboardRotWaring:true, showRelativeHeight:false);
			}
		}


		[Draw.Editor(typeof(BrushReadTrees))]
		public static void DrawReadTrees (BrushReadTrees gen)
		{
			using (Cell.LineStd) Draw.ToggleLeft(ref gen.specificPrefabs, "Only Specific Prefabs");

			if (gen.specificPrefabs)
				using (Cell.LineStd)
					ObjectsEditors.DrawObjectPrefabs(ref gen.prefabs, true, treeIcon:true);
		}


		[Draw.Editor(typeof(BrushWriteTrees))]
		public static void DrawWriteTrees (BrushWriteTrees gen)
		{
			using (Cell.LineStd)
				ObjectsEditors.DrawObjectPrefabs(ref gen.prefabs,multiPrefab:true, treeIcon:true);

			using (Cell.LinePx(0))
				using (Cell.Padded(2,2,0,0))
			{
				//using (Cell.LineStd) Draw.ToggleLeft(ref gen.guiMultiprefab, "Multi-Prefab");
			
				Cell.EmptyRowPx(4);

				using (Cell.LinePx(0))
					using (new Draw.FoldoutGroup(ref gen.guiProperties, "Properties"))
						if (gen.guiProperties)
						{
							Cell.current.fieldWidth = 0.481f;
							using (Cell.LineStd) Draw.Field(ref gen.color, "Color");
							using (Cell.LineStd) Draw.Field(ref gen.lightmapColor, "Lightmap");
							using (Cell.LineStd) gen.bendFactor = Draw.Field(gen.bendFactor, "Bend Factor");
						}

				Cell.EmptyRowPx(2);
				ObjectsEditors.DrawPositioningSettings(gen.posSettings, billboardRotWaring:true);
			}
		}

		[Draw.Editor(typeof(BrushTransferTrees))]
		public static void DrawTransferTrees (BrushTransferTrees gen)
		{
			using (Cell.LineStd) Draw.ToggleLeft(ref gen.specificPrefabs, "Only Specific Prefabs");

			if (gen.specificPrefabs)
				using (Cell.LineStd)
					ObjectsEditors.DrawObjectPrefabs(ref gen.prefabs, true, treeIcon:true);

			using (Cell.LinePx(0))
				using (Cell.Padded(2,2,0,0))
			{
				using (Cell.LinePx(0))
					using (new Draw.FoldoutGroup(ref gen.guiProperties, "Properties"))
						if (gen.guiProperties)
						{
							Cell.current.fieldWidth = 0.481f;
							using (Cell.LineStd) Draw.Field(ref gen.color, "Color");
							using (Cell.LineStd) Draw.Field(ref gen.lightmapColor, "Lightmap");
							using (Cell.LineStd) gen.bendFactor = Draw.Field(gen.bendFactor, "Bend Factor");
						}

				Cell.EmptyRowPx(2);
				ObjectsEditors.DrawPositioningSettings(gen.posSettings, billboardRotWaring:true, showRelativeHeight:false);
			}
		}
	}
}