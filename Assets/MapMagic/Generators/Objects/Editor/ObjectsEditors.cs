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
using MapMagic.Nodes.ObjectsGenerators;

namespace MapMagic.Nodes.GUI
{
	public static partial class ObjectsEditors
	{
		/*[Draw.Editor(typeof(ObjectsGenerators.Random200))]
		public static void DrawOutdated (ObjectsGenerators.Random200 gen)
		{
			using (Cell.Padded(1,1,0,0))
			{
				//Draw.Element(UI.current.styles.foldoutBackground);
				using (Cell.LinePx(70))
					Draw.Label("This node is outdated \nand should not be used. \nTry replacing it with the \nnew node with the \nsame name (if any).");
			}
		}*/

		/*[Draw.Editor(typeof(ObjectsGenerators.Scatter200))]
		public static void DrawScatter (ObjectsGenerators.Scatter200 gen)
		{
			//drawing original values

			using (Cell.Padded(1,1,0,0))
			{
				Cell.EmptyLinePx(2);
				using (Cell.LinePx(0))
					using (new Draw.FoldoutGroup(ref gen.guiAdvanced, "Advanced"))
					if (gen.guiAdvanced)
					{
						using (Cell.LineStd) Draw.Field(ref gen.relax, "Relax");
						using (Cell.LineStd) Draw.Field(ref gen.additionalMargins, "Add Margin");
					}
				Cell.EmptyLinePx(2);
			}
		}*/

		[Draw.Editor(typeof(ObjectsGenerators.Spread200))]
		public static void DrawSpread (ObjectsGenerators.Spread200 gen)
		{
			using (Cell.Padded(1,1,0,0)) 
			{
				using (Cell.LineStd) Draw.ToggleLeft(ref gen.retainOriginals, "Retain Originals");
				using (Cell.LineStd) Draw.Field(ref gen.seed, "Seed");
			
				Cell.EmptyLinePx(5);
				using (Cell.LineStd) Draw.Field(ref gen.growth, "Growth", "Min", "Max", 25);

				Cell.EmptyLinePx(5);
				using (Cell.LineStd) Draw.Field(ref gen.distance, "Dist", "Min", "Max", 25);

				Cell.EmptyLinePx(5);
				using (Cell.LineStd) Draw.Field(ref gen.sizeFactor, "Size Factor");
			}
		}


		[Draw.Editor(typeof(ObjectsGenerators.Adjust200))]
		public static void DrawObjectsAdjust (ObjectsGenerators.Adjust200 adj)
		{
			using (Cell.Padded(1,1,0,0)) 
			{
				if (!adj.useRandom)
				{
					using (Cell.LinePx(0))
					using (Cell.Padded(2,0,0,0))
					{
						using (Cell.LineStd) Draw.IconField(ref adj.height.x, "Height", UI.current.textures.GetTexture("DPUI/Icons/Height"));
						using (Cell.LineStd) Draw.IconField(ref adj.rotation.x, "Rotation", UI.current.textures.GetTexture("DPUI/Icons/Rotate"));
						using (Cell.LineStd) Draw.IconField(ref adj.scale.x, "Scale", UI.current.textures.GetTexture("DPUI/Icons/Scale"));
					}
				}

				else
				{
					using (Cell.LineStd) 
					{
						Cell.current.fieldWidth = 0.6f;
						using (Cell.RowPx(16)) Draw.Icon( UI.current.textures.GetTexture("DPUI/Icons/Height") );
						using (Cell.RowRel(0.5f)) 
							Draw.FieldDragIcon(ref adj.height.x);
						using (Cell.RowRel(0.5f)) 
							Draw.FieldDragIcon(ref adj.height.y);
					}

					using (Cell.LineStd) 
					{
						Cell.current.fieldWidth = 0.6f;
						using (Cell.RowPx(16)) Draw.Icon( UI.current.textures.GetTexture("DPUI/Icons/Rotate") );
						using (Cell.RowRel(0.5f)) 
							Draw.FieldDragIcon(ref adj.rotation.x);
						using (Cell.RowRel(0.5f)) 
							Draw.FieldDragIcon(ref adj.rotation.y);
					}

					using (Cell.LineStd) 
					{
						Cell.current.fieldWidth = 0.6f;
						using (Cell.RowPx(16)) Draw.Icon( UI.current.textures.GetTexture("DPUI/Icons/Scale") );
						using (Cell.RowRel(0.5f)) 
							Draw.FieldDragIcon(ref adj.scale.x);
						using (Cell.RowRel(0.5f)) 
							Draw.FieldDragIcon(ref adj.scale.y);
					}

				}

				using (Cell.LineStd) 
				{
					using (Cell.Row) Draw.Label("Random Range");
					using (Cell.RowPx(18)) Draw.Toggle(ref adj.useRandom);
				}

				if (adj.useRandom)
					using (Cell.LineStd) Draw.Field(ref adj.seed, "Seed");

				using (Cell.LineStd) Draw.Field(ref adj.sizeFactor, "Size Factor");

				Cell.EmptyLinePx(5);

				using (Cell.LineStd) Draw.Field(ref adj.relativeness, "Relativity");
			}
		}


