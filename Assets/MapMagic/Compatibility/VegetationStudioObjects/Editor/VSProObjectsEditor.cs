using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

using Den.Tools;
using Den.Tools.GUI;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Nodes;
using MapMagic.Nodes.GUI;

#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.Common;
using AwesomeTechnologies.VegetationSystem;
#endif

namespace MapMagic.VegetationStudio
{
	public static class VSProObjectsOutEditor
	{
		#if VEGETATION_STUDIO_PRO //otherwise will log a warning
		private static string[] objectNames;
		#endif

		[UnityEditor.InitializeOnLoadMethod]
		static void EnlistInMenu ()
		{
			CreateRightClick.generatorTypes.Add(typeof(VSProObjectsOut));
		}

		[Draw.Editor(typeof(VSProObjectsOut))]
		public static void DrawVSProObjectsOut (VSProObjectsOut gen)
		{
			#if VEGETATION_STUDIO_PRO
			VegetationPackagePro package = null;

			if (GraphWindow.current.mapMagic != null)
			{
				VegetationSystemPro system = GraphWindow.current.mapMagic.globals.vegetationSystem as VegetationSystemPro;
				using (Cell.LineStd) GeneratorDraw.DrawGlobalVar(ref system, "System");
				GraphWindow.current.mapMagic.globals.vegetationSystem = system;

				package = GraphWindow.current.mapMagic.globals.vegetationPackage as VegetationPackagePro;
				using (Cell.LineStd) GeneratorDraw.DrawGlobalVar(ref package, "Package");
				GraphWindow.current.mapMagic.globals.vegetationPackage = package;
			}
			else
				using (Cell.LinePx(18+18)) Draw.Label("Not assigned to current \nMapMagic object");

			//filling object names array for popup
			if (package != null)
			{
				if (objectNames == null || objectNames.Length != package.VegetationInfoList.Count)
					objectNames = new string[package.VegetationInfoList.Count];
				for (int i=0; i<objectNames.Length; i++)
					objectNames[i] = package.VegetationInfoList[i].Name;
			}
			else objectNames = null;

			if (GraphWindow.current.mapMagic == null)
				using (Cell.LineStd) Draw.Label("Graph is not in scene");
			else if (package == null)
				using (Cell.LineStd) Draw.Label("No package assigned");

			#else

			using (Cell.LinePx(76))
				Draw.Helpbox("Vegetation Studio Pro doesn't seem to be installed, or Vegetation Studio Pro compatibility is not enabled in settings");

			#endif

			using (Cell.LineStd) 
				LayersEditor.DrawLayers(
					ref gen.layers, 
					onDraw: n => DrawVSProObjectsLayer(gen, package, n), 
					onCreate: n => new VSProObjectsOut.Layer() );

				
			using (Cell.LinePx(0))
			{
				Cell.EmptyRowPx(2);

				using (Cell.Row)
				{
					//height
					Cell.EmptyLinePx(2);
					using (Cell.LinePx(0))
						using (new Draw.FoldoutGroup(ref gen.guiHeight, "Height", padding:0))
							if (gen.guiHeight)
							{
								using (Cell.LineStd) Draw.ToggleLeft(ref gen.objHeight, "Object Height");
								using (Cell.LineStd) Draw.ToggleLeft(ref gen.relativeHeight, "Relative Height");
							}

					//rotation
					Cell.EmptyLinePx(2);
					using (Cell.LinePx(0))
						using (new Draw.FoldoutGroup(ref gen.guiRotation, "Rotation", padding:0))
							if (gen.guiRotation)
							{
								using (Cell.LineStd) Draw.ToggleLeft(ref gen.useRotation, "Use Rotation");
								using (Cell.LineStd) Draw.ToggleLeft(ref gen.takeTerrainNormal, "Terrain Normal");
								using (Cell.LineStd) 
								{
									Cell.current.disabled = gen.takeTerrainNormal;
									Draw.ToggleLeft(ref gen.rotateYonly, "Rotate Y Only"); //
								}
								using (Cell.LineStd) Draw.ToggleLeft(ref gen.regardPrefabRotation, "Regard Prefab");
								using (Cell.LineStd) 
								{
									Cell.EmptyRowPx(18);
									using (Cell.Row) Draw.Label("Rotation"); 
								}
							}

					//scale
					Cell.EmptyLinePx(2);
					using (Cell.LinePx(0))
						using (new Draw.FoldoutGroup(ref gen.guiScale, "Scale", padding:0))
							if (gen.guiScale)
							{
								using (Cell.LineStd) Draw.ToggleLeft(ref gen.useScale, "Use Scale");
								using (Cell.LineStd) Draw.ToggleLeft(ref gen.scaleYonly, "Scale Y Only");
								using (Cell.LineStd) Draw.ToggleLeft(ref gen.regardPrefabScale, "Regard Prefab");
								using (Cell.LineStd) 
								{
									Cell.EmptyRowPx(18); 
									using (Cell.Row) Draw.Label("Scale");  
								}
							}

					Cell.EmptyLinePx(2);
					using (Cell.LinePx(0))
						using (new Draw.FoldoutGroup(ref gen.guiAdvanced, "Advanced", isLeft:false, padding:0))
							if (gen.guiAdvanced)
							{
								using (Cell.LinePx(0))
								{
									//using (Cell.LineStd) Draw.Label("Biome Blend");
									using (Cell.LineStd) 
									{
										//Cell.current.fieldWidth = 0.6f;
										Draw.Field(ref gen.biomeBlend, "Biome Blend");
									}
								}

								if (GraphWindow.current.mapMagic != null)
								using (Cell.LineStd) GeneratorDraw.DrawGlobalVar(
									ref GraphWindow.current.mapMagic.globals.vegetationSystemCopy, 
									"Copy VS");

								using (Cell.LineStd) Draw.Field(ref gen.outputLevel, "Out Level");
							}
				}

				Cell.EmptyRowPx(2);
			}
		}

