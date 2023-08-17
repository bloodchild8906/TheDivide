using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using MapMagic.Products;
using MapMagic.Expose;


namespace MapMagic.Nodes.Biomes
{
	public interface IFnLayer<out T> : IUnit where T:class
	{
		string Name { get; set; }
		IFnPortal<T> GetInternalPortal (Graph graph);
	}

	public interface IFnInlet<out T> : IFnLayer<T>, IInlet<T> where T:class {}

	public interface IFnOutlet<out T> : IFnLayer<T>, IOutlet<T> where T:class {}


	[Serializable]
	public class FnInlet<T> : IFnInlet<T> where T: class
	{
		[SerializeField] private string name; //properties not serialized
		public string Name { get=>name; set=>name=value; }

		[SerializeField] private Generator gen;
		public Generator Gen { get=>gen; private set=>gen=value; }
		public void SetGen (Generator gen) => Gen=gen;

		public ulong id;
		public ulong Id { get{return id;} set{id=value;} }
		public ulong LinkedOutletId { get; set; }  //if it's inlet. Assigned every before each clear or generate
		public ulong LinkedGenId { get; set; } 

		public IUnit ShallowCopy() => (Generator)this.MemberwiseClone();

		public FnInlet () { }


		public IFnPortal<T> GetInternalPortal (Graph graph)
		{
			foreach (IFnEnter<T> fnInput in graph.GeneratorsOfType<IFnEnter<T>>())
				if (fnInput.Name == Name) 
					return fnInput;
			return null;
		}
	}


	[Serializable]
	public class FnOutlet<T> : IFnOutlet<T> where T:class
	{
		[SerializeField] private string name; //properties not serialized
		public string Name { get=>name; set=>name=value; }

		[SerializeField] private Generator gen;
		public Generator Gen { get=>gen; private set=>gen=value; }
		public void SetGen (Generator gen) => Gen=gen;

		public ulong id;
		public ulong Id { get{return id;} set{id=value;} } 

		public IUnit ShallowCopy() => (FnOutlet<T>)this.MemberwiseClone();

		public FnOutlet () { }


		public IFnPortal<T> GetInternalPortal (Graph graph)
		{
			foreach (IFnExit<T> fnInput in graph.GeneratorsOfType<IFnExit<T>>())
				if (fnInput.Name == Name) 
					return fnInput;
			return null;
		}
	}

}