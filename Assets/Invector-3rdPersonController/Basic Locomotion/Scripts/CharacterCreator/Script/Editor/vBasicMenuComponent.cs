using UnityEngine;
using UnityEditor;
using Invector.Utils;

namespace Invector.vCharacterController.vActions
{
    // BASIC FEATURES
    public partial class vMenuComponent
    {
        [MenuItem("GameObject/Invector/Utils/Create SimpleTrigger", false)]
        static void AddSimpleTrigger()
        {
            var obj = new GameObject("SimpleTrigger", typeof(vSimpleTrigger));


            SceneView view = SceneView.lastActiveSceneView;
            if (SceneView.lastActiveSceneView == null)
                throw new UnityException("The Scene View can't be access");

            Vector3 spawnPos = view.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 5f));
            if (Selection.activeGameObject)
            {
                obj.transform.parent = Selection.activeGameObject.transform;
                spawnPos = Selection.activeGameObject.transform.position;
            }
            obj.transform.position = spawnPos;
            obj.layer = LayerMask.NameToLayer("Triggers");

            Selection.activeGameObject = obj.gameObject;
        }

        [MenuItem("GameObject/Invector/Utils/Create SimpleTrigger With Input", false)]
        static void AddSimpleTriggerWithInput()
        {
            var obj = new GameObject("SimpleTrigger WithInput", typeof(vSimpleTriggerWithInput));


            SceneView view = SceneView.lastActiveSceneView;
            if (SceneView.lastActiveSceneView == null)
                throw new UnityException("The Scene View can't be access");

            Vector3 spawnPos = view.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 5f));
            if (Selection.activeGameObject)
            {
                obj.transform.parent = Selection.activeGameObject.transform;
                spawnPos = Selection.activeGameObject.transform.position;
            }
            obj.transform.position = spawnPos;
            obj.layer = LayerMask.NameToLayer("Triggers");

            Selection.activeGameObject = obj.gameObject;
        }

        [MenuItem("Invector/Basic Locomotion/Actions/Generic Action")]
        static void GenericActionMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<vGenericAction>();
            else
                Debug.Log("Please select the Player to add this component.");
        }

        [MenuItem("Invector/Basic Locomotion/Components/Generic Animation")]
        static void GenericAnimationMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<vGenericAnimation>();
            else
                Debug.Log("Please select the Player to add this component.");
        }       

        [MenuItem("Invector/Basic Locomotion/Actions/Ladder Action")]
        static void LadderActionMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<vLadderAction>();
            else
                Debug.Log("Please select the Player to add this component.");
        }   

        [MenuItem("Invector/Basic Locomotion/Components/HitDamageParticle")]
        static void HitDamageMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<vHitDamageParticle>();
            else
                Debug.Log("Please select a vCharacter to add the component.");
        }

        [MenuItem("Invector/Basic Locomotion/Components/HeadTrack")]
        static void HeadTrackMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<vHeadTrack>();
            else
                Debug.Log("Please select a vCharacter to add the component.");
        }

        [MenuItem("Invector/Basic Locomotion/Components/FootStep")]
        static void FootStepMenu()
        {
            if (Selection.activeGameObject)
                Selection.activeGameObject.AddComponent<vFootStep>();
            else
                Debug.Log("Please select a GameObject to add the component.");
        }

        [MenuItem("Invector/Basic Locomotion/Resources/New AudioSurface")]
        static void NewAudioSurface()
        {
            vScriptableObjectUtility.CreateAsset<vAudioSurface>();
        }

        [MenuItem("Invector/Basic Locomotion/Resources/New Ragdoll Generic Template")]
        static void RagdollGenericTemplate()
        {
            vScriptableObjectUtility.CreateAsset<vRagdollGenericTemplate>();
        }
    }
}