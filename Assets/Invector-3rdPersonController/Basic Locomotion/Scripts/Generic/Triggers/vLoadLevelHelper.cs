using Invector.vCharacterController;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Invector.Utils
{

    public interface vISceneLoadListener
    {
        void OnStartLoadScene(string sceneName);
        void OnFinishLoadScene(string sceneName);
    }

    public static class LoadLevelHelper
    {
        public static vThirdPersonInput targetCharacter;
        public static string spawnPointName;
        public static string sceneName;

        public static void LoadScene(string _sceneName, string _spawnPointName, vThirdPersonInput tpInput)
        {
            if (!tpInput) return;
            targetCharacter = tpInput;
            spawnPointName = _spawnPointName;
            sceneName = _sceneName;
            if (targetCharacter.tpCamera) targetCharacter.tpCamera.transform.parent = targetCharacter.transform;

            if(targetCharacter)
            {
                var listeners = targetCharacter.GetComponents<vISceneLoadListener>();
                for(int i =0;i<listeners.Length;i++)
                {
                    listeners[i].OnStartLoadScene(_sceneName);
                }
                targetCharacter.StartCoroutine(LoadAsyncScene());
            } 
        }

        static IEnumerator LoadAsyncScene()
        {
            // Set the current Scene to be able to unload it later
            Scene currentScene = SceneManager.GetActiveScene();
            if (!currentScene.name.Equals(sceneName))
            {
                SceneManager.sceneUnloaded += OnSceneLoaded;
                // The Application loads the Scene in the background at the same time as the current Scene.
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                // Wait until the last operation fully loads to return anything
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
                // Move the GameObject (you attach this in the Inspector) to the newly loaded Scene
                SceneManager.MoveGameObjectToScene(targetCharacter.gameObject, SceneManager.GetSceneByName(sceneName));
                // Unload the previous Scene
                SceneManager.UnloadSceneAsync(currentScene);
            }
            else MoveCharaterToSpawnPoint();
        }

        static void OnSceneLoaded(Scene arg0)
        {
            var listeners = targetCharacter.GetComponents<vISceneLoadListener>();
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].OnFinishLoadScene(arg0.name);
            }
            MoveCharaterToSpawnPoint();
            SceneManager.sceneUnloaded -= OnSceneLoaded;
        }

        static void MoveCharaterToSpawnPoint()
        {
            var spawnPoint = GameObject.Find(spawnPointName);
            //Set character position to target spawnPoint
            if (spawnPoint && targetCharacter)
            {
               targetCharacter.lockCameraInput = true;

                if (targetCharacter.tpCamera)
                {
                    targetCharacter.tpCamera.FreezeCamera();
                }
                targetCharacter.transform.position = spawnPoint.transform.position;
                targetCharacter.transform.rotation = spawnPoint.transform.rotation;

                if (targetCharacter.tpCamera)
                {
                    targetCharacter.tpCamera.transform.parent = null;                 
                    targetCharacter.tpCamera.UnFreezeCamera();
                }
                targetCharacter.lockCameraInput = false;
               
            }           
            
        }
    }
}