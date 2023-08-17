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
using MapMagic.Nodes.Biomes;

namespace MapMagic.Nodes.GUI
{
	public static class BiomesEditors
	{
		[Draw.Editor(typeof(RefBiome))]
		public static void DrawRefBiome (RefBiome gen)
		{
			using (Cell.Padded(1,1,0,0)) 
			{
				using (Cell.LineStd) 
				{
					Draw.ObjectField(ref gen.subGraph, "Graph");

					if (Cell.current.valChanged)
						GraphWindow.RefreshMapMagic();
				}

				using (Cell.LineStd)
					if (Draw.Button("Open") && gen.subGraph!=null)
						UI.current.DrawAfter( ()=> GraphWindow.current.OpenBiome(gen.subGraph) );
			}
		}


		[Draw.Editor(typeof(BiomesSet200))]
		public static void BiomesSetEditor (BiomesSet200 gen)
		{
			using (Cell.LinePx(20)) GeneratorDraw.DrawLayersAddRemove(gen, ref gen.layers, inversed:true, unlinkBackground:true);
			using (Cell.LinePx(0)) GeneratorDraw.DrawLayersThemselves(gen, gen.layers, inversed:true, layerEditor:DrawBiomeLayer);
		}
		

		private static void DrawBiomeLayer (Generator tgen, int num)
		{
			BiomesSet200 gen = (BiomesSet200)tgen;
			BiomeLayer layer = gen.layers[num];
			if (layer == null) return;

			Cell.EmptyLinePx(3);
			using (Cell.LinePx(18))
			{
				if (num!=0) 
					using (Cell.RowPx(0)) GeneratorDraw.DrawInlet(layer, gen);
				else 
					//disconnecting last layer inlet
					if (GraphWindow.current.graph.IsLinked(layer))
						GraphWindow.current.graph.UnlinkInlet(layer);
				
				Cell.EmptyRowPx(12);

				Texture2D biomeIcon = UI.current.textures.GetTexture("MapMagic/Icons/Biomes");
				using (Cell.RowPx(14))	Draw.Icon(biomeIcon);

				using (Cell.Row) GeneratorDraw.SubGraph(layer, ref layer.graph);

				Cell.EmptyRowPx(10);

				using (Cell.RowPx(0)) GeneratorDraw.DrawOutlet(layer);
			}
			Cell.EmptyLinePx(3);
		}


		[Draw.Editor(typeof(Whittaker200))]
		public static void DrawWhittaker (Whittaker200 gen)
		{
			//using (Cell.LineStd) 
			//	using (Cell.Padded(1,1,0,0)) 
			//		Draw.Field(ref gen.sharpness, "Sharpness");

			foreach (WhittakerLayer layer in gen.layers)
			{
				Cell.EmptyLinePx(2);
				using (Cell.LinePx(0)) 
				{
					//outlet
					using (Cell.Full)
					{
						using (Cell.LineStd)
						{
							Cell.EmptyRow();
							using (Cell.RowPx(0)) GeneratorDraw.DrawOutlet(layer);
						}
						Cell.EmptyLine();
					}

					//layer itself
					using (Cell.Full)
						using (Cell.Padded(3,3,0,0)) 
						{
							if (layer.guiExpanded) Draw.Element(UI.current.styles.foldoutBackground);

							using (Cell.LineStd) 
							{
								Cell.EmptyRowPx(2);
								using (Cell.Row) Draw.FoldoutLeft(ref layer.guiExpanded, layer.name);
							}

							if (layer.guiExpanded)
							{
								using (Cell.LinePx(0))
								using (Cell.Padded(2,2,0,0)) 
								{
									using (Cell.LineStd)
										GeneratorDraw.SubGraph(layer, ref layer.graph, refreshOnGraphChange:false); //this has loop protection

									using (Cell.LineStd) Draw.Field(ref layer.opacity, "Influence");
									//using (Cell.LineStd) Draw.Field(ref layer.opacity, "Opacity");
								}
								Cell.EmptyLinePx(3);
							}
						}
				}
				Cell.EmptyLinePx(2);
			}
		}

	}
}