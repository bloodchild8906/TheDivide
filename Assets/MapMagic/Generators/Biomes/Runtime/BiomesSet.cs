using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Products;
using Den.Tools.GUI;

namespace MapMagic.Nodes.Biomes
{
	public class BiomeLayer : IBiome, IInlet<MatrixWorld>, IOutlet<MatrixWorld>
	{
		public float Opacity { get; set; }

		public Generator Gen { get { return gen; } private set { gen = value;} }
		public Generator gen; //property is not serialized
		public void SetGen (Generator gen) => this.gen=gen;

		public ulong id; //properties not serialized
		public ulong Id { get{return id;} set{id=value;} } 
		public ulong LinkedOutletId { get; set; }  //if it's inlet. Assigned every before each clear or generate
		public ulong LinkedGenId { get; set; } 

		public IUnit ShallowCopy() => (BiomeLayer)this.MemberwiseClone();

		public Expose.Override Override { get{return null;} set{} }

		public Graph graph;
		public Graph SubGraph => graph;
		public Graph AssignedGraph => graph;

		public BiomeLayer () => Opacity=1;
	}


	[Serializable]
	[GeneratorMenu (menu="Biomes", name ="Biomes Set", iconName="GeneratorIcons/Biome", priority = 1, colorType = typeof(IBiome))]
	public class BiomesSet200 : Generator, IMultiInlet, IMultiLayer, ICustomComplexity, ICustomClear, IPrepare
	{
		public BiomeLayer[] layers = new BiomeLayer[0];
		public IList<IUnit> Layers { get => layers; set => layers=ArrayTools.Convert<BiomeLayer,IUnit>(value); }
		public void SetLayers(object[] ls) => layers = Array.ConvertAll(ls, i=>(BiomeLayer)i);
		public bool Inversed => true;
		public bool HideFirst => true;

		public IEnumerable<IInlet<object>> Inlets() 
		{ 
			foreach (BiomeLayer layer in layers)
				yield return layer;
			//TODO: return layers
		}

		public IEnumerable<IBiome> Biomes() 
		{ 
			foreach (BiomeLayer layer in layers)
				yield return layer;
		}

		public float Complexity
		{get{
			float sum = 0;
			foreach (BiomeLayer layer in layers)
				if (layer.graph != null)
					sum += layer.graph.GetGenerateComplexity();
			return sum;
		}}

		public float Progress (TileData data)
		{
			float sum = 0;
			foreach (BiomeLayer layer in layers)
			{
				if (layer.graph == null) continue;

				TileData subData = data.GetSubData(layer.Id);
				if (subData == null) continue;

				sum += layer.graph.GetGenerateProgress(subData);
			}
			return sum;
		}


		public void Prepare (TileData data, Terrain terrain)
		{
			foreach (BiomeLayer layer in layers)
			{
				if (layer.graph == null) continue;

				TileData subData = data.CreateSubData(layer.Id);

				layer.graph.Prepare(subData, terrain);
			}
		}


