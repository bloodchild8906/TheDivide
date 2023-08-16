using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Invector.vCharacterController.AI
{
    public class vCreateEnemyEditor : EditorWindow
    {
        GUISkin skin;
        GameObject charObj;
        Animator charAnimator;
        RuntimeAnimatorController controller;
        Vector2 rect = new Vector2(500, 680);
        Vector2 scrool;
        Editor humanoidpreview;
        Texture2D m_Logo;

        public enum CharacterType
        {
            EnemyAI,
            CompanionAI
        }

        public CharacterType charType = CharacterType.EnemyAI;

        /// <summary>
        /// 3rdPersonController Menu 
        /// </summary>    
        [MenuItem("Invector/Melee Combat/Create Simple Melee AI")]
        public static void CreateNewCharacter()
        {
            GetWindow<vCreateEnemyEditor>();
        }

        bool isHuman, isValidAvatar, charExist;

        void OnGUI()
        {
            if (!skin)
            {
                skin = Resources.Load("vSkin") as GUISkin;
            }

            GUI.skin = skin;
            m_Logo = Resources.Load("icon_v2") as Texture2D;

            this.minSize = rect;
            this.titleContent = new GUIContent("Character", null, "Simple Melee AI Character Creator");

            GUILayout.BeginVertical("SIMPLE MELEE AI CHARACTER CREATOR WINDOW", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(5);

            GUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("This is a Simple Melee AI solution that comes with the Melee Combat Package, if you're looking for a more advanced and customizable AI check our AI Template in the AssetStore, it has Shooter Behavior, Waypoint System, NodeEditor for custom behaviours and more...", MessageType.Info);

            charType = (CharacterType)EditorGUILayout.EnumPopup("Character Type", charType);

            if (!charObj)
            {
                EditorGUILayout.HelpBox("Make sure to select the FBX model and not a Prefab already with components attached!", MessageType.Info);
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

            charObj = EditorGUILayout.ObjectField("FBX Model", charObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

            if (GUI.changed && charObj != null && charObj.GetComponent<vSimpleMeleeAI_Controller>() == null)
            {
                humanoidpreview = Editor.CreateEditor(charObj);
            }

            if (charObj != null && charObj.GetComponent<vSimpleMeleeAI_Controller>() != null)
            {
                EditorGUILayout.HelpBox("This gameObject already contains the component v_AIController", MessageType.Warning);
            }
            controller = EditorGUILayout.ObjectField("Animator Controller: ", controller, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;

            GUILayout.EndVertical();

            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Need to know how it works?");
            if (GUILayout.Button("Video Tutorial"))
            {
                Application.OpenURL("https://www.youtube.com/watch?v=tuwg-H8vjqY&list=PLvgXGzhT_qehtuCYl2oyL-LrWoT7fhg9d&index=3");
            }
            GUILayout.EndHorizontal();

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
                if (controller != null)
                {
                    if (GUILayout.Button("Create"))
                    {
                        Create();
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        bool CanCreate()
        {
            return isValidAvatar && isHuman && charObj != null && charObj.GetComponent<vSimpleMeleeAI_Controller>() == null; ;
        }

        /// <summary>
        /// Draw the Preview window
        /// </summary>
        void DrawHumanoidPreview()
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

            if (charType == CharacterType.CompanionAI)
            {
                newCharacter.tag = "CompanionAI";
                newCharacter.name = "vCompanionAI";
                newCharacter.AddComponent<vSimpleMeleeAI_Companion>();

                var p_layer = LayerMask.NameToLayer("CompanionAI");
                newCharacter.layer = p_layer;
                foreach (Transform t in newCharacter.transform.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = p_layer;
                }
            }
            else
            {
                newCharacter.name = "vEnemyAI";
                newCharacter.tag = "Enemy";
                newCharacter.AddComponent<vSimpleMeleeAI_Controller>();

                var p_layer = LayerMask.NameToLayer("Enemy");
                newCharacter.layer = p_layer;
                foreach (Transform t in newCharacter.transform.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = p_layer;
                }
            }

            // rigidbody settings
            var rigidbody = newCharacter.AddComponent<Rigidbody>();
            rigidbody.useGravity = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.mass = 50;

            // capsule collider settings
            var collider = newCharacter.AddComponent<CapsuleCollider>();
            collider.height = ColliderHeight(newCharacter.GetComponent<Animator>());
            collider.center = new Vector3(0, (float)System.Math.Round(collider.height * 0.5f, 2), 0);
            collider.radius = (float)System.Math.Round(collider.height * 0.15f, 2);

            // navmesh settings
            var navMesh = newCharacter.AddComponent<NavMeshAgent>();
            navMesh.radius = 0.4f;
            navMesh.height = 1.8f;
            navMesh.speed = 1f;
            navMesh.angularSpeed = 300f;
            navMesh.acceleration = 8f;
            navMesh.stoppingDistance = 2f;
            navMesh.autoBraking = false;

            if (controller)
            {
                newCharacter.GetComponent<Animator>().runtimeAnimatorController = controller;
            }

            this.Close();

        }

        /// <summary>
        /// Capsule Collider height based on the Character height
        /// </summary>
        /// <param name="animator">animator humanoid</param>
        /// <returns></returns>
        float ColliderHeight(Animator animator)
        {
            var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            return (float)System.Math.Round(Vector3.Distance(foot.position, hips.position) * 2f, 2);
        }

    }
}
