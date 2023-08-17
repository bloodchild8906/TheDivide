using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.Splines;
using Den.Tools.Matrices;
using Den.Tools.GUI;
using MapMagic.Core;
using MapMagic.Products;

namespace MapMagic.Nodes.SplinesGenerators
{
	public abstract partial class Pathfinding
	{
		protected const float maxWeight = 1000000;
		protected const int maxIterations = 1000000;

		public float distanceFactor = 1;
		public float elevationFactor = 5;
		public float straightenFactor = 1;
		public float maxElevation = 10000;
	}


	public class FixedListPathfinding : Pathfinding
	{
		private class FixedList
		{
			public int[] arr;
			public int count;

			public FixedList (int capacity)	{ arr = new int[capacity]; }
			public FixedList (int[] arr) { this.arr = arr; }
		}

		private FixedList changedPoses = new FixedList(10000);
		private FixedList newChangedPoses = new FixedList(10000); //to swap with changedcoords each iteration


		public Coord[] FindPathDijkstra (Coord from, Coord to, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs)
		/// Returns null if path could not be found (in manhattan dist * 2 cells)
		{
			weights.Fill(maxWeight+1);
			dirs.Fill(new Coord());

			//calculating weights
			weights[to] = 0;

			int fromPos = heights.rect.GetPos(from);
			int toPos = heights.rect.GetPos(to);

			changedPoses.arr[0] = toPos;
			changedPoses.count = 1;

			int maxIterations = Coord.DistanceManhattan(from, to) * 2;
			int counter = 0;
			bool pathFound = false;
			Coord min = heights.rect.Min; Coord max = heights.rect.Max;
			for (int i=0; i<maxIterations; i++)
			{
				for (int c=0; c<changedPoses.count; c++)
				{
					int pos = changedPoses.arr[c];
					Coord coord = heights.rect.GetCoord(pos);
					CalcNearWeights(coord, heights, weights, dirs, changedPosesFixedList:newChangedPoses);
					counter ++;
				}

				if (weights.arr[fromPos] < maxWeight) { pathFound = true; break; }

				FixedList tempList = changedPoses;
				changedPoses = newChangedPoses;
				newChangedPoses = tempList;
				newChangedPoses.count = 0;
			}
			if (!pathFound) return null;

			//drawing path
			List<Coord> path = new List<Coord>();
			path.Add(from);
			Coord curr = from;
			for (int i=0; i<1000; i++)
			{
				curr -= dirs[curr];
				path.Add(curr);

				if (curr == to) break;
			}

			/*for (int i=0; i<path.Count; i++)
				weights[path[i]] = -1;
			weights.Multiply(0.005f);
			DebugGizmos.ToMatrixPreviewCopyMainthread(weights);*/

			return path.ToArray(); 
		}


		private void CalcNearWeights (Coord coord, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs, FixedList changedPosesFixedList=null)
		{
			Coord rectMin = heights.rect.offset; Coord rectMax = heights.rect.offset + heights.rect.size;
			if (coord.x < rectMin.x  ||  coord.x > rectMax.x-1  ||
				coord.z < rectMin.z  ||  coord.z > rectMax.z-1) return;

			float pixelSize = heights.worldSize.x/heights.rect.size.x; //heights.PixelSize.x; 

			int pos = (coord.z-heights.rect.offset.z)*heights.rect.size.x + coord.x - heights.rect.offset.x;
			float thisHeight = heights.arr[pos];
			float thisWeight = weights.arr[pos];
			if (thisWeight > maxWeight) return;

			for (int nx=-1; nx<=1; nx++)
				for (int nz=-1; nz<=1; nz++)
				{
					if (nx==0 && nz==0) continue;

					if ((coord.x==rectMin.x && nx==-1) || (coord.x==rectMax.x-1 && nx==1) ||
						(coord.z==rectMin.z && nz==-1) || (coord.z==rectMax.z-1 && nz==1)) continue;

					int nPos = pos + heights.rect.size.x*nz + nx;
					float nWeight = weights.arr[nPos];
					float nNewWeight = CalcWeight(pos, nx, nz, heights, weights, dirs);

					if (nNewWeight < nWeight) 
					{
						weights.arr[nPos] = nNewWeight;
						dirs.arr[nPos].x = nx;
						dirs.arr[nPos].z = nz;

						if (changedPosesFixedList!=null) 
						{
							changedPosesFixedList.arr[changedPosesFixedList.count] = nPos;
							changedPosesFixedList.count++;
						}
					}
				}
		}


	}


