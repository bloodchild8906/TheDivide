using UnityEngine;

namespace Invector.vMelee
{
    [vClassHeader("Hit Effects", "Search for the 'AudioSource' prefab in the project or create your own custom AudioSource.")]
    public class vHitEffects : vMonoBehaviour
    {
        public GameObject audioSource;
        public AudioClip[] hitSounds;
        public AudioClip[] recoilSounds;
        public GameObject[] recoilParticles;
        public AudioClip[] defSounds;

        void Start()
        {
            var weaponObject = GetComponent<vMeleeWeapon>();
            if (weaponObject)
            {
                weaponObject.onDamageHit.AddListener(PlayHitEffects);
                weaponObject.onRecoilHit.AddListener(PlayRecoilEffects);
                weaponObject.onDefense.AddListener(PlayDefenseEffects);
            }
        }

        public void PlayHitEffects(vHitInfo hitInfo)
        {
            if (audioSource != null && hitSounds.Length > 0)
            {
                var clip = hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)];
                var audioObj = Instantiate(audioSource, transform.position, transform.rotation) as GameObject;
                audioObj.GetComponent<AudioSource>().PlayOneShot(clip);
            }
        }

        public void PlayRecoilEffects(vHitInfo hitInfo)
        {
            if (audioSource != null && recoilSounds.Length > 0)
            {
                var clip = recoilSounds[UnityEngine.Random.Range(0, recoilSounds.Length)];
                var audioObj = Instantiate(audioSource, transform.position, transform.rotation) as GameObject;
                audioObj.GetComponent<AudioSource>().PlayOneShot(clip);
            }
            if (recoilParticles.Length > 0)
            {
                var particles = recoilParticles[UnityEngine.Random.Range(0, recoilParticles.Length)];
                var hitrotation = Quaternion.LookRotation(new Vector3(transform.position.x, hitInfo.hitPoint.y, transform.position.z) - hitInfo.hitPoint);
                if (particles != null)
                    Instantiate(particles, hitInfo.hitPoint, hitrotation);
            }
        }

        public void PlayDefenseEffects()
        {
            if (audioSource != null && defSounds.Length > 0)
            {
                var clip = defSounds[UnityEngine.Random.Range(0, defSounds.Length)];
                var audioObj = Instantiate(audioSource, transform.position, transform.rotation) as GameObject;
                audioObj.GetComponent<AudioSource>().PlayOneShot(clip);
            }
        }
    }

}