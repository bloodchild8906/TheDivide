using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Terrains;
using MapMagic.Nodes;
using MapMagic.Nodes.MatrixGenerators;
using MapMagic.Expose;

using UnityEngine.Profiling;

namespace MapMagic.Brush
{
	[System.Serializable]
	[CreateAssetMenu(menuName = "MapMagic/Brush Preset", fileName = "Brush.asset", order = 124)]
	public class Preset : ScriptableObject
	{
		public Graph graph;

		public float radius = 100;
		public float hardness = 0.5f;
		public int margins = 0;
		public float spacing = 0.25f;

		public Override ovd = new Override();

		public bool guiShowUsedAutomatic = false;
		public bool guiShowAllAutomatic = false;

		public void SyncOvd () 
		{
			if (graph != null)
				ovd.Sync(graph.defaults);
		}

		public void CopyFrom (Preset other) 
		{
			graph = other.graph;
			radius = other.radius;
			hardness = other.hardness;
			margins = other.margins;
			spacing = other.spacing;
			ovd = new Override(other.ovd);
		}
	}
}