using UnityEngine;
namespace Invector.Utils
{   [vClassHeader("Load Level",openClose =false)]
    public class vLoadLevel : vMonoBehaviour
    {
        [Tooltip("Write the name of the level you want to load")]
        public string levelToLoad;   
        [Tooltip("Assign here the spawnPoint name of the scene that you will load")]
        public string spawnPointName;        

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var thirdPerson = other.transform.gameObject.GetComponent<vCharacterController.vThirdPersonInput>();
                LoadLevelHelper.LoadScene(levelToLoad, spawnPointName, thirdPerson);
            }
        }
    }
}