	public class ListPathfinding : Pathfinding
	/// Keeps the list of changed cell coordinates. The simplest implementation.
	{

		public Coord[] FindPathAstarFastList (Coord from, Coord to, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs)
		{
int statIterations = 0;
int statDistEvaluations = 0;
int statDistChecks = 0;

			Matrix priorities = new Matrix(heights.rect);
			priorities.Fill(-1);

			weights.Fill(maxWeight+1);
			dirs.Fill(new Coord());

			//calculating weights
			weights[to] = 0;
			priorities[to] = Coord.DistanceManhattan(from, to) * distanceFactor;

			List<int> changedPoses = new List<int>(10000);
			int fromPos = heights.rect.GetPos(from);
			int toPos = heights.rect.GetPos(to);
			changedPoses.Add(toPos);
			Coord closestCoord = new Coord();

			Coord min = heights.rect.Min; Coord max = heights.rect.Max;
			for (int i=0; i<maxIterations; i++)
			{
				//finding theoretically closest coord in changed
				int closestNum = 0;
				float closestPriority = float.MaxValue;

				int changedPosesCount = changedPoses.Count;
				for (int p=0; p<changedPosesCount; p++)
				{
					int pos = changedPoses[p];
					if (pos<0) continue;

					float priority = priorities.arr[pos];
					if (priority < 0)
					{
						Coord coord = heights.rect.GetCoord(pos);
						priority = Coord.DistanceManhattan(from, coord)*distanceFactor  +  weights.arr[pos];
						priorities.arr[pos] = priority;
						statDistEvaluations++;
					}

					if (priority < closestPriority)
					{
						closestPriority = priority;
						closestNum = p;
					}

					statDistChecks++;
				}

				closestCoord = heights.rect.GetCoord( changedPoses[closestNum] );
				changedPoses[closestNum] = -1;

				CalcNearWeights(closestCoord, heights, weights, dirs, changedPosesList:changedPoses);
				statIterations++;

				//if (changedPoses.Contains(fromPos)) { Debug.Log("AstarFastList " + i); break; }
				if (weights.arr[fromPos] < maxWeight) break; 
			}

			Debug.Log("AstarFastList iterations:" + statIterations + 
				" distChacks:" + statDistChecks + 
				" distEvaluations:" + statDistEvaluations + 
				" changedPosesCount:" + changedPoses.Count);

			//drawing path
			List<Coord> path = new List<Coord>();
			path.Add(from);
			Coord curr = from;
			for (int i=0; i<1000; i++)
			{
				curr -= dirs[curr];
				path.Add(curr);

				if (curr == to) break;
			}

			//debugging path
			//weights.Multiply(0.005f);
			//for (int i=0; i<path.Count; i++)
			//	weights[path[i]] = 2;
			//DebugGizmos.ToMatrixPreviewCopyMainthread(weights);

			//debugging changed poses
			Matrix weightsCpy = new Matrix(weights);
			weightsCpy.Multiply(0.01f);
			weightsCpy.arr[fromPos] = 0f;
			weightsCpy.arr[toPos] = 0f;
			weightsCpy[closestCoord] = 0.999f;
			for (int j=0; j<changedPoses.Count; j++)
			{
				if (changedPoses[j] < 0) continue;
				weightsCpy.arr[changedPoses[j]] = -1;
			}
			//weightsCpy[closestCoord] = 0.999f;
			DebugGizmos.ToMatrixPreview(weightsCpy);

			return path.ToArray(); 
		}





		private void CalcNearWeights (Coord coord, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs, List<int> changedPosesList=null)
		{
			if (coord.x <= heights.rect.offset.x  ||  coord.x >= heights.rect.offset.x+heights.rect.size.x-1  ||
				coord.z <= heights.rect.offset.z  ||  coord.z >= heights.rect.offset.z+heights.rect.size.z-1) return;

			float pixelSize = heights.worldSize.x/heights.rect.size.x; //heights.PixelSize.x; 

			int pos = (coord.z-heights.rect.offset.z)*heights.rect.size.x + coord.x - heights.rect.offset.x;
			float thisHeight = heights.arr[pos];
			float thisWeight = weights.arr[pos];
			if (thisWeight > maxWeight) return;

			for (int nx=-1; nx<=1; nx++)
				for (int nz=-1; nz<=1; nz++)
				{
					if (nx==0 && nz==0) continue;

					int nPos = pos + heights.rect.size.x*nz + nx;
					float nWeight = weights.arr[nPos];
					float nNewWeight = CalcWeight(pos, nx, nz, heights, weights, dirs);


					if (nNewWeight < nWeight) 
					{
						weights.arr[nPos] = nNewWeight;
						dirs.arr[nPos].x = nx;
						dirs.arr[nPos].z = nz;

						if (changedPosesList!=null && !changedPosesList.Contains(nPos))
						{
							int changedPosesCount = changedPosesList.Count;
							bool added = false;
							for (int c=0; c<changedPosesCount; c++)
								if (changedPosesList[c]<0)
								{
									changedPosesList[c] = nPos;
									added = true;
								}
							if (!added) changedPosesList.Add(nPos);
						}
					}
				}
		}
	}


