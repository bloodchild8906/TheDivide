using UnityEngine;
using System.Collections;
namespace Invector
{
    [RequireComponent(typeof(AudioSource))]
    public class vPlayRandomClip : MonoBehaviour
    {

        public AudioClip[] clips;
        public AudioSource audioSource;
        public bool playOnStart = true;
#if !UNITY_5_4_OR_NEWER
    protected System.Random random;
#endif
        void Start()
        {
            if (!audioSource) audioSource = GetComponent<AudioSource>();
            Random.InitState(Random.Range(0, System.DateTime.Now.Millisecond));
            if (playOnStart)
            {
                Play();
            }
        }
        public void Play()
        {
            if (audioSource)
            {
                var index = 0;

                index = Random.Range(0, clips.Length - 1);
                if (clips.Length > 0)
                    audioSource.PlayOneShot(clips[index]);
            }
        }
    }

}
