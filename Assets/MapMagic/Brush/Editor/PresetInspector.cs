
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
	[CustomEditor(typeof(Preset))]
	public class PresetInspector : Editor
	{
		//private static readonly HashSet<string> automaticNames = new HashSet<string>() { "Radius", "Hardness", "Margins", "Position", "Direction", "Shift", "Level" };	
		private static readonly Dictionary<string,Type> automaticNames = new Dictionary<string,Type>() {
			{"Radius", typeof(float)},
			{"Hardness", typeof(float)},
			//{"Margins", typeof(float)},
			{"Position", typeof(Vector3)},
			{"PrevPosition", typeof(Vector3)},
			{"CapturedPosition", typeof(Vector3)},
			{"TerrainHeight", typeof(float)},
			{"Shift", typeof(bool)} };

		UI ui = new UI();

		public void OnEnable ()
		{
			Preset preset = (Preset)target;
			preset.SyncOvd();
		}


		public override void  OnInspectorGUI ()
		{
			Preset preset = (Preset)target;

			if (ui.undo == null)
				ui.undo = new Den.Tools.GUI.Undo {
					undoObject = preset,
					undoName = "MapMagic Brush Value"
				};

			ui.Draw( ()=> DrawPreset(preset), inInspector:true );
		}


		#region Draw Preset

			public static void DrawPreset (Preset preset)
			{
				using (Cell.LineStd) 
				{
					preset.graph = Draw.ObjectField(preset.graph, "Graph");
					preset.SyncOvd();
				}

				//default parameters
				Cell.EmptyLinePx(3);
				using (Cell.LineStd) preset.radius = Draw.Field(preset.radius, "Radius", min:0.1f);
				using (Cell.LineStd) preset.hardness = Draw.Field(preset.hardness, "Hardness", min:0, max:1);
				using (Cell.LineStd) preset.spacing =Draw.Field(preset.spacing, "Spacing", min:0.05f, max:5);
				//using (Cell.LineStd) Draw.Field(ref preset.margins, "Margins");

				//overridden parameters
				Cell.EmptyLinePx(3);
				for (int i=0; i<preset.ovd.Count; i++)
				{
					(string name, Type type, object obj) = preset.ovd.GetOverrideAt(i);

					if (automaticNames.ContainsKey(name)) //it's either radius, hardness, etc
						continue;

					using (Cell.LineStd)
					{
						obj = Draw.UniversalField(obj, type, name);

						if (Cell.current.valChanged)
							preset.ovd.SetOverrideAt(i, name, type, obj);
					}
				}

				//automatic parameters			
				Cell.EmptyLinePx(1);
				using (Cell.LineStd)
					using (new Draw.FoldoutGroup(ref preset.guiShowUsedAutomatic, "Used Auto-Values", isLeft:false, padding:0))
						if (preset.guiShowUsedAutomatic)
						{
							Cell.current.disabled = true;

							for (int i=0; i<preset.ovd.Count; i++)
							//like overridden parameters, but displaying only automatic
							{
								(string name, Type type, object obj) = preset.ovd.GetOverrideAt(i);

								if (automaticNames.ContainsKey(name)) //it's either radius, hardness, etc
									using (Cell.LineStd)
									{
										obj = Draw.UniversalField(obj, obj.GetType(), name); //using obj.GetType instead of 'type' otherwise UniversalField will write an exception that it could not convert val (object) to Vector2D

										if (Cell.current.valChanged)
											preset.ovd.SetOverrideAt(i, name, type, obj);
									}
							}
						}

				//all automatic list	
				Cell.EmptyLinePx(1);
				using (Cell.LineStd)
					using (new Draw.FoldoutGroup(ref preset.guiShowAllAutomatic, "All Auto-Values", isLeft:false, padding:0))
						if (preset.guiShowAllAutomatic)
						{
							foreach (var kvp in automaticNames)
								using (Cell.LineStd) Draw.DualLabel(kvp.Key, kvp.Value.Name);
						}
			}

			

		#endregion


		#region Quick Selection

			public static void DrawQuickSelection (MapMagicBrush brush)
			{
				using (Cell.LineStd) Draw.Label("Quick Selection:");

				LayersEditor.DrawLayers(
					brush.quickPresets.Length,
					onDraw:n => DrawQuickSelectionLayer(brush, n),
					onAdd:n => ArrayTools.Add(ref brush.quickPresets, element:null), //ArrayTools.Insert(ref brush.quickPresets, 0, element:null),
					onMove:(n,m) => SwitchQuickSelectionLayers(brush, n, m),
					onRemove:n => ArrayTools.RemoveAt(ref brush.quickPresets, n) );
			}

			public static void DrawQuickSelectionLayer (MapMagicBrush brush, int n)
			{
				Preset preset = brush.quickPresets[n];

				//selection background
				if (brush.sourcePreset == preset)
				{
					GUIStyle style;
					if (selectedOffset==null || selectedOffset.top == 0)
						selectedOffset = new RectOffset(4,4,4,4); //can't create 4444 on serialize
					if (n==0) style = UI.current.textures.GetElementStyle("DPUI/Layers/SelectedTop", selectedOffset); 
					else if (n == brush.quickPresets.Length-1) style = UI.current.textures.GetElementStyle("DPUI/Layers/SelectedBottom", selectedOffset); 
					else if (brush.quickPresets.Length == 1) style = UI.current.textures.GetElementStyle("DPUI/Layers/SelectedOnly", selectedOffset); 
					else style = UI.current.textures.GetElementStyle("DPUI/Layers/SelectedMiddle", selectedOffset); 

					Draw.Element(style);
				}

				
				Cell.EmptyLinePx(4);
				using (Cell.LinePx(22))
				{
					//layer icon
					Cell.EmptyRowPx(4);

					using (Cell.RowPx(20))
						Draw.Icon( UI.current.textures.GetTexture("DPUI/Icons/Layer") );

					Cell.EmptyRowPx(4);

					//selection field
					using (Cell.Row)
					{
						Cell.EmptyRowPx(4);
						using (Cell.RowPx(20))
						{
							if (n < keyCodeNames.Length)
								Draw.Icon( UI.current.textures.GetTexture(keyCodeNames[n]) );
						}

						Cell.EmptyRowPx(4);

						using (Cell.Row)
							Draw.ObjectField(ref brush.quickPresets[n]);

						Cell.EmptyRowPx(4);

						//if (Draw.Button(visible:false))
						if (!UI.current.layout  &&  Event.current.isMouse  &&  Cell.current.Contains(UI.current.mousePos)  &&  Event.current.type == EventType.MouseDown)
						{
							brush.AssignPreset(preset);
							Event.current.Use();
						}
					}
				}
				Cell.EmptyLinePx(4);
			}

			private static void SwitchQuickSelectionLayers (MapMagicBrush brush, int n, int m)
			{
				if (brush.sourcePreset == brush.quickPresets[n])
					brush.sourcePreset = brush.quickPresets[m];

				else if(brush.sourcePreset == brush.quickPresets[m])
					brush.sourcePreset = brush.quickPresets[n];

				ArrayTools.Switch(brush.quickPresets, n, m);
			}

			private static readonly string[] keyCodeNames = new string[] {"DPUI/KeyCodes/KeyCode~", "DPUI/KeyCodes/KeyCode1", "DPUI/KeyCodes/KeyCode2",
				"DPUI/KeyCodes/KeyCode3", "DPUI/KeyCodes/KeyCode4", "DPUI/KeyCodes/KeyCode5", "DPUI/KeyCodes/KeyCode6", "DPUI/KeyCodes/KeyCode7",
				"DPUI/KeyCodes/KeyCode8", "DPUI/KeyCodes/KeyCode9", "DPUI/KeyCodes/KeyCode0" };
			public static RectOffset selectedOffset;// = new RectOffset(4,4,4,4);

		#endregion
	}
}