	public class HashSetPathfinding : Pathfinding
	{
		public Coord[] FindPathAstar (Coord from, Coord to, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs)
		{
			//Matrix priorities = new Matrix(heights.rect);
			//priorities.Fill(-1);

			weights.Fill(maxWeight+1);
			dirs.Fill(new Coord());

			//calculating weights
			weights[to] = 0;
			//priorities[to] = Coord.DistanceManhattan(from, to) * distanceFactor;

			HashSet<Coord> changedCoords = new HashSet<Coord>();
			changedCoords.Add(to);

			Coord min = heights.rect.Min; Coord max = heights.rect.Max;
			for (int i=0; i<100000000; i++)
			{
				//finding theoretically closest coord in changed
				Coord closestCoord = to;
				float closestPriority = float.MaxValue;
				foreach (Coord c in changedCoords)
				{
					int pos = (c.z-heights.rect.offset.z)*heights.rect.size.x + c.x - heights.rect.offset.x;
					float priority = Coord.DistanceManhattan(from, c) * distanceFactor  +  weights.arr[pos]; // priorities[c];

					if (priority < closestPriority)
					{
						closestPriority = priority;
						closestCoord.x = c.x;
						closestCoord.z = c.z;

					}
				}

				Coord coord = closestCoord;
				changedCoords.Remove(closestCoord);

				CalcNearWeights(coord, heights, weights, dirs, changedCoords);

				if (changedCoords.Contains(from)) { Debug.Log("Astar " + i); break; }
			}


			//drawing path
			List<Coord> path = new List<Coord>();
			path.Add(from);
			Coord curr = from;
			for (int i=0; i<1000; i++)
			{
				curr -= dirs[curr];
				path.Add(curr);

				if (curr == to) break;
			}

			for (int i=0; i<path.Count; i++)
				weights[path[i]] = -1;
			weights.Multiply(0.005f);

			return path.ToArray(); 
		}


