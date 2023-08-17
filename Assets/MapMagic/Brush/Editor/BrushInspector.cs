
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;

using MapMagic.Nodes;
using MapMagic.Terrains;
using MapMagic.Locks;
using MapMagic.Nodes.GUI; //to open graph


namespace MapMagic.Brush
{
	[CustomEditor(typeof(MapMagicBrush))]
	public partial class BrushInspector : Editor
	{
		public static BrushInspector current; //assigned on draw and removed after
		public static MapMagicBrush lastBrush; //last selected brush or any brush in scene (for graph nodes editors)

		//public Undo undo = new Undo();

		UI ui = new UI();
		bool guiAbout = false;

		//[RuntimeInitializeOnLoadMethod]
		[UnityEditor.InitializeOnLoadMethod] 
		static void Initialize () 
		{
			lastBrush = FindObjectOfType<MapMagicBrush>();
		}

		static void AssignCurrent (SceneView sceneView) 
		{
			//Debug.Log(Resources.FindObjectsOfTypeAll<MapMagicBrush>().Any());
			SceneView.duringSceneGui -= AssignCurrent; 
		}


		public void OnEnable ()
		{
			current = this;
			MapMagicBrush brush = (MapMagicBrush)target;
			lastBrush = brush;
			
			if (brush.preset.graph == null)
				brush.preset.graph = BrushGraphTemplate.CreateBrushAddTemplate();

//			UnityEditor.Undo.undoRedoPerformed -= OnUndoRedoPerformed; //done via undo itself
//			UnityEditor.Undo.undoRedoPerformed += OnUndoRedoPerformed;

			GraphWindow.OnGraphChanged -= RefreshTuneStroke;
			GraphWindow.OnGraphChanged += RefreshTuneStroke;

			brush.preset.SyncOvd();

			//adding initial terrains on first brush launch
			if (!brush.terrainsAdded)
			{
				Terrain[] terrains = brush.GetComponentsInChildren<Terrain>();
				TerrainManager.TryAddTerrains(ref brush.terrains, terrains, out string tmpErr); //no message for improper terrains
				brush.terrainsAdded = true;
			}
			
			//checking all terrains size/resolution consistency since it could be changed while in the other inspector
			if (!(TerrainManager.ExcludeImproperTerrains(ref brush.terrains, out string error)))
				EditorUtility.DisplayDialog("MapMagic Brush", error, "OK");
				
		}

//		public void OnUndoRedoPerformed () => ((MapMagicBrush)target).undo.Perform();
		public void RefreshTuneStroke (Graph graph)
		{
/*			MapMagicBrush brush = (MapMagicBrush)target;
			if (brush.graph == graph  &&  brush.tuneStroke.stamps.Length != 0)
				brush.tuneStroke.ApplyTune();*/
		}





		public override void  OnInspectorGUI ()
		{
			current = this;
			MapMagicBrush brush = (MapMagicBrush)target;

			if (ui.undo == null)
				ui.undo = new Den.Tools.GUI.Undo {
					undoObject = brush,
					undoName = "MapMagic Brush Value"
				};

			ui.Draw(DrawGUI, inInspector:true);

			//current = null;
		}

