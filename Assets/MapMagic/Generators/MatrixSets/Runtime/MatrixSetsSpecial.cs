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
	public enum PrototypeType { TerrainLayer, GrassTexture, GrassPrefab };


	[Serializable]
	[GeneratorMenu(
		menu = "Map Set", 
		name = "Add", 
		colorType = typeof(MatrixSet), 
		iconName="GeneratorIcons/MapSetAdd",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixSetGenerators/Add")]
	public class Add210 : Generator, IInlet<MatrixSet>, IMultiInlet, IOutlet<MatrixSet>
	{
		[Val("Add", "Inlet")]	public readonly Inlet<MatrixWorld> addIn = new Inlet<MatrixWorld>();
		public IEnumerable<IInlet<object>> Inlets () { yield return addIn; }

		public PrototypeType prototypeType = PrototypeType.TerrainLayer;
		public TerrainLayer terrainLayer = null;
		public Texture2D texture = null;
		public GameObject prefab = null;
		public int instanceNum = 0;
		public enum Mode { Set, Add, AddNormalize };
		public Mode mode = Mode.Add;
		//public bool normalize = false;
		public float opacity = 1;

		public MatrixSet.Prototype Prototype
		{get{
			switch (prototypeType)
			{
				case PrototypeType.TerrainLayer: return new MatrixSet.Prototype(terrainLayer, instanceNum);
				case PrototypeType.GrassPrefab: return new MatrixSet.Prototype(prefab, instanceNum);
				case PrototypeType.GrassTexture: default: return new MatrixSet.Prototype(texture, instanceNum);
			}
		}}


		public override void Generate (TileData data, StopToken stop) 
		{ 
			if (stop!=null && stop.stop) return;
			MatrixSet src = data.ReadInletProduct(this);

			MatrixWorld add = data.ReadInletProduct(addIn);
			if (!enabled || (texture==null && terrainLayer==null && prefab==null) || add==null)
			{
				data.StoreProduct(this, src);
				return;
			}

			if (opacity < 0.99f  ||  opacity > 1.01f)
			{
				add = new MatrixWorld(add);
				add.Multiply(opacity);
			}

			if (stop!=null && stop.stop) return;
			MatrixSet dst;
			if (src==null) dst = new MatrixSet(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			else dst = new MatrixSet(src);

			if (mode == Mode.Set)
				dst[Prototype] = add;
			else
				dst.Append(add, Prototype, normalized:mode==Mode.AddNormalize);

			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, dst);
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Map Set", 
		name = "Pick", 
		colorType = typeof(MatrixSet), 
		iconName="GeneratorIcons/MapSetPick",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixSetGenerators/Pick")]
	public class Pick210 : Generator, IInlet<MatrixSet>, IOutlet<MatrixWorld>
	{
		[Val("Add", "Inlet")]	public readonly Inlet<MatrixWorld> addIn = new Inlet<MatrixWorld>();
		public IEnumerable<IInlet<object>> Inlets () { yield return addIn; }

		public PrototypeType prototypeType = PrototypeType.TerrainLayer;

		public TerrainLayer terrainLayer = null;
		public Texture2D texture = null;
		public GameObject prefab = null;
		public int instanceNum = 0;
		public bool createIfNotExists = true;
		public float opacity = 1;

		public MatrixSet.Prototype Prototype
		{get{
			switch (prototypeType)
			{
				case PrototypeType.TerrainLayer: return new MatrixSet.Prototype(terrainLayer, instanceNum);
				case PrototypeType.GrassPrefab: return new MatrixSet.Prototype(prefab, instanceNum);
				case PrototypeType.GrassTexture: default: return new MatrixSet.Prototype(texture, instanceNum);
			}
		}}

		public override void Generate (TileData data, StopToken stop) 
		{ 
			if (stop!=null && stop.stop) return;
			MatrixSet src = data.ReadInletProduct(this);
			if (src == null) return;

			Matrix pick = src[Prototype];
			if (pick == null) //prototype not assigned
			{
				if (createIfNotExists) pick = new Matrix(data.area.full.rect);
				else return;
			}

			MatrixWorld pickW = new MatrixWorld(pick, src.worldPos, src.worldSize);

			if (opacity < 0.99f  ||  opacity > 1.01f)
			{
				pickW = new MatrixWorld(pickW);
				pickW.Multiply(1f/opacity); //opacity value inverted in pick
			}

			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, pickW);
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Map Set", 
		name = "Create", 
		colorType = typeof(MatrixSet), 
		iconName="GeneratorIcons/MapSetCreate",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixSetGenerators/Create")]
	public class Create210 : Generator, IMultiInlet, IOutlet<MatrixSet>
	{
		public class Layer : IInlet<MatrixWorld>
		{
			public PrototypeType prototypeType = PrototypeType.TerrainLayer;
			public TerrainLayer terrainLayer = null;
			public Texture2D texture = null;
			public GameObject prefab = null;
			public int instanceNum = 0;
			public float Opacity { get; set; }

			public Generator Gen { get; private set; }
			public void SetGen (Generator gen) => Gen=gen;
			public Layer (Generator gen) { this.Gen = gen; }
			public Layer () { Opacity = 1; }

			public ulong id; //properties not serialized
			public ulong Id { get{return id;} set{id=value;} } 
			public ulong LinkedOutletId { get; set; }  //if it's inlet. Assigned every before each clear or generate
			public ulong LinkedGenId { get; set; } 

			public IUnit ShallowCopy() => (Layer)this.MemberwiseClone();

			public MatrixSet.Prototype Prototype
			{get{
				switch (prototypeType)
				{
					case PrototypeType.TerrainLayer: return new MatrixSet.Prototype(terrainLayer, instanceNum);
					case PrototypeType.GrassPrefab: return new MatrixSet.Prototype(prefab, instanceNum);
					case PrototypeType.GrassTexture: default: return new MatrixSet.Prototype(texture, instanceNum);
				}
			}}

			public UnityEngine.Object Object
			{get{
				if (prototypeType==PrototypeType.TerrainLayer) return texture;
				if (prototypeType==PrototypeType.GrassTexture) return texture;
				else return prefab;
			}}
		}

		public Layer[] layers = new Layer[0];
		public Layer[] Layers => layers; 
		public void SetLayers(object[] ls) => layers = Array.ConvertAll(ls, i=>(Layer)i);
		public int guiExpanded;

		public IEnumerable<IInlet<object>> Inlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i];
		}


		public enum Normalize { None, Sum, Layered }
		public Normalize normalize = Normalize.None;

		public override void Generate (TileData data, StopToken stop) 
		{ 
			if (!enabled || layers.Length == 0) return;
			
			//reading/copying products
			MatrixWorld[] dstMatrices = new MatrixWorld[layers.Length];
			float[] opacities = new float[layers.Length];

			if (stop!=null && stop.stop) return;
			for (int i=0; i<layers.Length; i++)
			{
				if (stop!=null && stop.stop) return;

				MatrixWorld srcMatrix = data.ReadInletProduct(layers[i]);

				if (normalize == Normalize.None)
					dstMatrices[i] = srcMatrix;

				else
				{
					if (srcMatrix != null) dstMatrices[i] = new MatrixWorld(srcMatrix);
					else dstMatrices[i] = new MatrixWorld(data.area.full.rect, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize);
				}

				opacities[i] = layers[i].Opacity;
			}

			//normalizing
			if (stop!=null && stop.stop) return;
			if (normalize != Normalize.None)
			{
				dstMatrices.FillNulls(() => new MatrixWorld(data.area.full.rect, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize));
				
				if (normalize == Normalize.Sum)
					Matrix.NormalizeLayers(dstMatrices, opacities);
				
				if (normalize == Normalize.Layered)
				{
					dstMatrices[0].Fill(1);
					Matrix.BlendLayers(dstMatrices, opacities);
				}
			}

			//assembling
			if (stop!=null && stop.stop) return;
			MatrixSet set = new MatrixSet(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			for (int i=0; i<layers.Length; i++)
				set.Append(dstMatrices[i], layers[i].Prototype, normalized:false);
				//no need to normalize since it's already was normalized

			//storing products
			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, set);
		}
	}


	[Serializable]
	[GeneratorMenu(
		menu = "Map Set", 
		name = "Combine", 
		colorType = typeof(MatrixSet), 
		iconName="GeneratorIcons/MapSetCombine",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/-/wikis/MatrixSetGenerators/Combine")]
	public class Combine210 : Generator, IMultiInlet, IOutlet<MatrixSet>
	{
		public class Layer : IInlet<MatrixSet>
		{
			public Generator Gen { get; private set; }
			public void SetGen (Generator gen) => Gen=gen;
			public Layer (Generator gen) { this.Gen = gen; }
			public Layer () { } //for editor

			public ulong id; //properties not serialized
			public ulong Id { get{return id;} set{id=value;} } 
			public ulong LinkedOutletId { get; set; }  //if it's inlet. Assigned every before each clear or generate
			public ulong LinkedGenId { get; set; } 

			public IUnit ShallowCopy() => (Layer)this.MemberwiseClone();
		}

		public Layer[] layers = new Layer[0];
		public Layer[] Layers => layers; 
		public void SetLayers(object[] ls) => layers = Array.ConvertAll(ls, i=>(Layer)i);

		public IEnumerable<IInlet<object>> Inlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i];
		}

		public bool normalize = false;


		public override void Generate (TileData data, StopToken stop) 
		{ 
			if (!enabled || layers.Length == 0) return;

			//gathering all matrix and prototypes
			List<Matrix> matricesList = new List<Matrix>();
			List<MatrixSet.Prototype> prototypesList = new List<MatrixSet.Prototype>();

			if (stop!=null && stop.stop) return;
			foreach (Layer layer in layers)
			{
				MatrixSet layerSet = data.ReadInletProduct(layer);
				if (layerSet == null) continue;

				for (int m=0; m<layerSet.Count; m++)
				{
					matricesList.Add(layerSet.GetMatrixByNum(m));
					prototypesList.Add(layerSet.GetPrototypeByNum(m));
				}
			}

			Matrix[] matrices = matricesList.ToArray();
			MatrixSet.Prototype[] prototypes = prototypesList.ToArray();

			//copying to normalize
			if (normalize)
				for (int m=0; m<matrices.Length; m++)
					matrices[m] = new Matrix(matrices[m]);

			//normalizing (including copy)
			if (normalize)
				Matrix.NormalizeLayers(matrices);
				
			//assembling new set
			MatrixSet set = new MatrixSet(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			for (int i=0; i<layers.Length; i++)
				set.Append(matrices[i], prototypes[i], normalized:false);
				//no need to normalize since it's already was normalized
			
			if (stop!=null && stop.stop) return;
			data.StoreProduct(this, set);
		}
	}
}