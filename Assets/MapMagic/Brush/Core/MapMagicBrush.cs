using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Nodes;
using MapMagic.Terrains;
using MapMagic.Products;
using MapMagic.Expose;
using System.Threading;
using System;

namespace MapMagic.Brush
{
	public interface IBrushRead
	{
		void ReadTerrains(TerrainCache[] terrainCaches, TileData tileData);
	}

	public interface IBrushWrite
	{
		void WriteTerrains(TerrainCache[] terrainCaches, CacheChange change, TileData tileData);
	}





	[SelectionBase]
	[ExecuteInEditMode] //to call onEnable, then it will subscribe to editor update
	[HelpURL("https://gitlab.com/denispahunov/mapmagic/wikis/home")]
	[DisallowMultipleComponent]
	public class MapMagicBrush : MonoBehaviour
	{
		public static readonly SemVer version = new SemVer(1,0,0); 

		[NonSerialized] public bool draw;

		public Preset preset = null; //note brush is using copy. Can't create new instance (it's unityobj) here, so using OnEnable
		public Preset sourcePreset = null; //to save preset with 'Save' button
		public Preset[] quickPresets = new Preset[0]; //quick selection
		public void AssignPreset (Preset newPreset) 
		{ 
			if (newPreset == null) { Debug.Log("Selected Brush preset is null"); return; }
			sourcePreset = newPreset; 
			preset = Instantiate(newPreset); 
		}
		public void AssignQuickPreset (int num)
		{
			if (quickPresets.Length > num)
				AssignPreset(quickPresets[num]);
		}

		public Trace trace = new Trace(); //spacing tool

		public Terrain[] terrains = new Terrain[0];
		public Dictionary<Terrain,TerrainCache> terrainCaches = new Dictionary<Terrain,TerrainCache>();
		public CacheChange cacheChange = new CacheChange();
		public bool terrainsAdded = false; //called on OnEnable

		public TuneStroke tuneStroke = new TuneStroke();

		public Undo.Undo undo = new Undo.Undo();
		public bool temp = false; //switching on new undo - otherwise it won't be recordered
		
		//IMapMagic
		public Graph Graph => preset?.graph;
		public bool ContainsGraph (Graph graph) => preset?.graph==graph;
		public TileData PreviewData { get; set; }

		public Color mainColor = new Color(0, 0.5f, 1f, 1); //new Color(1,0.4f,0,1); 
		public Color falloffColor = new Color(0, 0.3f, 0.6f, 1); //new Color(0.6f, 0.1f, 0, 1); //alpha is 1
		public float lineThickness = 5;

		public bool guiBrush = true;
		public bool guiPresets = false;
		public bool guiGraph = false;
		public bool guiProperties = false;
		public bool guiTerrains;
		public bool guiTuneStroke;
		public bool guiSettings;
		
		public int curUndoId = 0;  //to change something on undo and find out if this is a proper undo on undo redo performed. See use case
		public int prevUndoId = 0;  //should be in object itself


		public void OnEnable ()
		{

			tuneStroke.brush = this;

			if (preset == null)
				preset = ScriptableObject.CreateInstance<Preset>();

			UpdateCaches();

			//updating cache on MapMagic change
			TerrainTile.OnTileApplied -= UpdateCache;
			TerrainTile.OnTileApplied += UpdateCache;
		}

		private void UpdateCache (TerrainTile tile, TileData data, StopToken stop) { UpdateCache(tile.draft?.terrain); UpdateCache(tile.main?.terrain); }


		public void UpdateCaches ()
		{
			for (int t=0; t<terrains.Length; t++)
			{
				TerrainCache cache;
				if (!terrainCaches.TryGetValue(terrains[t], out cache))
				{
					cache = new TerrainCache(terrains[t]);
					terrainCaches.Add(terrains[t], cache);
				}

				cache.UpdateCache();
			}
		}


		public void UpdateCache (Terrain terrain)
		{
			if (!terrains.Contains(terrain))
				return;

			TerrainCache cache;
			if (!terrainCaches.TryGetValue(terrain, out cache))
			{
				cache = new TerrainCache(terrain);
				terrainCaches.Add(terrain, cache);
			}

			cache.UpdateCache();
		}

		


