using Invector.vCamera;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Invector.vCharacterController
{
    public class vCreateBasicCharacterEditor : EditorWindow
    {
        GUISkin skin;
        public GameObject template;
        public bool useGameController = true;
        public GameObject charObj;
        Animator charAnimator;

        Vector2 rect = new Vector2(500, 590);
        Editor humanoidpreview;
        Texture2D m_Logo;

        /// <summary>
        /// 3rdPersonController Menu 
        /// </summary>    
        [MenuItem("Invector/Basic Locomotion/Create Basic Controller", false, 0)]
        public static void CreateNewCharacter()
        {
            GetWindow<vCreateBasicCharacterEditor>();
        }

        bool isHuman, isValidAvatar, charExist;
        public virtual void OnEnable()
        {
            m_Logo = Resources.Load("icon_v2") as Texture2D;
            if (Selection.activeObject)
            {
                charObj = Selection.activeGameObject;
            }
            if (charObj)
            {
                charAnimator = charObj.GetComponent<Animator>();
                humanoidpreview = Editor.CreateEditor(charObj);
            }

            charExist = charAnimator != null;
            isHuman = charExist ? charAnimator.isHuman : false;
            isValidAvatar = charExist ? charAnimator.avatar.isValid : false;
        }

        public virtual void OnGUI()
        {
            if (!skin)
            {
                skin = Resources.Load("vSkin") as GUISkin;
            }

            GUI.skin = skin;

            this.minSize = rect;
            this.titleContent = new GUIContent("Character", null, "Third Person Character Creator");
            GUILayout.BeginVertical("Character Creator Window", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(5);

            GUILayout.BeginVertical("box");

            if (!charObj)
            {
                EditorGUILayout.HelpBox("Make sure your FBX model is set as Humanoid!", MessageType.Info);
            }
            else if (!charExist)
            {
                EditorGUILayout.HelpBox("Missing a Animator Component", MessageType.Error);
            }
            else if (!isHuman)
            {
                EditorGUILayout.HelpBox("This is not a Humanoid", MessageType.Error);
            }
            else if (!isValidAvatar)
            {
                EditorGUILayout.HelpBox(charObj.name + " is a invalid Humanoid", MessageType.Info);
            }

            template = EditorGUILayout.ObjectField("Template", template, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
            charObj = EditorGUILayout.ObjectField("FBX Model", charObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("--- Optional---");

            useGameController = EditorGUILayout.Toggle("Add GameController", useGameController);

            if (GUI.changed && charObj != null && charObj.GetComponent<vThirdPersonController>() == null)
            {
                humanoidpreview = Editor.CreateEditor(charObj);
            }

            if (charObj != null && charObj.GetComponent<vThirdPersonController>() != null)
            {
                EditorGUILayout.HelpBox("This gameObject already contains the component vThirdPersonController", MessageType.Warning);
            }

            GUILayout.EndVertical();

            //GUILayout.BeginHorizontal("box");
            //EditorGUILayout.LabelField("Need to know how it works?");
            //if (GUILayout.Button("Video Tutorial"))
            //{
            //    Application.OpenURL("https://www.youtube.com/watch?v=KQ5xha36tfE&index=1&list=PLvgXGzhT_qehtuCYl2oyL-LrWoT7fhg9d");
            //}
            //GUILayout.EndHorizontal();

            if (charObj)
            {
                charAnimator = charObj.GetComponent<Animator>();
            }

            charExist = charAnimator != null;
            isHuman = charExist ? charAnimator.isHuman : false;
            isValidAvatar = charExist ? charAnimator.avatar.isValid : false;

            if (CanCreate())
            {
                DrawHumanoidPreview();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create"))
                {
                    Create();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        public virtual bool CanCreate()
        {
            return isValidAvatar && isHuman && charObj != null && charObj.GetComponent<vThirdPersonController>() == null;
        }

        /// <summary>
        /// Draw the Preview window
        /// </summary>
        public virtual void DrawHumanoidPreview()
        {
            GUILayout.FlexibleSpace();

            if (humanoidpreview != null)
            {
                humanoidpreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100, 400), "window");
            }
        }

        private GameObject InstantiateNewCharacter(GameObject selected)
        {
            if (selected == null)
            {
                return selected;
            }

            if (selected.scene.IsValid())
            {
                return selected;
            }

            return PrefabUtility.InstantiatePrefab(selected) as GameObject;
        }

        /// <summary>
        /// Created the Third Person Controller
        /// </summary>
        public virtual void Create()
        {
            // base for the character
            GameObject newCharacter = InstantiateNewCharacter(charObj);

            if (!newCharacter)
            {
                return;
            }

            GameObject _template = Instantiate(template, newCharacter.transform.position, newCharacter.transform.rotation);
            Transform modelParent = _template.transform.Find("3D Model");

            if (modelParent == null)
            {
                modelParent = new GameObject("3D Model").transform;
                modelParent.parent = _template.transform;
            }

            newCharacter.transform.parent = modelParent;
            newCharacter.transform.localPosition = Vector3.zero;
            newCharacter.transform.localEulerAngles = Vector3.zero;
            _template.name = "vBasicController_" + charObj.gameObject.name;

            Animator animatorController = newCharacter.GetComponent<Animator>();
            Animator animatorTemplate = _template.GetComponent<Animator>();

            animatorTemplate.avatar = animatorController.avatar;
            animatorTemplate.Rebind();
            DestroyImmediate(animatorController);

            newCharacter.tag = "Player";

            var p_layer = LayerMask.NameToLayer("Player");
            newCharacter.layer = p_layer;

            foreach (Transform t in newCharacter.transform.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = p_layer;
            }

            Selection.activeGameObject = _template;

            // search for a MainCamera and attach to the tpCamera
            var mainCamera = Camera.main;
            var tpCamera = _template.GetComponentInChildren<vThirdPersonCamera>();

            if (mainCamera == null)
            {
                mainCamera = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
                mainCamera.tag = "MainCamera";
            }

            if (mainCamera.transform.parent != tpCamera.transform)
            {
                mainCamera.transform.parent = tpCamera.transform;
                mainCamera.transform.localPosition = Vector3.zero;
                mainCamera.transform.localEulerAngles = Vector3.zero;
            }

            // add the gameController example
            if (useGameController)
            {
                GameObject gC = null;
                var gameController = FindObjectOfType<vGameController>();
                if (gameController == null)
                {
                    gC = new GameObject("vGameController_Example");
                    gC.AddComponent<vGameController>();
                }
            }

            UnityEditor.SceneView.lastActiveSceneView.FrameSelected();
            this.Close();
        }
    }
}