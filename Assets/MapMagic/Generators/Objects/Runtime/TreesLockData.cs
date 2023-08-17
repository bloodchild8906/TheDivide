using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Terrains;
using MapMagic.Nodes;
using MapMagic.Nodes.ObjectsGenerators;

namespace MapMagic.Locks
{
	public class TreesData : ILockData
	{
		public TreeInstance[] lockInstances;  //tree positions use 0-1 range (percent relatively to terrain)
		public TreePrototype[] lockPrototypes;

		public Vector2D center; //int 0-1 range relatively to terrain
		public float radius;
		public float transition;

		
		public void Read (Terrain terrain, Lock lk) 
		{
			Vector3 terrainPos = terrain.transform.position;
			TerrainData terrainData = terrain.terrainData;
			Vector3 terrainSize = terrainData.size;

			center = new Vector2D(
				(lk.worldPos.x-terrainPos.x)/terrainSize.x, 
				(lk.worldPos.z-terrainPos.z)/terrainSize.z);
			radius = lk.worldRadius/terrainSize.x;
			transition = lk.worldTransition/terrainSize.x;

			lockInstances = TreesInRange(terrainData.treeInstances, center, radius+transition).ToArray();
			lockPrototypes = terrainData.treePrototypes;
		}


		public void WriteInThread (IApplyData applyData)
		{
			if (! (applyData is TreesOutput.ApplyTreesData applyTreesData) ) return;
			
			TreeInstance[] generatedInstances = applyTreesData.treeInstances;
			TreePrototype[] generatedPrototypes = applyTreesData.treePrototypes;
			
			UnifyPrototypes(ref generatedPrototypes, ref generatedInstances, ref lockPrototypes, ref lockInstances);

			List<TreeInstance> newTrees = TreesOutRange(generatedInstances, center, radius+transition); //clearing a glade to place locked trees
			newTrees.AddRange(lockInstances);

			applyTreesData.treeInstances = newTrees.ToArray();
			applyTreesData.treePrototypes = lockPrototypes;
		}

		public void WriteInApply (Terrain terrain, bool resizeTerrain=false) 
		{ 
			if (lockInstances == null || lockPrototypes == null) return; // Don't perform lock if nothing is stored

			terrain.terrainData.treePrototypes = lockPrototypes;

			List<TreeInstance> trees = TreesOutRange(terrain.terrainData.treeInstances, center, radius+transition);
			trees.AddRange(lockInstances);
			terrain.terrainData.treeInstances = trees.ToArray();
		}

		public void ApplyHeightDelta (Matrix srcHeights, Matrix dstHeights)
		{
			Vector2D relRectStart = center-(radius+transition);
			float relRectSize = radius*2+transition*2;

			for (int i=0; i<lockInstances.Length; i++)
			{
				Vector2D relPos = ((Vector2D)lockInstances[i].position - relRectStart) / relRectSize;
				Vector2D matrixPos = new Vector2D(
					relPos.x*srcHeights.rect.size.x + srcHeights.rect.offset.x,
					relPos.z*srcHeights.rect.size.z + srcHeights.rect.offset.z);

				float hSrc = srcHeights.GetInterpolated(matrixPos.x, matrixPos.z);
				float hDst = dstHeights.GetInterpolated(matrixPos.x, matrixPos.z);
				float heightDelta = hDst-hSrc;

				lockInstances[i].position.y += heightDelta;
			}
		}

		public void ResizeFrom (ILockData src) {  }


		private static List<TreeInstance> TreesInRange (TreeInstance[] srcInstances, Vector2D center, float radius)
		{
			Vector2D min = new Vector2D(center.x-radius, center.z-radius);
			Vector2D max = new Vector2D(center.x+radius, center.z+radius);

			List<TreeInstance> dstInstances = new List<TreeInstance>();
			for (int i=0; i<srcInstances.Length; i++)
			{
				TreeInstance instance = srcInstances[i];

				Vector3 pos = instance.position;
				if (pos.x < min.x || pos.z < min.z ||
					pos.x > max.x || pos.z > max.z) continue;

				float dist = Mathf.Sqrt((pos.x-center.x)*(pos.x-center.x) + (pos.z-center.z)*(pos.z-center.z));
				if (dist > radius) continue;
				
				dstInstances.Add(instance);
			}

			return dstInstances;
		}
		
		private static List<TreeInstance> TreesOutRange (TreeInstance[] srcInstances, Vector2D center, float radius)
		/// TreesInRange inverted
		{
			List<TreeInstance> dstInstances = new List<TreeInstance>();
			for (int i=0; i<srcInstances.Length; i++)
			{
				TreeInstance instance = srcInstances[i];

				Vector3 pos = instance.position;
				float dist = Mathf.Sqrt((pos.x-center.x)*(pos.x-center.x) + (pos.z-center.z)*(pos.z-center.z));
				if (dist < radius) 
					continue;
				
				dstInstances.Add(instance);
			}

			return dstInstances;
		}


		private static void UnifyPrototypes (ref TreePrototype[] basePrototypes, ref TreeInstance[] baseInstances, 
											 ref TreePrototype[] addPrototypes, ref TreeInstance[] addInstances)
		/// Makes both datas prototypes arrays equal, and the layers arrays relevant to prototypes (empty arrays)
		/// Safe per-channel blend could be performed after this operation
		{
			//guard if prototypes have not been changed
			if (ArrayTools.MatchExactly(basePrototypes, addPrototypes)) return;

			//creating array of unified prototypes
			List<TreePrototype> unifiedPrototypes = new List<TreePrototype>();
			unifiedPrototypes.AddRange(basePrototypes); //do not change the base prototypes order
			for (int p=0; p<addPrototypes.Length; p++)
			{
				if (!unifiedPrototypes.Contains(addPrototypes[p]))
					unifiedPrototypes.Add(addPrototypes[p]);
			}

			//lut to convert prototypes indexes
			Dictionary<int,int> baseToUnifiedIndex = new Dictionary<int, int>();
			Dictionary<int,int> addToUnifiedIndex = new Dictionary<int, int>();

			for (int p=0; p<basePrototypes.Length; p++)
				baseToUnifiedIndex.Add(p, unifiedPrototypes.IndexOf(basePrototypes[p]));  //should be 1,2,3,4,5, but doing this in case unified prototypes gather will be optimized

			for (int p=0; p<addPrototypes.Length; p++)
				addToUnifiedIndex.Add(p, unifiedPrototypes.IndexOf(addPrototypes[p]));

			//re-creating base data
			for (int i=0; i<baseInstances.Length; i++)
				baseInstances[i].prototypeIndex = baseToUnifiedIndex[ baseInstances[i].prototypeIndex ];

			//re-creating add data
			for (int i=0; i<addInstances.Length; i++)
				addInstances[i].prototypeIndex = addToUnifiedIndex[ addInstances[i].prototypeIndex ];

			//saving prototypes
			basePrototypes = unifiedPrototypes.ToArray();
			addPrototypes = unifiedPrototypes.ToArray();
		}
	}
}