		public void DrawGUI ()
		{
			Cell.EmptyLinePx(4);

			MapMagicBrush brush = (MapMagicBrush)target;
			lastBrush = brush;

			//Draw
			using (Cell.LinePx(28))
			{
				if (TerrainEditorEnabled)
					brush.draw = false; //disabling brush if were editing terrain

				Draw.CheckButton(ref brush.draw, visible:false);

				GUIStyle style = UI.current.textures.GetElementStyle(brush.draw ? "MapMagic/PinButtons/UniversalButton_pressed" : "MapMagic/PinButtons/UniversalButton");
				Draw.Element(style);

				Texture2D colorTex = UI.current.textures.GetColorizedTexture(brush.draw ? "MapMagic/PinButtons/ColorButton_pressed" : "MapMagic/PinButtons/ColorButton", brush.mainColor);
				GUIStyle colorStyle = UI.current.textures.GetElementStyle(colorTex);
				Draw.Element(colorStyle);

				Cell.EmptyRowPx(10);

				using (Cell.RowPx(30))
				{
					Texture2D icon = UI.current.textures.GetTexture("MapMagicBrush/DrawIcon");
					Draw.Icon(icon, scale:0.5f);
				}

				using (Cell.Row) Draw.Label("Draw", style:UI.current.styles.middleLabel);
				if (Cell.current.valChanged)
				{
					if (brush.draw  &&  brush.preset.graph == null)
					{
						brush.draw = false;
						EditorUtility.DisplayDialog("Error", "Can't enable brush draw since no graph is assigned", "OK");
						return;
					}

					if (brush.draw)
					{
						brush.UpdateCaches();
						TerrainEditorEnabled = false;
					}
				}
			}

			if (brush.draw  &&  brush.tuneStroke.drawTune) brush.tuneStroke.drawTune = false;
			if (brush.draw  &&  brush.tuneStroke.stamps.Length!=0) brush.tuneStroke.stamps = new Stamp[0];

			//Current Preset
			Cell.EmptyLinePx(4);
			using (Cell.LineStd)
				using (new Draw.FoldoutGroup(ref brush.guiBrush, "Brush", isLeft:true))
					if (brush.guiBrush)
					{
						using (Cell.LineStd)
						{
							Preset newPreset = Draw.ObjectField(brush.sourcePreset, "Preset Source");
							if (Cell.current.valChanged)
								brush.AssignPreset(newPreset);
						}

						Cell.EmptyLinePx(6);
						using (Cell.LinePx(0))
							PresetInspector.DrawPreset(brush.preset);

						Cell.EmptyLinePx(4);
						using (Cell.LinePx(22))
						{
							Cell.EmptyRow();
							using (Cell.RowPx(80))
							{
								Cell.current.disabled = brush.sourcePreset==null;
								if (Draw.Button("Save"))
									brush.sourcePreset.CopyFrom(brush.preset);
							}
							using (Cell.RowPx(80))
							{
								if (Draw.Button("Save As..."))
								{
									ScriptableAssetExtensions.SaveAsset(brush.preset, filename:"BrushPreset", caption:"Save current preset to file:");
									brush.AssignPreset(brush.preset); //this will create it's copy
								}
							}
						}
					}

			//Quick Selection
			Cell.EmptyLinePx(4);
			using (Cell.LineStd)
				using (new Draw.FoldoutGroup(ref brush.guiPresets, "Presets", isLeft:true))
					if (brush.guiPresets)
						PresetInspector.DrawQuickSelection(brush);

			//Terrains
			Cell.EmptyLinePx(4);
			using (Cell.LineStd)
				using (new Draw.FoldoutGroup(ref brush.guiTerrains, "Terrains", isLeft:true))
					if (brush.guiTerrains)
					{
						for (int t=0; t<brush.terrains.Length; t++)
							using (Cell.LineStd)
						{
							Terrain terrain = brush.terrains[t];

							using(Cell.RowPx(20))
							{
								if (!TerrainManager.CheckSplatResolution(terrain))
								{	
									//Cell.current.tooltip = TerrainManager.
									if (Draw.Button(UI.current.textures.GetTexture("DPUI/Icons/Warning"), visible:false))
										DrawSplatResWarning(terrain);
								}
							}

							using (Cell.Row) Draw.ObjectField(terrain);

							using (Cell.RowPx(20))
								if (Draw.Button(UI.current.textures.GetTexture("DPUI/Icons/Remove"), visible:false))
									ArrayTools.RemoveAt(ref brush.terrains, t);
						}

						using (Cell.LineStd)
						{
							using (Cell.RowPx(40))
								Draw.Label("Add");

							using (Cell.Row)
							{
								//using (Cell.LineStd)
								//	if (Draw.Button("Select"))
								//		ScriptableAssetExtensions.ShowObjectSelector(typeof(Terrain), 12345, true, onClosed:TrySelectTerrain);

								using (Cell.LineStd)
									if (Draw.Button("This Component"))
									{
										Terrain terrain = brush.GetComponent<Terrain>();
										if (terrain != null)
										{
											if (!TerrainManager.TryAddTerrain(ref brush.terrains, terrain, out string error))
												EditorUtility.DisplayDialog("MapMagic Brush", error, "OK");
											else if (!TerrainManager.CheckSplatResolution(terrain))
												DrawSplatResWarning(terrain);
										}
									}

								using (Cell.LineStd)
									if (Draw.Button("Child Terrains"))
									{
										Terrain[] terrains = brush.GetComponentsInChildren<Terrain>();
										if (!TerrainManager.TryAddTerrains(ref brush.terrains, terrains, out string error))
												EditorUtility.DisplayDialog("MapMagic Brush", error, "OK");
									}

								using (Cell.LineStd)
									if (Draw.Button("All Terrains"))
									{
										Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
										if (!TerrainManager.TryAddTerrains(ref brush.terrains, terrains, out string error))
											EditorUtility.DisplayDialog("MapMagic Brush", error, "OK");
									}

								using (Cell.LineStd)
								{
									Terrain newTerrain = Draw.ObjectField<Terrain>(null, "Custom", allowSceneObject:true);
									if (newTerrain != null)
									{
										if (!TerrainManager.TryAddTerrain(ref brush.terrains, newTerrain, out string error))
											EditorUtility.DisplayDialog("MapMagic Brush", error, "OK");
										else if (!TerrainManager.CheckSplatResolution(newTerrain))
											DrawSplatResWarning(newTerrain);
									}
								}
							}
						}
					}

			//Tune
			/*Cell.EmptyLinePx(4);
			using (Cell.LineStd)
				using (new Draw.FoldoutGroup(ref brush.guiTuneStroke, "Tune Stroke", isLeft:true))
					if (brush.guiTuneStroke)
					{
						using (Cell.LinePx(26))
						{
							if (brush.tuneStroke.stamps.Length == 0)
								Draw.CheckButton(ref brush.tuneStroke.drawTune, "Draw Stroke");

							else
							{
								using (Cell.Row)
								{
									bool enabled = true;
									Draw.CheckButton(ref enabled, "Tune Stroke");
									if (!enabled) brush.tuneStroke.Clear();
								}
								
								using (Cell.RowPx(60)) 
									if (Draw.Button("Apply"))
									{
										brush.tuneStroke.ApplyTune();
										brush.tuneStroke.Clear();
									}
							}
						}
						
						using (Cell.LineStd) Draw.Label($"Stamps Count: {brush.tuneStroke.stamps.Length}");
					}*/


			//Settings
			Cell.EmptyLinePx(4);
			using (Cell.LineStd)
				using (new Draw.FoldoutGroup(ref brush.guiSettings, "Settings", isLeft:true))
					if (brush.guiSettings)
					{
						using (Cell.LineStd) Draw.Field(ref brush.mainColor, "Hard Color");
						using (Cell.LineStd) Draw.Field(ref brush.falloffColor, "Falloff Color");
						using (Cell.LineStd) Draw.Field(ref brush.lineThickness, "Thickness");
					}

			//About
			Cell.EmptyLinePx(4);
			using (Cell.LineStd)
				using (new Draw.FoldoutGroup(ref guiAbout, "About", isLeft:true))
					if (guiAbout)
					{
						using (Cell.Line)
						{
							using (Cell.RowPx(100))
								Draw.Icon(UI.current.textures.GetTexture("MapMagic/Icons/AssetBig"), scale:0.5f);

							using (Cell.Row)
							{
								using (Cell.LineStd) Draw.Label("Brush " + MapMagicBrush.version.ToString());
								using (Cell.LineStd) Draw.Label("MapMagic " + Core.MapMagicObject.version.ToString());

								Cell.EmptyLinePx(10);

								using (Cell.LineStd) Draw.URL(" - Online Documentation", "https://gitlab.com/denispahunov/mapmagic/wikis/home");
								using (Cell.LineStd) Draw.URL(" - Video Tutorials", url:"https://www.youtube.com/playlist?list=PL8fjbXLqBxvbsJ56kskwA2tWziQx3G05m");
								using (Cell.LineStd) Draw.URL(" - Forum Thread", url:"https://forum.unity.com/threads/released-mapmagic-2-infinite-procedural-land-generator.875470/");
								using (Cell.LineStd) Draw.URL(" - Issues / Ideas", url:"http://mm2.idea.informer.com");
							}
						}
					}
		}


