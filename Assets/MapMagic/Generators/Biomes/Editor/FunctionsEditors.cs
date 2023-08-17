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
	public static class FunctionsEditors
	{
		[Draw.Editor(typeof(Function210), cat="Header")]
		public static void DrawFunctionHeader (Function210 fn)
		{
			RefreshOnSubgraphChange(fn); //since it's first encounter fn gui appears checking for internal graph change and refreshing if needed
			DrawInletsOutlets(fn);
		}

		[Draw.Editor(typeof(Loop210), cat="Header")]
		public static void DrawLoopHeader (Loop210 fn) 
		{
			RefreshOnSubgraphChange(fn);
			DrawInletsOutlets(fn);
		}


		private static void DrawInletsOutlets (BaseFunctionGenerator fn)
		{
			using (Cell.LinePx(0))
			{
				using (Cell.Row)
				{
					for (int i=0; i<fn.inlets.Length; i++)
					{
						using (Cell.LineStd)
						{
							using (Cell.RowPx(0)) GeneratorDraw.DrawInlet(fn.inlets[i], fn);
							Cell.EmptyRowPx(8);
							using (Cell.Row) Draw.Label(fn.inlets[i].Name);
						}
					}
				}

				Cell.EmptyRowPx(10);

				using (Cell.Row)
				{
					for (int i=0; i<fn.outlets.Length; i++)
					{
						using (Cell.LineStd)
						{
							using (Cell.Row) Draw.Label(fn.outlets[i].Name);
							Cell.EmptyRowPx(8);
							using (Cell.RowPx(0)) GeneratorDraw.DrawOutlet(fn.outlets[i]);
						}
					}
				}
			}

			if (fn.guiSameInletsName != null)
				using (Cell.LinePx(54))
					Draw.Label($"Two or more inlets \nhave the same name \n{fn.guiSameInletsName}");

			if (fn.guiSameOutletsName != null)
				using (Cell.LinePx(54))
					Draw.Label($"Two or more outlets \nhave the same name \n{fn.guiSameOutletsName}");
		}


		[Draw.Editor(typeof(Function210))]
		public static void DrawFunction (Function210 fn)
		{
			using (Cell.Padded(1,1,0,0)) 
			{
				Cell.EmptyLinePx(2);
				using (Cell.LineStd) 
				{
					Graph prevGraph = fn.subGraph;
					GeneratorDraw.SubGraph(fn, ref fn.subGraph, refreshOnGraphChange:false);
					
					if (prevGraph != fn.subGraph  &&  fn.subGraph != null)
					{
						fn.ovd = new Expose.Override(fn.subGraph.defaults);
						GraphWindow.RefreshMapMagic();	
					}
				}

				bool genExposed = GraphWindow.current.graph.exposed.Contains(fn.id);

				Cell.EmptyLinePx(5);
				if (fn.subGraph != null)
				{
					for (int i=0; i<fn.ovd.Count; i++)
					{
						(string name, Type type, object obj) = fn.ovd.GetOverrideAt(i);

						using (Cell.LineStd)
						{
							obj = Draw.UniversalField(obj, type, name);
							Cell.current.Expose(fn.id, name, type, genExposed:genExposed);

							if (Cell.current.valChanged)
							{
								fn.ovd.SetOverrideAt(i, name, type, obj);

//								foreach (IUnit unit in fn.subGraph.exposed.UnitsByReference(fn.subGraph, name))
//									if (unit is Generator gen)
//										mm.Clear(gen);
							}
						}
					}
				}

				Cell.EmptyLinePx(2);
			}
		}


		[Draw.Editor(typeof(Loop210))]
		public static void DrawLoop (Loop210 loop)
		{
			using (Cell.Padded(1,1,0,0)) 
			{
				Cell.EmptyLinePx(2);
				using (Cell.LineStd) 
				{
					Graph prevGraph = loop.subGraph;
					GeneratorDraw.SubGraph(loop, ref loop.subGraph, refreshOnGraphChange:false);
					
					if (prevGraph != loop.subGraph  &&  loop.subGraph != null)
					{
						loop.ovd = new Expose.Override(loop.subGraph.defaults);
						GraphWindow.RefreshMapMagic();	
					}
				}

				bool genExposed = GraphWindow.current.graph.exposed.Contains(loop.id);

				Cell.EmptyLinePx(5);
				using (Cell.LineStd) 
				{
					Draw.Field(ref loop.iterations, "Iterations");
					Cell.current.Expose(loop.id, "iterations", typeof(int), genExposed:genExposed);
				}

				Cell.EmptyLinePx(4);
				if (loop.subGraph != null)
					for (int i=0; i<loop.ovd.Count; i++)
					{
						(string name, Type type, object obj) = loop.ovd.GetOverrideAt(i);

						using (Cell.LineStd)
						{
							obj = Draw.UniversalField(obj, type, name);
							Cell.current.Expose(loop.id, name, type, genExposed:genExposed);

							if (Cell.current.valChanged)
								loop.ovd.SetOverrideAt(i, name, type, obj);
						}
					}

				Cell.EmptyLinePx(2);
			}
		}


		private static void RefreshOnSubgraphChange (BaseFunctionGenerator fn)
		/// Checks if fn subgraph changed and performs complex operations if it's so
		{
			if (fn.guiPrevGraph == fn.subGraph  &&  fn.guiPrevGraphVersion == fn.subGraph?.changeVersion)
				return;

			//removing all if there is no graph
			if (fn.subGraph == null) 
			{
				if (fn.inlets.Length != 0  ||  fn.outlets.Length != 0)
				{
					fn.inlets = new FnInlet<object>[0];
					fn.outlets = new FnOutlet<object>[0];
					GraphWindow.current.graph.UnlinkGenerator(fn);
				}

				if (fn.ovd.Count != 0)
					fn.ovd.Clear();
			}

			else
			{
				//syncing inlets/outlets
				SyncLayersPortals<IFnInlet<object>, IFnEnter<object>>(ref fn.inlets, fn, fn.subGraph, GraphWindow.current.graph);
				SyncLayersPortals<IFnOutlet<object>, IFnExit<object>>(ref fn.outlets, fn, fn.subGraph, GraphWindow.current.graph);

				//refreshing override
				fn.ovd.Sync(fn.subGraph.defaults);

				//checking same inlet/outlet names
				fn.guiSameInletsName = CheckSameLayersNames(fn.inlets);
				fn.guiSameOutletsName = CheckSameLayersNames(fn.outlets);
			}

			fn.guiPrevGraph = fn.subGraph;
			fn.guiPrevGraphVersion = fn.subGraph ? fn.subGraph.changeVersion : 0;
		}


		private static bool SyncLayersPortals<TL,TP> (ref TL[] layers, Generator gen, Graph subGraph, Graph parentGraph) 
			where TL:IFnLayer<object>
			where TP:IFnPortal<object>
			/// TL is layer type, TP is portal type
		/// Synchronizes function inlets/outlets with function portals used in internal graph
		/// ParentGraph to unlink link on remove, gen - to assign layer Gen and Id
		{
			List<TL> newLayers = null;
			lock (layers) //SyncInlets may change inputs
			{
				//gathering all internal portals by name
				Dictionary<string,TP> namesPortals = new Dictionary<string,TP>();
				foreach (TP portal in subGraph.GeneratorsOfType<TP>())
				{
					HashSet<TP> portalSet = new HashSet<TP>();
					namesPortals.Add(portal.Name, portal);
				}
			

				//skipping if there is no change
				bool noChange = true;

				if (namesPortals.Count != layers.Length)
					noChange = false;

				foreach (IFnLayer<object> layer in layers)
				{
					if (!namesPortals.ContainsKey(layer.Name))
						{ noChange = false; break; }
				}

				if (noChange)
					return false;

			
				//copying only layers with portals to newarray
				newLayers = new List<TL>();

				foreach (TL layer in layers)
				{
					string name = layer.Name;
					if (!namesPortals.TryGetValue(name, out TP portal))
					{
						//unlinking removed inlet from graph
						if (layer is IInlet<object> inlet) parentGraph.UnlinkInlet(inlet);
						if (layer is IOutlet<object> outlet) parentGraph.UnlinkOutlet(outlet);
					
						continue;
					}

					newLayers.Add(layer);
					namesPortals.Remove(name);
				}


				//creating layers for portals left
				foreach (TP portal in namesPortals.Values)
				{
					//Type genericType = portal.GetType().BaseType.GetGenericArguments()[0]; //might have IFNPortal not the only interface
					Type genericType = null;
					foreach(Type iType in portal.GetType().GetInterfaces()) 
					{
						if (typeof(IFnPortal<object>).IsAssignableFrom(iType))
							genericType = iType.GetGenericArguments()[0];
					}

					TL layer = CreateLayer<TL>(gen, genericType);

					layer.Name = portal.Name;
					newLayers.Add(layer);
				}
			}

			layers = newLayers.ToArray();
			return true;
		}


		private static TL CreateLayer<TL> (Generator gen, Type genericType) where TL:IFnLayer<object>
		{
			Type layerBaseType;
			if (typeof(TL).IsAssignableFrom(typeof(FnInlet<>))) layerBaseType = typeof(FnInlet<>);
			else layerBaseType = typeof(FnOutlet<>);

			Type layerType = layerBaseType.GetGenericTypeDefinition().MakeGenericType(genericType);
			
			object layerObj = Activator.CreateInstance(layerType);
			TL layer = (TL)layerObj;

			layer.Id = Id.Generate();
			layer.SetGen(gen);

			return layer;
		}


		private static string CheckSameLayersNames<TL> (TL[] layers) where TL: class, IFnLayer<object>
		/// It two outlets have the same name returns this name
		/// Returns null if no same name found
		{
			foreach(TL outlet1 in layers)
				foreach(TL outlet2 in layers) //faster than creating hashset and no garbage
					if (outlet1 != outlet2  &&  outlet1.Name == outlet2.Name) return outlet1.Name;
			return null;
		}
	}
}