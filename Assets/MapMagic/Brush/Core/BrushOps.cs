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

using UnityEngine.Profiling;

namespace MapMagic.Brush
{
	public static class BrushOps
	{
		public static bool CheckHeightPixelSize (Terrain terrain, float refPixelSize)
		/// Checks if terrain pixel size matches reference pixel size
		{
			TerrainData data = terrain.terrainData;
			float pixelSize = data.size.x / (data.heightmapResolution-1);
			return !(refPixelSize > pixelSize+0.0001f  ||  refPixelSize < pixelSize-0.0001f);
		}


		public static CoordRect GetHeightPixelRect (Terrain terrain) => GetTerrainPixelRect(terrain, terrain.terrainData.heightmapResolution);

		public static CoordRect GetTerrainPixelRect (Terrain terrain, int resolution)
		{
			float terrainPixelSize = terrain.terrainData.size.x / (resolution-1);

			return new CoordRect(
				Mathf.RoundToInt(terrain.transform.position.x/terrainPixelSize),
				Mathf.RoundToInt(terrain.transform.position.z/terrainPixelSize),
				resolution,
				resolution);
		}


	}
}