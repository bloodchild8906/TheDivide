using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;  
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Products;

namespace MapMagic.Nodes.ObjectsGenerators
{
	[System.Serializable]
	[GeneratorMenu (menu="Objects/Initial", name ="Positions", iconName="GeneratorIcons/Position", disengageable = true, helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/object_generators/Scatter")]
	public class Positions200 : Generator, IOutlet<TransitionsList>
	{
		public Vector3[] positions= new Vector3[1];

		public override void Generate (TileData data, StopToken stop)
		{
			if (!enabled) return;

			TransitionsList trns = new TransitionsList();
			for (int p=0; p<positions.Length; p++)
			{
				Transition trs = new Transition(positions[p].x, positions[p].y, positions[p].z);
				trns.Add(trs);
			}

			data.StoreProduct(this, trns);
		}

	}


	[System.Serializable]
	[GeneratorMenu (menu="Objects/Initial", 
		name ="Scatter", 
		iconName="GeneratorIcons/Scatter", 
		disengageable = true, 
		advancedOptions = true,
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/object_generators/Scatter")]
	public class Scatter200 : Generator, IOutlet<TransitionsList>
	{
		[Val("Seed")]			public int seed = 12345;
		//						public enum Algorithm { Random, SquareCells, HexCells };
		//[Val("Algorithm")]		public Algorithm algorithm = Algorithm.SquareCells;
		[Val("Density")]		public float density = 10; //doesn't guarantee that chunk 1km*1km will have this number of objects (since they positioned randomly), but gurantee that on infinite terrain there will be Density objs per km
		[Val("Uniformity")]		public float uniformity = 0.1f;		
		
		[Val("Relax", "Advanced")]	public float relax = 0.5f;
		[Val("Add.Margin", "Advanced")]	public float additionalMargins = 0;

		public override void Generate (TileData data, StopToken stop)
		{
			if (!enabled) return;
			if (density==0) { data.StoreProduct(this, new TransitionsList()); return; }

			Noise random = new Noise(data.random, seed);
			TransitionsList trns = Scatter(
				(Vector3)data.area.full.worldPos - new Vector3(additionalMargins, 0, additionalMargins), 
				(Vector3)data.area.full.worldSize + new Vector3(additionalMargins*2, 0, additionalMargins*2), 
				random);

			data.StoreProduct(this, trns);
		}

		public TransitionsList Scatter (Vector3 worldPos, Vector3 worldSize, Noise random)
		{
			float cellSize = 1000 / Mathf.Sqrt(density);

			CoordRect rect = CoordRect.WorldToPixel((Vector2D)worldPos, (Vector2D)worldSize, (Vector2D)cellSize);  //will convert to cells as well
			worldPos = new Vector3(rect.offset.x*cellSize, worldPos.y, rect.offset.z*cellSize);  //modifying worldPos/size too
			worldSize = new Vector3(rect.size.x*cellSize, worldSize.y, rect.size.z*cellSize);

			rect.offset -= 1; rect.size += 2; //leaving 1-cell margins
			worldPos.x -= cellSize; worldPos.z -= cellSize; 
			worldSize.x += cellSize*2; worldSize.z += cellSize*2;
				
			PositionMatrix posMatrix = new PositionMatrix(rect, worldPos, worldSize);
			posMatrix.Scatter(uniformity, random);
			posMatrix = posMatrix.Relaxed(relax);

			//DebugGizmos.DrawCoordRect("posMatrix", posMatrix.rect,  posMatrix.worldPos, posMatrix.worldSize);

			return posMatrix.ToTransitionsList();
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Objects/Initial", name ="Random", iconName="GeneratorIcons/Random", disengageable = true, 
		colorType = typeof(TransitionsList),
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/object_generators/Scatter")]
	public class Random207 : Generator, IInlet<MatrixWorld>, IOutlet<TransitionsList>
	{
		[Val("Seed")]			public int seed = 12345;
		[Val("Density")]		public float density = 10;
		[Val("Uniformity")]		public float uniformity = 0.1f;


		public override void Generate (TileData data, StopToken stop)
		{
			if (!enabled) return;
			MatrixWorld probMatrix = data.ReadInletProduct(this);

			Noise random = new Noise(data.random, seed);

			float square = data.area.active.worldSize.x * data.area.active.worldSize.z; //note using the real size, density should not depend on margins
			float count = square*(density/1000000); //number of items per terrain

			PosTab posTab = new PosTab((Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize, 16);
			RandomScatter((int)count, uniformity, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize, posTab, random, probMatrix, stop:null);  
			TransitionsList transitions = posTab.ToTransitionsList();

			data.StoreProduct(this, transitions);
		}


		public static void RandomScatter (int count, float uniformity, Vector3 offset, Vector3 size, PosTab posTab, Noise rnd, MatrixWorld prob, StopToken stop = null)
		{
			int candidatesNum = (int)(uniformity*100);
			if (candidatesNum < 1) candidatesNum = 1;
			
			for (int i=0; i<count; i++)
			{
				if (stop!=null && stop.stop) return;

				float bestCandidateX = 0;
				float bestCandidateZ = 0;
				float bestDist = 0;
				
				for (int c=0; c<candidatesNum; c++)
				{
					float candidateX = (offset.x+1) + (rnd.Random((int)posTab.pos.x, (int)posTab.pos.z, i*candidatesNum+c, 0)*(size.x-2.01f)); //TODO: do not use pos since it changes between preview/full
					float candidateZ = (offset.z+1) + (rnd.Random((int)posTab.pos.x, (int)posTab.pos.z, i*candidatesNum+c, 1)*(size.z-2.01f));

					//checking if candidate is the furthest one
					Transition closest = posTab.Closest(candidateX, candidateZ, minDist:0.001f);
					float dist = (closest.pos.x-candidateX)*(closest.pos.x-candidateX) + (closest.pos.z-candidateZ)*(closest.pos.z-candidateZ);

					//distance to the edge
					float bd = (candidateX-offset.x)*2; if (bd*bd < dist) dist = bd*bd;
					bd = (candidateZ-offset.z)*2; if (bd*bd < dist) dist = bd*bd;
					bd = (offset.x+size.x-candidateX)*2; if (bd*bd < dist) dist = bd*bd;
					bd = (offset.z+size.z-candidateZ)*2; if (bd*bd < dist) dist = bd*bd;

					//probability
					if (prob != null)
					{
						float probValue = prob.GetWorldInterpolatedValue(candidateX, candidateZ);
						dist *= probValue;
					}

					if (dist>bestDist) { bestDist=dist; bestCandidateX = candidateX; bestCandidateZ = candidateZ; }
				}

				if (bestDist>0.001f) //adding only if some suitable candidate found
				{
					Transition trs = new Transition(bestCandidateX, bestCandidateZ);
					posTab.Add(trs); 
				}
			}
			posTab.Flush();
		}
	}

	[System.Serializable] 
	[GeneratorMenu (menu=null, name ="Random", iconName="GeneratorIcons/Random", disengageable = true, 
		colorType = typeof(TransitionsList),
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/object_generators/Scatter")]
	public class Random200 : Random207,  IInlet<MatrixWorld>
	{
		//outdated, but has inlet and could not be removed
	}
}
