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

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MapMagic.Tests.Editor")]

namespace MapMagic.Brush
{
	public class TerrainApplyData
	{
		private RenderTexture heightTex;
		private Texture2D[] splatsTexs;
		private TerrainLayer[] splatsPrototypes;
		
		public void ReadHeight (TerrainData terrainData) => heightTex = terrainData.heightmapTexture;

		public void ReadSplats (TerrainData terrainData)
		{
			splatsTexs = terrainData.alphamapTextures;
			splatsPrototypes = terrainData.terrainLayers;
		}

		public void Apply (TerrainData terrainData)
		{
			//if (heightTex != null) terrainData.SetHeightmapTexture(heightTex);
		}
	}

	public class UndoFull
	{
		const string undoName = "Brush Stroke";
		public string lastUndoName;  //the last undo group name (kept to know it on UndoRedoPerformed)

		//private Stack<Set> sets = new Stack<Set>();  //we've got to remove first items from it, so using list instead
		[SerializeField] private List< Dictionary<Terrain,TerrainApplyData> > sets = new List< Dictionary<Terrain,TerrainApplyData> >();

		#if UNITY_EDITOR
		///Calling Undo
		///Clone of Tools.GUI.Undo, except it's working with brush rather than gui
			public UndoFull ()
			{
				UnityEditor.Undo.undoRedoPerformed -= OnUndoRedoPerformed;
				UnityEditor.Undo.undoRedoPerformed += OnUndoRedoPerformed;
			}

			public void OnUndoRedoPerformed ()
			{
				string currGroupName = UnityEditor.Undo.GetCurrentGroupName();

				if (currGroupName == undoName || currGroupName == lastUndoName)
				// a bit hacky here. On undoRedoPerformed there is already no current group in stack, and no way to get current group name
				// so we store previous (before mm change) name and performing undo if this name is first in stack
				// TODO: use undo from MapMagicBrush with ids, it's more stable
				{
					Perform();

					if (currGroupName == lastUndoName)
						lastUndoName = null;
				}
			}
		#endif

		public void NewGroup (MapMagicBrush brush)
		///Registering new undo (at the start of each stroke)
		{
			/*if (sets.Count > 10)
			{
				List<Set> last10 = new List<Set>(sets);
			}*/

			sets.Add( new Dictionary<Terrain,TerrainApplyData>() );

			#if UNITY_EDITOR
				string currGroupName = UnityEditor.Undo.GetCurrentGroupName();
				if (currGroupName != undoName)
					lastUndoName = currGroupName;

				UnityEditor.Undo.RecordObject(brush, undoName);
				brush.temp = !brush.temp;
			#endif
		}


		public void Append (Terrain[] terrains, Vector2D worldPos, Vector2D worldSize,
			bool readHeight=false, bool readSplats=false)
		{
			foreach (Terrain terrain in terrains)
			{
				Dictionary<Terrain,TerrainApplyData> topSet = sets[sets.Count-1];
				if (topSet.ContainsKey(terrain))
					continue;

				TerrainData terrainData = terrain.terrainData;

				Vector2D terrainPos = (Vector2D)terrain.transform.position;
				Vector2D terrainSize = (Vector2D)terrain.terrainData.size;
				if (!Vector2D.Intersects(worldPos, worldSize, terrainPos, terrainSize))
					continue;

				TerrainApplyData applyData = new TerrainApplyData();
				if (readHeight) applyData.ReadHeight(terrainData);
				if (readSplats) applyData.ReadSplats(terrainData);
				topSet.Add(terrain, applyData);
			}
		}

		
		public void Perform ()
		{
			Dictionary<Terrain,TerrainApplyData> topSet = sets[sets.Count-1];
			sets.RemoveAfter(sets.Count-2);

			foreach (var kvp in topSet)
			{
				TerrainData terrainData = kvp.Key.terrainData;
				TerrainApplyData applyData = kvp.Value;

				applyData.Apply(terrainData);
			}

			foreach (MapMagicBrush brush in GameObject.FindObjectsOfType<MapMagicBrush>())
				brush.UpdateCaches();
		}
	}
}