		public override void Generate (TileData data, StopToken stop) 
		{
			#if MM_DEBUG
			Log.Add("Biome start (draft:" + data.isDraft + " gen:" + id);
			#endif

			if (layers.Length == 0) return;
			BiomeLayer[] layersCopy = layers.Copy(); //layers count can be changed during generate

			//reading/copying products
			MatrixWorld[] dstMatrices = new MatrixWorld[layersCopy.Length];
			float[] opacities = new float[layersCopy.Length];

			if (stop!=null && stop.stop) return;
			for (int i=0; i<layersCopy.Length; i++)
			{
				if (stop!=null && stop.stop) return;

				MatrixWorld srcMatrix = data.ReadInletProduct(layersCopy[i]);
				if (srcMatrix != null) dstMatrices[i] = new MatrixWorld(srcMatrix);
				else dstMatrices[i] = new MatrixWorld(data.area.full.rect, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize);

				opacities[i] = layersCopy[i].Opacity;
			}

			//normalizing
			if (stop!=null && stop.stop) return;
			dstMatrices.FillNulls(() => new MatrixWorld(data.area.full.rect, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize));
			dstMatrices[0].Fill(1);
			Matrix.BlendLayers(dstMatrices, opacities);

			//saving products
			if (stop!=null && stop.stop) return;
			for (int i=0; i<layersCopy.Length; i++)
				data.StoreProduct(layersCopy[i], dstMatrices[i]);

			//generating biomes
			for (int i=0; i<layersCopy.Length; i++)
			{
				if (stop!=null && stop.stop) return;

				BiomeLayer layer = layersCopy[i];

				MatrixWorld mask;
				if (data.biomeMask == null)
					mask = dstMatrices[i]; //no need to copy for first-level biome
				else
				{
					mask = new MatrixWorld(dstMatrices[i]);
					mask.Multiply(data.biomeMask);
				}

				Graph subGraph = layer.SubGraph;
				if (subGraph == null) continue;

				//TileData subData = data.GetSubData(layer.Id);
				//if (subData == null) subData = data.CreateSubData(layer.Id, mask);
				//subData.mask = mask;
				//SubData could be created at prepare stage (Whittaker has prepare), but I have not tested re-assigning existing data yet
				TileData subData = data.CreateSubData(layer.Id, mask);

				layer.graph.Generate(subData, stop:stop, ovd:layer.graph.defaults);
			}

			#if MM_DEBUG
			Log.Add("Biome generated (draft:" + data.isDraft + " gen:" + id);
			#endif
		}


		public bool CheckClear (TileData data)
		/// Called when clearing recursively from graph
		/// Called once, but no matter generator is ready or not
		{
			bool ready = true;

			foreach (BiomeLayer layer in layers)
				ready = CheckClearBiome(layer, data) && ready;

			return ready;
		}


		public void ForceClear (TileData data)
		/// Called when this gen changed directly
		{
			foreach (BiomeLayer layer in layers)
				ForceClearBiome(layer, data);
		}


		public static bool CheckClearBiome (IBiome layer, TileData data)
		/// Static to share between single biome, biome set and whittaker
		{
			TileData subData = data.GetSubData(layer.Id);
			Graph subGraph = layer.SubGraph;
			IInlet<object> inlet = layer as IInlet<object>; //yes, biome should be an inlet as well (not function)
			if (inlet == null) return true; //otherwise won't draw whittaker and textures

			if (subGraph == null  ||  subData == null)
				return true;

			bool ready = true;

			//if inlet changed - resetting relevants for biome (for function reset input portals)
			ulong linkedId = inlet.LinkedGenId;
			if (linkedId == 0)
				return true; //not connected

			if (!data.IsReady(linkedId)) //should have inlet state changed at the moment
			{
				foreach (Generator relGen in subGraph.RelevantGenerators(data.isDraft)) 
				{
					if (relGen is ICustomClear cgen) cgen.ForceClear(subData);
					subData.ClearReady(relGen);
				}

				ready = false;
			}

			//checking if any of the layers has internal graph changed, and proceeding cleared fn enters
			if (!subGraph.ClearChanged(subData))  //false if any internal relevant/dependent gen not ready
				ready = false;
				//don't break, clear changed other layers

			return ready;
		}


		public static void ForceClearBiome (IBiome layer, TileData data)
		//Just clearing relevant outputs
		//should copy expose from function if wand exposable params
		{
			TileData subData = data.GetSubData(layer.Id);
			Graph subGraph = layer.SubGraph;
			IInlet<object> inlet = layer as IInlet<object>; //yes, biome should be an inlet as well (not function)
			if (inlet == null) return;	//except Whittaker

			if (subGraph == null  ||  subData == null)
				return;

			foreach (Generator relGen in subGraph.RelevantGenerators(data.isDraft)) 
			{
				if (relGen is ICustomClear cgen) cgen.ForceClear(subData);
				subData.ClearReady(relGen);
			}
		}
	}
}
