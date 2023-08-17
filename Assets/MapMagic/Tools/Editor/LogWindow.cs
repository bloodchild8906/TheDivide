using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Den.Tools
{
	public class LogWindow : EditorWindow
	{
		const int toolbarHeight = 18;
		const int scrollWidth = 15;
		const int lineHeight = 18;
		const int rowMinWidth = 100;
		const float namesWidthPercent = 0.4f;
		Vector2 scrollPosition;
		bool threadedView = false;
		//int selectedLine = 1;

		GUIStyle labelStyle = null;

		bool groupEnabled = false;
		bool prevEnabled;

		int prevLogCount = 0;

		public void OnInspectorUpdate () 
		{
			if (Log.Count != prevLogCount)
				Repaint();
			prevLogCount = Log.Count;
		}


		public void OnGUI () 
		{
			DrawToolbar();

			if (threadedView)
			{
				Dictionary<string,int> threadToRow = GetIdsToRows();
				DrawHeaders(threadToRow);
				DrawThreadedValues(threadToRow);
			}

			else
				DrawValues();
			
		}

		public void DrawToolbar ()
		{
			//toolbar/header
			if (Event.current.type == EventType.Repaint)
				EditorStyles.toolbar.Draw(new Rect(0,0,position.width, toolbarHeight), new GUIContent(), 0);
			
			//Record
			Log.enabled = EditorGUI.Toggle(new Rect(5,0,50, toolbarHeight), Log.enabled, style:EditorStyles.toolbarButton);
			EditorGUI.LabelField(new Rect(9,1,50,toolbarHeight), "Record", style:EditorStyles.miniBoldLabel);

			//Group (not used, just for future)
			bool newGroupEnabled = EditorGUI.Toggle(new Rect(55,0,50,toolbarHeight), groupEnabled, style:EditorStyles.toolbarButton);
			if (newGroupEnabled && groupEnabled) //just pressed
			{ 
				groupEnabled = true; 
			}
			if (!newGroupEnabled && groupEnabled)
			{
				groupEnabled = false;
			}
			EditorGUI.LabelField(new Rect(63,1,50,toolbarHeight), "Group", style:EditorStyles.miniBoldLabel);

			//Clear
			if (UnityEngine.GUI.Button(new Rect(105,0,50,toolbarHeight), "Clear", style:EditorStyles.toolbarButton))
				Log.Clear();

			//threaded view
			threadedView = UnityEngine.GUI.Toggle(new Rect(165,-8,100,35), threadedView, "Threaded view");
		}


		public Dictionary<string,int> GetIdsToRows ()
		/// Generates a lut of idName -> row number if thread view is enabled
		{
			HashSet<string> usedIds = Log.UsedThreads();

			Dictionary<string,int> threadToRow = new Dictionary<string,int>(usedIds.Count);
			int counter = 0;

			if (usedIds.Contains(Log.defaultId))
				{ threadToRow.Add(Log.defaultId, 0); counter++; }

			List<string> orderedIds = new List<string>();
			orderedIds.AddRange(usedIds);
			orderedIds.Sort();

			foreach (string id in orderedIds)
				if (!threadToRow.ContainsKey(id))
					{ threadToRow.Add(id, counter); counter++; }

			return threadToRow;
		}


		public void DrawHeaders (Dictionary<string,int> threadToRow)
		{
			float rowWidth = (position.width-scrollWidth) / threadToRow.Count;
			if (rowWidth < rowMinWidth) rowWidth = rowMinWidth;

			UnityEngine.GUI.BeginScrollView(
				position:new Rect(0, toolbarHeight, position.width-18, lineHeight), 
				scrollPosition:new Vector2(scrollPosition.x,0), 
				viewRect:new Rect(0, 0, threadToRow.Count*rowWidth, lineHeight),
				alwaysShowHorizontal:false,
				alwaysShowVertical:false,
				horizontalScrollbar:GUIStyle.none,
				verticalScrollbar:GUIStyle.none);

				foreach (string id in threadToRow.Keys)
				{
					int rowNum = threadToRow[id];
					Rect rect = new Rect(rowNum*rowWidth,1,rowWidth,lineHeight);

					EditorGUI.LabelField(rect, id.ToString(), style:EditorStyles.boldLabel);
				}

			UnityEngine.GUI.EndScrollView();
		}


		public void DrawThreadedValues (Dictionary<string,int> threadToRow)
		// if idToRow is null using non-threaded view (1 row)
		{
			float rowWidth = (position.width-scrollWidth) / threadToRow.Count;
			if (rowWidth < rowMinWidth) rowWidth = rowMinWidth;

			int totalHeight = 0;
			foreach (Log.Entry entry in Log.AllEntries())
				totalHeight += GetEntryHeight(entry);

			Rect valsInternalRect = new Rect();
			valsInternalRect.width = threadToRow.Count*rowWidth;
			valsInternalRect.height = totalHeight;

			scrollPosition = UnityEngine.GUI.BeginScrollView(
				position:new Rect(0, toolbarHeight+lineHeight, position.width, position.height-toolbarHeight-lineHeight), 
				scrollPosition:scrollPosition, 
				viewRect:valsInternalRect,
				alwaysShowHorizontal:true,
				alwaysShowVertical:true);

				if (labelStyle == null)
					labelStyle = new GUIStyle(UnityEditor.EditorStyles.label); 
				labelStyle.alignment = TextAnchor.UpperLeft;

				//background
				EditorGUI.DrawRect(valsInternalRect, new Color(0.9f, 0.9f, 0.9f));

				//row separators
				for (int i=0; i<threadToRow.Count; i++)
					EditorGUI.DrawRect(new Rect(i*rowWidth, valsInternalRect.y, 1, valsInternalRect.size.y), new Color(0.6f,0.6f,0.6f));

				int currHeight = 0;
				foreach (Log.Entry entry in Log.AllEntries())
				{
					int rowNum = threadToRow[entry.id];
					int entryHeight = GetEntryHeight(entry);
					Rect rect = new Rect(rowNum*rowWidth, currHeight, rowWidth, entryHeight);
					DrawEntry(rect, entry);

					currHeight += entryHeight;
				}

			UnityEngine.GUI.EndScrollView();
		}


		public void DrawValues ()
		// if idToRow is null using non-threaded view (1 row)
		{
			if (Log.root.subs == null)
				return;

			int totalHeight = 0;
			foreach (Log.Entry entry in Log.root.subs)
				totalHeight += GetEntryHeight(entry, recursively:true);

			Rect valsInternalRect = new Rect();
			valsInternalRect.width = position.width - scrollWidth;
			valsInternalRect.height = totalHeight;

			scrollPosition = UnityEngine.GUI.BeginScrollView(
				position:new Rect(0, toolbarHeight, position.width, position.height-toolbarHeight), 
				scrollPosition:scrollPosition, 
				viewRect:valsInternalRect,
				alwaysShowHorizontal:true,
				alwaysShowVertical:true);

				if (labelStyle == null)
					labelStyle = new GUIStyle(UnityEditor.EditorStyles.label); 
				labelStyle.alignment = TextAnchor.UpperLeft;

				//background
				EditorGUI.DrawRect(valsInternalRect, new Color(0.9f, 0.9f, 0.9f));

				int currHeight = 0;
				foreach (Log.Entry entry in Log.root.subs)
				{
					int entryHeight = GetEntryHeight(entry, recursively:true);
					Rect rect = new Rect(valsInternalRect.x, currHeight, valsInternalRect.width, lineHeight);
					DrawEntry(rect, entry, recursively:true);

					currHeight += entryHeight;
				}

			UnityEngine.GUI.EndScrollView();
		}


		public int DrawEntry (Rect rect, Log.Entry entry, bool recursively=false)
		/// returns the number of entries actually drawn
		{
			//line separator
			EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), new Color(0.6f,0.6f,0.6f));

			//what's written
			string name = entry.name;
			if (entry.startTicks != 0) name += $" ({((entry.disposeTicks-entry.startTicks)/(float)System.TimeSpan.TicksPerMillisecond).ToString("0.0")} ms)";
			GUIContent content = new GUIContent(name, name);

			//label/foldout
			if (!recursively || entry.subs==null)
				EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), content, labelStyle);
			else
				entry.guiExpanded = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, lineHeight), entry.guiExpanded, content);

			//subs
			int counter = 1;
			if (entry.guiExpanded && recursively && entry.subs != null)
			{
				foreach (Log.Entry sub in entry.subs)
					counter += DrawEntry(new Rect(rect.x+20, rect.y+counter*lineHeight, rect.width-20, rect.y-lineHeight), sub, true);
			}

			return counter;
		}


		public int GetEntryHeight (Log.Entry entry, bool recursively=false)
		{
			int height = lineHeight;

			if (recursively && entry.subs != null  &&  entry.guiExpanded)
			{
				foreach (Log.Entry sub in entry.subs)
					height += GetEntryHeight(sub, true);
			}

			return height;
		}


		[MenuItem ("Window/Log")]
		public static void ShowEditor ()
		{
			EditorWindow.GetWindow<LogWindow>("Log");
		}
	}
}