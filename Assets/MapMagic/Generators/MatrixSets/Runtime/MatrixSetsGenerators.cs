using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Products;

namespace MapMagic.Nodes.MatrixSetsGenerators
{
	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Curve", iconName="GeneratorIcons/Curve", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Curve")]
	public class Curve200 : Generator, IInlet<MatrixSet>, IOutlet<MatrixSet> 
	{
		public Curve curve = new Curve( new Vector2(0,0), new Vector2(1,1) );   

		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixSet src = data.ReadInletProduct(this);
			if (!enabled) { data.StoreProduct(this, src); return; }

			curve.Refresh(updateLut:true);

			MatrixSet dst = new MatrixSet(src);

			for (int i=0; i<dst.Count; i++)
			{
				if (stop!=null && stop.stop) return;
				dst[i].UniformCurve(curve.lut);
			}

			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, dst);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Levels", iconName="GeneratorIcons/Levels", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Levels")]
	public class Levels200 : Generator, IInlet<MatrixSet>, IOutlet<MatrixSet> 
	{
		public float inMin = 0;
		public float inMax = 1;
		public float gamma = 1f; //min/max bias. 0 for min 2 for max, 1 is straight curve

		public float outMin = 0;
		public float outMax = 1;

		public bool guiParams = false;


		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixSet src = data.ReadInletProduct(this);
			if (!enabled) { data.StoreProduct(this, src); return; }

			if (stop!=null && stop.stop) return;
			MatrixSet dst = new MatrixSet(src);

			for (int i=0; i<dst.Count; i++)
			{
				if (stop!=null && stop.stop) return;
				dst[i].Levels(inMin, inMax, gamma, outMin, outMax);
			}

			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, dst);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Contrast", iconName="GeneratorIcons/Contrast", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Contrast")]
	public class Contrast200 : Generator, IInlet<MatrixSet>, IOutlet<MatrixSet> 
	{
		[Val(name="Intensity")] public float brightness = 0f;
		[Val(name="Contrast")] public float contrast = 1f;


		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixSet src = data.ReadInletProduct(this);
			if (!enabled) { data.StoreProduct(this, src); return; }

			if (stop!=null && stop.stop) return;
			MatrixSet dst = new MatrixSet(src);

			for (int i=0; i<dst.Count; i++)
			{
				if (stop!=null && stop.stop) return;
				dst[i].BrighnesContrast(brightness, contrast);
			}

			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, dst);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Unity Curve", iconName="GeneratorIcons/UnityCurve", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/UnityCurve")]
	public class UnityCurve200 : Generator, IMultiInlet, IOutlet<MatrixSet> 
	{
		[Val("Inlet", "Inlet")] public readonly IInlet<MatrixSet> srcIn = new Inlet<MatrixSet>();
		[Val("Mask", "Inlet")]	public readonly IInlet<MatrixWorld> maskIn = new Inlet<MatrixWorld>();
		public IEnumerable<IInlet<object>> Inlets() { yield return srcIn; yield return maskIn; }

		public AnimationCurve curve = new AnimationCurve( new Keyframe[] { new Keyframe(0,0,1,1), new Keyframe(1,1,1,1) } );
		public Vector2 min = new Vector2(0,0);
		public Vector2 max = new Vector2(1,1);

		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixSet src = data.ReadInletProduct(srcIn);
			MatrixWorld mask = data.ReadInletProduct(maskIn);
			if (src == null) return; 
			if (!enabled) { data.StoreProduct(this, src); return; }

			//preparing output
			if (stop!=null && stop.stop) return;
			MatrixSet dst = new MatrixSet(src);

			//curve
			for (int i=0; i<dst.Count; i++)
			{
				if (stop!=null && stop.stop) return;
				AnimCurve c = new AnimCurve(curve);
				float[] arr = dst[i].arr;
				for (int p=0; p<arr.Length; p++) 
					arr[p] = c.Evaluate(arr[p]);
			}

			//mask
			if (stop!=null && stop.stop) return;
			if (mask != null) 
			{
				for (int i=0; i<dst.Count; i++)
					dst[i].InvMix(src[i],mask);
			}

			data.StoreProduct(this, dst);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Mask", iconName="GeneratorIcons/MapMask", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Mask")]
	public class Mask200 : Generator, IMultiInlet, IOutlet<MatrixSet> 
	{
		[Val("Input A", "Inlet")]	public readonly Inlet<MatrixSet> aIn = new Inlet<MatrixSet>();
		[Val("Input B", "Inlet")]	public readonly Inlet<MatrixSet> bIn = new Inlet<MatrixSet>();
		[Val("Mask", "Inlet")]	public readonly Inlet<MatrixWorld> maskIn = new Inlet<MatrixWorld>();
		public IEnumerable<IInlet<object>> Inlets () { yield return aIn; yield return bIn; yield return maskIn; }

		[Val("Invert")]	public bool invert = false;


		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixSet setA = data.ReadInletProduct(aIn);
			MatrixSet setB = data.ReadInletProduct(bIn);
			MatrixWorld mask = data.ReadInletProduct(maskIn);
			if (setA == null || setB == null) return; 
			if (!enabled || mask == null) { data.StoreProduct(this, setA); return; }

			if (stop!=null && stop.stop) return;
			MatrixSet dst = new MatrixSet(setA);
			//dst.SyncPrototypes(setA.Prototypes); //already has prototypes
			dst.SyncPrototypes(setB.Prototypes);

			foreach (MatrixSet.Prototype prototype in dst.Prototypes)
			{
				Matrix matrixB = setB[prototype];
				if (matrixB == null)
					continue;

				dst[prototype].Mix(matrixB, mask, 0, 1, invert, false, 1);
			}

			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, dst);
		}
	}


	/*[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Blend", iconName="GeneratorIcons/Blend", disengageable = true, colorType = typeof(MatrixWorld), helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/blend")]
	public class Blend200 : Generator, IMultiInlet, IOutlet<MatrixWorld>
	{
		public class Layer
		{
			public readonly Inlet<MatrixWorld> inlet = new Inlet<MatrixWorld>();
			public BlendAlgorithm algorithm = BlendAlgorithm.add;
			public float opacity = 1;
			public bool guiExpanded = false;
		}

		public Layer[] layers = new Layer[] { new Layer(), new Layer() };
		public Layer[] Layers => layers; 
		public void SetLayers(object[] ls) => layers = Array.ConvertAll(ls, i=>(Layer)i);

		public IEnumerable<IInlet<object>> Inlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i].inlet;
		}

		public override void Generate (TileData data, StopToken stop) 
		{
			if (stop!=null && stop.stop) return;
			if (!enabled) return;
			MatrixWorld matrix = new MatrixWorld(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			
			if (stop!=null && stop.stop) return;

			if (stop!=null && stop.stop) return;
				for (int i = 0; i < layers.Length; i++)
			{
				Layer layer = layers[i];
				if (layer.inlet == null) continue;

				MatrixWorld blendMatrix = data.ReadInletProduct(layer.inlet);
				if (blendMatrix == null) continue;

				Blend(matrix, blendMatrix, layer.algorithm, layer.opacity);
			}
			
			data.StoreProduct(this, matrix);
		}


		public enum BlendAlgorithm {
			mix=0, 
			add=1, 
			subtract=2, 
			multiply=3, 
			divide=4, 
			difference=5, 
			min=6, 
			max=7, 
			overlay=8, 
			hardLight=9, 
			softLight=10} 
			
		public static void Blend (Matrix m1, Matrix m2, BlendAlgorithm algorithm, float opacity=1)
		{
			switch (algorithm)
			{
				case BlendAlgorithm.mix: default: m1.Mix(m2, opacity); break;
				case BlendAlgorithm.add: m1.Add(m2, opacity); break;
				case BlendAlgorithm.subtract: m1.Subtract(m2, opacity); break;
				case BlendAlgorithm.multiply: m1.Multiply(m2, opacity); break;
				case BlendAlgorithm.divide: m1.Divide(m2, opacity); break;
				case BlendAlgorithm.difference: m1.Difference(m2, opacity); break;
				case BlendAlgorithm.min: m1.Min(m2, opacity); break;
				case BlendAlgorithm.max: m1.Max(m2, opacity); break;
				case BlendAlgorithm.overlay: m1.Overlay(m2, opacity); break;
				case BlendAlgorithm.hardLight: m1.HardLight(m2, opacity); break;
				case BlendAlgorithm.softLight: m1.SoftLight(m2, opacity); break;
			}
		}
	}*/


	/*[System.Serializable]
	[GeneratorMenu (
		menu="Map/Modifiers", 
		name ="Normalize", 
		disengageable = true, 
		helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/normalize",
		iconName="GeneratorIcons/Normalize",
		drawInlets = false,
		drawOutlet = false,
		colorType = typeof(MatrixWorld))]
	public class Normalize200 : Generator, IMultiInlet, IMultiOutlet
	{
		public class NormalizeLayer : IInlet<MatrixWorld>, IOutlet<MatrixWorld>
		{
			public float Opacity { get; set; }

			public Generator Gen { get; private set; }
			public void SetGen (Generator gen) => Gen=gen;
			public NormalizeLayer (Generator gen) { this.Gen = gen; }
			public NormalizeLayer () { Opacity = 1; }

			public ulong id; //properties not serialized
			public ulong Id { get{return id;} set{id=value;} } 
			public ulong LinkedOutletId { get; set; }  //if it's inlet. Assigned every before each clear or generate
			public ulong LinkedGenId { get; set; } 

			public IUnit ShallowCopy() => (NormalizeLayer)this.MemberwiseClone();
		}

		public NormalizeLayer[] layers = new NormalizeLayer[0];
		public NormalizeLayer[] Layers => layers; 
		public void SetLayers(object[] ls) => layers = Array.ConvertAll(ls, i=>(NormalizeLayer)i);


		public IEnumerable<IInlet<object>> Inlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i];
		}

		public IEnumerable<IOutlet<object>> Outlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i];
		}

		public override void Generate (TileData data, StopToken stop)
		{
			if (layers.Length == 0) return;
			
			//reading/copying products
			MatrixWorld[] dstMatrices = new MatrixWorld[layers.Length];
			float[] opacities = new float[layers.Length];

			if (stop!=null && stop.stop) return;
			for (int i=0; i<layers.Length; i++)
			{
				if (stop!=null && stop.stop) return;

				MatrixWorld srcMatrix = data.ReadInletProduct(layers[i]);
				if (srcMatrix != null) dstMatrices[i] = new MatrixWorld(srcMatrix);
				else dstMatrices[i] = new MatrixWorld(data.area.full.rect, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize);

				opacities[i] = layers[i].Opacity;
			}

			//normalizing
			if (stop!=null && stop.stop) return;
			dstMatrices.FillNulls(() => new MatrixWorld(data.area.full.rect, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize));
			dstMatrices[0].Fill(1);
			Matrix.BlendLayers(dstMatrices, opacities);

			//saving products
			if (stop!=null && stop.stop) return;
			for (int i=0; i<layers.Length; i++)
				data.StoreProduct(layers[i], dstMatrices[i]);
		}
	}*/


	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Blur", iconName="GeneratorIcons/Blur", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Blur")]
	public class Blur200 : Generator, IInlet<MatrixSet>, IOutlet<MatrixSet>
	{
		[Val("Downsample")] public float downsample = 10f;
		[Val("Blur")] public float blur = 3f;

		public override void Generate (TileData data, StopToken stop)
		{
			MatrixSet src = data.ReadInletProduct(this);
			if (src == null) return; 
			if (!enabled) { data.StoreProduct(this, src); return; }

			MatrixSet dst = new MatrixSet(src);

			int rrDownsample = (int)(downsample / Mathf.Sqrt(dst.PixelSize.x));
			float rrBlur = blur / dst.PixelSize.x;

			for (int m=0; m<src.Count; m++)
			{
				if (stop!=null && stop.stop) return;

				if (rrDownsample > 1)
					MatrixOps.DownsampleBlur(src[m], dst[m], rrDownsample, rrBlur);
				else
					MatrixOps.GaussianBlur(src[m], dst[m], rrBlur);
			}

			data.StoreProduct(this, dst);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Cavity", iconName="GeneratorIcons/Cavity", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Cavity")]
	public class Cavity200 : Generator, IInlet<MatrixSet>, IOutlet<MatrixSet>
	{
		[Val("Type")]		public MatrixGenerators.Cavity200.CavityType type = MatrixGenerators.Cavity200.CavityType.Convex;
		[Val("Intensity")]	public float intensity = 3;
		[Val("Spread")]		public float spread = 10; //actually the pixel size (in world units) of the lowerest mipmap. Same for draft and main

		public override void Generate (TileData data, StopToken stop)
		{
			MatrixSet src = data.ReadInletProduct(this);
			if (src == null) return; 
			if (!enabled) { data.StoreProduct(this, src); return; }

			if (stop!=null && stop.stop) return;
			MatrixSet dst = new MatrixSet(src.rect, src.worldPos, src.worldSize, src.Prototypes); //empty matrices, not copy

			for (int m=0; m<src.Count; m++)
			{
				if (stop!=null && stop.stop) return;
				MatrixGenerators.Cavity200.Cavity(src[m], dst[m], type, intensity, spread, src.PixelSize.x, data.area.active.worldSize, stop);
			}
			
			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, dst);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Slope", iconName="GeneratorIcons/Slope", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Slope")]
	public class Slope200 : Generator, IInlet<MatrixSet>, IOutlet<MatrixSet>
	{
		[Val("From")]			public float from = 30;
		[Val("To")]				public float to = 90;
		[Val("Smooth Range")]	public float range = 30f;
		
		public override void Generate (TileData data, StopToken stop)
		{
			MatrixSet src = data.ReadInletProduct(this);
			if (src==null) return; 
			if (!enabled) { data.StoreProduct(this, src); return; }

			MatrixSet dst = new MatrixSet(src.rect, src.worldPos, src.worldSize, src.Prototypes); //empty matrices, not copy
			
			for (int m=0; m<src.Count; m++)
			{
				if (stop!=null && stop.stop) return;
				dst[m] = MatrixGenerators.Slope200.Slope(src[m], src.worldPos, src.worldSize, data.globals.height, from, to, range);
			}

			data.StoreProduct(this, dst);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Selector", iconName="GeneratorIcons/Selector", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Selector")]
	public class Selector200 : Generator, IInlet<MatrixSet>, IOutlet<MatrixSet>
	{
		public enum RangeDet { Transition, MinMax}
		public RangeDet rangeDet = RangeDet.Transition;
		public enum Units { Map, World }
		public Units units = Units.Map;
		public Vector2 from = new Vector2(0.4f, 0.6f);
		public Vector2 to = new Vector2(1f, 1f);
		
		public override void Generate (TileData data, StopToken stop)
		{
			MatrixSet src = data.ReadInletProduct(this);
			if (src==null) return; 
			if (!enabled) { data.StoreProduct(this, src); return; }

			if (stop!=null && stop.stop) return;
			MatrixSet dst = new MatrixSet(src);

			for (int m=0; m<src.Count; m++)
			{
				if (stop!=null && stop.stop) return;
				MatrixWorld dstW = new MatrixWorld(dst[m], dst.worldPos, dst.worldSize);
				MatrixGenerators.Selector200.Select(dstW, from, to, inWorldUnits:units==Units.World, worldHeight:data.globals.height);
			}

			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, dst);
		}
	}


	[System.Serializable]
	[GeneratorMenu (
		menu="Map Set", 
		name ="Terrace", 
		iconName="GeneratorIcons/Terrace", 
		disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Terrace")]
	public class Terrace200 : Generator, IInlet<MatrixSet>, IOutlet<MatrixSet>
	{
		[Val("Seed")]		 public int seed = 12345;
		[Val("Num")]		 public int num = 10;
		[Val("Uniformity")] public float uniformity = 0.5f;
		[Val("Steepness")]	 public float steepness = 0.5f;
		//[Val("Intensity")]	 public float intensity = 1f;

		
		public override void Generate (TileData data, StopToken stop)
		{
			MatrixSet src = data.ReadInletProduct(this);
			if (src == null || num <= 1) return; 
			if (!enabled) { data.StoreProduct(this, src); return; }

			MatrixSet dst = new MatrixSet(src); 
			
			for (int m=0; m<src.Count; m++)
			{
				if (stop!=null && stop.stop) return;
				float[] terraceLevels = MatrixGenerators.Terrace200.TerraceLevels(new Noise(data.random,seed), num, uniformity);
			
				if (stop!=null && stop.stop) return;
				dst[m].Terrace(terraceLevels, steepness);
			}

			data.StoreProduct(this, dst);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map Set", name ="Erosion", iconName="GeneratorIcons/Erosion", disengageable = true, 
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Erosion")]
	public class Erosion200 : Generator, IInlet<MatrixSet>, IOutlet<MatrixSet>, ICustomComplexity
	{
		[Val("Iterations")]		 public int iterations = 3;
		[Val("Durability")] public float terrainDurability=0.9f;
		//[Val("Erosion")]	 
			public float erosionAmount=1f;
		[Val("Sediment")]	 public float sedimentAmount=0.75f;
		[Val("Fluidity")] public int fluidityIterations=3;
		[Val("Relax")]		public float relax=0.0f;

		public float Complexity {get{ return iterations*2; }}
		public float Progress (TileData data) { return data.GetProgress(this); }


		public override void Generate (TileData data, StopToken stop)
		{
			MatrixSet src = data.ReadInletProduct(this);
			if (src == null) return; 
			if (!enabled || iterations <= 0) { data.StoreProduct(this, src); return; }

			MatrixSet dst = new MatrixSet(src);

			for (int m=0; m<src.Count; m++)
			{
				if (stop!=null && stop.stop) return;
				MatrixWorld dstW = new MatrixWorld(dst[m], dst.worldPos, dst.worldSize);
				MatrixGenerators.Erosion200.Erosion(dstW, data.isDraft, data, iterations, terrainDurability, erosionAmount, sedimentAmount, fluidityIterations, relax, this, stop);
			}

			data.StoreProduct(this, dst);
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Map Set", 
		name = "Parallax", 
		section=2, 
		colorType = typeof(MatrixSet), 
		iconName="GeneratorIcons/Parallax",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixGenerators/Parallax")]
	public class Parallax210 : Generator, IInlet<MatrixSet>, IMultiInlet, IOutlet<MatrixSet>
	{
		[Val("Intensity X", "Inlet")]	public readonly Inlet<MatrixWorld> intensityInX = new Inlet<MatrixWorld>();
		[Val("Intensity Z", "Inlet")]	public readonly Inlet<MatrixWorld> intensityInZ = new Inlet<MatrixWorld>();
		public virtual IEnumerable<IInlet<object>> Inlets () { yield return intensityInX; yield return intensityInZ; }

		[Val("Offset")] public Vector2D offset;
		public enum Interpolation { None, Always, OnTransitions }
		[Val("Interpolation")] public Interpolation interpolation = Interpolation.Always;

		public override void Generate (TileData data, StopToken stop) 
		{ 
			if (stop!=null && stop.stop) return;
			MatrixSet maps = data.ReadInletProduct(this);
			if (maps == null) return;
			if (!enabled) { data.StoreProduct(this,maps); return; }

			MatrixWorld intensityX = data.ReadInletProduct(intensityInX);
			MatrixWorld intensityZ = data.ReadInletProduct(intensityInZ);

			if (stop!=null && stop.stop) return;
			Vector2D pixelOffset = offset / (Vector2D)data.area.full.PixelSize;
			MatrixSet results = new MatrixSet(maps.rect, maps.worldPos, maps.worldSize, maps.Prototypes);

			for (int m=0; m<maps.Count; m++)
			{
				if (stop!=null && stop.stop) return;
				results.GetMatrixByNum(m).Parallax(pixelOffset, maps.GetMatrixByNum(m), intensityX, intensityZ, (int)interpolation);
			}

			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, results);
		}
	}
}