		[Draw.Editor(typeof(ObjectsGenerators.Flatten200))]
		public static void DrawFlatten (ObjectsGenerators.Flatten200 gen)
		{
			//drawing standard inspector (radius, hardness)

			using (Cell.Padded(1,1,0,0)) 
			{
				Cell.EmptyLinePx(3);

				using (Cell.LineStd) Draw.ToggleLeft(ref gen.noiseFallof, "Use Noise on Falloff");

				if (gen.noiseFallof)
				{
					using (Cell.LineStd) Draw.Field(ref gen.noiseAmount, "Amount");
					using (Cell.LineStd) Draw.Field(ref gen.noiseSize, "Size");
				}

				//radius warning
				MapMagicObject mapMagic = GraphWindow.current.mapMagic;
				if (mapMagic != null)
				{
					float pixelSize = mapMagic.tileSize.x / (int)mapMagic.tileResolution;
					if (gen.radius > mapMagic.tileMargins * pixelSize)
					{
						Cell.EmptyLinePx(2);
						using (Cell.LinePx(40))
							using (Cell.Padded(2,2,0,0)) 
							{
								Draw.Element(UI.current.styles.foldoutBackground);
								using (Cell.LinePx(15)) Draw.Label("Current setup can");
								using (Cell.LinePx(15)) Draw.Label("create tile seams");
								using (Cell.LinePx(15)) Draw.URL("More", url:"https://gitlab.com/denispahunov/mapmagic/-/wikis/Tile_Seams_Reasons");
							}
						Cell.EmptyLinePx(2);
					}
				}
			}
		}


		[Draw.Editor(typeof(ObjectsGenerators.Stroke200))]
		public static void DrawStroke (ObjectsGenerators.Stroke200 gen)
		{
			//drawing standard inspector (radius, hardness)

			using (Cell.Padded(1,1,0,0)) 
			{
				Cell.EmptyLinePx(3);

				using (Cell.LineStd) Draw.ToggleLeft(ref gen.noiseFallof, "Use Noise on Falloff");

				if (gen.noiseFallof)
				{
					using (Cell.LineStd) Draw.Field(ref gen.noiseAmount, "Amount");
					using (Cell.LineStd) Draw.Field(ref gen.noiseSize, "Size");
				}

				//radius warning
				MapMagicObject mapMagic = GraphWindow.current.mapMagic;
				if (mapMagic != null)
				{
					float pixelSize = mapMagic.tileSize.x / (int)mapMagic.tileResolution;
					if (gen.radius > mapMagic.tileMargins * pixelSize)
					{
						Cell.EmptyLinePx(2);
						using (Cell.LinePx(40))
							using (Cell.Padded(2,2,0,0)) 
							{
								Draw.Element(UI.current.styles.foldoutBackground);
								using (Cell.LinePx(15)) Draw.Label("Current setup can");
								using (Cell.LinePx(15)) Draw.Label("create tile seams");
								using (Cell.LinePx(15)) Draw.URL("More", url:"https://gitlab.com/denispahunov/mapmagic/-/wikis/Tile_Seams_Reasons");
							}
						Cell.EmptyLinePx(2);
					}
				}
			}
		}


		
		[Draw.Editor(typeof(ObjectsGenerators.Split200))]
		public static void SplitGeneratorEditor (ObjectsGenerators.Split200 gen)
		{
			Cell.EmptyLinePx(5);
			using (Cell.LinePx(20)) GeneratorDraw.DrawLayersAddRemove(gen, ref gen.layers, inversed:true);
			using (Cell.LinePx(0)) GeneratorDraw.DrawLayersThemselves(gen, gen.layers, inversed:true, layerEditor:DrawSplitLayer);
		}
		
