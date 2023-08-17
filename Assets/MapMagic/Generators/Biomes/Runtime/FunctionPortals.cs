using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using MapMagic.Products;

namespace MapMagic.Nodes
{
	[GeneratorMenu (menu = "Functions/Enter", menuName = "Map", name = "Enter", iconName = "GeneratorIcons/FunctionIn", lookLikePortal=true)]  public class MatrixFnEnter : FnEnter<Den.Tools.Matrices.MatrixWorld> { }
	[GeneratorMenu (menu = "Functions/Exit", menuName = "Map", name = "Exit", iconName = "GeneratorIcons/FunctionOut", lookLikePortal=true)]  public class MatrixFnExit : FnExit<Den.Tools.Matrices.MatrixWorld> { }
	[GeneratorMenu (menu = "Functions/Enter", menuName = "Objects", name = "Enter", iconName = "GeneratorIcons/FunctionIn", lookLikePortal=true)] public class ObjectsFnEnter : FnEnter<TransitionsList> { }
	[GeneratorMenu (menu = "Functions/Exit", menuName = "Objects", name = "Exit", iconName = "GeneratorIcons/FunctionOut", lookLikePortal=true)]  public class ObjectsFnExit : FnExit<TransitionsList> { }
	//[GeneratorMenu (menu = "Functions/Enter", menuName = "Spline", name = "Enter", iconName = "GeneratorIcons/FunctionIn", lookLikePortal=true)] public class SplineFnEnter : FnEnter<Den.Tools.Segs.SplineSys> { }
	//[GeneratorMenu (menu = "Functions/Exit",  menuName = "Spline", name = "Exit", iconName = "GeneratorIcons/FunctionOut", lookLikePortal=true)] public class SplineFnExit : FnExit<Den.Tools.Segs.SplineSys> { }
	[GeneratorMenu (menu = "Functions/Enter", menuName = "Spline", name = "Enter", iconName = "GeneratorIcons/FunctionIn", lookLikePortal=true)] public partial class SplineFunctionInput : FnEnter<Den.Tools.Splines.SplineSys> { }
	[GeneratorMenu (menu = "Functions/Exit",  menuName = "Spline", name = "Exit", iconName = "GeneratorIcons/FunctionOut", lookLikePortal=true)] public partial class SplineFunctionOutput : FnExit<Den.Tools.Splines.SplineSys> { }
	//no way to initialize portals in modules since they require FnEnter and FnExit class

	
	//gui interfaces
	//public interface IFnPortal<out T>
	//{ 
	//	string Name { get; set; }
	//}

	//public interface IFnEnter<out T> : IFnPortal<T>, IOutlet<T>  where T: class { }  //to use objects of type IFnEnter<object>
	//public interface IFnExit<out T> : IFnPortal<T>, IInlet<T>, IRelevant where T: class { } //fnExit is always generated (should be IRelevant)
	//interfaces required in draw editor, so they are stored in portals.cs, not module


	[Serializable]
	public class FnEnter<T> : Generator, IFnEnter<T>, IOutlet<T>, IRelevant where T: class
	{
		[Val("Name")]	public string name = "Input";
		public string Name { get{return name;} set{name=value;} }

		public override void Generate (TileData data, StopToken stop) {}
	}


	[Serializable]
	public class FnExit<T> : Generator, IInlet<T>, IOutlet<T>, IFnExit<T>, IRelevant where T: class
	{
		[Val("Name")]	public string name = "Output";
		public string Name { get{return name;} set{name=value;} }

		public override void Generate (TileData data, StopToken stop) 
		{
			if (stop!=null && stop.stop) return;  

			//just passing link products to this
			object product = data.ReadInletProduct(this);
			data.StoreProduct(this, product);
		}
	}
}
