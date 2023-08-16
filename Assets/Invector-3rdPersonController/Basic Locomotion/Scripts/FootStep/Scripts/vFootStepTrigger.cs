using UnityEngine;
using UnityEngine.Events;

namespace Invector
{
    public class vFootStepTrigger : MonoBehaviour
    {
        protected Collider _trigger;
        protected vFootStepBase _fT;
        public UnityEvent OnStep;

        void OnDrawGizmos()
        {
            if (!trigger) return;
            Color color = Color.green;
            color.a = 0.5f;
            Gizmos.color = color;
            if (trigger is SphereCollider)
            {
                Gizmos.DrawSphere((trigger.bounds.center), (trigger as SphereCollider).radius);
            }
        }

        void Start()
        {
            _fT = GetComponentInParent<vFootStepBase>();

            var r = gameObject.GetComponent<Rigidbody>();
            if (r == null)
                gameObject.AddComponent<Rigidbody>().isKinematic = true;
            else
                r.isKinematic = true;
           
            if (_fT == null)
            {
                Debug.Log(gameObject.name + " can't find the FootStepFromTexture");
                gameObject.SetActive(false);
            }
            else
            {
                var colliders = _fT.gameObject.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < colliders.Length; i++)
                {
                    var c = colliders[i];                  
                    if (c!=null && c.gameObject != trigger.gameObject)
                    {
                        Physics.IgnoreCollision(c, trigger, true);
                    }
                }
            }
        }

        public Collider trigger
        {
            get
            {
                if (_trigger == null) _trigger =gameObject.GetComponent<Collider>();
                return _trigger;
            }
        }

        protected Collider lastCollider;

        internal FootStepObject footstepObj;
       
        void OnTriggerEnter(Collider other)
        {
            if (_fT == null) return;         
            if ((lastCollider == null || lastCollider != other) || footstepObj == null)
            {
                footstepObj = new FootStepObject(transform, other);
                lastCollider = other;
            }            

            if (footstepObj.isTerrain) //Check if trigger objet is a terrain
            {                
                _fT.StepOnTerrain(footstepObj);
                OnStep.Invoke();
            }
            else
            {                
                _fT.StepOnMesh(footstepObj);
                OnStep.Invoke();
            }
        }
      
    }

    /// <summary>
    /// Foot step Object work with FootStepFromTexture
    /// </summary>
    public class FootStepObject
    {
        public string name;
        public Transform sender;
        public Collider ground;
        public Terrain terrain;
        public bool isTerrain { get { return terrain != null; } }
        public vFootStepHandler stepHandle;
        public Renderer renderer;
        public bool spawnSoundEffect;
        public bool spawnStepMarkEffect;
        public bool spawnParticleEffect;
        public float volume;
        public FootStepObject(Transform sender, Collider ground)
        {            
            this.name = "";
            this.sender = sender;
            this.ground = ground;
            this.terrain = ground.GetComponent<Terrain>();
            this.stepHandle = ground.GetComponent<vFootStepHandler>();
            this.renderer = ground.GetComponent<Renderer>();
            spawnSoundEffect = true;
            spawnStepMarkEffect = true;
            spawnParticleEffect = true;
            volume = 1;
            if (renderer != null && renderer.material != null)
            {
                var index = 0;
                this.name = string.Empty;
                if (stepHandle != null && stepHandle.material_ID > 0)// if trigger contains a StepHandler to pass material ID. Default is (0)
                    index = stepHandle.material_ID;
                if (stepHandle)
                {
                    // check  stepHandlerType
                    switch (stepHandle.stepHandleType)
                    {
                        case vFootStepHandler.StepHandleType.materialName:
                            this.name = renderer.materials[index].name;
                            break;
                        case vFootStepHandler.StepHandleType.textureName:
                            this.name = renderer.materials[index].mainTexture.name;
                            break;
                    }
                }
                else
                    this.name = renderer.materials[index].name;
            }
        }
    }
}