		public Coord[] FindPathDijkstraHashSet (Coord from, Coord to, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs)
		{
			weights.Fill(maxWeight+1);
			dirs.Fill(new Coord());

			//calculating weights
			weights[to] = 0;

			HashSet<Coord> changedCoords = new HashSet<Coord>();
			changedCoords.Add(to);

			HashSet<Coord> newChangedCoords = new HashSet<Coord>(); //to swap with changedcoords each iteration

			int counter = 0;
			Coord min = heights.rect.Min; Coord max = heights.rect.Max;
			for (int i=0; i<1000; i++)
			{
				int changedCoordsCount = changedCoords.Count;
				foreach (Coord c in changedCoords)
				{
					CalcNearWeights(c, heights, weights, dirs, newChangedCoords);
					counter ++;
				}

				if (changedCoords.Contains(from)) break;

				HashSet<Coord> tmp = changedCoords;
				changedCoords = newChangedCoords;
				newChangedCoords = tmp;
				newChangedCoords.Clear();
			}

			Debug.Log("DijkstraHashSet " + counter);

			//drawing path
			List<Coord> path = new List<Coord>();
			path.Add(from);
			Coord curr = from;
			for (int i=0; i<1000; i++)
			{
				curr -= dirs[curr];
				path.Add(curr);

				if (curr == to) break;
			}

			for (int i=0; i<path.Count; i++)
				weights[path[i]] = -1;
			weights.Multiply(0.005f);
			DebugGizmos.ToMatrixPreviewCopyMainthread(weights);

			return path.ToArray(); 
		}


		public Coord[] FindPathDijkstraListTemp (Coord from, Coord to, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs)
		{
			weights.Fill(maxWeight+1);
			dirs.Fill(new Coord());

			//calculating weights
			weights[to] = 0;

			HashSet<Coord> changedCoords = new HashSet<Coord>();
			changedCoords.Add(to);

			HashSet<Coord> newChangedCoords = new HashSet<Coord>(); //to swap with changedcoords each iteration

			Coord min = heights.rect.Min; Coord max = heights.rect.Max;
			for (int i=0; i<1000; i++)
			{
				int changedCoordsCount = changedCoords.Count;

				for (int j=0; j<changedCoordsCount; j++)
				{
					Coord closestCoord = to;
					foreach (Coord c in changedCoords)
						{ closestCoord = c; break; }

					CalcNearWeights(closestCoord, heights, weights, dirs, newChangedCoords);
					changedCoords.Remove(closestCoord);

					if (changedCoords.Count==0) break;
					if (newChangedCoords.Contains(from)) break;
				}

				if (changedCoords.Contains(from)) break;

				HashSet<Coord> tmp = changedCoords;
				changedCoords = newChangedCoords;
				newChangedCoords = tmp;
				newChangedCoords.Clear();
			}


			//drawing path
			List<Coord> path = new List<Coord>();
			path.Add(from);
			Coord curr = from;
			for (int i=0; i<1000; i++)
			{
				curr -= dirs[curr];
				path.Add(curr);

				if (curr == to) break;
			}

			for (int i=0; i<path.Count; i++)
				weights[path[i]] = -1;
			weights.Multiply(0.005f);
			DebugGizmos.ToMatrixPreviewCopyMainthread(weights);

			return path.ToArray(); 
		}



		private void CalcNearWeights (Coord coord, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs, HashSet<Coord> changedCoordsHash=null)
		{
			if (coord.x <= heights.rect.offset.x  ||  coord.x >= heights.rect.offset.x+heights.rect.size.x-1  ||
				coord.z <= heights.rect.offset.z  ||  coord.z >= heights.rect.offset.z+heights.rect.size.z-1) return;

			float pixelSize = heights.worldSize.x/heights.rect.size.x; //heights.PixelSize.x; 

			int pos = (coord.z-heights.rect.offset.z)*heights.rect.size.x + coord.x - heights.rect.offset.x;
			float thisHeight = heights.arr[pos];
			float thisWeight = weights.arr[pos];
			if (thisWeight > maxWeight) return;

			for (int nx=-1; nx<=1; nx++)
				for (int nz=-1; nz<=1; nz++)
				{
					if (nx==0 && nz==0) continue;

					int nPos = pos + heights.rect.size.x*nz + nx;
					float nWeight = weights.arr[nPos];
					float nNewWeight = CalcWeight(pos, nx, nz, heights, weights, dirs);


					if (nNewWeight < nWeight) 
					{
						weights.arr[nPos] = nNewWeight;
						dirs.arr[nPos].x = nx;
						dirs.arr[nPos].z = nz;
					}
				}
		}
	
	}


