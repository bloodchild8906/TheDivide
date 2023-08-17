using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using Den.Tools.Splines;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Terrains;


namespace MapMagic.Nodes.SplinesGenerators
{
	[System.Serializable]
	[GeneratorMenu(
		menu = "Spline", 
		name = "Output", 
		section=2, 
		colorType = typeof(SplineSys), 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Height")]
	public sealed class  SplineOutput200 : OutputGenerator, IInlet<SplineSys>  //virtually standard generator (never writes to products)
	{
		public OutputLevel outputLevel = OutputLevel.Main;
		public override OutputLevel OutputLevel { get{ return outputLevel; } }

		public override void Generate (TileData data, StopToken stop)
		{
			//loading source
			if (stop!=null && stop.stop) return;
			SplineSys src = data.ReadInletProduct(this);
			if (src == null) return; 

			//adding to finalize
			if (enabled)
				data.StoreOutput(this, typeof(SplineOutput200), this, src); 
			//else 
			//	data.RemoveFinalize(finalizeAction);
		}


		public void Finalize (TileData data, StopToken stop)
		{
			//purging if no outputs
			int splinesCount = data.OutputsCount(typeof(SplineOutput200), inSubs:true);
			if (splinesCount == 0)
			{
				if (stop!=null && stop.stop) return;
				data.MarkApply(ApplyData.Empty);
				return;
			}

			//merging splines
			SplineSys mergedSpline = null;
			if (splinesCount > 1)
			{
				mergedSpline = new SplineSys();
				
				//foreach (SplineSys spline in outputs)
				//	mergedSpline.Add...
			}
			else 
			{
				foreach ((SplineOutput200 output, SplineSys product, MatrixWorld biomeMask) 
					in data.Outputs<SplineOutput200,SplineSys,MatrixWorld>(typeof(SplineOutput200), inSubs:true))
						{ mergedSpline = product; break; }
			}

			//pushing to apply
			if (stop!=null && stop.stop) return;
			ApplyData applyData = new ApplyData() {spline=mergedSpline};
			Graph.OnOutputFinalized?.Invoke(typeof(SplineOutput200), data, applyData, stop);
			data.MarkApply(applyData);
		}
		

		public class ApplyData : IApplyData
		{
			public SplineSys spline;

			public void Apply(Terrain terrain)
			{
				//finding holder
				SplineObject splineObj = terrain.GetComponent<SplineObject>(); 
				if (splineObj == null) splineObj = terrain.transform.parent.GetComponentInChildren<SplineObject>();

				//or creating it
				if (splineObj == null)
				{
					GameObject go = new GameObject();
					go.transform.parent = terrain.transform.parent;
					go.transform.localPosition = new Vector3();
					go.name = "Spline";
					splineObj = go.AddComponent<SplineObject>();
				}

				splineObj.splineSys = spline;
			}

			public static ApplyData Empty 
				{get{ return new ApplyData() { spline = null }; }}

			public int Resolution {get{ return 0; }}
		}


		public static void Purge(CoordRect rect, Terrain terrain)
		{

		}

		public override void ClearApplied (TileData data, Terrain terrain)
		{
			/*TerrainData terrainData = terrain.terrainData;
			Vector3 terrainSize = terrainData.size;

			terrainData.detailPrototypes = new DetailPrototype[0];
			terrainData.SetDetailResolution(32, 32);*/

			throw new System.NotImplementedException();
		}
	}


}