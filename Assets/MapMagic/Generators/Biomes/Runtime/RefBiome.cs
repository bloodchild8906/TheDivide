using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Products;
using Den.Tools.GUI;

namespace MapMagic.Nodes
{
	[Serializable]
	//[GeneratorMenu (menu="Biomes", name ="Ref Biome", priority = 1)]
	public class RefBiome : Generator, IMultiInlet, IBiome, ICustomComplexity
	{
		//could be Inlet<mask> but do so since mask is not mandatory
		[Val("Mask", "Inlet")] public readonly Inlet<MatrixWorld> maskIn = new Inlet<MatrixWorld>();
		public IEnumerable<IInlet<object>> Inlets() { yield return maskIn; }

		public Graph subGraph;
		public Graph SubGraph => subGraph;
		public Graph AssignedGraph => subGraph;

		public Expose.Override Override { get{return null;} set{} }

		public float Complexity => subGraph!=null ? subGraph.GetGenerateComplexity() : 0;
		public float Progress (TileData data)
		{
/*			TileData subData = data.subDatas[this];
			if (subGraph == null  ||  subData == null) return 0;
			return subGraph.GetGenerateProgress(subData);*/
return 0;
		}

		private TileData GetSubData (TileData parentData)
		{
/*			TileData usedData = parentData.subDatas[this];
			if (usedData == null)
			{
				usedData = new TileData(parentData);
				parentData.subDatas[this] = usedData;
			}
			return usedData;*/
return null;
		}

		public override void Generate (TileData data, StopToken stop) 
		{
/*			if (stop!=null && stop.stop) return;
			if (subGraph == null) return;

			MatrixWorld mask = data.ReadInletProduct(maskIn);
			GetSubData(data).SetBiomeMask(mask, data.currentBiomeMask);  */
		}

	}
}
