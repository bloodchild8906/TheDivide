
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;

using MapMagic.Core;
using MapMagic.Terrains;
using MapMagic.Core.GUI;
using MapMagic.Terrains.GUI;

namespace MapMagic.Brush
{
	public partial class BrushInspector
	{
		const int numCorners = 64;
		static private PolyLine polyLine = new PolyLine(numCorners);
		static private PolyLine traceLine = new PolyLine(numCorners);
		static private PolyLine dashLine = new PolyLine(numCorners);
		static private Vector3[] corners = new Vector3[numCorners];
		

		public void OnSceneGUI ()
		{	
			current = this;

			MapMagicBrush brush = (MapMagicBrush)target;
			lastBrush = brush;

			TerrainManager.ExcludeNullTerrains(ref brush.terrains);  //doing before each SceneGui since we can unpin MM tile without de-selecting Brush

			if (brush.enabled  &&  
				brush.draw  &&  
				IsMouseInSceneView()  &&  
				brush.terrains != null  &&
				brush.terrains.Length != 0)
			{
				DrawBrush(brush);

				//redrawing scene
				SceneView.lastActiveSceneView.Repaint();
			}

			if (brush.enabled  &&  Event.current.type == EventType.KeyDown)
			{
				bool keyPressed = false;

				switch (Event.current.keyCode)
				{
					case KeyCode.LeftBracket:  brush.preset.radius = brush.preset.radius / 1.25f;  keyPressed=true;  break;
					case KeyCode.RightBracket:  brush.preset.radius = brush.preset.radius * 1.25f;  keyPressed = true;  break;

					case KeyCode.BackQuote: brush.AssignQuickPreset(0);  keyPressed = true;  break; //not tilde
					case KeyCode.Alpha1: brush.AssignQuickPreset(1);  keyPressed = true;  break;
					case KeyCode.Alpha2: brush.AssignQuickPreset(2);  keyPressed = true;  Event.current.Use();  break;
					case KeyCode.Alpha3: brush.AssignQuickPreset(3);  keyPressed = true;  break;
				}

				if (keyPressed)
					{ Repaint(); UI.RepaintAllWindows(); }
			}
		}


		public static void DrawBrush (MapMagicBrush brush) 
		{
			//disabling selection
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

			//finding position
			Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

			HashSet<Terrain> possibleTerrains = new HashSet<Terrain>(brush.terrains);
			RaycastHit hit = TerrainAiming.GetAimedTerrainHit(worldRay, possibleTerrains, null);

			Vector3 aimPos;
			if (hit.collider == null) aimPos = TerrainAiming.GetAimPosAtZeroLevel(worldRay);
			else aimPos = hit.point;


			//drawing circles
			if (brush.draw  &&  Event.current.type == EventType.Repaint)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Drawing Locks");

				DrawCircle(polyLine, aimPos, brush.preset.radius*brush.preset.hardness, brush.mainColor, brush.lineThickness, corners, brush.terrains);
				DrawCircle(polyLine, aimPos, brush.preset.radius, brush.falloffColor, brush.lineThickness, corners, brush.terrains);

				UnityEngine.Profiling.Profiler.EndSample();
			}

			//pressing
			if (brush.draw  &&  
				Event.current.type==EventType.MouseDown  && 
				Event.current.button==0  &&  
				!Event.current.alt)
					brush.trace.Start(aimPos, brush.Apply, brush);

			//drawing
			if (brush.draw  &&  
				Event.current.type==EventType.MouseDrag  && 
				Event.current.button==0  &&  
				!Event.current.alt)
					brush.trace.Drag(aimPos, brush.preset.spacing*brush.preset.radius, brush.Apply, brush);

			//releasing
			if (Event.current.rawType == EventType.MouseUp)
			{
				brush.trace.Release(brush);

				//if (!brush.readStack.Empty)  //recording undo if brush stroked (anything in current stack)
				//	BrushInspector.current.undo.RecordUndo(brush);

				if (Event.current.button==0) brush.tuneStroke.drawTune = false; //un-pressing tune stroke on mouse up

				foreach (Terrain terrain in brush.terrains) //updating collision/lods
					terrain.terrainData.SyncHeightmap();
			}

			//drawing trace
			if (brush.trace.isDrawing)
			{
			//	DrawTrace(traceLine, brush.trace.poses, falloffColor, brush.opTerrains);
				/*DrawDashLine(dashLine, 
					start: brush.trace.LastPos, 
					end: brush.trace.ApproxNextStep(brush.radius*brush.spacing), 
					color: mainColor, 
					dashSize: brush.radius/10, 
					terrains: brush.opTerrains);*/
				Vector2D approxNextPos = brush.trace.ApproxNextStep(brush.preset.radius*brush.preset.spacing);

				//DrawTrace(traceLine, new Vector2D[]{brush.trace.LastPos, approxNextPos}, mainColor, brush.opTerrains);
				//DrawTrace(traceLine, new Vector2D[]{brush.trace.LastStampPos, brush.trace.LastPos}, falloffColor, brush.opTerrains);
			}