		private void DrawSplatResWarning (Terrain terrain)
		{
			switch (EditorUtility.DisplayDialogComplex("MapMagic Brush", TerrainManager.splatResError, "Info", "Fix", "Ignore"))
			{
				case 0: Application.OpenURL("https://gitlab.com/denispahunov/mapmagic/-/wikis/Brush/Resolution"); break;
				case 1: TerrainManager.FixSplatResolution(terrain); break;
			}
		}


		private void TrySelectTerrain (UnityEngine.Object obj)
		/// Called on selecting any object in scene with object picker
		{
			if (obj == null) return;
			if (!(obj is GameObject go)) return;
			
			Terrain terrain = go.GetComponent<Terrain>();
			if (terrain == null) return;

			MapMagicBrush brush = (MapMagicBrush)target;
			if (!TerrainManager.TryAddTerrain(ref brush.terrains, terrain, out string error))
				EditorUtility.DisplayDialog("MapMagic Brush", error, "OK");
			else if (!TerrainManager.CheckSplatResolution(terrain))
				DrawSplatResWarning(terrain);
		}

		


		/*[MenuItem ("GameObject/3D Object/MapMagic")]
		public static MapMagicObject CreateMapMagic () { return CreateMapMagic(null); }
		
		public static MapMagicObject CreateMapMagic (Graph graph)
		{
			GameObject go = new GameObject();
			go.SetActive(false); //to avoid starting generate while graph not assigned
			go.name = "MapMagic";
			MapMagicObject mapMagic = go.AddComponent<MapMagicObject>();
			Selection.activeObject = mapMagic;

			mapMagic.graph = graph;
			go.SetActive(true);
			mapMagic.tiles.Pin( new Coord(0,0), false, mapMagic );

			//registering undo
			UnityEditor.Undo.RegisterCreatedObjectUndo (go, "MapMagic Create");
			EditorUtility.SetDirty(mapMagic);

//			Selection.activeGameObject = mapMagic.gameObject;
			//MapMagicWindow.Show(mapMagic.gens, mapMagic, asBiome:false);

			return mapMagic;
		}*/

