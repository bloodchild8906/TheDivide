using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Invector
{
    public class vAudioSurface : ScriptableObject
    {
        public AudioSource audioSource;
        public AudioMixerGroup audioMixerGroup;                 // The AudioSource that will play the clips.   
        public List<string> TextureOrMaterialNames;             // The tag on the surfaces that play these sounds.
        public List<AudioClip> audioClips;                      // The different clips that can be played on this surface.    
        public GameObject particleObject;

        private vFisherYatesRandom randomSource = new vFisherYatesRandom();       // For randomly reordering clips.   

        public bool useStepMark;
        [vHideInInspector("useStepMark")]
        public GameObject stepMark;
        [vHideInInspector("useStepMark")]
        public LayerMask stepLayer;
        [vHideInInspector("useStepMark")]
        public float timeToDestroy = 5f;

        public vAudioSurface()
        {
            audioClips = new List<AudioClip>();
            TextureOrMaterialNames = new List<string>();
        }
        /// <summary>
        /// Spawn surface effect
        /// </summary>
        /// <param name="footStepObject">step object surface info</param>
        /// <param name="playSound">Spawn sound effect</param>
        /// <param name="spawnParticle">Spawn particle effect</param>
        /// <param name="spawnStepMark">Spawn step Mark effect</param>

        public virtual void SpawnSurfaceEffect(FootStepObject footStepObject)
        {

            // initialize variable if not already started
            if (randomSource == null)
            {
                randomSource = new vFisherYatesRandom();
            }
            ///Create audio Effect
            if (footStepObject.spawnSoundEffect)
            {
                PlaySound(footStepObject);
            }
            ///Create particle Effect
            if (footStepObject.spawnParticleEffect && particleObject && footStepObject.ground && stepLayer.ContainsLayer(footStepObject.ground.gameObject.layer))
            {
                SpawnParticle(footStepObject);
            }
            ///Create Step Mark Effect
            if (footStepObject.spawnStepMarkEffect && useStepMark)
            {
                StepMark(footStepObject);
            }
        }


        /// <summary>
        /// Spawn Sound effect
        /// </summary>
        /// <param name="footStepObject">Step object surface info</param>      
        protected virtual void PlaySound(FootStepObject footStepObject)
        {
            // if there are no clips to play return.
            if (audioClips == null || audioClips.Count == 0)
            {
                return;
            }

            AudioSource source = null;
            if (audioSource != null)
            {
                source = Instantiate(audioSource, footStepObject.sender.position, Quaternion.identity);
                source.transform.SetParent(vObjectContainer.root, true);
            }
            if (audioSource)
            {
                if (audioMixerGroup != null)
                {
                    source.outputAudioMixerGroup = audioMixerGroup;
                }
            }
            int index = randomSource.Next(audioClips.Count);
            source.PlayOneShot(audioClips[index], footStepObject.volume);
        }
        /// <summary>
        /// Spawn Particle effect
        /// </summary>
        /// <param name="footStepObject">Step object surface info</param>
        protected virtual void SpawnParticle(FootStepObject footStepObject)
        {
            var obj = Instantiate(particleObject, footStepObject.sender.position, footStepObject.sender.rotation);
            obj.transform.SetParent(vObjectContainer.root, true);
        }
        /// <summary>
        /// Spawn Step Mark effect
        /// </summary>
        /// <param name="footStepObject">Step object surface info</param>
        protected virtual void StepMark(FootStepObject footStep)
        {
            RaycastHit hit;
            if (Physics.Raycast(footStep.sender.transform.position + new Vector3(0, 0.25f, 0), Vector3.down, out hit, 1f, stepLayer))
            {
                if (stepMark)
                {
                    var angle = Quaternion.FromToRotation(footStep.sender.up, hit.normal);
                    var step = Instantiate(stepMark, hit.point, angle * footStep.sender.rotation);
                    step.transform.SetParent(vObjectContainer.root, true);
                    Destroy(step, timeToDestroy);
                }
            }
        }
    }
}