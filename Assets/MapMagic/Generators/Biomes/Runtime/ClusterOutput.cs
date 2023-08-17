using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools.Matrices;
using Den.Tools;
using Den.Tools.Splines;

public class ClusterOutput : MonoBehaviour
{
	public class Result
	{
		public string name;
		public uint nodeId;


		public MatrixWorld matrix;
		public ObjectsPool objects;
		public SplineSys splines;

		public object Object 
		{
			get{
				if (matrix != null) return matrix;
				if (objects != null) return objects;
				if (splines != null) return splines;

				var tmp = new Den.Tools.Noise(1, 2);

				return null;
			}

			set{
				if (value is MatrixWorld m) { matrix = m; objects = null; splines = null; }
				else if (value is ObjectsPool o) { objects = o; matrix = null; splines = null; }
				else if (value is SplineSys s) { splines = s; matrix = null; splines = null; }
			}

			
		}
	}


	public Result[] results;


}