		private static void DrawSplitLayer (Generator tgen, int num)
		{
			Split200 splitGen = (Split200)tgen;
			Split200.SplitLayer layer = splitGen.layers[num];
			if (layer == null) return;

			using (Cell.LinePx(0))
			{	
				Cell.EmptyLinePx(2);
				using (Cell.LineStd)
				{
					using (Cell.Row) Draw.EditableLabel(ref layer.name);

					using (Cell.RowPx(20)) Draw.LayerChevron(num, ref splitGen.guiExpanded);

					Cell.EmptyRowPx(10);
					using (Cell.RowPx(0)) GeneratorDraw.DrawOutlet(layer);
				}
				Cell.EmptyLinePx(2);
			}

			if (splitGen.guiExpanded == num)
				using (Cell.LinePx(0))
					using (Cell.Padded(1,1,0,0))
			{
				Cell.EmptyLinePx(5);
				using (Cell.LineStd) Draw.Field(ref layer.chance, "Probability");
				Cell.EmptyLinePx(5);

				using (Cell.LineStd) Draw.ToggleLeft(ref layer.heightConditionActive, "Height Condition");	
				if (layer.heightConditionActive)
				{
					using (Cell.LineStd)
					{
						using (Cell.RowRel(0.5f)) Draw.FieldDragIcon(ref layer.heightCondition.x);
						using (Cell.RowRel(0.5f)) Draw.FieldDragIcon(ref layer.heightCondition.y);
					}
					Cell.EmptyLinePx(5);
				}

				using (Cell.LineStd) Draw.ToggleLeft(ref layer.rotationConditionActive, "Rotation Condition");	
				if (layer.rotationConditionActive)
				{
					using (Cell.LineStd)
					{
						using (Cell.RowRel(0.5f)) Draw.FieldDragIcon(ref layer.rotationCondition.x);
						using (Cell.RowRel(0.5f)) Draw.FieldDragIcon(ref layer.rotationCondition.y);
					}
					Cell.EmptyLinePx(5);
				}

				using (Cell.LineStd) Draw.ToggleLeft(ref layer.scaleConditionActive, "Scale Condition");	
				if (layer.scaleConditionActive)
				{
					using (Cell.LineStd)
					{
						using (Cell.RowRel(0.5f)) Draw.FieldDragIcon(ref layer.scaleCondition.x);
						using (Cell.RowRel(0.5f)) Draw.FieldDragIcon(ref layer.scaleCondition.y);
					}
					Cell.EmptyLinePx(5);
				}
			}
		}

		
		[Draw.Editor(typeof(ObjectsGenerators.ObjectsOutput))]
		public static void DrawObjectsOutput (ObjectsGenerators.ObjectsOutput gen) 
		{
			if (gen.posSettings == null) gen.posSettings = ObjectsOutput.CreatePosSettings(gen);

			using (Cell.LineStd)
				DrawObjectPrefabs(ref gen.prefabs, gen.guiMultiprefab, treeIcon:true);

			using (Cell.LinePx(0))
				using (Cell.Padded(2,2,0,0))
			{
				using (Cell.LineStd) Draw.ToggleLeft(ref gen.guiMultiprefab, "Multi-Prefab");
			
				Cell.EmptyRowPx(4);

				using (Cell.LinePx(0))
					using (new Draw.FoldoutGroup(ref gen.guiProperties, "Properties"))
						if (gen.guiProperties)
						{
							Cell.current.fieldWidth = 0.481f;
							using (Cell.LineStd) Draw.ToggleLeft(ref gen.allowReposition, "Use Pool");
							using (Cell.LineStd) Draw.ToggleLeft(ref gen.instantiateClones, "As Clones"); 
							using (Cell.LineStd) Draw.Field(ref gen.biomeBlend, "Biome Blend");

							using (Cell.LineStd) GeneratorDraw.DrawGlobalVar(ref GraphWindow.current.mapMagic.globals.objectsNumPerFrame, "Num/Frame");
						}

				Cell.EmptyRowPx(2);
				DrawPositioningSettings(gen.posSettings, billboardRotWaring:true);
			}
		}


