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
using MapMagic.Nodes.MatrixSetsGenerators;

namespace MapMagic.Nodes.GUI
{
	public static class MatrixSetsEditors
	{

		[Draw.Editor(typeof(Add210))]
		public static void DrawAddGen (Add210 gen)
		{
			using (Cell.Padded(1,1,0,0))
			{
				Cell.current.fieldWidth = 0.6f;

				using (Cell.RowPx(35))
					using (Cell.LinePx(35))
						using (Cell.Padded(4,2,4,2))
					{
						Texture2D tex;
						if (gen.prototypeType == PrototypeType.TerrainLayer  &&  gen.terrainLayer != null)
							tex = gen.terrainLayer.diffuseTexture;
						if (gen.prototypeType == PrototypeType.GrassTexture  &&  gen.texture != null)
							tex = gen.texture;
						else
							tex = UI.current.textures.GetTexture("DPUI/Backgrounds/Empty");

						Draw.TextureIcon(tex);
					}

				using (Cell.Row)
				{
					Cell.current.fieldWidth = 0.58f;

					using (Cell.LineStd) Draw.Field(ref gen.prototypeType, "Type");

					using (Cell.LineStd) 
					{
						if (gen.prototypeType == PrototypeType.TerrainLayer)
						{
							Draw.ObjectField(ref gen.terrainLayer, "Layer");
							Cell.current.Expose(gen.id, "terrainLayer", typeof(TerrainLayer));
						}

						else if (gen.prototypeType == PrototypeType.GrassTexture)
						{
							Draw.ObjectField(ref gen.texture, "Texture");
							Cell.current.Expose(gen.id, "texture", typeof(Texture2D));
						}

						else
						{
							Draw.ObjectField(ref gen.prefab, "Prefab");
							Cell.current.Expose(gen.id, "prefab", typeof(GameObject));
						}
					}

					using (Cell.LineStd) 
					{ 
						Draw.Field(ref gen.instanceNum, "Num"); 
						Cell.current.Expose(gen.id, "instanceNum", typeof(int));
					}

					using (Cell.LineStd) 
					{
						Draw.Field(ref gen.mode, "Mode");
						Cell.current.Expose(gen.id, "mode", typeof(int));
					}

					using (Cell.LineStd) 
					{ 
						Draw.Field(ref gen.opacity, "Opacity"); 
						Cell.current.Expose(gen.id, "opacity", typeof(float));
					}

					/*using (Cell.LineStd) 
					{
						Draw.ToggleLeft(ref gen.normalize, "Normalize");
						Draw.AddFieldToCellObj(typeof(Add210), "normalize");
					}*/
				}
			}
		}


		[Draw.Editor(typeof(Pick210))]
		public static void DrawPickGen (Pick210 gen)
		{
			using (Cell.Padded(1,1,0,0))
			{
				Cell.current.fieldWidth = 0.57f;

				using (Cell.RowPx(35))
					using (Cell.LinePx(35))
						using (Cell.Padded(4,2,4,2))
					{
						Texture2D tex;
						if (gen.prototypeType == PrototypeType.TerrainLayer  &&  gen.terrainLayer != null)
							tex = gen.terrainLayer.diffuseTexture;
						if (gen.prototypeType == PrototypeType.GrassTexture  &&  gen.texture != null)
							tex = gen.texture;
						else
							tex = UI.current.textures.GetTexture("DPUI/Backgrounds/Empty");

						Draw.TextureIcon(tex);
					}

				using (Cell.Row)
				{
					using (Cell.LineStd) Draw.Field(ref gen.prototypeType, "Type");

					using (Cell.LineStd) 
					{
						if (gen.prototypeType == PrototypeType.TerrainLayer)
						{
							Draw.ObjectField(ref gen.terrainLayer, "Layer");
							Cell.current.Expose(gen.id, "terrainLayer", typeof(TerrainLayer));
						}

						else if (gen.prototypeType == PrototypeType.GrassTexture)
						{
							Draw.ObjectField(ref gen.texture, "Texture");
							Cell.current.Expose(gen.id, "texture", typeof(Texture2D));
						}

						else
						{
							Draw.ObjectField(ref gen.prefab, "Prefab");
							Cell.current.Expose(gen.id, "prefab", typeof(GameObject));
						}
					}

					using (Cell.LineStd) 
					{ 
						Draw.Field(ref gen.instanceNum, "Num"); 
						Cell.current.Expose(gen.id, "instanceNum", typeof(int));
					}

					using (Cell.LineStd) 
					{ 
						Draw.ToggleLeft(ref gen.createIfNotExists, "Create if Empty"); 
						Cell.current.Expose(gen.id, "createIfNotExists", typeof(bool));
						Draw.AddFieldToCellObj(typeof(Pick210), "createIfNotExists");
					}

					using (Cell.LineStd) 
					{ 
						Draw.Field(ref gen.opacity, "Opacity"); 
						Cell.current.Expose(gen.id, "opacity", typeof(float));
					}
				}
			}
		}


