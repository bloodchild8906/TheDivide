using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using MapMagic.Products;
using MapMagic.Expose;


namespace MapMagic.Nodes.Biomes
{
	[Serializable]
	[GeneratorMenu (menu="Functions", name ="Loop", iconName="GeneratorIcons/Function", priority = 1, colorType = typeof(IBiome))]
	public class Loop210 : BaseFunctionGenerator, IMultiInlet, IMultiOutlet, IPrepare, IBiome, ICustomComplexity//, ISerializationCallbackReceiver
	/// Function which executed several times
	/// Inlets and Outlets should have same name to transfer results for next iteration
	{
		public int iterations = 1;


		private TileData GetSubData (int i, TileData parentData)
		{
			if (parentData == null) 
				return null;

			ulong subId = (ulong)(id*10000 + (ulong)i); //id of not existing node
			return parentData.GetSubData(id);
		}


		private IEnumerable<TileData> SubDatas (TileData parentData)
		{
			if (parentData == null) 
				yield break;

			for (int i=0; i<iterations; i++)
				yield return GetSubData(i, parentData);
		}


		public float Complexity => subGraph!=null ? subGraph.GetGenerateComplexity()*iterations : 0;
		public float Progress (TileData data)
		{
			if (subGraph == null) 
				return 0;	

			float totalProgress = 0;
			foreach (TileData subData in SubDatas(data))
				totalProgress += subData!=null ? subGraph.GetGenerateProgress(subData) : 0;
			
			return totalProgress;
		}


		public void Prepare (TileData parentData, Terrain terrain)
		{
			if (subGraph == null) 
				return;

			foreach (TileData subData in SubDatas(parentData))
				subGraph.Prepare(subData, terrain);
		}


		public override void Generate (TileData parentData, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			if (subGraph == null) return;

			for (int it=0; it<iterations; it++)
			{
				TileData subData = GetSubData(it,parentData);

				//sending inlet products to sub-graph enters
				if (stop!=null && stop.stop) return;
				for (int i=0; i<inlets.Length; i++)
				{
					IFnInlet<object> inlet = inlets[i];
					object product = parentData.ReadInletProduct(inlet);

					IFnEnter<object> fnEnter = (IFnEnter<object>)inlet.GetInternalPortal(subGraph); 
					subData.StoreProduct(fnEnter, product);
					subData.MarkReady(fnEnter.Id);
				}

				//overriding loop iteration
				if (stop!=null && stop.stop) return;
				ovd.SetOrAdd("LoopIteration", typeof(int), it);

				//generating
				if (stop!=null && stop.stop) return;
				subGraph.Generate(subData, stop:stop, ovd:ovd); //with fn override, not graph defaults

				//returning products back from sub-graph exists
				if (stop!=null && stop.stop) return;
				for (int o=0; o<outlets.Length; o++)
				{
					IFnOutlet<object> outlet = outlets[o];
					IFnExit<object> fnExit = (IFnExit<object>)outlet.GetInternalPortal(subGraph);
					object product = subData.ReadInletProduct(fnExit);

					parentData.StoreProduct(outlet, product);
				}
			}
		}

	}
}