		[Draw.Editor(typeof(ObjectsGenerators.TreesOutput))]
		public static void DrawTreesOutput (ObjectsGenerators.TreesOutput gen) 
		{
			if (gen.posSettings == null) gen.posSettings = TreesOutput.CreatePosSettings(gen);

			using (Cell.LineStd)
				DrawObjectPrefabs(ref gen.prefabs, gen.guiMultiprefab, treeIcon:true);

			using (Cell.LinePx(0))
				using (Cell.Padded(2,2,0,0))
			{
				using (Cell.LineStd) Draw.ToggleLeft(ref gen.guiMultiprefab, "Multi-Prefab");
			
				Cell.EmptyRowPx(4);

				using (Cell.LinePx(0))
					using (new Draw.FoldoutGroup(ref gen.guiProperties, "Properties"))
						if (gen.guiProperties)
						{
							Cell.current.fieldWidth = 0.481f;
							using (Cell.LineStd) Draw.Field(ref gen.color, "Color");
							using (Cell.LineStd) Draw.Field(ref gen.lightmapColor, "Lightmap");
							using (Cell.LineStd) gen.bendFactor = Draw.Field(gen.bendFactor, "Bend Factor");
							using (Cell.LineStd) Draw.Field(ref gen.biomeBlend, "Biome Blend");
						}

				Cell.EmptyRowPx(2);
				DrawPositioningSettings(gen.posSettings, billboardRotWaring:true);
			}
		}


		[Draw.Editor(typeof(ObjectsGenerators.Rarefy200))]
		public static void DrawRarefyGenerator (ObjectsGenerators.Rarefy200 gen)
		{
			using (Cell.LinePx(0))
				using (Cell.Padded(1,1,0,0)) 
			{
				using (Cell.LineStd) Draw.Field(ref gen.distance, "Distance");
				using (Cell.LineStd) Draw.Field(ref gen.sizeFactor, "Size Factor");
				using (Cell.LineStd) Draw.Toggle(ref gen.self, "Use Self");
			}

			using (Cell.LinePx(0))
			LayersEditor.DrawLayers(ref gen.layers, 
				onDraw: num =>
				{
					if (num>=gen.layers.Length) return; //on layer remove
					int iNum = gen.layers.Length-1 - num;

					Cell.EmptyLinePx(2);
					using (Cell.LineStd)
					{
						using (Cell.RowPx(0)) 
							GeneratorDraw.DrawInlet(gen.layers[iNum].inlet, gen);
						Cell.EmptyRowPx(10);
						using (Cell.RowPx(20)) Draw.Icon( UI.current.textures.GetTexture("DPUI/Icons/Layer") );

						using (Cell.Row)
						{
							using (Cell.RowPx(43)) Draw.Label("Dist");
							using (Cell.Row) Draw.FieldDragIcon(ref gen.layers[iNum].distance);

							Cell.current.Expose(gen.layers[iNum].id, "distance", typeof(float));
						}


						Cell.EmptyRowPx(2);
					}
					Cell.EmptyLinePx(2);
				},
				onCreate: num => new ObjectsGenerators.Rarefy200.Layer(gen) );

			if (Cell.current.valChanged)
				GraphWindow.RefreshMapMagic(gen);
		}


