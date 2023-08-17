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
	public static class TerrainManager
	{

		public static bool TryAddTerrain (ref Terrain[] brushTerrains, Terrain terrain, out string error)
		/// Checks if terrain resolution matches brush terrains resolutions and add it if it does
		/// Or returns false and outs error if not
		{
			//first terrain
			if (brushTerrains.Length==0)
			{ 
				error =null; 
				brushTerrains = new Terrain[] {terrain};
				return true; 
			}

			//additional terrain
			else
			{
				if (brushTerrains.Contains(terrain))
					{ error=null; return true; }

				if (CheckTerrain(brushTerrains[0], terrain, out string terrError))
				{
					ArrayTools.Add(ref brushTerrains, terrain);

					error = null;
					return true;
				}
				else
				{
					error = $"Could not add terrain {terrain.name}:\n\n{terrError}";
					return false;
				}
			}
		}


		public static bool TryAddTerrains (ref Terrain[] brushTerrains, Terrain[] newTerrains, out string error)
		/// Tries to add new terrains one by one
		/// Skips if their resolution doesn't match brush terrains, will return false and give summary error in this case
		{
			Terrain refTerrain;
			if (brushTerrains.Length==0)
				refTerrain = newTerrains.FindBiggest(t => t.terrainData.heightmapResolution); //finds the terrain with biggest heightmap resolution
			else
				refTerrain = brushTerrains[0]; //takes any brush terrain

			bool check = true;
			error = null;

			List<Terrain> addingTerrains = new List<Terrain>();
			foreach (Terrain terrain in newTerrains)
			{
				if (brushTerrains.Contains(terrain))
					continue;

				if (CheckTerrain(refTerrain, terrain, out string terrError))
					addingTerrains.Add(terrain);
				else
				{
					check = false;

					if (error == null)
						error = "Skipping adding these terrains:";

					error += $"\n\n{terrain.name}: {terrError}";
				}
			}

			ArrayTools.AddRange(ref brushTerrains, addingTerrains.ToArray());
			return check;
		}

		public static void ExcludeNullTerrains (ref Terrain[] brushTerrains)
		/// Ensures that all brush terrains have the same size/resolution
		/// Exclude terrains that does not and returns false
		{
			if (brushTerrains.Length == 0)
				return;

			//excluding null terrains
			for (int i=brushTerrains.Length-1; i>=0; i--)
				if (brushTerrains[i] == null)
					ArrayTools.RemoveAt(ref brushTerrains, i);
		}


		public static bool ExcludeImproperTerrains (ref Terrain[] brushTerrains, out string error)
		/// Ensures that all brush terrains have the same size/resolution
		/// Exclude terrains that does not and returns false
		{
			if (brushTerrains.Length <= 1)
				{ error = null; return true; }

			bool check = true;
			error = null;

			Terrain refTerrain = brushTerrains[0]; 
			for (int i=brushTerrains.Length-1; i>=0; i--)
			{
				Terrain terrain = brushTerrains[i];
				bool checkTerrain = CheckTerrain(refTerrain, terrain, out string terrError);
				if (!checkTerrain)
				{
					check = false;
					ArrayTools.RemoveAt(ref brushTerrains, i);

					if (error == null) 
						error = "MapMagic Brush terrains list resolution or size does not match:";
					
					error += $"\n\n{terrain.name}: {terrError}";
				}
			}

			if (!check)
				error += "\n\nExcluding these terrain from brush list. You can add them manually after changing the size/resolution.";

			return check;
		}


		private static bool CheckExcludeTerrains (Terrain refTerrain, ref Terrain[] newTerrains, out string terrError)
		/// Excluding terrains from newTerrains which resolution/size is not matching ref terrain
		/// False if any of terrains was excluded
		{
			terrError = null;
			bool check = true;

			for (int i=newTerrains.Length; i>=0; i--)
			{
				Terrain terrain = newTerrains[i];
				bool checkTerrain = CheckTerrain(refTerrain, terrain, out string checkTerrainError);
				if (!checkTerrain)
				{
					check = false;
					ArrayTools.RemoveAt(ref newTerrains, i);

					if (terrError == null) 
						terrError = checkTerrainError;
					else
						terrError += "\n\n" + checkTerrainError;
				}
			}

			return check;
		}


		private static bool CheckTerrain (Terrain refTerrain, Terrain newTerrain, out string terrError)
		/// Checks if size/resolution/etc match the reference terrain
		{
			TerrainData refData = refTerrain.terrainData;
			TerrainData newData = newTerrain.terrainData;
			terrError = null;

			int refDetRes = refData.detailResolution;
			int newDetRes = newData.detailResolution;
			if (refDetRes!=newDetRes  &&  refDetRes!=newDetRes-1  &&  refDetRes!=newDetRes+1)
				terrError = $"Terrains detail resolution don't match: {refTerrain.name}:{refData.detailResolution} and {newTerrain.name}:{newData.detailResolution}"; 
				
			int refAlphaRes = refData.alphamapResolution;
			int newAlphaRes = newData.alphamapResolution;
			if (refAlphaRes!=newAlphaRes  &&  refAlphaRes!=newAlphaRes-1  &&  refAlphaRes!=newAlphaRes+1)
				terrError = $"Terrains alpha maps (splats textures) resolution don't match: {refTerrain.name}:{refData.alphamapResolution} and {newTerrain.name}:{newData.alphamapResolution}"; 
				
			if (refData.heightmapResolution != newData.heightmapResolution)
				terrError = $"Terrains heighmap resolution don't match: {refTerrain.name}:{refData.heightmapResolution} and {newTerrain.name}:{newData.heightmapResolution}"; 

			Vector3 refSize = refData.size;
			Vector3 newSize = newData.size;
			if (newSize.x < refSize.x-0.001f  ||  newSize.x > refSize.x+0.001f  ||
				newSize.z < refSize.z-0.001f  ||  newSize.z > refSize.z+0.001f)
					terrError = $"Terrains size don't match: {refTerrain.name}:{refSize.x},{refSize.z} and {newTerrain.name}:{newSize.x},{newSize.z}"; 

			if (terrError == null)
				return true;
			else
				return false;
		}


		public static bool CheckSplatResolution (Terrain terrain)
		/// True if terrain splat resolution is PO2 + 1
		{
			int terrainRes = terrain.terrainData.alphamapResolution;
			int potRes = Mathf.ClosestPowerOfTwo(terrainRes);
			return potRes+1 == terrainRes;
		}

		public const string splatResError = "Terrain splat resolution does not match heightmap resolution formula (POT+1). This can lead to inconsistency between height and splat data.";
	
		
		public static void FixSplatResolution (Terrain terrain)
		/// Rescales splat resolution so it is PO2 + 1
		{
			TerrainData data = terrain.terrainData;
			int shouldBeRes = Mathf.ClosestPowerOfTwo(data.alphamapResolution) + 1;
			RescaleSplatResolution(data,shouldBeRes);
		}


		private static void RescaleSplatResolution (TerrainData data, int dstRes)
		{
			int numLayers = data.alphamapLayers;
			int srcRes = data.alphamapResolution;
			if (srcRes == dstRes) 
				return;

			float[,,] srcSplats = data.GetAlphamaps(0,0,srcRes,srcRes);
			Matrix[] dstMatrices = new Matrix[numLayers];
			for (int i=0; i<numLayers; i++)
			{
				Matrix srcMatrix = new Matrix(0,0,srcRes,srcRes);
				srcMatrix.ImportSplats(srcSplats,i);

				Matrix dstMatrix = new Matrix(0,0,dstRes,dstRes);
				//MatrixOps.Resize(srcMatrix, dstMatrix);
				Matrix.Resize(srcMatrix, dstMatrix); //this one will use NN interpolation, which is better when scaling 1 pixel

				dstMatrices[i] = dstMatrix;
			}

			Matrix.NormalizeLayers(dstMatrices);

			float[,,] dstSplats = new float[dstRes,dstRes,numLayers];
			for (int i=0; i<numLayers; i++)
				dstMatrices[i].ExportSplats(dstSplats, i);
				
			data.alphamapResolution = dstRes;
			data.SetAlphamaps(0,0,dstSplats);
		}


		public static Terrain[] GetTerrains (Terrain[] allTerrains, Vector2D worldPos, Vector2D worldSize)
		/// Gathers the array of terrains that are affected by this rect
		/// Closest terrain goes first [0]
		{
			if (allTerrains.Length == 0)
					return new Terrain[0];

			List<Terrain> stampTerrains = new List<Terrain>();

			Vector2D stampMin = worldPos;
			Vector2D stampMax = worldPos+worldSize;
			Vector2D stampCenter = worldPos+worldSize/2;

			float closestDist = float.MaxValue;
			Terrain closestTerrain = null;

			foreach (Terrain terrain in allTerrains)
			{
				Vector3 terrainPos = terrain.transform.position;
				Vector3 terrainSize = terrain.terrainData.size;
				Vector3 terrainMax = terrainPos+terrainSize;

				if (stampMax.x < terrainPos.x  ||  stampMin.x > terrainMax.x  ||
					stampMax.z < terrainPos.z  ||  stampMin.z > terrainMax.z)
						continue;

				stampTerrains.Add(terrain);

				//if stamp is on this terrain - it should be 100% closest
				if (stampCenter.x > terrainPos.x  &&  stampCenter.x < terrainMax.x  &&
					stampCenter.z > terrainPos.z  &&  stampCenter.z < terrainMax.z)
					{
						closestTerrain = terrain;
						closestDist = 0;
					}

				//finding other closest
				else
				{
					Vector2D pDist = (Vector2D)terrainPos - stampCenter;
					Vector2D sDist = stampCenter - (Vector2D)terrainMax;
					float dist = Mathf.Max( Mathf.Max(pDist.x, pDist.z), Mathf.Max(sDist.x, sDist.z) );

					if (dist < closestDist)
						{ closestDist=dist; closestTerrain=terrain; }
				}
			}

			//making closest terrain first in array
			int closestIndex = stampTerrains.IndexOf(closestTerrain);
			if (closestIndex != 0  &&  stampTerrains.Count != 0) 
			{
				stampTerrains[closestIndex] = stampTerrains[0];
				stampTerrains[0] = closestTerrain;
			}

			//TODO: excluding terrains that have improper resolutions

			return stampTerrains.ToArray();
		}
	}
}