	public class CellPathfinding : Pathfinding
	/// Uses the matrix of combined values (weight, dir, etc)
	{
		private struct PathCell
		{
			public float weight;
			public Coord dir;
			public bool active;
			public float weightLeft;
		}


		public Coord[] FindPathAstar (Coord from, Coord to, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs)
		{
			Matrix2D<PathCell> cells = new Matrix2D<PathCell>(heights.rect);

			for (int i=0; i<cells.arr.Length; i++)
			{
				cells.arr[i].weight = maxWeight+1;
				cells.arr[i].dir.x = 0;
				cells.arr[i].dir.z = 0;
				cells.arr[i].active = false;
			}

			Coord min = heights.rect.Min; Coord max = heights.rect.Max;

			for (int i=0; i<1000; i++)
			{
				//finding closest active cell both to from and to
				Coord closestCoord = new Coord();
				float closestSum = float.MaxValue;

				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
				{
					int pos = (z-heights.rect.offset.z)*heights.rect.size.x + x - heights.rect.offset.x;
					if (!cells.arr[pos].active) continue;

					float sum = cells.arr[pos].weight + cells.arr[pos].weightLeft;
					if (sum < closestSum)
					{
						closestSum = sum;
						closestCoord.x = x;
						closestCoord.z = z;
					}
				}

				//calculating weight to neigbour cells
				CalcNearWeights(closestCoord, heights, cells, null, from);

				if (cells[from].active) break;
			}

			//drawing path
			List<Coord> path = new List<Coord>();
			path.Add(from);
			Coord curr = from;
			for (int i=0; i<1000; i++)
			{
				curr -= cells[curr].dir;
				path.Add(curr);

				if (curr == to) break;
			}

			return path.ToArray(); 
		}


		public Coord[] FindPathDijkstra (Coord from, Coord to, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs)
		{
			Matrix2D<PathCell> cells = new Matrix2D<PathCell>(heights.rect);

			for (int i=0; i<cells.arr.Length; i++)
			{
				cells.arr[i].weight = maxWeight+1;
				cells.arr[i].dir.x = 0;
				cells.arr[i].dir.z = 0;
				cells.arr[i].active = false;
			}

			var toCell = cells[to];
			toCell.active = true;
			toCell.weight = 0;
			cells[to] = toCell;

			Coord min = heights.rect.Min; Coord max = heights.rect.Max;
			for (int i=0; i<1000; i++)
			{
				Matrix2D<PathCell> newCells = new Matrix2D<PathCell>(cells);

				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
				{
					int pos = (z-heights.rect.offset.z)*heights.rect.size.x + x - heights.rect.offset.x;
					if (!cells.arr[pos].active) continue;

					CalcNearWeights(new Coord(x,z), heights, cells, newCells, from);
				}

				cells = newCells;
			}

			//drawing path
			List<Coord> path = new List<Coord>();
			path.Add(from);
			Coord curr = from;
			for (int i=0; i<1000; i++)
			{
				curr -= cells[curr].dir;
				path.Add(curr);

				if (curr == to) break;
			}

			//for (int i=0; i<cells.arr.Length; i++)
			//	weights.arr[i] = cells.arr[i].weight * 0.001f;
			//DebugGizmos.ToMatrixPreviewCopyMainthread(weights);

			return path.ToArray(); 
		}


		private void CalcNearWeights (Coord coord, MatrixWorld heights, Matrix2D<PathCell> cells,  Matrix2D<PathCell> newCells, Coord from)
		{
			int pos = (coord.z-heights.rect.offset.z)*heights.rect.size.x + coord.x - heights.rect.offset.x;
			cells.arr[pos].active = false;

			if (coord.x <= heights.rect.offset.x  ||  coord.x >= heights.rect.offset.x+heights.rect.size.x-1  ||
				coord.z <= heights.rect.offset.z  ||  coord.z >= heights.rect.offset.z+heights.rect.size.z-1) return;

			float pixelSize = heights.worldSize.x/heights.rect.size.x; //heights.PixelSize.x; 

			float thisHeight = heights.arr[pos];
			float thisWeight = cells.arr[pos].weight;
			if (thisWeight > maxWeight) return;

			for (int nx=-1; nx<=1; nx++)
				for (int nz=-1; nz<=1; nz++)
				{
					if (nx==0 && nz==0) continue;

					int nPos = pos + heights.rect.size.x*nz + nx;
					float nWeight = cells.arr[nPos].weight;
					float nHeight = heights.arr[nPos];


					//calculating factors
					float diagonalFactor = 1;
					if (nx*nz!=0) diagonalFactor = 1.414213562373f;

					float elevation = nHeight - thisHeight;
					if (elevation < 0) elevation = -elevation;
					elevation *= heights.worldSize.y;
					elevation = elevation/(pixelSize*diagonalFactor)*0.8f + elevation/diagonalFactor*0.2f;

					float xDirDelta = cells.arr[pos].dir.x-nx;
					float zDirDelta = cells.arr[pos].dir.z-nz;
					float dirDelta = xDirDelta*xDirDelta + zDirDelta*zDirDelta;


					//passability
					bool passable = elevation <= maxElevation;


					//new weight
					float nNewWeight;
					if (passable)
						nNewWeight = thisWeight + 
							diagonalFactor*distanceFactor +
							elevation*elevation*elevationFactor +
							dirDelta*straightenFactor;
					else
						nNewWeight = maxWeight+1;


					if (nNewWeight < nWeight) 
					{
						newCells.arr[nPos].weight = nNewWeight;
						newCells.arr[nPos].active = true;
						newCells.arr[nPos].dir.x = nx;
						newCells.arr[nPos].dir.z = nz;
						newCells.arr[nPos].weightLeft = Coord.DistanceManhattan(from, new Coord(coord.x+nx, coord.z+nz)) * distanceFactor;
					}
				}
		}
	}


