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

namespace MapMagic.Nodes.GUI
{
	public class SplineEditors
	{
		[Draw.Editor(typeof(SplinesGenerators.Stamp200))]
		public static void DrawStamp (SplinesGenerators.Stamp200 gen)
		{
			using (Cell.Padded(1,1,0,0)) 
			{
				using (Cell.LineStd) Draw.Field(ref gen.algorithm, "Algorithm");

				if (gen.algorithm == SplinesGenerators.Stamp200.Algorithm.Flatten || gen.algorithm == SplinesGenerators.Stamp200.Algorithm.Both)
				{
					using (Cell.LineStd) 
					{
						Draw.Field(ref gen.flatRange, "Flat Range");
						Cell.current.Expose(gen.id, "flatRange", typeof(float));
					}
					using (Cell.LineStd) 
					{
						Draw.Field(ref gen.blendRange, "Blend Range");
						Cell.current.Expose(gen.id, "blendRange", typeof(float));
					}
				}

				if (gen.algorithm == SplinesGenerators.Stamp200.Algorithm.Detail || gen.algorithm == SplinesGenerators.Stamp200.Algorithm.Both)
				{
					using (Cell.LineStd) 
					{
						Draw.Field(ref gen.detailRange, "Detail Range");
						Cell.current.Expose(gen.id, "detailRange", typeof(float));
					}
					using (Cell.LineStd) 
					{
						Draw.Field(ref gen.detail, "Detail Size");
						Cell.current.Expose(gen.id, "detail", typeof(float));
					}
				}

				using (Cell.LineStd) 
				{
					Draw.Field(ref gen.fallof, "Fallof");
					Cell.current.Expose(gen.id, "fallof", typeof(float));
				}
			}
		}


		[Draw.Editor(typeof(SplinesGenerators.Manual210))]
		public static void DrawManualGenerator (SplinesGenerators.Manual210 gen)
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
						}

						Cell.current.Expose(gen.id, "positions", typeof(Vector3), arrIndex:num);

						Cell.EmptyRowPx(2);
					}
					Cell.EmptyLinePx(2);
				} );

			if (Cell.current.valChanged)
				GraphWindow.RefreshMapMagic(gen);
		}


		[Draw.Editor(typeof(SplinesGenerators.Combine217))]
		public static void BlendGeneratorEditor (SplinesGenerators.Combine217 gen)
		{
			using (Cell.LinePx(20)) GeneratorDraw.DrawLayersAddRemove(gen, ref gen.layers, inversed:false);
			using (Cell.LinePx(0)) GeneratorDraw.DrawLayersThemselves(gen, gen.layers, inversed:false, layerEditor:DrawCombineLayer);
		}

		private static void DrawCombineLayer (Generator tgen, int num)
		{
			SplinesGenerators.Combine217 gen = (SplinesGenerators.Combine217)tgen;
			SplinesGenerators.Combine217.Layer layer = gen.layers[num];

			Cell.EmptyLinePx(2);

			using (Cell.LineStd)
			{
				using (Cell.RowPx(0)) 
					GeneratorDraw.DrawInlet(layer.inlet, gen);
				Cell.EmptyRowPx(10);

				using (Cell.RowPx(20)) Draw.Icon(UI.current.textures.GetTexture("DPUI/Icons/Layer"));

				using (Cell.Row) Draw.Label("Spline " + num);

				Cell.EmptyRowPx(3);
			}


			Cell.EmptyLinePx(2);
		}
	}
}