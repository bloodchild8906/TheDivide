using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Nodes;
using MapMagic.Terrains;
using MapMagic.Products;
using MapMagic.Expose;
using System.Threading;
using System;

namespace MapMagic.Brush
{
	public class Trace
	/// Performs the proper brush drawing on terrain with Start, Drag and Release
	/// Periodically stamps the brush on terrain using it's spacing
	{
		[NonSerialized] public bool isDrawing = false;
		[NonSerialized] public List<Vector3> framePoses = new List<Vector3>(); //stores position each frame to apply spacing
		[NonSerialized] public float length = 0; //current distance of the spacing line, to avoid recalculating it each frame
		[NonSerialized] public List<Vector3> stampPoses = new List<Vector3>();
		[NonSerialized] public Vector3 capturedPosition; //world position used for Constant or other presets

		public Vector3 LastPos => framePoses[framePoses.Count-1];
		public Vector3 LastStampPos => stampPoses[stampPoses.Count-1]; //position of last applid stamp
		public Vector3 PrevStampPos => stampPoses.Count>1 ? stampPoses[stampPoses.Count-2] : stampPoses[stampPoses.Count-1]; //position of previously applied stamp

		public Vector2D ApproxNextStep (float spacingDist)
		/// Approximate next step position to draw dashed line in scene view
		{
			Vector2D realDir = ((Vector2D)LastPos - (Vector2D)LastStampPos).Normalized; //next position based on current cursor
			Vector2D prevDir = stampPoses.Count >= 2 ?
				((Vector2D)LastStampPos - (Vector2D)stampPoses[stampPoses.Count-2]).Normalized :
				realDir;

//			float percent = lastLength / spacingDist; //blending dirs based on distance passed
//			Vector2D dir = realDir*percent + prevDir*(1-percent);

//			return (Vector2D)LastPos + dir*(spacingDist-lastLength);
			return (new Vector2D());
		}


		public void Start (Vector3 pos, Action<Vector3,bool> stampFn, MapMagicBrush brush)
		{
			#if UNITY_EDITOR
			if (Event.current.control)
				{ capturedPosition = pos; return; }
			#endif

			isDrawing = true;
			framePoses.Add(pos);
			stampPoses.Add(pos);
			stampFn(pos,true); //StampBrush(pos);

			//applying changes to terrain on first click
			foreach (Terrain terrain in brush.terrains)
			{
				TerrainCache terrainCache = brush.terrainCaches[terrain];
				terrainCache.ApplyChanges(brush.cacheChange);
			}

			brush.cacheChange.Clear(); //after each apply
		}


		public void Drag (Vector3 pos, float spacingDist, Action<Vector3,bool> stampFn, MapMagicBrush brush)
		/// Checks if brush passed the spacing distance and applies it if needed (multiple times if really needed)
		/// Called each frame when mouse button is pressed
		/// spacingDist = radius*spacing
		{
			if (!isDrawing)
				return;

			//saving previous data
			float prevLength = length;
			Vector3 prevPos = framePoses[framePoses.Count-1];
			Vector3 moveDir = pos - prevPos;  moveDir.y = 0;
			float segDist = moveDir.magnitude; //Mathf.Sqrt((pos.x-prevPos.x)*(pos.x-prevPos.x) + (pos.z-prevPos.z)*(pos.z-prevPos.z));  //distance brush traveled this frame. Only in X and Z dimensions
			moveDir.Normalize();

			//adding point to spacing poses list and update it's length
			framePoses.Add(pos);
			length += segDist;

			//do need to apply stamp this frame?
			int stampsShouldBeApplied = (int)(length/spacingDist) + 1;
			int stampsApplied = stampPoses.Count;
			int numStamps = stampsShouldBeApplied-stampsApplied;

			//applying stamps if needed
			float prevPassedLength = prevLength - stampsApplied*spacingDist; //distance since last stamp to last frame pos
			for (int i=0; i<numStamps; i++)
			{
				float currStampLength = i*spacingDist - prevPassedLength; //at what distance to prevPos should the stamp num i beplaced

				Vector3 stampPos = prevPos + moveDir*currStampLength;
				stampPoses.Add(stampPos);
				stampFn(stampPos,false); //StampBrush(stampPos);
			}

			//applying changes to terrain
			foreach (Terrain terrain in brush.terrains)
			{
				TerrainCache terrainCache = brush.terrainCaches[terrain];
				terrainCache.ApplyChanges(brush.cacheChange);
			}
			brush.cacheChange.Clear(); //after each apply
		}


		public void Release (MapMagicBrush brush)
		/// Called each frame when mouse button is not pressed
		/// Clears spacing path. Not included in StrokeSpacingBrush to make these two fn more readable
		{
			isDrawing = false;
			framePoses.Clear();
			stampPoses.Clear();
			length = 0;

			foreach (Terrain terrain in brush.terrains)
			{
				TerrainCache terrainCache = brush.terrainCaches[terrain];
				terrainCache.EndTrace();
			}
		}
	}
}