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
	public class ObjectsData : ILockData
	{
		//public ObjectsPool.Prototype[] lockPrototypes;
		//public List<Transition>[] lockTransitions;
		//do not use them, but clearing a glade to place locked objects

		public Transform lockParent;
		public Dictionary<GameObject,Vector3> lockedObjsPoses = new Dictionary<GameObject,Vector3>(); //local terrain coordsys
		public Dictionary<GameObject,Vector3> adjustedObjsPoses = new Dictionary<GameObject,Vector3>(); //same with height delta applied

		public Vector2D center; //local terrain coordsys
		public float radius;
		public float transition;
		public float terrainSize;
		public float terrainHeight;
		
		Vector2D min; //terrain local too
		Vector2D max;

		bool heightChanged;


		public void Read (Terrain terrain, Lock lk) 
		{
			if (terrain.name == "Draft Terrain") return;

			Vector2D terrainPos = (Vector2D)terrain.transform.position;

			center = (Vector2D)lk.worldPos - terrainPos;
			radius = lk.worldRadius;
			transition = lk.worldTransition;
			terrainSize = terrain.terrainData.size.x;
			terrainHeight = terrain.terrainData.size.y;
			
			min = center - radius - transition;
			max = center + radius + transition;

			//finding locked parent if it exists
			string lockParentName = $"LockedObjects {lk.guiName}";
			if (lockParent == null)
			{
				Transform tileTfm = terrain.transform.parent;

				int tileChildCount = tileTfm.childCount;
				for (int c=0; c<tileChildCount; c++)
				{
					Transform child = tileTfm.GetChild(c);
					if (child.name == lockParentName || //name match
						((Vector2D)child.localPosition == center  &&  child.name.StartsWith("LockedObject")))  //or position match, in case of renaming (no s at the end)
							{ lockParent = child; break; }
				}
			}

			//creating locked objects parent if it wasn't found
			if (lockParent == null)
			{
				GameObject go = new GameObject();
				go.transform.parent = terrain.transform.parent;
				go.transform.localPosition = (Vector3)center;
				lockParent = go.transform;
			}

			lockParent.gameObject.name = lockParentName;

			//filling objects list
			lockedObjsPoses.Clear();
			adjustedObjsPoses.Clear();
			int childCount = lockParent.childCount;
			for (int i=0; i<childCount; i++)
			{
				Transform child = lockParent.GetChild(i);
				lockedObjsPoses.Add(child.gameObject, child.position - (Vector3)terrainPos);
			}
			
			//de-pooling objects in range from the pool
			ObjectsPool pool = terrain.transform.parent.GetComponentInChildren<ObjectsPool>();
			if (pool != null)
			{
				for (int i=pool.transform.childCount-1; i>=0; i--)
				{
					Transform child = pool.transform.GetChild(i);

					Vector3 pos = child.localPosition;
					if (pos.x < min.x || pos.x > max.x ||
						pos.z < min.z || pos.z > max.z) continue;

					float dist = (center.x-pos.x)*(center.x-pos.x) + (center.z-pos.z)*(center.z-pos.z);
				
					if (dist < (radius+transition)*(radius+transition))
					{
						pool.Depool(child.gameObject);

						if (dist < radius*radius) //adding only objects within lock center
						{
							lockedObjsPoses.Add(child.gameObject, child.transform.position - (Vector3)terrainPos);
							child.parent = lockParent;
						}
						else
							GameObject.DestroyImmediate(child.gameObject);
					}
				}
			}

			//ensuring all objects are parented to lockParent
			foreach (GameObject obj in lockedObjsPoses.Keys)
				obj.transform.parent = lockParent;
				//if (obj.transform.parent == lockParent)
				//	throw new Exception("Does not belong to lock parent");
		}


		public void WriteInThread (IApplyData applyData)
		{
			if (! (applyData is ObjectsOutput.ApplyObjectsData applyObjsData) ) return;

			//clearing a glade to place locked objs
			for (int p=0; p<applyObjsData.transitions.Length; p++)
			{
				List<Transition> transitionList = applyObjsData.transitions[p];
				int transitionsCount = transitionList.Count;
				for (int t=transitionList.Count-1; t>=0; t--)
				{
					Vector3 pos = transitionList[t].pos;
					if (pos.x < min.x || pos.x > max.x ||
						pos.z < min.z || pos.z > max.z) continue;

					float dist = (center.x-pos.x)*(center.x-pos.x) + (center.z-pos.z)*(center.z-pos.z);

					if (dist < (radius+transition)*(radius+transition))
						transitionList.RemoveAt(t);
				}
			}
		}

		public void WriteInApply (Terrain terrain, bool resizeTerrain=false)
		{
			if (!heightChanged) return;
			if (terrain.name == "Draft Terrain") return;

			Dictionary<GameObject,Vector3> lockedPoeseDict = adjustedObjsPoses.Count == 0 ?
				lockedObjsPoses : adjustedObjsPoses;
				//using the adjusted poses dict if height has been applied

			int childCount = lockParent.childCount;
			for (int i=0; i<childCount; i++)
			{
				Transform child = lockParent.GetChild(i);

				if (lockedPoeseDict.TryGetValue(child.gameObject, out Vector3 pos))
					child.position = new Vector3(child.position.x, pos.y, child.position.z);
			}

			heightChanged = false;
		}


		public void ApplyHeightDelta (Matrix srcHeights, Matrix dstHeights)
		{
			heightChanged = true;

			Vector2D relRectStart = center-(radius+transition);
			float relRectSize = radius*2+transition*2;

			adjustedObjsPoses.Clear();

			foreach (var kvp in lockedObjsPoses)
			{
				Vector3 pos = kvp.Value;
				GameObject obj = kvp.Key;

				Vector2D relPos = ((Vector2D)pos-center) / relRectSize;  //relative to lock circle
				relPos += 0.5f; //relative to lock matrix
				Vector2D matrixPos = new Vector2D(
					relPos.x*srcHeights.rect.size.x + srcHeights.rect.offset.x,
					relPos.z*srcHeights.rect.size.z + srcHeights.rect.offset.z);

				float hSrc = srcHeights.GetRelative(relPos.x, relPos.z);
				float hDst = dstHeights.GetRelative(relPos.x, relPos.z);
				float heightDelta = hDst-hSrc;

				pos.y += heightDelta*terrainHeight;

				adjustedObjsPoses.Add(obj, pos);
			}
		}


		public void ResizeFrom (ILockData src) {  }


		private static void EnlistObjsWithinRange (Vector3 center, float range, Transform parent, List<(GameObject,float)> list)
		{
			int childCount = parent.childCount;
			for (int i=0; i<childCount; i++)
			{
				Transform child = parent.GetChild(i);
				Vector3 pos = child.position;
				if (pos.x < center.x-range || pos.x > center.x+range ||
					pos.z < center.z-range || pos.z > center.z+range) continue;

				float dist = Mathf.Sqrt( (center.x-pos.x)*(center.x-pos.x) + (center.z-pos.z)*(center.z-pos.z) );
				
				if (dist < range)
				{
					GameObject childObj = child.gameObject;
					//if (!list.Contains(childObj)) 
						list.Add( (childObj, childObj.transform.position.y) );
				}
			}
		}
	}

}