			//drawing tune circle
			if (brush.tuneStroke.stamps.Length != 0)
			{
				(Vector2D pos, float radius) = brush.tuneStroke.GuiCircle();
				DrawCircle(polyLine, (Vector3)pos, radius, brush.mainColor, brush.lineThickness, corners, brush.terrains);
			}
		}


		private static void DrawTrace (PolyLine line, IList<Vector2D> points, Color color, Terrain[] terrains, int startNum=0, int endNum=-1)
		{
			if (points.Count <= 1)
				return;

			Vector3[] flooredPoints = new Vector3[points.Count];
			for(int i=0; i<flooredPoints.Length; i++)
				flooredPoints[i] = (Vector3)points[i];

			FloorPoints(flooredPoints, terrains);

			line.DrawLine(flooredPoints, color, 8, zMode:PolyLine.ZMode.Overlay, offset:0.1f);
		}


		private static void DrawDashLine (PolyLine line, Vector2D start, Vector2D end, Color color, float dashSize, Terrain[] terrains)
		{
			float dist = (start-end).Magnitude;
			float numDashes = dist/dashSize;
			if (numDashes == 0)
				return;

			Vector3[][] dashes = new Vector3[(int)numDashes+1][];

			Vector3[] flooredStartEnd = new Vector3[] { (Vector3)start, (Vector3)end };
			FloorPoints(flooredStartEnd, terrains);

			for (int d=0; d<dashes.Length; d++)
			{
				float percent = d / numDashes;
				float ePercent = (d+0.5f) / numDashes;

				dashes[d] = new Vector3[] { 
					flooredStartEnd[0]*percent + flooredStartEnd[1]*(1-percent),
					flooredStartEnd[0]*ePercent + flooredStartEnd[1]*(1-ePercent) };
			}

			line.DrawLine(dashes, color, 8, zMode:PolyLine.ZMode.Overlay, offset:0.1f);
		}


		private static float DrawCircle (PolyLine line, Vector3 center, float radius, Color color, float thickness, Vector3[] corners, Terrain[] terrains)
		/// Return an average height BTW almost for free :)
		/// Copy of lock circle, not reference since I'd like to be free to change it
		{
			int numCorners = corners.Length;
			float step = 360f/(numCorners-1);

			for (int i=0; i<corners.Length; i++)
				corners[i] = new Vector3( Mathf.Sin(step*i*Mathf.Deg2Rad), 0, Mathf.Cos(step*i*Mathf.Deg2Rad) ) * radius + center;

			FloorPoints(corners, terrains);
			
			line.DrawLine(corners, color, thickness, zMode:PolyLine.ZMode.Overlay, offset:0.1f);
			//Handles.DrawAAPolyLine(lineThickness, corners);

			//adjusting center height
			float heightSum = 0;
			for (int i=0; i<corners.Length; i++)
				heightSum += corners[i].y;
			return heightSum/(corners.Length-1);
		}
	
		
		private static void FloorPoints (Vector3[] points, Terrain[] terrains)
		/// Floors the array of points to terrain
		{
			Terrain prevTerrain = null;
			Rect prevRect = new Rect();

			for (int i=0; i<points.Length; i++)
			{
				Vector3 point = points[i];
				Vector2 point2D = new Vector2(point.x, point.z); //to find out if rects contains point

				//checking if the point lays within the same terrain first
				Terrain terrain = null;
				if (prevRect.Contains(point2D))
					terrain = prevTerrain;

				//finding proper terrain in all terrains in it's not in rect
				else
				{
					foreach (Terrain newTerrain in terrains)
					{
						if (newTerrain == null)
							continue;

						Vector3 terrainPos = newTerrain.transform.position;
						Vector3 terrainSize = newTerrain.terrainData.size;
						Rect terrainRect = new Rect(terrainPos.x, terrainPos.z, terrainSize.x, terrainSize.z);

						if (terrainRect.Contains(point2D))
						{
							terrain = newTerrain;
							prevTerrain = newTerrain;
							prevRect = terrainRect;
							break; 
						}
					}
				}

				//sampling height
				if (terrain != null) points[i].y = terrain.SampleHeight(point);
			}
		}

		
		private static bool IsMouseInSceneView ()
		{
			foreach (SceneView sceneView in SceneView.sceneViews)
				if (EditorWindow.mouseOverWindow == sceneView)
					return true;
			return false;
		}
	}

}
