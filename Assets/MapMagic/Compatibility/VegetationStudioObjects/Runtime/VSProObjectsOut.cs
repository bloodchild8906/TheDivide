using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Products;
using MapMagic.Terrains;
using MapMagic.Nodes;
using MapMagic.Nodes.ObjectsGenerators;

#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.Billboards;
#endif

namespace MapMagic.VegetationStudio
{
	[System.Serializable]
	[GeneratorMenu(
		menu = "Objects/Outputs", 
		name = "VS Pro Objs", 
		section =2,
		drawButtons = false,
		colorType = typeof(TransitionsList), 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Grass")]
	public class VSProObjectsOut : OutputGenerator, IInlet<TransitionsList>
	{
		public OutputLevel outputLevel = OutputLevel.Main;
		public override OutputLevel OutputLevel { get{ return outputLevel; } }

		public PositioningSettings posSettings = null; // new PositioningSettings(); //to load older output
		public BiomeBlend biomeBlend = BiomeBlend.Random;

		//[Val("Package", type = typeof(VegetationPackagePro))] public VegetationPackagePro package; //in globals

		[System.Serializable]
		public class Layer
		{
			public string id; //= "d825a526-4ba2-4c8f-9f4d-3f855049718a";

			public string lastUsedName;
			public string lastUsedType;
		}

		public Layer[] layers = new Layer[] { new Layer() }; //do not use BaseObjectsOutput prefabs


		public const byte VS_MM_id = 15; //15 for MapMagic, 18 for Voxeland

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

		public PositioningSettings CreatePosSettings () =>
			posSettings = new PositioningSettings() {
			objHeight=objHeight, relativeHeight=relativeHeight, guiHeight=guiHeight, 
			useRotation=useRotation, takeTerrainNormal=takeTerrainNormal, rotateYonly=rotateYonly, regardPrefabRotation=regardPrefabRotation, guiRotation=guiRotation, 
			useScale=useScale, scaleYonly=scaleYonly, regardPrefabScale=regardPrefabScale, guiScale=guiScale }; 


		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			if (!enabled) { data.RemoveFinalize(finalizeAction); return; }

			TransitionsList trns = data.ReadInletProduct(this);
			if (trns == null) return;
			if (data.globals.vegetationSystem == null || data.globals.vegetationPackage == null) return;
				
			//adding to finalize
			if (stop!=null && stop.stop) return;
			if (enabled)
			{
				data.StoreOutput(this, typeof(VSProObjectsOut), this, trns);  //adding src since it's not changing
				data.MarkFinalize(Finalize, stop);
			}
			else 
				data.RemoveFinalize(finalizeAction);
		}


		public static FinalizeAction finalizeAction = Finalize; //class identified for FinalizeData
		public static void Finalize (TileData data, StopToken stop)
		{
			#if VEGETATION_STUDIO_PRO

			if (stop!=null && stop.stop) return;
			Noise random = new Noise(data.random, 12345);

			Dictionary<string, List<Transition>> idsObjs = new Dictionary<string, List<Transition>>();
			//key is id ("d825a526-4ba2-4c8f-9f4d-3f855049718a"), while value is the list of objects positions

			foreach ((VSProObjectsOut output, TransitionsList trns, MatrixWorld biomeMask) 
				in data.Outputs<VSProObjectsOut,TransitionsList,MatrixWorld>(typeof(VSProObjectsOut), inSubs:true))
			{
				if (stop!=null && stop.stop) return;

				if (trns == null) continue;
				if (biomeMask!=null  &&  biomeMask.IsEmpty()) continue; 

				if (output.posSettings == null)
					output.posSettings = output.CreatePosSettings();

				foreach (Layer layer in output.layers)
					if (!idsObjs.ContainsKey(layer.id)) idsObjs.Add(layer.id, new List<Transition>());

				//objects
				for (int t=0; t<trns.count; t++)
				{
					Transition trn = trns.arr[t]; //using copy since it's changing in MoveRotateScale

					if (!data.area.active.Contains(trn.pos)) continue; //skipping out-of-active area
					if (PositioningSettings.SkipOnBiome(ref trn, output.biomeBlend, biomeMask, data.random)) continue;

					output.posSettings.MoveRotateScale(ref trn, data);

					float rnd = random.Random(trn.hash);
					int layerNum = (int)(rnd*output.layers.Length);
					string id = output.layers[layerNum].id;
					idsObjs[id].Add(trn);
				}
			}

			//pushing to apply
			if (stop!=null && stop.stop) return;

			List<Transition>[] allTrns = new List<Transition>[idsObjs.Count];
			string[] allIds = new string[idsObjs.Count];

			int i=0;
			foreach (var kvp in idsObjs)
			{
				allTrns[i] = kvp.Value;
				allIds[i] = kvp.Key;
				i++;
			}

			//adding coord to ids to clear tile on change
/*			string coordId = null;
			if (!data.globals.vegetationSystemCopy) //if copy - just clearing it's storage
			{
				Coord coord = data.area.Coord;
				coordId = $"_c{coord.x},{coord.z}";
				for (int j=0; j<allIds.Length; j++)
					allIds[j] = allIds[j] + coordId;
			}*/

			ApplyData applyData = new ApplyData() { 
				trns=allTrns, 
				ids=allIds,
				srcSystem=data.globals.vegetationSystem as VegetationSystemPro, 
				package=data.globals.vegetationPackage as VegetationPackagePro, 
				copyVS= data.globals.vegetationSystemCopy};
			Graph.OnOutputFinalized?.Invoke(typeof(ObjectsOutput), data, applyData, stop);
			data.MarkApply(applyData);

		#endif
		}


		public override void ClearApplied (TileData data, Terrain terrain)
		{

		}

		#if VEGETATION_STUDIO_PRO
		public class ApplyData : IApplyData
		{
			public VegetationSystemPro srcSystem;
			private VegetationSystemPro copySystem; //assigned from ApplyCopy only
			public VegetationPackagePro package; //to update copy system
			public bool copyVS;

			public List<Transition>[] trns;
			public string[] ids;
		
			public void Read (Terrain terrain)  { throw new System.NotImplementedException(); }

			public void Apply (Terrain terrain)
			{
				//checking persistent storage assigned (too much users missing this part)
				PersistentVegetationStorage storage = srcSystem.PersistentVegetationStorage;
				#if UNITY_EDITOR
				if (storage.PersistentVegetationStoragePackage == null)
					UnityEditor.EditorUtility.DisplayDialog("VSPro Storage not assigned",
						"VSPro stoarage asset is not assigned. Assign storage asset to VegetationSystemPro object -> Persistent Vegetation Storage component, " +
						"and click Initialize Persistent Storage button.",
						"OK");
				#endif

				//moving all objects in case MM object is not placed in zero
				TerrainTile tile = terrain.transform.parent.GetComponent<TerrainTile>();
				Core.MapMagicObject mapMagic = tile.mapMagic;
				Vector3 pos = mapMagic.transform.position;
				if (pos != Vector3.zero)
				{
					for (int i=0; i<trns.Length; i++)
						for (int j=0; j<trns[i].Count; j++)
						{
							Transition trn = trns[i][j];
							trn.pos += pos;
							trns[i][j] = trn;
						}
				}

				//getting approximate tile number (to clear storage on non-copy - hacky)
				int tileNum = VS_MM_id;
				if (!copyVS)
				{
					foreach (TerrainTile atile in mapMagic.tiles.All()) 
					{
						if (atile == tile) break;
						tileNum++;
					}
					tileNum = tileNum % 255;
				}

				//updating system
				VegetationSystemPro copySystem = null;  //we'll need it to set up tile
				if (copyVS)
				{
					copySystem = VSProOps.GetCopyVegetationSystem(terrain); 
					if (copySystem == null) copySystem = VSProOps.CopyVegetationSystem(srcSystem, terrain.transform.parent);
					VSProOps.UpdateCopySystem(copySystem, terrain, package, srcSystem);
					copySystem.PersistentVegetationStorage.InitializePersistentStorage();
				}

				else
				{
					VSProOps.UpdateSourceSystem(srcSystem, terrain);

					foreach (string id in ids)
						storage.RemoveVegetationItemInstances(id, (byte)tileNum);
				}

				//applying
				VSProOps.SetObjects(copyVS ? copySystem : srcSystem, trns, ids, (byte)tileNum);

				//tile obj (serialization and disable purpose)
				Transform tileTfm = terrain.transform.parent;
				VSProObjectsTile vsTile = tileTfm.GetComponent<VSProObjectsTile>();
				if (vsTile == null) vsTile = tileTfm.gameObject.AddComponent<VSProObjectsTile>();
				vsTile.system = copyVS ? copySystem : srcSystem;
				vsTile.package = package;
				vsTile.terrainRect = terrain.GetWorldRect();
				vsTile.transitions = trns;
				vsTile.objectIds = ids;
				vsTile.objectApplied = true;
			}


			public static ApplyData Empty
			{get{
				return new ApplyData() { 
					trns = new List<Transition>[0],
					ids = new string[0]  };
			}}

			public int Resolution {get{ return 0; }}
		}
		#endif
	}
}
