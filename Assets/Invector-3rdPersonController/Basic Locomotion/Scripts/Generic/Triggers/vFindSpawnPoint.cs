using UnityEngine;
using System.Collections;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
namespace Invector.vCharacterController
{
    public class vFindSpawnPoint : MonoBehaviour
    {
        public Transform spawnPoint;
        public string spawnPointName;
        public GameObject target;

        public void AlighObjetToSpawnPoint(GameObject target, string spawnPointName)
        {
            this.target = target;
            this.spawnPointName = spawnPointName;
            //		Debug.Log(spawnPointName+" "+gameObject.name);
#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
#endif
            DontDestroyOnLoad(gameObject);

        }
#if UNITY_5_4_OR_NEWER
        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            var spawnPoint = GameObject.Find(spawnPointName);
            if (spawnPoint && target)
            {
                target.transform.position = spawnPoint.transform.position;
                target.transform.rotation = spawnPoint.transform.rotation;
            }
            else
            {
                try
                {
                    Destroy(gameObject);
                }
                catch { }
            }
        }
#else
    public void OnLevelWasLoaded(int level)
    {
	     var spawnPoint = GameObject.Find(spawnPointName);
		if(spawnPoint && target)
		{
			target.transform.position = spawnPoint.transform.position;
			target.transform.rotation = spawnPoint.transform.rotation;
		}
		else
		{
            try
            {
                Destroy(gameObject);
            }
            catch { }
		}
    }
#endif

    }
}