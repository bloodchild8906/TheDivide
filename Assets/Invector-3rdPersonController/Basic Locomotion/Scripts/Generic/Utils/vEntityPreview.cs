
using UnityEngine;
namespace Invector.Utils
{
    [vClassHeader("Entity Preview")]
    public sealed class vEntityPreview : vMonoBehaviour
    {

#if UNITY_EDITOR
        public Color previewColor = new Color(1, 1, 1, 0.25f);
        public bool drawBase = true;
        [ vHideInInspector("drawBase")] public float baseRadius = 1;
        public bool useCustomMesh;
        [vHideInInspector("useCustomMesh")] public Mesh previewMesh;
        public bool showInPlayMode;
        #region Static
        static Mesh defaultPreviewMesh;
        static Material defaultPreviewMaterial;
        static Mesh sphereBaseMesh;
        static void LoadDefaultMaterial()
        {
            if (!defaultPreviewMaterial)
            {
                var defaultMat = (Material)Resources.Load("EntityPreview/EntityPreview", typeof(Material));
                defaultPreviewMaterial = defaultMat;
            }
        }
        static void LoadDefaultMesh()
        {
           
            if (!defaultPreviewMesh)
            {
                var defaultMesh = (Mesh)Resources.Load("EntityPreview/EntityPreview", typeof(Mesh));
                defaultPreviewMesh = defaultMesh;
            }
        }
        static void LoadSphereBase()
        {
            if (!sphereBaseMesh)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereBaseMesh = go.GetComponent<MeshFilter>().sharedMesh;
                DestroyImmediate(go);
            }
        }
        #endregion
      
        void OnDrawGizmos()
        {
            if (Application.isPlaying && !showInPlayMode) return;
               
            if (!useCustomMesh)
            {                   
                LoadDefaultMesh();
                if(previewMesh!=defaultPreviewMesh) previewMesh = defaultPreviewMesh;
            }
            if (previewMesh)
            {
                if (defaultPreviewMaterial)
                {
                    Gizmos.color = Color.clear;
                    Gizmos.DrawCube(transform.position + Vector3.up * 1, new Vector3(1, 2, 1));
                    if (drawBase)
                    {
                        if (sphereBaseMesh)
                        {
                            Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position + Vector3.up * 0.01f, transform.rotation, new Vector3(1, 0.0f, 1) * (baseRadius));
                            defaultPreviewMaterial.color = previewColor;
                            defaultPreviewMaterial.SetColor("_TintColor", previewColor);
                            for (int pass = 0; pass < defaultPreviewMaterial.passCount; pass++)
                            {
                                if (defaultPreviewMaterial.SetPass(pass))
                                {
                                    Graphics.DrawMeshNow(sphereBaseMesh, cubeTransform);
                                }
                            }
                        }
                        else
                        {
                            LoadSphereBase();
                        }
                    }
                    defaultPreviewMaterial.color = previewColor;
                    defaultPreviewMaterial.SetColor("_TintColor", previewColor);

                    for (int pass = 0; pass < defaultPreviewMaterial.passCount; pass++)
                    {

                        if (defaultPreviewMaterial.SetPass(pass))
                        {
                            Graphics.DrawMeshNow(previewMesh, transform.localToWorldMatrix);
                        }
                    }

                    Gizmos.color = Color.white;
                }
                else
                {
                    LoadDefaultMaterial();
                }
            }

        }
#endif
    }

}
