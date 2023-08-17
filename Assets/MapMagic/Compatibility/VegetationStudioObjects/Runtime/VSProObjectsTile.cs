using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools;
using MapMagic.Terrains;

#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.Billboards;
#endif

namespace MapMagic.VegetationStudio
{
	[ExecuteInEditMode]
	public class VSProObjectsTile : MonoBehaviour, ISerializationCallbackReceiver
	/// Helper script to automatically add/remove and serialize textures and persistent storage data
	/// This will clear objects on moving terrain and playmode disable as well
	{
		#if VEGETATION_STUDIO_PRO
		public VegetationSystemPro system;		//either source or clone system
		public VegetationPackagePro package;
		public Rect terrainRect;

		public List<Transition>[] transitions;  //serialized via OnBeforeSerialize. Unity stores arrays, stores lists, but doesn't store arrays of lists
		public string[] objectIds;
		[System.NonSerialized] public bool objectApplied;


		#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		#endif
		[RuntimeInitializeOnLoadMethod] 
		static void Subscribe ()  => TerrainTile.OnTileMoved += ClearOnMove;
		static void ClearOnMove (TerrainTile tile) 
		{
			VSProObjectsTile vsTile = tile.GetComponent<VSProObjectsTile>();
			vsTile?.OnDisable();
		}


		//public void OnEnable ()  //VSPro has got to run OnEnable first (objects only)
		public void Start () 
		{
			if (package == null) 
				return;

			if (!objectApplied) VSProOps.SetObjects(system, transitions, objectIds, VSProObjectsOut.VS_MM_id);   
			objectApplied = true;
		}


		public void OnDisable ()
		{
			if (objectApplied) VSProOps.FlushObjects(system, terrainRect);
			objectApplied = false;
		}

		#endif


		#region Serialization

			[System.Serializable]
			public struct SerList  { public List<Transition> list; }
			public SerList[] serTransitions;

			public void OnBeforeSerialize ()
			{
				#if VEGETATION_STUDIO_PRO
				serTransitions = new SerList[transitions.Length];
				for (int i=0; i<transitions.Length; i++)
					serTransitions[i].list = transitions[i];
				#endif
			}

			public void OnAfterDeserialize ()
			{
				#if VEGETATION_STUDIO_PRO
				transitions = new List<Transition>[serTransitions.Length];
				for (int i=0; i<transitions.Length; i++)
					transitions[i] = serTransitions[i].list;
				#endif
			}

		#endregion
	}
}