		public static bool TerrainEditorEnabled 
		{
			get
			{
				if (activeTerrainInspectorField == null)
					activeTerrainInspectorField = GetActiveTerrainInspectorField();
				return (int)activeTerrainInspectorField.GetValue(null) != 0;
			}
			set
			{
				if (activeTerrainInspectorField == null)
					activeTerrainInspectorField = GetActiveTerrainInspectorField();
				if (!value) 
					activeTerrainInspectorField.SetValue(null, 0);
			}
		}

		private static System.Reflection.FieldInfo GetActiveTerrainInspectorField ()
		{
			Type type = Type.GetType("UnityEditor.TerrainInspector, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			return type.GetField("s_activeTerrainInspector", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		}

		private static System.Reflection.FieldInfo activeTerrainInspectorField = null;

	}//class


	public class SplatResWarningWindow : EditorWindow
	{
		UI ui = new UI();

		public MapMagicBrush brush;
		public Terrain terrain;

		public void OnGUI () => ui.Draw(DrawGUI, inInspector:false);
		
		public void DrawGUI ()
		{
			using (Cell.LinePx(100)) Draw.Label("Splat res");
			using (Cell.LineStd) 
				if (Draw.Button("Fix"))
				{
					TerrainManager.FixSplatResolution(terrain);
					Close();
				}
		}


		public static void Show (MapMagicBrush brush, Terrain terrain)
		{
			SplatResWarningWindow window = new SplatResWarningWindow();
			window.position = new Rect(Screen.currentResolution.width/2-100, Screen.currentResolution.height/2-50, 200, 100);
			window.brush = brush;
			window.terrain = terrain;
			window.ShowAuxWindow();
		}
	}


	public class PixelErrorWindow : EditorWindow
	{
		UI ui = new UI();

		public void OnGUI () => ui.Draw(DrawGUI, inInspector:false);
		
		public void DrawGUI ()
		{
			using (Cell.LinePx(100)) Draw.Label("Splat res");
		}


		public static void ShowWindow ()
		{
			PixelErrorWindow window = new PixelErrorWindow();
			window.position = new Rect(Screen.currentResolution.width/2-100, Screen.currentResolution.height/2-50, 200, 100);
			window.ShowAuxWindow();
		}
	}



		

}//namespace