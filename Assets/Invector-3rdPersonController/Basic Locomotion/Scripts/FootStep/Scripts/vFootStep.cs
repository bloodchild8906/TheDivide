using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
    public class vFootStep : vFootStepBase
    {
        public AnimationType animationType = AnimationType.Humanoid;
        public bool debugTextureName;

        [SerializeField, Range(0, 1f)] protected float _volume = 1f;
        [vHelpBox("Enable or disable spawn particle when foot step is triggered")]
        [SerializeField] protected bool _spawnParticle = true;
        [vHelpBox("Enable or disable spawn step mark when foot step is triggered")]
        [SerializeField] protected bool _spawnStepMark = true;
        [vHelpBox("The step effect is spawned from on trigger enter event of the Foot Step Triggers. If you need to play step sound only by external events you need to disable this variable.<b>\n*Disable this to play step sound using animation events</b>")]
        [SerializeField] protected bool _useTriggerEnter = true;

        public float Volume { get { return _volume; } set { _volume = value; } }
        public bool SpawnParticle { get { return _spawnParticle; } set { _spawnParticle = value; } }
        public bool SpawnStepMark { get { return _spawnStepMark; } set { _spawnStepMark = value; } }

        protected int surfaceIndex = 0;
        protected Terrain terrain;
        protected TerrainCollider terrainCollider;
        protected TerrainData terrainData;
        protected Vector3 terrainPos;

        public vFootStepTrigger leftFootTrigger;
        public vFootStepTrigger rightFootTrigger;
        public Transform currentStep;
        public List<vFootStepTrigger> footStepTriggers;

        protected FootStepObject currentFootStep;

        protected virtual void Start()
        {
            InitFootStep();
        }

        public virtual void InitFootStep()
        {
            var colls = GetComponentsInChildren<Collider>();
            if (animationType == AnimationType.Humanoid)
            {
                if (leftFootTrigger == null && rightFootTrigger == null)
                {
                    Debug.Log("Missing FootStep Sphere Trigger, please unfold the FootStep Component to create the triggers.");
                    return;
                }
                else
                {
                    leftFootTrigger.trigger.isTrigger = true;
                    rightFootTrigger.trigger.isTrigger = true;
                    Physics.IgnoreCollision(leftFootTrigger.trigger, rightFootTrigger.trigger);
                    for (int i = 0; i < colls.Length; i++)
                    {
                        var coll = colls[i];
                        if (coll.enabled && coll.gameObject != leftFootTrigger.gameObject)
                        {
                            Physics.IgnoreCollision(leftFootTrigger.trigger, coll);
                        }

                        if (coll.enabled && coll.gameObject != rightFootTrigger.gameObject)
                        {
                            Physics.IgnoreCollision(rightFootTrigger.trigger, coll);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < colls.Length; i++)
                {
                    var coll = colls[i];
                    for (int a = 0; a < footStepTriggers.Count; a++)
                    {
                        var trigger = footStepTriggers[a];
                        trigger.trigger.isTrigger = true;
                        if (coll.enabled && coll.gameObject != trigger.gameObject)
                        {
                            Physics.IgnoreCollision(trigger.trigger, coll);
                        }
                    }
                }
            }
        }

        protected virtual void UpdateTerrainInfo(Terrain newTerrain)
        {
            if (terrain == null || terrain != newTerrain)
            {
                terrain = newTerrain;
                if (terrain != null)
                {
                    terrainData = terrain.terrainData;
                    terrainPos = terrain.transform.position;
                    terrainCollider = terrain.GetComponent<TerrainCollider>();
                }
            }
        }

        protected virtual float[] GetTextureMix(FootStepObject footStepObj)
        {
            // returns an array containing the relative mix of textures
            // on the main terrain at this world position.

            // The number of values in the array will equal the number
            // of textures added to the terrain.

            UpdateTerrainInfo(footStepObj.terrain);

            // calculate which splat map cell the worldPos falls within (ignoring y)
            var worldPos = footStepObj.sender.position;
            int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

            // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
            if (!terrainCollider.bounds.Contains(worldPos))
            {
                return new float[0];
            }

            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            // extract the 3D array data to a 1D array:
            float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

            for (int n = 0; n < cellMix.Length; n++)
            {
                cellMix[n] = splatmapData[0, 0, n];
            }
            return cellMix;
        }

        protected virtual int GetMainTexture(FootStepObject footStepObj)
        {
            // returns the zero-based index of the most dominant texture
            // on the main terrain at this world position.
            float[] mix = GetTextureMix(footStepObj);

            if (mix == null)
            {
                return -1;
            }

            float maxMix = 0;
            int maxIndex = 0;

            // loop through each mix value and find the maximum
            for (int n = 0; n < mix.Length; n++)
            {
                if (mix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = mix[n];
                }
            }
            return maxIndex;
        }

        protected virtual void OnDestroy()
        {
            if (leftFootTrigger != null)
            {
                Destroy(leftFootTrigger.gameObject);
            }

            if (rightFootTrigger != null)
            {
                Destroy(rightFootTrigger.gameObject);
            }

            if (footStepTriggers != null && footStepTriggers.Count > 0)
            {
                foreach (var comp in footStepTriggers)
                {
                    Destroy(comp.gameObject);
                }
            }
        }

        /// <summary>
        /// Step on Terrain
        /// </summary>
        /// <param name="footStepObject"></param>
        public override void StepOnTerrain(FootStepObject footStepObject)
        {
            if (currentStep != null && currentStep == footStepObject.sender && _useTriggerEnter)
            {
                return;
            }

            currentStep = footStepObject.sender;
            surfaceIndex = GetMainTexture(footStepObject);

            if (surfaceIndex != -1)
            {
#if UNITY_2018_3_OR_NEWER
                var name = (terrainData != null && terrainData.terrainLayers.Length > 0) ? (terrainData.terrainLayers[surfaceIndex]).diffuseTexture.name : "";
#else
                var name = (terrainData != null && terrainData.splatPrototypes.Length > 0) ? (terrainData.splatPrototypes[surfaceIndex]).texture.name : "";
#endif
                footStepObject.name = name;
                currentFootStep = footStepObject;
                if (_useTriggerEnter)
                {
                    PlayFootStepEffect();
                    if (debugTextureName)
                    {
                        Debug.Log(terrain.name + " " + name);
                    }
                }
            }
        }

        /// <summary>
        /// Step on Mesh
        /// </summary>
        /// <param name="footStepObject"></param>
        public override void StepOnMesh(FootStepObject footStepObject)
        {
            if (currentStep != null && currentStep == footStepObject.sender && _useTriggerEnter)
            {
                return;
            }

            currentStep = footStepObject.sender;
            currentFootStep = footStepObject;
            if (_useTriggerEnter)
            {
                PlayFootStepEffect();
                if (debugTextureName)
                {
                    Debug.Log(footStepObject.name);
                }
            }
        }

        /// <summary>
        /// Play foot Step effect
        /// </summary>
        public override void PlayFootStepEffect()
        {
            if (currentFootStep != null)
            {
                currentFootStep.volume = Volume;
                currentFootStep.spawnParticleEffect = SpawnParticle;
                currentFootStep.spawnStepMarkEffect = SpawnStepMark;
                SpawnSurfaceEffect(currentFootStep);
            }
        }

        /// <summary>
        /// Play foot step effect from animation event
        /// </summary>
        /// <param name="evt"></param>
        public override void PlayFootStep(AnimationEvent evt)
        {
            if (evt.animatorClipInfo.weight > 0.5)
            {
                PlayFootStepEffect();
            }
        }

        /// <summary>
        /// Play left foot step effect from animation event
        /// </summary>
        /// <param name="evt"></param>
        public override void PlayFootStepLeft(AnimationEvent evt)
        {
            if (evt.animatorClipInfo.weight > 0.5)
            {
                currentFootStep.sender = leftFootTrigger.transform;
                PlayFootStepEffect();
            }
        }

        /// <summary>
        /// Play right foot step effect from animation event
        /// </summary>
        /// <param name="evt"></param>
        public override void PlayFootStepRight(AnimationEvent evt)
        {
            if (evt.animatorClipInfo.weight > 0.15)
            {
                currentFootStep.sender = rightFootTrigger.transform;
                PlayFootStepEffect();
            }
        }

    }

    public enum AnimationType
    {
        Humanoid, Generic
    }
}