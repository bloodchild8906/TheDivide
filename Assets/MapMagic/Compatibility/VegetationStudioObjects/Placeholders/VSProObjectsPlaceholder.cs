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


#if !VEGETATION_STUDIO_PRO
namespace MapMagic.VegetationStudio
{
	[System.Serializable]
	[GeneratorMenu(
		name = "VS Pro Objs", 
		section =2,
		drawButtons = false,
		colorType = typeof(TransitionsList))]
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


		public override void Generate (TileData data, StopToken stop) { }

		public override void ClearApplied (TileData data, Terrain terrain) { }
	}
}
#endif