	public partial class Pathfinding
	{
		protected float CalcWeight (int pos, int nx, int nz, MatrixWorld heights, Matrix weights, Matrix2D<Coord> dirs)
		{
			int nPos = pos + heights.rect.size.x*nz + nx;
			float nWeight = weights.arr[nPos];
			float nHeight = heights.arr[nPos];
			float pixelSize = heights.worldSize.x/heights.rect.size.x;

			//calculating factors
			float diagonalFactor = 1;
			if (nx*nz!=0) diagonalFactor = 1.414213562373f;

			float elevation = nHeight - heights.arr[pos]; //thisHeight;
			if (elevation < 0) elevation = -elevation;
			elevation *= heights.worldSize.y;
			elevation = elevation/(pixelSize*diagonalFactor)*0.8f + elevation/diagonalFactor*0.2f;

			float xDirDelta = dirs.arr[pos].x-nx;
			float zDirDelta = dirs.arr[pos].z-nz;
			float dirDelta = xDirDelta*xDirDelta + zDirDelta*zDirDelta;


			//passability
			bool passable = elevation <= maxElevation;


			//new weight
			float nNewWeight;
			if (passable)
				nNewWeight = weights.arr[pos] + 
					diagonalFactor*distanceFactor +
					elevation*elevation*elevationFactor +
					dirDelta*straightenFactor;
			else
				nNewWeight = maxWeight+1;

			return nNewWeight;
		}
	}
}