		public static void DrawVSProObjectsLayer (VSProObjectsOut gen, VegetationPackagePro package, int n)
		{
			if (n>=gen.layers.Length) return; //on layer remove

			VSProObjectsOut.Layer layer = gen.layers[n];

			#if !VEGETATION_STUDIO_PRO
			{
				using (Cell.LineStd) Draw.Label(" Item id: " + layer.id??"none");
				return;
			}

			#else

			if (package == null)
			{
				using (Cell.LineStd) Draw.Label(" Item id: " + layer.id??"none");
				return;
			}

			int itemInfoIndex = package.VegetationInfoList.FindIndex(i => i.VegetationItemID == layer.id);
			VegetationItemInfoPro itemInfo = itemInfoIndex>=0 ? package.VegetationInfoList[itemInfoIndex] : null;

			Texture2D icon = null;
			if (itemInfo != null)
			{
				#if UNITY_EDITOR
				if (itemInfo.PrefabType == VegetationPrefabType.Mesh) icon = AssetPreviewCache.GetAssetPreview(itemInfo.VegetationPrefab);
				else icon = AssetPreviewCache.GetAssetPreview(itemInfo.VegetationTexture);
				#endif
			}

			Cell.EmptyLinePx(4);
			using (Cell.LinePx(24)) 
			{
				Cell.EmptyRowPx(4);

				using (Cell.RowPx(24)) 
					if (icon!=null)
						Draw.TextureIcon(icon);

				Cell.EmptyRowPx(2);

				using (Cell.Row) 
				{
					Cell.EmptyLine();
					using (Cell.LineStd)
					{
						Draw.PopupSelector(ref itemInfoIndex, objectNames); 
						if (Cell.current.valChanged) 
							layer.id = package.VegetationInfoList[itemInfoIndex].VegetationItemID;
					}
					Cell.EmptyLine();
				}
				Cell.EmptyRowPx(4);
			}
			Cell.EmptyLinePx(4);

			#endif
		}
	}
}