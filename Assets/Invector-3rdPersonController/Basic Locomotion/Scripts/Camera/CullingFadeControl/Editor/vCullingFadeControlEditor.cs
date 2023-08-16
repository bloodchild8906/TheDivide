using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Invector
{
    [CustomEditor(typeof(vCullingFadeControl))]
    public class vCullingFadeControlEditor : Editor
    {
        GUISkin skin;
        vCullingFadeControl _fadeControl { get { return (vCullingFadeControl)target; } }
        private Texture2D m_Logo = null;

        void OnEnable()
        {
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
        }

        void OnSceneGUI()
        {
            if (!_fadeControl.targetObject) return;
            var tpos = (_fadeControl.targetObject.position + _fadeControl.offset);
            Handles.color = new Color(1, 1, 1, 0.5f);            
            Handles.SphereHandleCap(0, tpos, Quaternion.identity, 0.1f, EventType.Ignore);
            Ray ray = new Ray(tpos, _fadeControl.cameraTransform.position - tpos);
            Handles.DrawLine(tpos, ray.GetPoint(_fadeControl.distanceToEndFade));
            Handles.color = (_fadeControl.distanceToEndFade < _fadeControl.distanceToStartFade) ? Color.green : Color.red;
            Handles.DrawAAPolyLine(10f, new Vector3[] { ray.GetPoint(_fadeControl.distanceToEndFade), ray.GetPoint(_fadeControl.distanceToStartFade) });
            Handles.CubeHandleCap(0, ray.GetPoint(_fadeControl.distanceToEndFade), Quaternion.LookRotation(_fadeControl.cameraTransform.position - ray.GetPoint(_fadeControl.distanceToEndFade)), 0.05f, EventType.Ignore);
            Handles.ConeHandleCap(0, ray.GetPoint(_fadeControl.distanceToStartFade), Quaternion.LookRotation(ray.GetPoint(_fadeControl.distanceToStartFade) - _fadeControl.cameraTransform.position), 0.1f, EventType.Ignore);
            Handles.color = new Color(1, 1, 1, 0.5f);
            Handles.DrawLine(ray.GetPoint(_fadeControl.distanceToStartFade), _fadeControl.cameraTransform.position);
            Handles.DrawPolyLine(new Vector3[] { ray.GetPoint(_fadeControl.distanceToEndFade), ray.GetPoint(_fadeControl.distanceToStartFade) });
        }

        public override void OnInspectorGUI()
        {
            if (!skin) skin = Resources.Load("vSkin") as GUISkin;
            GUI.skin = skin;

            GUILayout.BeginVertical("Culling Fade", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(10);

            base.OnInspectorGUI();
            if (!Application.isPlaying)
                CheckRenderers();
            GUILayout.BeginHorizontal("box", GUILayout.ExpandHeight(false));

            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            if (_fadeControl.fadeMeshRenderers != null && _fadeControl.fadeMeshRenderers.Count > 0)
            {
                for (int a = 0; a < _fadeControl.fadeMeshRenderers.Count; a++)
                {
                    EditorGUILayout.ObjectField("Renderer", _fadeControl.fadeMeshRenderers[a].renderer, typeof(Renderer), true);
                    var renderer = _fadeControl.fadeMeshRenderers[a].renderer as MeshRenderer;
                    GUILayout.BeginHorizontal();
                    
                    GUILayout.BeginVertical("window", GUILayout.ExpandHeight(false));

                    for (int b = 0; b < renderer.sharedMaterials.Length; b++)
                        renderer.sharedMaterials[b] = (Material)EditorGUILayout.ObjectField(renderer.sharedMaterials[b], typeof(Material), false);

                    GUILayout.EndVertical();
                    
                    GUILayout.BeginVertical("window", GUILayout.ExpandHeight(false));

                    for (int b = 0; b < _fadeControl.fadeMeshRenderers[a].fadeMaterials.Length; b++)
                        _fadeControl.fadeMeshRenderers[a].fadeMaterials[b] = (Material)EditorGUILayout.ObjectField(_fadeControl.fadeMeshRenderers[a].fadeMaterials[b], typeof(Material), false);

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }

            if (_fadeControl.fadeSkinnedMeshRenderes != null && _fadeControl.fadeSkinnedMeshRenderes.Count > 0)
            {
                for (int a = 0; a < _fadeControl.fadeSkinnedMeshRenderes.Count; a++)
                {
                    EditorGUILayout.ObjectField("Renderer", _fadeControl.fadeSkinnedMeshRenderes[a].renderer, typeof(Renderer), true);
                    var renderer = _fadeControl.fadeSkinnedMeshRenderes[a].renderer as SkinnedMeshRenderer;
                    GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
                    
                    GUILayout.BeginVertical("box", GUILayout.ExpandHeight(false));
                    GUILayout.Label("Original Material");
                    for (int b = 0; b < renderer.sharedMaterials.Length; b++)
                        renderer.sharedMaterials[b] = (Material)EditorGUILayout.ObjectField(renderer.sharedMaterials[b], typeof(Material), false);

                    GUILayout.EndVertical();
                    
                    GUILayout.BeginVertical("box", GUILayout.ExpandHeight(false));
                    GUILayout.Label("Optional Fade");

                    for (int b = 0; b < _fadeControl.fadeSkinnedMeshRenderes[a].fadeMaterials.Length; b++)
                        _fadeControl.fadeSkinnedMeshRenderes[a].fadeMaterials[b] = (Material)EditorGUILayout.ObjectField(_fadeControl.fadeSkinnedMeshRenderes[a].fadeMaterials[b], typeof(Material), false);

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.HelpBox("Your material has to be Transparent or Fade, you will also need to change the Z-write (check documentation for more information)", MessageType.Info);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if (GUI.changed)
                EditorUtility.SetDirty(_fadeControl);
        }

        [MenuItem("Invector/Basic Locomotion/Components/Culling Fade")]
        static void MenuComponent()
        {
            Selection.activeGameObject.AddComponent<vCullingFadeControl>();
        }

        public void CheckRenderers()
        {
            if (_fadeControl.targetObject == null) return;
            var meshRenderers = _fadeControl.targetObject.GetComponentsInChildren<MeshRenderer>(true);

            if (_fadeControl.fadeMeshRenderers == null)
                _fadeControl.fadeMeshRenderers = new List<FadeMaterials>();

            if (_fadeControl.fadeMeshRenderers.Count != meshRenderers.Length)
            {
                _fadeControl.fadeMeshRenderers.Resize(meshRenderers.Length);
                EditorUtility.SetDirty(_fadeControl);
            }

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (_fadeControl.fadeMeshRenderers[i] == null)
                    _fadeControl.fadeMeshRenderers[i] = new FadeMaterials();
                if (_fadeControl.fadeMeshRenderers[i].renderer == null || (_fadeControl.fadeMeshRenderers[i].renderer != null && _fadeControl.fadeMeshRenderers[i].renderer != (meshRenderers[i] as Renderer)))
                    _fadeControl.fadeMeshRenderers[i].renderer = meshRenderers[i] as Renderer;
                if (_fadeControl.fadeMeshRenderers[i].originalMaterials == null || (_fadeControl.fadeMeshRenderers[i].originalMaterials != null && _fadeControl.fadeMeshRenderers[i].originalMaterials != meshRenderers[i].sharedMaterials))
                    _fadeControl.fadeMeshRenderers[i].originalMaterials = meshRenderers[i].sharedMaterials;
                if (_fadeControl.fadeMeshRenderers[i].fadeMaterials == null)
                    _fadeControl.fadeMeshRenderers[i].fadeMaterials = new Material[meshRenderers[i].sharedMaterials.Length];
                else if (_fadeControl.fadeMeshRenderers[i].fadeMaterials.Length != meshRenderers[i].sharedMaterials.Length)
                    Array.Resize(ref _fadeControl.fadeMeshRenderers[i].fadeMaterials, meshRenderers[i].sharedMaterials.Length);
            }
                        
            var skinnedRenderers = _fadeControl.targetObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (_fadeControl.fadeSkinnedMeshRenderes == null)
                _fadeControl.fadeSkinnedMeshRenderes = new List<FadeMaterials>();
            if (_fadeControl.fadeSkinnedMeshRenderes.Count != skinnedRenderers.Length)
            {
                _fadeControl.fadeSkinnedMeshRenderes.Resize(skinnedRenderers.Length);
                EditorUtility.SetDirty(_fadeControl);
            }

            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                if (_fadeControl.fadeSkinnedMeshRenderes[i] == null)
                    _fadeControl.fadeSkinnedMeshRenderes[i] = new FadeMaterials();
                if (_fadeControl.fadeSkinnedMeshRenderes[i].renderer == null || (_fadeControl.fadeSkinnedMeshRenderes[i].renderer != null && _fadeControl.fadeSkinnedMeshRenderes[i].renderer != (skinnedRenderers[i] as Renderer)))
                    _fadeControl.fadeSkinnedMeshRenderes[i].renderer = skinnedRenderers[i] as Renderer;
                if (_fadeControl.fadeSkinnedMeshRenderes[i].originalMaterials == null || (_fadeControl.fadeSkinnedMeshRenderes[i].originalMaterials != null && _fadeControl.fadeSkinnedMeshRenderes[i].originalMaterials != skinnedRenderers[i].sharedMaterials))
                    _fadeControl.fadeSkinnedMeshRenderes[i].originalMaterials = skinnedRenderers[i].sharedMaterials;
                if (_fadeControl.fadeSkinnedMeshRenderes[i].fadeMaterials == null)
                    _fadeControl.fadeSkinnedMeshRenderes[i].fadeMaterials = new Material[skinnedRenderers[i].sharedMaterials.Length];
                else if (_fadeControl.fadeSkinnedMeshRenderes[i].fadeMaterials.Length != skinnedRenderers[i].sharedMaterials.Length)
                    Array.Resize(ref _fadeControl.fadeSkinnedMeshRenderes[i].fadeMaterials, skinnedRenderers[i].sharedMaterials.Length);
            }
        }

        #region Control Zwrite of Standard Material
        [MenuItem("Assets/Change Zwrite of Standard Material")]
        private static void ChangeZWRITE()
        {
            var material = Selection.activeObject as Material;
            if (material != null)
            {
                if (material.GetInt("_ZWrite") == 0)
                    material.SetInt("_ZWrite", 1);
                else
                    material.SetInt("_ZWrite", 0);
            }
        }

        [MenuItem("Assets/Change Zwrite of Standard Material", true)]
        private static bool ValidateChangeZWRITE()
        {
            var material = Selection.activeObject as Material;

            if (material != null)
            {
                if (material.HasProperty("_ZWrite") && material.HasProperty("_Mode"))
                    return true;
            }
            return false;
        }
        #endregion
    }

    public static class ListExtras
    {
        //    list: List<T> to resize
        //    size: desired new size
        // element: default value to insert

        public static void Resize<T>(this List<T> list, int size, T element = default(T))
        {
            int count = list.Count;

            if (size < count)
                list.RemoveRange(size, count - size);
            else if (size > count)
            {
                if (size > list.Capacity)   // Optimization
                    list.Capacity = size;

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }
    }
}