		[Draw.Editor(typeof(Create210))]
		public static void DrawAssembleGen (Create210 gen)
		{
			using (Cell.LinePx(20)) GeneratorDraw.DrawLayersAddRemove(gen, ref gen.layers, inversed:true, unlinkBackground:false);
			using (Cell.LinePx(0)) GeneratorDraw.DrawLayersThemselves(gen, gen.layers, inversed:true, layerEditor:DrawAssembleLayer);

			Cell.EmptyLinePx(2);

			using (Cell.LineStd)
				using (Cell.Padded(1,1,0,0))
					Draw.Field(ref gen.normalize, "Normalize");

			Cell.EmptyLinePx(2);
		}
		
		private static void DrawAssembleLayer (Generator tgen, int num)
		{
			Create210 gen = (Create210)tgen;
			Create210.Layer layer = gen.layers[num];

			if (layer == null) return;

			using (Cell.LinePx(20))
			{
				Cell.current.fieldWidth = 0.52f;

				using (Cell.RowPx(0)) GeneratorDraw.DrawInlet(layer, gen);

				Cell.EmptyRowPx(7);

				using (Cell.RowPx(25))
					using (Cell.LinePx(25+2))
						using (Cell.Padded(2,2,4,2))
						{
							Texture2D tex;
							if (layer.prototypeType == PrototypeType.TerrainLayer  &&  layer.terrainLayer != null)
								tex = layer.terrainLayer.diffuseTexture;
							if (layer.prototypeType == PrototypeType.GrassTexture  &&  layer.texture != null)
								tex = layer.texture;
							else
								tex = UI.current.textures.GetTexture("DPUI/Backgrounds/Empty");

							Draw.TextureIcon(tex);
						}

				using (Cell.Row)
				{
					Cell.EmptyLinePx(2);

					if (gen.guiExpanded == num)
					{
						using (Cell.LineStd) Draw.Field(ref layer.prototypeType, "Type");

						using (Cell.LineStd) 
						{
							if (layer.prototypeType == PrototypeType.TerrainLayer)
							{
								Draw.ObjectField(ref layer.terrainLayer, "Layer");
								Cell.current.Expose(layer.id, "terrainLayer", typeof(TerrainLayer));
							}

							else if (layer.prototypeType == PrototypeType.GrassTexture)
							{
								Draw.ObjectField(ref layer.texture, "Texture");
								Cell.current.Expose(layer.id, "texture", typeof(Texture2D));
								Draw.AddFieldToCellObj(typeof(Add210), "texture");
							}

							else
							{
								Draw.ObjectField(ref layer.prefab, "Prefab");
								Cell.current.Expose(layer.id, "prefab", typeof(GameObject));
							}
						}

						using (Cell.LineStd) 
						{ 
							Draw.Field(ref layer.instanceNum, "Num"); 
							Cell.current.Expose(layer.id, "instanceNum", typeof(int));
						}

						using (Cell.LineStd) layer.Opacity = Draw.Field(layer.Opacity, "Opacity");
					}

					else
						Draw.Label(layer.Object!=null ? layer.Object.name : "Empty");
				}

				using (Cell.RowPx(20)) 
					using (Cell.LinePx(25))
					{
						Cell.current.trackChange = false;
						Draw.LayerChevron(num, ref gen.guiExpanded);
					}
			}
		}


		[Draw.Editor(typeof(Combine210))]
		public static void DrawCombineGen (Combine210 gen)
		{
			using (Cell.LinePx(0))
			{
				using (Cell.LinePx(20)) GeneratorDraw.DrawLayersAddRemove(gen, ref gen.layers, inversed:true, unlinkBackground:false);
				using (Cell.LinePx(0)) GeneratorDraw.DrawLayersThemselves(gen, gen.layers, inversed:true, layerEditor:DrawCombineLayer);

				//using (Cell.Padded(1,1,0,0))
				{
					using (Cell.LineStd) 
					{
						Draw.Toggle(ref gen.normalize, "Normalize");
						Cell.current.Expose(gen.id, "normalize", typeof(bool));
					}
				}
			}
		}
		
		private static void DrawCombineLayer (Generator tgen, int num)
		{
			Combine210 gen = (Combine210)tgen;
			Combine210.Layer layer = gen.layers[num];

			if (layer == null) return;

			using (Cell.LinePx(20))
			{
				using (Cell.RowPx(0)) GeneratorDraw.DrawInlet(layer, gen);

				Cell.EmptyRowPx(10);

				using (Cell.RowPx(73)) Draw.Label("Layer " + num);
			}
		}

	}
}