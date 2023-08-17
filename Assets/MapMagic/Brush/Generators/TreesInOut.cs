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
		name = "Trees", 
		section=2, 
		colorType = typeof(TransitionsList), 
		iconName="GeneratorIcons/TreesIn",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/TreesInOut")]
	public class  BrushReadTrees : Generator, IOutlet<TransitionsList>,  IFnEnter<TransitionsList>, IBrushRead
	{
		public bool specificPrefabs = false;
		public GameObject[] prefabs = new GameObject[1];

		public string Name { get => "Trees"; set {} } //as function portal


		public override void Generate (TileData data, StopToken stop) { }


		public void ReadTerrains (TerrainCache[] terrainCaches, TileData tileData)
		{
			TransitionsList tlist = new TransitionsList();

			foreach (TerrainCache terrainCache in terrainCaches)
			{
				HashSet<int> onlyIndexes = null;
				if (specificPrefabs) onlyIndexes = TreesOps.UsedPrefabsIndexes(terrainCache.terrain.terrainData.treePrototypes, prefabs);

				TreesOps.TreesFromTerrain(terrainCache.terrain, tlist, tileData.area.full.worldPos, tileData.area.full.worldSize, onlyIndexes:onlyIndexes);
			}

			//if (relativeHeight)
			//	for (int i=0; i<tlist.count; i++)
			//		tlist.arr[i].pos.y -= BrushReadObjects.GetTerrainHeight(tlist.arr[i].pos, tileData); //terrain.SampleHeight(pos);

			tileData.StoreProduct(this, tlist);
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Brush/Output", 
		name = "Transfer Trees", 
		section=2, 
		colorType = typeof(TransitionsList), 
		iconName="GeneratorIcons/BrushTreesOut",
		disengageable = true,
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/TreesInOut")]
	public class  BrushTransferTrees : Generator, IInlet<TransitionsList>, IFnExit<TransitionsList>, IBrushWrite, IRelevant
	{
		//common settings
		public bool specificPrefabs = false;
		public GameObject[] prefabs = new GameObject[1];
		public PositioningSettings posSettings = new PositioningSettings() { relativeHeight = false };
		public bool guiProperties;

		//specific settings
		public Color color = Color.white;
		public Color lightmapColor = Color.white;
		public float bendFactor;


		public string Name { get => "Move Trees"; set {} } //as function portal


		public override void Generate (TileData data, StopToken stop) { }

		public void WriteTerrains (TerrainCache[] terrainCaches, CacheChange change, TileData tileData)
		{
			TransitionsList trnList = tileData.ReadInletProduct(this);
			if (trnList == null) return;

			//reading height if not loaded for relative pos
			if (tileData.heights == null  &&  posSettings.relativeHeight) 
			{
				Area.Dimensions full = tileData.area.full;
				tileData.heights = new MatrixWorld(full.rect, full.worldPos, full.worldSize, tileData.globals.height);
	
				foreach (TerrainCache terrainCache in terrainCaches)
					BrushReadHeight206.ReadHeights(terrainCache.terrain, tileData.heights);
			}

			//clearing old/adding new trees
			foreach (TerrainCache terrainCache in terrainCaches)
			{
				Terrain terrain = terrainCache.terrain;

				TreeInstance[] trees = terrain.terrainData.treeInstances;
				TreePrototype[] prototypes = terrain.terrainData.treePrototypes;
				
				//clearing
				HashSet<int> onlyIndexes = null;
				if (specificPrefabs) onlyIndexes = TreesOps.UsedPrefabsIndexes(prototypes, prefabs);
				TreesOps.ClearTreesWithinRect(ref trees, tileData.area.active.worldPos, tileData.area.active.worldSize, terrain, onlyIndexes:onlyIndexes);

				//adding instances
				TreesOps.ReCreateTrees(ref trees, trnList, terrain, tileData, posSettings, color, onlyIndexes:onlyIndexes);
				terrain.terrainData.treeInstances = trees;
			}

			//change.AddRect(matrix.rect);
			change.AddFlag(CacheChange.Type.Trees);
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Brush/Output", 
		name = "Write Trees", 
		section=2, 
		colorType = typeof(TransitionsList), 
		iconName="GeneratorIcons/BrushTreesOut",
		disengageable = true,
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/TreesInOut")]
	public class  BrushWriteTrees : Generator, IInlet<TransitionsList>, IFnExit<TransitionsList>, IBrushWrite, IRelevant
	{
		//common settings
		public GameObject[] prefabs = new GameObject[1];
		public PositioningSettings posSettings = new PositioningSettings() { relativeHeight = false };

		//public bool guiMultiprefab;
		public bool guiProperties;

		//specific settings
		public bool useOriginalPrototype = false;
		public Color color = Color.white;
		public Color lightmapColor = Color.white;
		public float bendFactor;


		public string Name { get => "Trees"; set {} } //as function portal


		public override void Generate (TileData data, StopToken stop) { }

		public void WriteTerrains (TerrainCache[]  terrainCaches, CacheChange change, TileData tileData)
		{
			TransitionsList trnList = tileData.ReadInletProduct(this);
			if (trnList == null) return;

			//reading height if not loaded for relative pos
			if (tileData.heights == null  &&  posSettings.relativeHeight) 
			{
				Area.Dimensions full = tileData.area.full;
				tileData.heights = new MatrixWorld(full.rect, full.worldPos, full.worldSize, tileData.globals.height);
	
				foreach (TerrainCache terrainCache in terrainCaches)
					BrushReadHeight206.ReadHeights(terrainCache.terrain, tileData.heights);
			}

			//clearing old/adding new trees
			foreach (TerrainCache terrainCache in terrainCaches)
			{
				Terrain terrain = terrainCache.terrain;

				TreeInstance[] trees = terrain.terrainData.treeInstances;
				TreePrototype[] prototypes = terrain.terrainData.treePrototypes;
				
				//clearing
				HashSet<int> usedIndexes = TreesOps.UsedPrefabsIndexes(prototypes, prefabs);
				TreesOps.ClearTreesWithinRect(ref trees, tileData.area.active.worldPos, tileData.area.active.worldSize, terrain, onlyIndexes:usedIndexes);
				
				//adding prototypes
				bool prototypesAdded = TreesOps.AddNewPrototypes(ref prototypes, prefabs, bendFactor);
				if (prototypesAdded)
					terrain.terrainData.treePrototypes = prototypes;

				//adding instances
				int[] prefabIndexes = TreesOps.PrefabsToIndexes(prototypes, prefabs);
				TreesOps.CreateTrees(ref trees, trnList, prefabIndexes, terrain, tileData, posSettings, color);
				terrain.terrainData.treeInstances = trees;
			}

			//change.AddRect(matrix.rect);
			change.AddFlag(CacheChange.Type.Trees);
		}
	}


	public static class TreesOps
	{
		public static void TreesFromTerrain (Terrain terrain, TransitionsList tlist, Vector2D worldPos, Vector2D worldSize, HashSet<int> onlyIndexes=null)
		{
			TerrainData data = terrain.terrainData;
			TreeInstance[] allTrees = data.treeInstances;
			TreePrototype[] treePrototypes = data.treePrototypes;

			//getting 0-1 range brush rect (since tree positions are relative to terain)
			Vector3 terrainPos = terrain.transform.position;
			Vector3 terrainSize = terrain.terrainData.size;
			Vector2D relPos = (Vector2D)(worldPos-terrainPos) / (Vector2D)terrainSize;
			Vector2D relSize = worldSize / (Vector2D)terrainSize;

			for (int i=0; i<allTrees.Length; i++)
			{
				Vector3 pos = allTrees[i].position;
				if (pos.x <= relPos.x || pos.x >= relPos.x+relSize.x ||
					pos.z <= relPos.z || pos.z >= relPos.z+relSize.z)
						continue;

				if (onlyIndexes != null  &&  !onlyIndexes.Contains(allTrees[i].prototypeIndex))
					continue;

				TreePrototype prototype = treePrototypes[allTrees[i].prototypeIndex];
				//not useing number since prototypes might change on apply

				tlist.Add(new Transition(allTrees[i], terrainPos, terrainSize, prototype.prefab.transform));
			}
		}


		public static void ClearTreesWithinRect (ref TreeInstance[] trees, Vector2D worldPos, Vector2D worldSize, Terrain terrain, HashSet<int> onlyIndexes=null)
		/// Clears all of the trees within world rect
		/// If onlyIndexes is not provided - clearing ALL trees
		{
			//getting 0-1 range brush rect (since tree positions are relative to terrain)
			Vector3 terrainPos = terrain.transform.position;
			Vector3 terrainSize = terrain.terrainData.size;
			Vector2D relPos = (Vector2D)(worldPos-terrainPos) / (Vector2D)terrainSize;
			Vector2D relSize = worldSize / (Vector2D)terrainSize;

			//finding trees in rect
			bool[] isInRect = new bool[trees.Length];
			int inRectCount = 0;

			for (int i=0; i<trees.Length; i++)
			{
				if (onlyIndexes != null  &&  !onlyIndexes.Contains(trees[i].prototypeIndex))
					continue;

				Vector3 pos = trees[i].position;

				if (pos.x > relPos.x && pos.x < relPos.x+relSize.x &&
					pos.z > relPos.z && pos.z < relPos.z+relSize.z)
						{ isInRect[i] = true; inRectCount++; }
			}

			//re-creating trees skipping rect
			if (inRectCount != 0)
			{
				TreeInstance[] newTrees = new TreeInstance[trees.Length-inRectCount];

				int c = 0;
				for (int i=0; i<trees.Length; i++)
				{
					if (isInRect[i]) continue;

					newTrees[c] = trees[i];
					c++;
				}

				trees = newTrees;
			}
		}


		public static void ReCreateTrees (ref TreeInstance[] trees, TransitionsList trnList, 
			Terrain terrain, TileData data, PositioningSettings posSettings, Color color,
			HashSet<int> onlyIndexes=null)
		/// Adding only those trees from tlist that have their instance prefab assigned (and setting their index to prototype with this prefab)
		/// Works within brush only (won't add it to other trees)
		{
			//terrain position/size to calculate 0-1 range tree positions
			Vector3 terrainPos = terrain.transform.position;
			Vector3 terrainSize = terrain.terrainData.size;

			//prefabs-indexes lut
			Dictionary<GameObject,int> prefabToPrototypeIndex = new Dictionary<GameObject,int>();
			TreePrototype[] prototypes = terrain.terrainData.treePrototypes;
			for (int p=0; p<prototypes.Length; p++)
				prefabToPrototypeIndex.TryAdd(prototypes[p].prefab, p);

			//adding
			List<TreeInstance> dstInstances = new List<TreeInstance>(trnList.count);
			for (int t=0; t<trnList.count; t++)
			{
				Transition trn = trnList.arr[t]; //using copy since it's changing in MoveRotateScale

				Transform prefab = trn.instance;
				if (prefab == null)
					continue;

				int prototypeIndex;
				if (!prefabToPrototypeIndex.TryGetValue(prefab.gameObject, out prototypeIndex))
					continue;

				if (onlyIndexes != null  &&  !onlyIndexes.Contains(prototypeIndex))
					continue;

				//if (!data.area.active.Contains(trn.pos)) //skipping out-of-active area
				//	continue; 
				//otherwise it will remove the tree (not good for relocation)

				posSettings.MoveRotateScale(ref trn, data);

				TreeInstance tree = CreateTree(trn, prototypeIndex, terrainPos, terrainSize, color);

				if (tree.position.x < 0 || tree.position.z < 0 ||
					tree.position.x > 1 || tree.position.z > 1)
						continue; //this tree is now in the other terrain

				dstInstances.Add(tree);
			}

			ArrayTools.AddRange(ref trees, dstInstances.ToArray());
		}


		public static void CreateTrees (ref TreeInstance[] trees, TransitionsList trnList, int[] prefabsIndexes,
			Terrain terrain, TileData data, PositioningSettings posSettings, Color color)
		/// Adds trnList trees to trees instances array
		/// PrefabsIndexes is a list of gen prefabs, converted to prototype indees number as they used on terrain
		{
			if (prefabsIndexes.Length == 0)
				return;

			//preparing noise to randomly select prefabs
			Noise random = null;
			if (prefabsIndexes.Length > 1) //no need to randomize only prefab
				random = new Noise(12345);

			//terrain position/size to calculate 0-1 range tree positions
			Vector3 terrainPos = terrain.transform.position;
			Vector3 terrainSize = terrain.terrainData.size;

			//adding
			List<TreeInstance> treeList = new List<TreeInstance>();

			for (int t=0; t<trnList.count; t++)
			{
				Transition trn = trnList.arr[t]; //using copy since it's changing in MoveRotateScale

				if (!data.area.active.Contains(trn.pos)) continue; //skipping out-of-active area

				posSettings.MoveRotateScale(ref trn, data);

				//selecting
				int index;
				if (prefabsIndexes.Length == 1)
					index = prefabsIndexes[0];
				else
				{
					float rnd = random.Random(trn.hash);
					index = prefabsIndexes[ (int)(rnd*prefabsIndexes.Length) ];
				}

				if (index < 0)
					continue;

				//spawning
				TreeInstance tree = CreateTree(trn, index, terrainPos, terrainSize, color);

				if (tree.position.x < 0 || tree.position.z < 0 ||
					tree.position.x > 1 || tree.position.z > 1)
						continue;

				treeList.Add(tree);
			}

			ArrayTools.AddRange(ref trees, treeList.ToArray());
		}


		private static TreeInstance CreateTree (Transition trn, int prototypeIndex, Vector3 terrainPos, Vector3 terrainSize, Color color)
		/// Creates tree instance from floored/rotated/scaled transition using current gen parameters
		{
			TreeInstance tree = new TreeInstance();

			tree.position.x = (trn.pos.x - terrainPos.x) / terrainSize.x; //trees should be in 0-1 range
			tree.position.z = (trn.pos.z - terrainPos.z) / terrainSize.z;
			tree.position.y = trn.pos.y / terrainSize.y; 

			tree.rotation = trn.Yaw;
			tree.widthScale = trn.scale.x; // + trs.scale.z)/2;
			tree.heightScale = trn.scale.y;
			tree.prototypeIndex = prototypeIndex;
			tree.color = color;
			tree.lightmapColor = Color.white;

			return tree;
		}


		public static bool AddNewPrototypes (ref TreePrototype[] prototypes, GameObject[] prefabs, float bendFactor=1)
		/// If prototypes do not contain prefab from prefabs - appends prototype with new prefab
		{
			bool change = false;

			HashSet<GameObject> usedPrefabs = new HashSet<GameObject>();

			for (int i=0; i<prototypes.Length; i++)
				if (!usedPrefabs.Contains(prototypes[i].prefab))
					usedPrefabs.Add(prototypes[i].prefab);

			foreach (GameObject prefab in prefabs)
				if (!usedPrefabs.Contains(prefab))
				{
					ArrayTools.Add(ref prototypes, new TreePrototype() {prefab=prefab, bendFactor=bendFactor });
					usedPrefabs.Add(prefab); //in case it's twice in prefabs
					change = true;
				}

			return change;
		}


		public static Dictionary<GameObject,int> PrefabToPrototypeIndexLut (TreePrototype[] prototypes)
		/// Creates a dictionary to convert prefab into a tree prototype index
		/// Does not actually needs layers prefabs - will return all of the prefabs on terrain
		{
			Dictionary<GameObject,int> lut = new Dictionary<GameObject, int>();
			
			for (int i=0; i<prototypes.Length; i++)
			{
				GameObject prefab = prototypes[i].prefab;
				if (!lut.ContainsKey(prefab))
					lut.Add(prefab, i);
			}

			return lut;
		}


		public static HashSet<int> UsedPrefabsIndexes (TreePrototype[] prototypes, GameObject[] prefabs)
		/// Gets currently used prototype indexes for all prefab layers
		/// If tree prototype index is in hashSet - then it's prefab is among generator prefabs
		{
			HashSet<int> opIndexes = new HashSet<int>();

			foreach (GameObject prefab in prefabs)
			{
				int index = -1;

				for (int i=0; i<prototypes.Length; i++)
					if (prototypes[i].prefab == prefab)
					{
						index = i;
						break;
					}

				if (index >= 0)
					opIndexes.Add(index);
			}

			return opIndexes;
		}


		public static int[] PrefabsToIndexes (TreePrototype[] prototypes, GameObject[] prefabs)
		/// For each of the prefabs finds an index matching in prototype
		{
			Dictionary<GameObject,int> lut = new Dictionary<GameObject, int>();
			
			for (int i=0; i<prototypes.Length; i++)
			{
				GameObject protPrefab = prototypes[i].prefab;
				if (!lut.ContainsKey(protPrefab))
					lut.Add(protPrefab, i);
			}

			int[] prefabIndexes = new int[prefabs.Length];
			for (int i=0; i<prefabs.Length; i++)
			{
				if (lut.TryGetValue(prefabs[i], out int index))
					prefabIndexes[i] = index;
				else
					prefabIndexes[i] = -1;
			}

			return prefabIndexes;
		}
	}
}