using System.Collections.Generic;
using UnityEngine;
namespace Invector.vItemManager
{
    [vClassHeader("Master Window", openClose = false)]
    public class vMasterWindow : vMonoBehaviour
    {
        [vHelpBox("Window that always opens when this window is activated")]
        public GameObject mainWindow;
        public bool sequenceWindows;
        [vReadOnly]
        public vMasterWindow parentWindow;
        [vReadOnly]
        public GameObject currentWindow;
        [vReadOnly]
        public List<GameObject> windows;
        public UnityEngine.Events.UnityEvent onEnable, onDisable;

        protected virtual void OnDisable()
        {
            GetParentWindow();
            if (parentWindow)
            {
                //parentWindow.RemoveWindow(gameObject);
            }

            //if (gameObject.activeSelf) gameObject.SetActive(false);

            CloseAllMasterWindows();
            onDisable.Invoke();
        }

        protected virtual void GetParentWindow()
        {
            if (!parentWindow) parentWindow = transform.parent.GetComponentInParent<vMasterWindow>();
        }

        protected virtual void OnEnable()
        {
            GetParentWindow();
            if (parentWindow)
                parentWindow.SetCurrentWindow(gameObject);

            if (windows.Count == 0 && mainWindow) SetCurrentWindow(mainWindow);
            onEnable.Invoke();
        }

        public virtual void RemoveWindow(GameObject window)
        {
            if (!windows.Contains(window) || window == mainWindow) return;
            if (!sequenceWindows || currentWindow == window)
                windows.Remove(window);

            currentWindow = null;
            if (sequenceWindows && windows.Count > 0)
            {
                currentWindow = windows[windows.Count - 1];
                if (!currentWindow.activeSelf) currentWindow.SetActive(true);
            }
            if (windows.Count == 0 && mainWindow) SetCurrentWindow(mainWindow);
        }

        public virtual void SetCurrentWindow(GameObject window)
        {
            if (currentWindow == window)
            {
                if (!currentWindow.activeSelf) currentWindow.SetActive(true);
                return;
            }
            if (!windows.Contains(window))
            {
                windows.Add(window);
            }
            if (!sequenceWindows && currentWindow)
            {
                windows.Remove(currentWindow);
                if (currentWindow.activeSelf) currentWindow.SetActive(false);
            }
            currentWindow = window;
            if (!currentWindow.activeSelf) currentWindow.SetActive(true);

        }

        public virtual void CloseAllMasterWindows()
        {
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].SetActive(false);
            }
            windows.Clear();
            if(mainWindow)
            {
                mainWindow.SetActive(true);
            }
        }      
    }
}