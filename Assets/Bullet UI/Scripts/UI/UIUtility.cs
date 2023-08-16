using UnityEngine;
using System;

namespace DuloGames.UI
{
	public static class UIUtility {
		
		/// <summary>
		/// Brings the game object to the front.
		/// </summary>
		/// <param name="go">Game Object.</param>
		public static void BringToFront(GameObject go)
		{
			Canvas canvas = UIUtility.FindInParents<Canvas>(go);
			
			// If the object has a parent canvas
			if (canvas != null)
				go.transform.SetParent(canvas.transform, true);
			
			// Set as last sibling
			go.transform.SetAsLastSibling();

            // Handle the always on top components
            if (canvas != null)
            {
                UIAlwaysOnTop[] alwaysOnTopComponenets = canvas.gameObject.GetComponentsInChildren<UIAlwaysOnTop>();

                if (alwaysOnTopComponenets.Length > 0)
                {
                    // Sort them by order
                    Array.Sort(alwaysOnTopComponenets);

                    foreach (UIAlwaysOnTop component in alwaysOnTopComponenets)
                    {
                        component.transform.SetAsLastSibling();
                    }
                }
            }
        }
		
		/// <summary>
		/// Finds the component in the game object's parents.
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="go">Game Object.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T FindInParents<T>(GameObject go) where T : Component
		{
			if (go == null)
				return null;
			
			var comp = go.GetComponent<T>();
			
			if (comp != null)
				return comp;
			
			Transform t = go.transform.parent;
			
			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
			
			return comp;
		}
	}
}
