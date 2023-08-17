using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Products;
using MapMagic.Terrains;

namespace MapMagic.Nodes.ObjectsGenerators
{

	[System.Serializable]
	public abstract class BaseObjectsOutput : OutputGenerator
	// doesn't have to be Generator, but we can't inherit from both BaseObjectsOutput and Generator
	// should be mostly replaced with PositioningSettings
	{
		public string name = "(Empty)";
		public GameObject[] prefabs = new GameObject[1];

		public bool guiMultiprefab;
		public bool guiProperties;

		public BiomeBlend biomeBlend = BiomeBlend.Random;

		//outdated:
		public bool objHeight = true;
		public bool relativeHeight = true;

		public bool useRotation = true; //in base since tree could also be rotated. Not the imposter ones, but anyways
		public bool takeTerrainNormal = false;
		public bool rotateYonly = false;
		public bool regardPrefabRotation = false;

		public bool useScale = true;
		public bool scaleYonly = false;
		public bool regardPrefabScale = false;

		public enum BiomeBlend { Sharp, Random, Scale, Pure }
	}


	[System.Serializable]
	[GeneratorMenu(menu = "Objects/Outputs", name = "Objects", section=2, colorType = typeof(TransitionsList), iconName="GeneratorIcons/ObjectsOut", helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Objects")]
	public class ObjectsOutput : OutputGenerator, IInlet<TransitionsList>, IPrepare
	{
		//common settings
		public GameObject[] prefabs = new GameObject[1];
		public PositioningSettings posSettings = null; // new PositioningSettings(); //to load older output
		public BiomeBlend biomeBlend = BiomeBlend.Random;

		public OutputLevel outputLevel = OutputLevel.Main;
		public override OutputLevel OutputLevel { get{ return outputLevel; } }

		public bool guiMultiprefab;
		public bool guiProperties;

		//specific settings
		public bool allowReposition = true;
		public bool instantiateClones = false;

		//moved to PositioningSettings, and thus outdated:
		public bool objHeight = true;
		public bool relativeHeight = true;
		public bool guiHeight;
		public bool useRotation = true;
		public bool takeTerrainNormal = false;
		public bool rotateYonly = false;
		public bool regardPrefabRotation = false;
		public bool guiRotation;
		public bool useScale = true;
		public bool scaleYonly = false;
		public bool regardPrefabScale = false;
		public bool guiScale;

		public static PositioningSettings CreatePosSettings (ObjectsOutput output)
		{ 
			PositioningSettings ps = new PositioningSettings();
			ps.objHeight=output.objHeight; ps.relativeHeight=output.relativeHeight; ps.guiHeight=output.guiHeight; 
			ps.useRotation=output.useRotation; ps.takeTerrainNormal=output.takeTerrainNormal; ps.rotateYonly=output.rotateYonly; ps.regardPrefabRotation=output.regardPrefabRotation; ps.guiRotation=output.guiRotation; 
			ps.useScale=output.useScale; ps.scaleYonly=output.scaleYonly; ps.regardPrefabScale=output.regardPrefabScale; ps.guiScale=output.guiScale; 
			return ps;
		}


		public void Prepare (TileData data, Terrain terrain)
		{
			//resetting modified objects to real nulls - otherwise they won't appear in thread
			for (int p=0; p<prefabs.Length; p++)
				if ((UnityEngine.Object)prefabs[p] == (UnityEngine.Object)null)  //if (prefabs[p] == null) 
					prefabs[p] = null;
		}

		public List<ObjectsPool.Prototype> GetPrototypes ()
		{
			List<ObjectsPool.Prototype> prototypes = new List<ObjectsPool.Prototype>();
			for (int p=0; p<prefabs.Length; p++)
				if (!prefabs[p].IsNull())  //if (prefabs[p] != null) 
					prototypes.Add (new ObjectsPool.Prototype() {
						prefab = prefabs[p],
						allowReposition = allowReposition,
						instantiateClones = instantiateClones,
						regardPrefabRotation = posSettings.regardPrefabRotation,
						regardPrefabScale = posSettings.regardPrefabScale } );
			return prototypes;
		}

		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
//			if (!enabled) { data.finalize.Remove(finalizeAction, this); return; }
			TransitionsList trns = data.ReadInletProduct(this);
				
			//adding to finalize
			if (enabled)
			{
				data.StoreOutput(this, typeof(ObjectsOutput), this, trns);  //adding src since it's not changing
				data.MarkFinalize(Finalize, stop);
			}
			else 
				data.RemoveFinalize(finalizeAction);
		}


		#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		#endif
		[RuntimeInitializeOnLoadMethod] 
		static void Subscribe () => Graph.OnOutputFinalized += FinalizeIfHeightFinalized;
		static void FinalizeIfHeightFinalized (Type type, TileData tileData, IApplyData applyData, StopToken stop)
		{
			if (type == typeof(MatrixGenerators.HeightOutput200))
				tileData.MarkFinalize(finalizeAction, stop);
		}

		public static FinalizeAction finalizeAction = Finalize; //class identified for FinalizeData
		public static void Finalize (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			Noise random = new Noise(data.random, 12345);

			//List<ObjectsPool.Prototype> objPrototypesList = new List<ObjectsPool.Prototype>();
			//List<List<Transition>> objTransitionsList = new List<List<Transition>>();
			//List<(ObjectsPool.Prototype prot, List<Transition> trns)> allObjsList = new List<(ObjectsPool.Prototype, List<Transition>)>();
			Dictionary<ObjectsPool.Prototype, List<Transition>> objs = new Dictionary<ObjectsPool.Prototype, List<Transition>>();

			foreach ((ObjectsOutput output, TransitionsList trns, MatrixWorld biomeMask) 
				in data.Outputs<ObjectsOutput,TransitionsList,MatrixWorld>(typeof(ObjectsOutput), inSubs:true))
			{
				if (stop!=null && stop.stop) return;

				if (trns == null) continue;
				if (biomeMask!=null  &&  biomeMask.IsEmpty()) continue; 

				if (output.posSettings == null)
					output.posSettings = CreatePosSettings(output);

				List<ObjectsPool.Prototype> prototypes = output.GetPrototypes();
				if (prototypes.Count == 0) continue;

				foreach (ObjectsPool.Prototype prot in prototypes)
					if (!objs.ContainsKey(prot)) objs.Add(prot, new List<Transition>());

				//objects
				for (int t=0; t<trns.count; t++)
				{
					Transition trn = trns.arr[t]; //using copy since it's changing in MoveRotateScale

					if (!data.area.active.Contains(trn.pos)) continue; //skipping out-of-active area
					if (PositioningSettings.SkipOnBiome(ref trn, output.biomeBlend, biomeMask, data.random)) continue; //after area check since uses biome matrix

					output.posSettings.MoveRotateScale(ref trn, data);

					trn.pos -= (Vector3)data.area.active.worldPos; //objects pool use local positions

					//float rnd = random.Random(trs.hash);
					//int listNum = transitionsCount + (int)(rnd*output.prefabs.Length);
					//objTransitionsList[listNum].Add(trsCpy);
					//objsList.[listNum].Add(trsCpy);

					float rnd = random.Random(trn.hash);
					ObjectsPool.Prototype prototype = prototypes[ (int)(rnd*prototypes.Count) ];
					objs[prototype].Add(trn);
				}
			}

			//pushing to apply
			if (stop!=null && stop.stop) return;
			ApplyObjectsData applyData = new ApplyObjectsData() { 
				prototypes=objs.Keys.ToArray(), 
				transitions=objs.Values.ToArray(), 
				terrainHeight = data.globals.height,
				objsPerIteration = data.globals.objectsNumPerFrame};
			Graph.OnOutputFinalized?.Invoke(typeof(ObjectsOutput), data, applyData, stop);
			data.MarkApply(applyData);
		}


		public class ApplyObjectsData : IApplyDataRoutine 
		{
			public ObjectsPool.Prototype[] prototypes;
			public List<Transition>[] transitions;
			public float terrainHeight; //to get relative object height (since all of the terrain data is 0-1). //TODO: maybe move it to HeightData in "Height in meters" task
			public int objsPerIteration=500;

			public void Apply(Terrain terrain)
			{
				ObjectsPool pool = terrain.transform.parent.GetComponent<TerrainTile>().objectsPool;
				pool.Reposition(prototypes, transitions);
			}

			public IEnumerator ApplyRoutine (Terrain terrain)
			{
				ObjectsPool pool = terrain.transform.parent.GetComponent<TerrainTile>().objectsPool;
		
				IEnumerator e = pool.RepositionRoutine(prototypes, transitions, objsPerIteration);
				while (e.MoveNext()) { yield return null; }
			}

			public int Resolution {get{ return 0; }}
		}


		public override void ClearApplied (TileData data, Terrain terrain)
		{
			if (posSettings == null)
				posSettings = CreatePosSettings(this);

			TerrainData terrainData = terrain.terrainData;
			Vector3 terrainSize = terrainData.size;

			ObjectsPool pool = terrain.transform.parent.GetComponent<TerrainTile>().objectsPool;
			List<ObjectsPool.Prototype> prototypes = GetPrototypes();
			pool.ClearPrototypes(prototypes.ToArray());
		}
	}


	[System.Serializable]
	[GeneratorMenu(menu = "Objects/Outputs", name = "Trees", section=2, colorType = typeof(TransitionsList), iconName="GeneratorIcons/TreesOut", helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Objects")]
	public class TreesOutput : OutputGenerator, IInlet<TransitionsList>, IPrepare
	{
		//common settings
		public GameObject[] prefabs = new GameObject[1];
		public PositioningSettings posSettings = null; // new PositioningSettings(); //to load older output
		public BiomeBlend biomeBlend = BiomeBlend.Random;

		public OutputLevel outputLevel = OutputLevel.Main;
		public override OutputLevel OutputLevel { get{ return outputLevel; } }

		public bool guiMultiprefab;
		public bool guiProperties;

		//specific settings
		public Color color = Color.white;
		public Color lightmapColor = Color.white;
		public float bendFactor;

		//moved to PositioningSettings, and thus outdated:
		public bool objHeight = true;
		public bool relativeHeight = true;
		public bool guiHeight;
		public bool useRotation = true;
		public bool takeTerrainNormal = false;
		public bool rotateYonly = false;
		public bool regardPrefabRotation = false;
		public bool guiRotation;
		public bool useScale = true;
		public bool scaleYonly = false;
		public bool regardPrefabScale = false;
		public bool guiScale;

		public static PositioningSettings CreatePosSettings (TreesOutput output)
		{ 
			PositioningSettings ps = new PositioningSettings();
			ps.objHeight=output.objHeight; ps.relativeHeight=output.relativeHeight; ps.guiHeight=output.guiHeight; 
			ps.useRotation=output.useRotation; ps.takeTerrainNormal=output.takeTerrainNormal; ps.rotateYonly=output.rotateYonly; ps.regardPrefabRotation=output.regardPrefabRotation; ps.guiRotation=output.guiRotation; 
			ps.useScale=output.useScale; ps.scaleYonly=output.scaleYonly; ps.regardPrefabScale=output.regardPrefabScale; ps.guiScale=output.guiScale; 
			return ps;
		}

		public void Prepare (TileData data, Terrain terrain)
		{
			//resetting modified objects to real nulls - otherwise they won't appear in thread
			for (int p=0; p<prefabs.Length; p++)
				if ((UnityEngine.Object)prefabs[p] == (UnityEngine.Object)null)  //if (prefabs[p] == null) 
					prefabs[p] = null;
		}

		public List<TreePrototype> GetPrototypes ()
		{
			List<TreePrototype> prototypes = new List<TreePrototype>();
			for (int p=0; p<prefabs.Length; p++)
				if (!prefabs[p].IsNull())  //if (prefabs[p] != null)
					prototypes.Add (new TreePrototype() { prefab = prefabs[p], bendFactor = bendFactor } );
			return prototypes;
		}


		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
//			if (!enabled) 
//				{ data.finalize.Remove(finalizeAction, this); return; }

			TransitionsList trns = data.ReadInletProduct(this);
				
			//adding to finalize
			if (stop!=null && stop.stop) return;
			if (enabled)
			{
				data.StoreOutput(this, typeof(TreesOutput), this, trns);  //adding src since it's not changing
				data.MarkFinalize(Finalize, stop);
			}
			else 
				data.RemoveFinalize(finalizeAction);
		}


		#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		#endif
		[RuntimeInitializeOnLoadMethod] 
		static void Subscribe () => Graph.OnOutputFinalized += FinalizeIfHeightFinalized;
		static void FinalizeIfHeightFinalized (Type type, TileData tileData, IApplyData applyData, StopToken stop)
		{
			if (type == typeof(MatrixGenerators.HeightOutput200))
				tileData.MarkFinalize(finalizeAction, stop);
		}

		public static FinalizeAction finalizeAction = Finalize; //class identified for FinalizeData
		public static void Finalize (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			Noise random = new Noise(data.random, 12345);

			List<TreeInstance> instancesList = new List<TreeInstance>();
			List<TreePrototype> prototypesList = new List<TreePrototype>();

			int prototypesCount = 0; //the total number of prototypes added to give unique index for trees

			foreach ((TreesOutput output, TransitionsList trns, MatrixWorld biomeMask) 
				in data.Outputs<TreesOutput,TransitionsList,MatrixWorld>(typeof(TreesOutput), inSubs:true))
			{
				if (stop!=null && stop.stop) return;

				if (trns == null) continue;
				if (biomeMask!=null  &&  biomeMask.IsEmpty()) continue; 

				if (output.posSettings == null)
					output.posSettings = CreatePosSettings(output);

				//prototypes
				//TODO: use GetPrototypes (and skip RemoveNullPrototypes)
				TreePrototype[] prototypesArr = new TreePrototype[output.prefabs.Length];
				for (int p=0; p<output.prefabs.Length; p++)
					prototypesArr[p] = new TreePrototype() { prefab =  output.prefabs[p], bendFactor = output.bendFactor };
				prototypesList.AddRange(prototypesArr);
				
				//instances
				for (int t=0; t<trns.count; t++)
				{
					Transition trn = trns.arr[t]; //using copy since it's changing in MoveRotateScale

					if (!data.area.active.Contains(trn.pos)) continue; //skipping out-of-active area
					if (PositioningSettings.SkipOnBiome(ref trn, output.biomeBlend, biomeMask, data.random)) continue;
					
					output.posSettings.MoveRotateScale(ref trn, data);

					float rnd = random.Random(trn.hash);
					int index = (int)(rnd*output.prefabs.Length);
						
					TreeInstance tree = new TreeInstance();

					tree.position = (trn.pos - (Vector3)data.area.active.worldPos) / data.area.active.worldSize.x;
					if (tree.position.x < 0 || tree.position.z < 0 ||
						tree.position.x > 1 || tree.position.z > 1)
							continue;

					tree.position.y = trn.pos.y / data.globals.height; //trees should be in 0-1 range

					tree.rotation = trn.Yaw;
					tree.widthScale = trn.scale.x; // + trs.scale.z)/2;
					tree.heightScale = trn.scale.y;
					tree.prototypeIndex = prototypesCount + index;
					tree.color = output.color;
					tree.lightmapColor = output.lightmapColor;

					instancesList.Add(tree);
				}

				prototypesCount += output.prefabs.Length;
			}

			//RemoveNullPrototypes(prototypesList, instancesList); //could not be executed in thread

			//pushing to apply
			if (stop!=null && stop.stop) return;
			ApplyTreesData applyData = new ApplyTreesData() { treePrototypes=prototypesList.ToArray(), treeInstances=instancesList.ToArray() };
			Graph.OnOutputFinalized?.Invoke(typeof(TreesOutput), data, applyData, stop);
			data.MarkApply(applyData);
		}


		public class ApplyTreesData : IApplyData
		{
			public TreeInstance[] treeInstances;  //tree positions use 0-1 range (percent relatively to terrain)
			public TreePrototype[] treePrototypes;

			public void Read (Terrain terrain) 
			{ 
				TerrainData data = terrain.terrainData;
				treeInstances = data.treeInstances;
				treePrototypes = data.treePrototypes;
			}

			public void Apply (Terrain terrain)
			{
				if (treePrototypes.Contains( p=>p.prefab==null ))
					RemoveNullPrototypes(ref treePrototypes, ref treeInstances);

				if (treePrototypes.Length == 0  &&  terrain.terrainData.treeInstanceCount != 0)
				{
					terrain.terrainData.treeInstances = new TreeInstance[0]; //setting instances first
					terrain.terrainData.treePrototypes = new TreePrototype[0];
				}

				terrain.terrainData.treePrototypes = treePrototypes;
				terrain.terrainData.treeInstances = treeInstances;
			}

			public int Resolution {get{ return 0; }}
		}


		public static void RemoveNullPrototypes (List<TreePrototype> prototypes, List<TreeInstance> instances)
		{
			Dictionary<int,int> indexToOptimized = new Dictionary<int, int>();
			
			int originalPrototypesCount = prototypes.Count;
			int counter = 0;
			for (int p=0; p<originalPrototypesCount; p++)
				if (prototypes[p].prefab != null)
				{
					indexToOptimized.Add(p,counter);
					counter++;
				}

			for (int p=originalPrototypesCount-1; p>=0; p--)
				if (prototypes[p].prefab == null)
					prototypes.RemoveAt(p);

			for (int i=instances.Count-1; i>=0; i--)
			{
				if (!indexToOptimized.TryGetValue(instances[i].prototypeIndex, out int optimizedIndex))
					instances.RemoveAt(i);

				else if (instances[i].prototypeIndex != optimizedIndex)
				{
					TreeInstance instance = instances[i];
					instance.prototypeIndex = optimizedIndex;
					instances[i] = instance;
				}
			}
		}

		public static void RemoveNullPrototypes (ref TreePrototype[] prototypes, ref TreeInstance[] instances)
		{
			List<TreePrototype> prototypesList = new List<TreePrototype>(prototypes);
			List<TreeInstance> instancesList = new List<TreeInstance>(instances);
			RemoveNullPrototypes(prototypesList, instancesList);
			prototypes = prototypesList.ToArray();
			instances = instancesList.ToArray();
		}

		public override void ClearApplied (TileData data, Terrain terrain)
		{
			if (posSettings == null)
				posSettings = CreatePosSettings(this);

			TerrainData terrainData = terrain.terrainData;
			TreePrototype[] prototypes = terrainData.treePrototypes;
			TreeInstance[] instances = terrainData.treeInstances;

			List<TreePrototype> newPrototypes = new List<TreePrototype>();
			Dictionary<int,int> prototypeNumsLut = new Dictionary<int, int>();  //old -> new prototype number. If not contains then should be removed
			for (int num=0; num<prototypes.Length; num++)
			{
				bool contains = false;  //if terrain tree prototype contains in this generator
				for (int p=0; p<prefabs.Length; p++)
				{
					if (prototypes[num].prefab == prefabs[p] && prototypes[num].bendFactor < bendFactor+0.0001f && prototypes[num].bendFactor > bendFactor-0.0001f) 
					{
						contains = true;
						break;
					}
				}
				
				if (!contains)
				{
					prototypeNumsLut.Add(num, newPrototypes.Count);
					newPrototypes.Add(prototypes[num]);
				}
			}

			List<TreeInstance> newInstances = new List<TreeInstance>();
			for (int i=0; i<instances.Length; i++)
			{
				if (prototypeNumsLut.TryGetValue(instances[i].prototypeIndex, out int newIndex))
				{
					TreeInstance instance = instances[i];
					instance.prototypeIndex = newIndex;
					newInstances.Add(instance);
				}
			}

			terrainData.treeInstances = newInstances.ToArray();
			terrainData.treePrototypes = newPrototypes.ToArray();
		}
	}
}