		public void Apply (Vector3 pos, bool isFirst=false)
		/// Main fn to apply brush to terrain
		/// isFirst: is this first (just clicked) stamp in stroke?
		{
			Stamp stamp = new Stamp() {pos=(Vector2D)pos, radius=preset.radius, hardness=preset.hardness, margins=preset.margins};
			
			Terrain[] stampTerrains = TerrainManager.GetTerrains(terrains, stamp.Min, stamp.Size);
			if (stampTerrains.Length == 0)
				return;

			TerrainCache[] stampTerrainCaches = new TerrainCache[stampTerrains.Length];
			for (int t=0; t<stampTerrains.Length; t++)
			{
				if (!terrainCaches.TryGetValue(stampTerrains[t], out TerrainCache cache))
					throw new Exception("No cache for terrain " + stampTerrains[t]);
				stampTerrainCaches[t] = cache;
			}

			TileData tileData = null;
			//using (new Log.Timer("Area"))
			{
				tileData = new TileData();
				tileData.area = stamp.GetArea(stampTerrains[0]);
				tileData.globals = new Globals(); //TODO: avoid creating it al the time
				tileData.globals.height = stampTerrains[0].terrainData.size.y;
				tileData.random = preset.graph.random;
				tileData.isPreview = true;
				tileData.isDraft = false;
			}

			//using (new Log.Timer("Override"))
			{
				preset.ovd.SetIfContains("Position", typeof(Vector2D), pos);
				preset.ovd.SetIfContains("Position", typeof(Vector3), pos);
				preset.ovd.SetIfContains("Radius", typeof(float), preset.radius);
				preset.ovd.SetIfContains("Hardness", typeof(float), preset.hardness);
				preset.ovd.SetIfContains("PrevPosition", typeof(Vector2D), (Vector2D)trace.PrevStampPos);
				preset.ovd.SetIfContains("PrevPosition", typeof(Vector3), trace.PrevStampPos);
				preset.ovd.SetIfContains("CapturedPosition", typeof(Vector3), trace.capturedPosition);
				preset.ovd.SetIfContains("TerrainHeight", typeof(float), stampTerrains[0].terrainData.size.y);
				preset.ovd.SetIfContains("TerrainHeight", typeof(int), stampTerrains[0].terrainData.size.y);
				#if UNITY_EDITOR
				preset.ovd.SetIfContains("Shift", typeof(bool), Event.current.shift);
				preset.ovd.SetIfContains("Shift", typeof(int), Event.current.shift ? 1 : 0);
				preset.ovd.SetIfContains("Shift", typeof(float), Event.current.shift ? 1 : 0);
				#endif
			}

			//using (new Log.Timer("Read"))
				foreach (IBrushRead brushReadNode in BrushReadsOrdered(preset.graph))
					brushReadNode.ReadTerrains(stampTerrainCaches, tileData);

			using (Log.Group("Generate"))
			{
				StopToken stop = new StopToken();
				preset.graph.Generate(tileData, stop:stop, ovd:preset.ovd);
				preset.graph.Finalize(tileData, stop);
			}

			//using (new Log.Timer("Undo"))
			{
				if (isFirst) undo.NewGroup(this);
				undo.Append(stampTerrains, stamp.Min, stamp.Size,
					readHeight: preset.graph.ContainsGeneratorOfType<BrushWriteHeight206>(),
					readSplats: preset.graph.ContainsGeneratorOfType<BrushWriteTextureSet>(), 
					readGrass: preset.graph.ContainsGeneratorOfType<BrushWriteGrassSet>(),
					readTrees: preset.graph.ContainsGeneratorOfType<BrushWriteTrees>() 
					);
			}

			//using (new Log.Timer("Write"))
				foreach (IBrushWrite brushWriteNode in BrushWritesOrdered(preset.graph))
					brushWriteNode.WriteTerrains(stampTerrainCaches, cacheChange, tileData);
		}


		private IEnumerable<IBrushRead> BrushReadsOrdered (Graph graph)
		/// Equivalent of foreach GeneratorsOfType<IBrushRead> but heigts goes first (and checking enabled)
		{
			foreach (IBrushRead heightNode in preset.graph.GeneratorsOfType<BrushReadHeight206>())
				yield return heightNode;

			foreach (IBrushRead brushNode in preset.graph.GeneratorsOfType<IBrushRead>())
				if (!(brushNode is BrushReadHeight206) && (brushNode as Generator).enabled)
					yield return brushNode;
		}


		private IEnumerable<IBrushWrite> BrushWritesOrdered (Graph graph)
		/// Equivalent of foreach GeneratorsOfType<IBrushRead> but heigts goes first (and checking enabled)
		{
			foreach (BrushWriteHeight206 heightNode in preset.graph.GeneratorsOfType<BrushWriteHeight206>())
				if (heightNode.enabled)
					yield return heightNode;

			foreach (IBrushWrite brushNode in preset.graph.GeneratorsOfType<IBrushWrite>())
				if (!(brushNode is BrushWriteHeight206) && (brushNode as Generator).enabled)
					yield return brushNode;
		}
	}
}
