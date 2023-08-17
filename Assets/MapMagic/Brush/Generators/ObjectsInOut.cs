using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Terrains;
using MapMagic.Nodes;
//using MapMagic.Nodes.ObjectsGenerators; //not in objects module anymore

using UnityEngine.Profiling;

namespace MapMagic.Brush
{
	[Serializable]
	[GeneratorMenu(
		menu = "Brush/Input", 
		name = "Objects", 
		section=2, 
		colorType = typeof(TransitionsList), 
		iconName="GeneratorIcons/BrushObjectsIn",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/ObjectsInOut")]
	public class  BrushReadObjects : Generator, IOutlet<TransitionsList>,  IFnEnter<TransitionsList>, IBrushRead
	{
		public bool specificPrefabs = false;
		public GameObject[] prefabs = new GameObject[1];

		public string Name { get => "Objects"; set {} } //as function portal

		public override void Generate (TileData data, StopToken stop) { }

		public void ReadTerrains (TerrainCache[] terrainCaches, TileData tileData)
		{
			TransitionsList tlist = new TransitionsList();

			HashSet<GameObject> usedPrefabs = null;
			if (specificPrefabs) 
			{
				usedPrefabs = new HashSet<GameObject>();
				usedPrefabs.AddRange(prefabs);
			}

			foreach (TerrainCache terrainCache in terrainCaches)
			{
				Terrain terrain = terrainCache.terrain;
				foreach (Transform tfm in ObjectsOps.RelatedTransforms(terrain, tileData.area.full.worldPos, tileData.area.full.worldSize))
				{
					if (specificPrefabs)
					{
						GameObject prefab = ObjectsOps.GetPrefab(tfm);
						if (prefab == null  ||  !usedPrefabs.Contains(prefab))
							continue;
					}

					Transition trs = new Transition(tfm);
					tlist.Add(trs);
				}
			}

			tileData.StoreProduct(this, tlist);
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Brush/Output", 
		name = "Transfer Objects", 
		section=2, 
		colorType = typeof(TransitionsList), 
		iconName="GeneratorIcons/BrushObjectsOut",
		disengageable = true,
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Textures")]
	public class  BrushTransferObjects : Generator, IInlet<TransitionsList>,  IFnExit<TransitionsList>, IBrushWrite, IRelevant
	{
		public bool specificPrefabs = false;
		public GameObject[] prefabs = new GameObject[1];
		public PositioningSettings posSettings = new PositioningSettings() { relativeHeight = false };
		
		public string Name { get => "Objects"; set {} } //as function portal


		public override void Generate (TileData data, StopToken stop) { }


		public void WriteTerrains (TerrainCache[] terrainCaches, CacheChange change, TileData tileData)
		{
			TransitionsList trnList = tileData.ReadInletProduct(this);
			if (trnList == null) return;

			HashSet<Transform> trnInstances = new HashSet<Transform>(); //transforms mentioned in trn instance
			for (int i=0; i<trnList.count; i++)
			{
				Transform instance = trnList.arr[i].instance;
				if (instance != null  &&  !trnInstances.Contains(instance))
					trnInstances.Add(instance);
			}

			HashSet<GameObject> usedPrefabs = null;
			if (specificPrefabs) 
			{
				usedPrefabs = new HashSet<GameObject>();
				usedPrefabs.AddRange(prefabs);
			}

			Terrain[] terrains = terrainCaches.Select(t=>t.terrain);
			ObjectsOps.DestroyObjects(terrains, tileData.area.full.worldPos, tileData.area.full.worldSize, 
				exceptObjs: trnInstances,
				onlyPrefabs: specificPrefabs ? usedPrefabs : null );

			ObjectsOps.TransferObjects(trnList, 
				onlyPrefabs: specificPrefabs ? usedPrefabs : null );

			//change.AddRect(matrix.rect);
			change.AddFlag(CacheChange.Type.Objects);
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Brush/Output", 
		name = "Objects", 
		section=2, 
		colorType = typeof(TransitionsList), 
		iconName="GeneratorIcons/BrushObjectsOut",
		disengageable = true,
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/ObjectsInOut")]
	public class  BrushWriteObjects : Generator, IInlet<TransitionsList>, IFnExit<TransitionsList>, IBrushWrite, IRelevant
	{
		//common settings
		public GameObject[] prefabs = new GameObject[1];
		public PositioningSettings posSettings = new PositioningSettings() {relativeHeight=false};

		//specific settings
		public bool instantiateClones = false;
		public bool guiProperties;

		public string Name { get => "Objects"; set {} } //as function portal


		public override void Generate (TileData data, StopToken stop) { }


		public void WriteTerrains (TerrainCache[] terrainCaches, CacheChange change, TileData tileData)
		{
			HashSet<GameObject> usedPrefabs = new HashSet<GameObject>();
			usedPrefabs.AddRange(prefabs);

			Terrain[] terrains = terrainCaches.Select(t=>t.terrain);
			ObjectsOps.DestroyObjects(terrains, tileData.area.full.worldPos, tileData.area.full.worldSize, onlyPrefabs:usedPrefabs);

			TransitionsList tlist = tileData.ReadInletProduct(this);
			if (tlist == null) return;

			ObjectsOps.CreateObjects(terrains, tlist, prefabs, posSettings, tileData);
			
			//change.AddRect(matrix.rect);
			change.AddFlag(CacheChange.Type.Objects);
		}
	}


	public static class ObjectsOps
	{
		public static IEnumerable<Transform> RelatedTransforms (Terrain terrain, Vector2D worldPos, Vector2D worldSize)
		/// Iterates all transforms in possible objects parent that are within given world rect
		{
			//terrain children (goes first)
			foreach (Transform tfm in ChildRelatedTransforms(terrain.transform, worldPos, worldSize))
				yield return tfm;

			//upper level objects
			if (terrain.transform.parent != null)
				foreach (Transform par in terrain.transform.parent)
			{
				if (par == terrain) //ignore terrain itself
					continue;

				if (par.GetComponent<Terrain>() != null) //ignore draft terrains
					continue;

				foreach (Transform tfm in ChildRelatedTransforms(par, worldPos, worldSize))
					yield return tfm;
			}
		}


		private static IEnumerable<Transform> ChildRelatedTransforms (Transform parent, Vector2D worldPos, Vector2D worldSize)
		{
				//foreach (Transform tfm in parent) //sometimes returns null instead of transform

				int childCount = parent.childCount;
				for (int i=0; i<childCount; i++)
				{
					Transform tfm = parent.GetChild(i);

					Vector3 pos = tfm.position;
					if (pos.x < worldPos.x  ||  pos.x > worldPos.x+worldSize.x  || pos.z < worldPos.z  ||  pos.z > worldPos.z+worldSize.z)
						continue;

					yield return tfm;
				}
		}


		public static void DestroyObjects (Terrain[] terrains, Vector2D worldPos, Vector2D worldSize, HashSet<Transform> exceptObjs=null, HashSet<GameObject> onlyPrefabs=null)
		/// If related object instance in scene is not contained in transitions list (trs.instance) it is removed
		{
			List<Transform> objsToRemove = new List<Transform>();

			foreach (Terrain terrain in terrains)
				foreach (Transform tfm in RelatedTransforms(terrain, worldPos, worldSize))
				{
					if (exceptObjs != null  &&  exceptObjs.Contains(tfm))
						continue;

					if (onlyPrefabs != null)
					{
						GameObject prefab = GetPrefab(tfm);
						if (prefab == null  ||  !onlyPrefabs.Contains(prefab))
							continue;
					}

					objsToRemove.Add(tfm);
				}

			for (int i=objsToRemove.Count-1; i>=0; i--)
				GameObject.DestroyImmediate(objsToRemove[i].gameObject);
			
		}


		public static void TransferObjects (TransitionsList tlist, HashSet<GameObject> onlyPrefabs=null)
		/// Moves/rotates/scales objects that are both in list and in scene
		/// if heights is null then heights are not considered as relative
		{
			for (int t=0; t<tlist.count; t++)
			{
				Transition trs = tlist.arr[t];

				Transform instance = trs.instance;
				if (instance == null)
					continue;

				if (onlyPrefabs != null)
				{
					GameObject prefab = GetPrefab(instance);
					if (prefab == null  ||  !onlyPrefabs.Contains(prefab))
						continue;
				}

				/*if (heights != null)
				{
					float terrainHeight = heights.GetWorldInterpolatedValue(trs.pos.x, trs.pos.z, roundToShort:true);
					terrainHeight *= heights.worldSize.y;
					trs.pos.y += terrainHeight;
				}*/
				
				#if UNITY_EDITOR
				UnityEditor.Undo.RecordObject(instance, Undo.Undo.undoName);
				#endif

				instance.localPosition = trs.pos; //transformation.MultiplyPoint3x4(draft.pos);
				instance.localRotation = trs.rotation;
				instance.localScale = trs.scale;
			}
		}


		public static void CreateObjects (Terrain[] terrains, TransitionsList trns, GameObject[] prefabs, MapMagic.Nodes.PositioningSettings posSettings, TileData data)
		{
			Noise random = null;
			if (prefabs.Length > 1)
				random = new Noise(12345);

			(Vector3,Vector3,Transform)[] minMaxParents = GatherPerTerrainParents(terrains);

			for (int t=0; t<trns.count; t++)
			{
				Transition trn = trns.arr[t]; //using copy since it's changing in MoveRotateScale

				Transform parent = GetParent(minMaxParents, trn.pos);
				if (parent == null) continue; //do not spawn object if it is out of terrains

				if (!data.area.active.Contains(trn.pos)) continue; //skipping out-of-active area

				posSettings.MoveRotateScale(ref trn, data);
	
				//selecting
				GameObject prefab;
				if (prefabs.Length == 1)
					prefab = prefabs[0];
				else
				{
					float rnd = random.Random(trn.hash);
					prefab = prefabs[ (int)(rnd*prefabs.Length) ];
				}

				//spawning
				GameObject instance;
				#if UNITY_EDITOR
				if (!UnityEditor.EditorApplication.isPlaying && 
					UnityEditor.PrefabUtility.GetPrefabAssetType(prefab)!=UnityEditor.PrefabAssetType.NotAPrefab)  //if not playing and prefab is prefab
						instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
				else
						instance = GameObject.Instantiate(prefab); 

				UnityEditor.Undo.RegisterCreatedObjectUndo(instance, Undo.Undo.undoName);
				#else
				instance = GameObject.Instantiate(prefab);
				#endif

				instance.transform.parent = parent;
				instance.hideFlags = parent.gameObject.hideFlags;

				//moving
				Transform tfm = instance.transform;

				tfm.localPosition = trn.pos; //transformation.MultiplyPoint3x4(draft.pos);
				

				if (posSettings.regardPrefabRotation) tfm.localRotation = trn.rotation * prefab.transform.rotation;
				else tfm.localRotation = trn.rotation;

				if (posSettings.regardPrefabScale) tfm.transform.localScale = trn.scale.x * prefab.transform.localScale;
				else tfm.transform.localScale = trn.scale;
			}
		}


		private static (Vector3,Vector3,Transform)[] GatherPerTerrainParents (Terrain[] terrains)
		/// For each of the terrains finds it's min, max and transform parent object to spawn objects
		{
			(Vector3,Vector3,Transform)[] minMaxParents = new (Vector3,Vector3,Transform)[terrains.Length];

			for (int t=0; t<minMaxParents.Length; t++)
			{
				Terrain terrain = terrains[t];

				Vector3 terrainPos = terrain.transform.position;
				Vector3 terrainSize = terrain.terrainData.size;
				Vector3 terrainMax = terrainPos+terrainSize;

				Transform parent = terrain.transform;

				//mapmagic tile is preferable
				if (terrain.transform.parent != null)
					foreach (Transform par in terrain.transform.parent)
					{
						if (par.GetComponent<TerrainTile>() != null)
							parent = par;
					}

				minMaxParents[t] = (terrainPos, terrainMax, parent);
			}

			return minMaxParents;
		}


		private static Transform GetParent ((Vector3,Vector3,Transform)[] minMaxParents, Vector3 pos)
		
		{
			foreach ((Vector3 terrainMin, Vector3 terrainMax, Transform parent) in minMaxParents)
			{
				if (pos.x > terrainMin.x  &&  pos.x < terrainMax.x  &&
					pos.z > terrainMin.z  &&  pos.z < terrainMax.z)
						return parent;
			}

			return null;
		}


		public static GameObject GetPrefab (Transform tfm)
		/// Returns a prefab of transform instance
		{
			GameObject prefab = null;

			#if UNITY_EDITOR
			prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(tfm.gameObject);
			#endif

			return prefab;
		}
	}
}