		[Draw.Editor(typeof(ObjectsGenerators.Positions200))]
		public static void DrawPositionsGenerator (ObjectsGenerators.Positions200 gen)
		{
			using (Cell.LinePx(0))
			LayersEditor.DrawLayers(ref gen.positions,
				onDraw: num =>
				{
					if (num>=gen.positions.Length) return; //on layer remove
					int iNum = gen.positions.Length-1 - num;

					Cell.EmptyLinePx(2);
					using (Cell.LineStd)
					{
						Cell.current.fieldWidth = 0.7f;
						Cell.EmptyRowPx(2);
						using (Cell.RowPx(15)) Draw.Icon( UI.current.textures.GetTexture("DPUI/Icons/Layer") );
						using (Cell.Row)
						{
							using (Cell.LineStd) Draw.Field(ref gen.positions[num].x, "X");
							using (Cell.LineStd) Draw.Field(ref gen.positions[num].y, "Y");
							using (Cell.LineStd) Draw.Field(ref gen.positions[num].z, "Z");

							Cell.current.Expose(gen.id, "positions", typeof(Vector3), arrIndex:num);
						}
						Cell.EmptyRowPx(2);
					}
					Cell.EmptyLinePx(2);
				} );

			if (Cell.current.valChanged)
				GraphWindow.RefreshMapMagic(gen);
		}


		[Draw.Editor(typeof(ObjectsGenerators.Combine200))]
		public static void DrawCombineGenerator (ObjectsGenerators.Combine200 gen)
		{
			using (Cell.LinePx(0))
			LayersEditor.DrawLayers(ref gen.inlets,
				onDraw: num =>
				{
					if (num>=gen.inlets.Length) return; //on layer remove
					int iNum = gen.inlets.Length-1 - num;

					Cell.EmptyLinePx(2);
					using (Cell.LineStd)
					{
						using (Cell.RowPx(0)) 
							GeneratorDraw.DrawInlet(gen.inlets[iNum], gen);
						Cell.EmptyRowPx(10);
						using (Cell.RowPx(15)) Draw.Icon( UI.current.textures.GetTexture("DPUI/Icons/Layer") );
						using (Cell.Row) Draw.Label("Layer " + iNum);
					}
					Cell.EmptyLinePx(2);
				},
				onCreate: num => new Inlet<TransitionsList>() );

			if (Cell.current.valChanged)
				GraphWindow.RefreshMapMagic(gen);
		}


		[Draw.Editor(typeof(ObjectsGenerators.Stamp200))]
		public static void DrawStampGenerator (ObjectsGenerators.Stamp200 gen)
		{
			//drawing standard class values

			using (Cell.Padded(1,1,0,0)) 
			{
				Cell.current.fieldWidth = 0.4f;

				//Draw.Class(gen, "Custom");
				CellExpose.ExposableClass(gen, gen.id, "Custom");

				/*using (Cell.LineStd) Draw.Field(ref gen.size, "Size");
				using (Cell.LineStd) Draw.Field(ref gen.intensity, "Intensity");
				using (Cell.LineStd) Draw.Field(ref gen.sizeFactor, "Size Factor");
				using (Cell.LineStd) Draw.Field(ref gen.intensityFactor, "Intensity Factor");
				using (Cell.LineStd) Draw.Field(ref gen.blendType, "Blend Type");

				using (Cell.LineStd) Draw.ToggleLeft(ref gen.useRotation, "Use Rotation");
				using (Cell.LineStd) Draw.ToggleLeft(ref gen.useFalloff, "Use Falloff");*/

				if (gen.useFalloff)
				//	using (Cell.LineStd) Draw.Field(ref gen.hardness, "Hardness");
				//	Draw.Class(gen, "UseFallof");
					CellExpose.ExposableClass(gen, gen.id, "UseFallof");

				//radius warning
				MapMagicObject mapMagic = GraphWindow.current.mapMagic;
				if (mapMagic != null)
				{
					float pixelSize = mapMagic.tileSize.x / (int)mapMagic.tileResolution;
					if (gen.size > mapMagic.tileMargins * pixelSize)
					{
						Cell.EmptyLinePx(2);
						using (Cell.LinePx(40))
							using (Cell.Padded(2,2,0,0)) 
							{
								Draw.Element(UI.current.styles.foldoutBackground);
								using (Cell.LinePx(15)) Draw.Label("Current setup can");
								using (Cell.LinePx(15)) Draw.Label("create tile seams");
								using (Cell.LinePx(15)) Draw.URL("More", url:"https://gitlab.com/denispahunov/mapmagic/-/wikis/Tile_Seams_Reasons");
							}
						Cell.EmptyLinePx(2);
					}
				}
			}
		}
	}
}