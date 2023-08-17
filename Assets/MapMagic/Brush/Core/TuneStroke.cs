using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Nodes;
using MapMagic.Terrains;
using MapMagic.Products;
using System.Threading;
using System;

namespace MapMagic.Brush
{
	[System.Serializable]
	public class TuneStroke
	{
		public MapMagicBrush brush;

		public bool drawTune;

		public Stamp[] stamps = new Stamp[0];

		
		public void StampTune (Vector2D pos)
		{
			/*BrushStamp stamp = new BrushStamp() {pos=pos, radius=brush.radius, hardness=brush.hardness, margins=brush.margins};

			Terrain closestTerrain = MapMagicBrush.GetClosestTerrain(brush.allTerrains, stamp.pos);
			TileData tileData = MapMagicBrush.GetTileData(closestTerrain, stamp, brush.graph);
			ReadData readData = new ReadData();

			foreach (IBrushRead brushReadNode in brush.graph.GeneratorsOfType<IBrushRead>())
				brushReadNode.ReadTerrains(brush.opTerrains, stamp, readData, tileData);

			readData.terrains = brush.opTerrains;

			ArrayTools.Add(ref stamps, stamp);
			ArrayTools.Add(ref originalReads, readData);*/
		}


		public void ApplyTune ()
		{
/*			//erasing previous stuff
			for (int s=0; s<stamps.Length; s++)
				originalReads[s].Apply();
			
			//applying new
			for (int s=0; s<stamps.Length; s++)
				brush.StampBrush(stamps[s]);*/
		}


		public void Clear ()
		{
			/*for (int s=0; s<stamps.Length; s++)
				originalReads[s].Apply();

			stamps = new Stamp[0];
			originalReads = new ReadData[0];*/
		}


		public (Vector2D,float) GuiCircle ()
		{
			Vector2D min = new Vector2D(float.MaxValue, float.MaxValue); 
			Vector2D max = new Vector2D(float.MinValue, float.MinValue);

			for (int s=0; s<stamps.Length; s++)
			{
				Stamp stamp = stamps[s];
				if (stamp.pos.x < min.x) min.x = stamp.pos.x;
				if (stamp.pos.z < min.z) min.z = stamp.pos.z;
				if (stamp.pos.x > max.x) max.x = stamp.pos.x;
				if (stamp.pos.z > max.z) max.z = stamp.pos.z;
			}

			Vector2D center = (min+max) / 2;
			Vector2D size = (max-min);
			float radius = Mathf.Max(size.x, size.z) / 2;

			return (center,radius + stamps[0].radius);
		}
	}
}