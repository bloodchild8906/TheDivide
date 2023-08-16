using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController
{
    [vClassHeader("RAGDOLL SYSTEM", true, "ragdollIcon", true, "Every gameobject children of the character must have their tag added in the IgnoreTag List.")]
    public class vRagdoll : vMonoBehaviour
    {
        #region public variables

        [vEditorToolbar("Debug")]
        public bool startRagdolled = false;
        [vButton("Active Ragdoll And Keep Ragdolled", "ActivateRagdollWithDelayToGetUp", typeof(vRagdoll))]
        [vButton("Active Ragdoll", "ActivateRagdoll", typeof(vRagdoll))]
        [SerializeField] protected float debugTimeToStayRagdolled;

        [vEditorToolbar("Settings")]
        public LayerMask groundLayer = 1 << 0;
        public bool keepRagdolled;
        public bool invertGetUpAnim;
        public bool _ignoreGetUpAnimation;
        public bool removePhysicsAfterDie;

        [Tooltip("SHOOTER: Keep false to use detection hit on each children collider, don't forget to change the layer to BodyPart from hips to all children. MELEE: Keep true to only hit the main Capsule Collider.")]
        public bool disableColliders = false;
        public AudioSource collisionSource;
        public AudioClip collisionClip;

        [Header("Add Tags for Weapons or Itens here:")]
        public List<string> ignoreTags = new List<string>() { "Weapon", "Ignore Ragdoll" };
        public AnimatorStateInfo stateInfo;
        [Range(0, 2f)]
        [Tooltip("The velocity of the parent rigidbody will be applied to the Ragdoll when enabled, creating a more realistic physics")]
        public float horizontalMultiplier = 1f;
        [Range(0, 2f)]
        public float verticalMultiplier = 0.5f;

        #endregion

        #region private variables
        internal vICharacter iChar;
        protected Animator animator;
        protected Rigidbody _parentRigb;
        internal Transform characterChest, characterHips;
        [vEditorToolbar("Debug")]
        [vReadOnly]
        public bool isActive;

        [vReadOnly]
        public bool inStabilize, updateBehaviour;
        //The current state
        [vReadOnly]
        public RagdollState state = RagdollState.animated;

        public bool ignoreGetUpAnimation { get => _ignoreGetUpAnimation; set => _ignoreGetUpAnimation = value; }

        protected Vector3 localY { get; set; }

        bool ragdolled
        {
            get
            {
                return state != RagdollState.animated;
            }
            set
            {
                if (value == true)
                {
                    if (state == RagdollState.animated)
                    {
                        //Transition from animated to ragdolled
                        setKinematic(false); //allow the ragdoll RigidBodies to react to the environment
                        setCollider(false);
                        animator.enabled = false; //disable animation
                        state = RagdollState.ragdolled;
                    }
                }
                else
                {
                    characterHips.SetParent(hipsParent);
                    isActive = false;
                    ragdollingEndTime = Time.time;
                    if (state == RagdollState.ragdolled)
                    {
                        setKinematic(true); //disable gravity etc.
                        setCollider(true);
                        //store the state change time

                        animator.enabled = true; //enable animation
                        state = RagdollState.blendToAnim;

                        //Store the ragdolled position for blending
                        foreach (BodyPart b in bodyParts)
                        {
                            b.storedRotation = b.transform.rotation;
                            b.storedPosition = b.transform.position;
                        }

                        //Remember some key positions
                        ragdolledFeetPosition = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftToes).position + animator.GetBoneTransform(HumanBodyBones.RightToes).position);
                        ragdolledHeadPosition = animator.GetBoneTransform(HumanBodyBones.Head).position;
                        ragdolledHipPosition = animator.GetBoneTransform(HumanBodyBones.Hips).position;
                        float lastlocalY = characterHips.TransformDirection(localY).y;
                        //animator.Rebind();
                        //Initiate the get up animation
                        //hip hips forward vector pointing upwards, initiate the get up from back animation
                        if (!ignoreGetUpAnimation && animator.enabled && gameObject.activeInHierarchy)
                        {
                            if (lastlocalY > 0)
                            {
                                animator.Play("StandUp@FromBack");
                            }
                            else
                            {
                                animator.Play("StandUp@FromBelly");
                            }
                        }

                    }
                    else if (state == RagdollState.animated)
                    {
                        setKinematic(true); //disable gravity etc.
                        setCollider(true);
                        animator.enabled = true; //enable animation
                    }
                }
            }
        }

        //Possible states of the ragdoll
        public enum RagdollState
        {
            animated,    //Mecanim is fully in control
            ragdolled,   //Mecanim turned off, physics controls the ragdoll
            blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
        }


        //How long do we blend when transitioning from ragdolled to animated
        readonly float ragdollToMecanimBlendTime = 0.25f;
        readonly float mecanimToGetUpTransitionTime = 0.05f;
        //A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
        float ragdollingEndTime = -100;
        //Additional vectores for storing the pose the ragdoll ended up in.
        Vector3 ragdolledHipPosition, ragdolledHeadPosition, ragdolledFeetPosition;

        //Declare a list of body parts, initialized in Start()
        readonly List<BodyPart> bodyParts = new List<BodyPart>();
        // used to reset parent of hips
        Transform hipsParent;
        //used to controll damage frequency
        bool inApplyCollisionSound;
        private GameObject _ragdollContainer;
        class BodyPart
        {

            public Transform transform;
            public Rigidbody rigidbody;
            public Collider collider;
            public Vector3 storedPosition;
            public Quaternion storedRotation;
            public BodyPart(Transform t)
            {
                this.transform = t;
                this.rigidbody = t.GetComponent<Rigidbody>();
                this.collider = t.GetComponent<Collider>();
            }
        }
        #endregion

        protected virtual void Start()
        {
            // store the Animator component
            _parentRigb = GetComponent<Rigidbody>();
            iChar = GetComponent<vICharacter>();
            if (iChar != null)
            {
                iChar.onActiveRagdoll.AddListener(ActivateRagdoll);
            }

            if (!collisionSource)
            {
                var _collisionPrefab = new GameObject("ragdollAudioSource");
                _collisionPrefab.transform.SetParent(gameObject.transform);
                _collisionPrefab.transform.position = transform.position;
                collisionSource = _collisionPrefab.AddComponent<AudioSource>();
            }

            LoadBodyPart();
            //CreateRagdollContainer();

            if (startRagdolled)
            {
                Invoke("ActivateRagdoll", 0.1f);
            }

            _ragdollContainer = vObjectContainer.root.gameObject;
        }

        protected virtual void OnDisable()
        {
            inStabilize = false;
            Invoke("ChangeToHipsParent", 0.01f);
            setKinematic(true);
            setCollider(true);
        }

        protected virtual void ChangeToHipsParent()
        {
            if (characterHips.parent != hipsParent)
            {
                characterHips.position = transform.position;
                characterHips.SetParent(hipsParent);
            }

        }

        public virtual void RestoreRagdoll()
        {
            if (isActive)
            {
                ChangeToHipsParent();
                iChar.ResetRagdoll();
                isActive = false;
                state = RagdollState.animated;
                inStabilize = false;
                setKinematic(true); //disable gravity etc.
                setCollider(true);
                animator.enabled = true; //enable animation
            }
        }

        public void LoadBodyPart()
        {
            bodyParts.Clear();
            // find character chest and hips
            if (!animator)
            {
                animator = GetComponent<Animator>();
            }

            characterChest = animator.GetBoneTransform(HumanBodyBones.Chest);
            characterHips = animator.GetBoneTransform(HumanBodyBones.Hips);
            localY = characterHips.InverseTransformDirection(transform.forward);
            hipsParent = characterHips.parent;
            // set all RigidBodies to kinematic so that they can be controlled with Mecanim
            // and there will be no glitches when transitioning to a ragdoll 
            // find all the transforms in the character, assuming that this script is attached to the root
            if (characterHips)
            {
                Component[] components = characterHips.GetComponentsInChildren(typeof(Transform));
                bodyParts.Add(new BodyPart(characterHips));
                // for each of the transforms, create a BodyPart instance and store the transform 

                for (int i = 0; i < components.Length; i++)
                {
                    var c = components[i];
                    if (!ignoreTags.Contains(c.tag) && c)
                    {
                        var t = c as Transform;
                        if (t != transform && t.TryGetComponent<Rigidbody>(out Rigidbody r))
                        {
                            BodyPart bodyPart = new BodyPart(t);

                            if (bodyPart.rigidbody != null)
                            {
                                bodyPart.rigidbody.isKinematic = true;
                                c.tag = gameObject.tag;
                            }
                            bodyParts.Add(bodyPart);
                        }
                    }
                }

                setKinematic(true);
                setCollider(true);
            }
        }

        //void CreateRagdollContainer()
        //{
        //    if (!_ragdollContainer)
        //    {
        //        _ragdollContainer = new GameObject("RagdollContainer " + gameObject.name);
        //    }
        //    //_ragdollContainer.hideFlags = HideFlags.HideInHierarchy;
        //}

        protected virtual void LateUpdate()
        {
            if (animator == null)
            {
                return;
            }

            if (!updateBehaviour && animator.updateMode == AnimatorUpdateMode.AnimatePhysics)
            {
                return;
            }

            updateBehaviour = false;
            RagdollBehaviour();
        }

        protected virtual void FixedUpdate()
        {
            updateBehaviour = true;
            if (!isActive)
            {

                return;
            }

            //if (!_ragdollContainer)
            //{
            //    CreateRagdollContainer();
            //}
            if (characterHips.parent != _ragdollContainer.transform)
            {
                characterHips.SetParent(_ragdollContainer.transform);
            }

            if (ragdolled && !inStabilize && !keepRagdolled && !iChar.isDead)
            {
                ragdolled = false;
                StartCoroutine(ResetPlayer(1.1f));
            }
            else if (animator != null && !animator.isActiveAndEnabled && ragdolled || (animator == null && ragdolled))
            {
                transform.position = characterHips.position;
            }

        }

        protected virtual void OnDestroy()
        {
            try
            {
                if (_ragdollContainer && characterHips && characterHips.parent == _ragdollContainer.transform)
                {
                    //characterHips.SetParent(hipsParent);
                    Destroy(characterHips.gameObject);
                }
            }
            catch (UnityException e)
            {
                Debug.LogWarning(e.Message, gameObject);
            }
        }

        /// <summary>
        /// Reset the inApplyDamage variable. Set to false;
        /// </summary>
        protected virtual void ResetCollisionSound()
        {
            inApplyCollisionSound = false;
        }

        public virtual void ActivateRagdollWithDelayToGetUp()
        {
            ActivateRagdoll(null, debugTimeToStayRagdolled);
        }

        protected virtual void KeepRagdolled(float time)
        {
            if (time > 0)
            {
                keepRagdolled = true;
            }

            CancelInvoke("ResetStayRagdolled");
            Invoke("ResetStayRagdolled", time);

        }

        /// <summary>
        /// Reset keep ragdolled time
        /// </summary>
        public virtual void ResetStayRagdolled()
        {
            keepRagdolled = false;
        }

        /// <summary>
        /// Active Ragdoll
        /// </summary>
        public virtual void ActivateRagdoll()
        {
            ActivateRagdoll(null);
        }

        /// <summary>
        /// Active Ragdoll
        /// </summary>
        /// <param name="damage">Damage</param>
        /// <param name="timeToStayRagdolled">Time to keep ragdolled active</param>
        public virtual void ActivateRagdoll(vDamage damage, float timeToStayRagdolled)
        {
            if (isActive || (damage != null && !damage.activeRagdoll) || state == RagdollState.blendToAnim)
            {
                return;
            }

            ActivateRagdoll(damage);
            KeepRagdolled(timeToStayRagdolled);
        }

        // active ragdoll - call this method to turn the ragdoll on      
        public virtual void ActivateRagdoll(vDamage damage)
        {
            if (isActive || (damage != null && !damage.activeRagdoll) || state == RagdollState.blendToAnim)
            {
                return;
            }

            StopAllCoroutines();

            //if (!_ragdollContainer)
            //{
            //    CreateRagdollContainer();
            //}

            if (damage != null && damage.senselessTime > 0)
            {
                KeepRagdolled(damage.senselessTime);
            }

            inApplyCollisionSound = true;
            isActive = true;

            if (transform.parent != null && !transform.parent.gameObject.isStatic)
            {
                transform.parent = null;
            }
            // turn ragdoll on
            inStabilize = true;
            ragdolled = true;
            if (iChar != null)
            {
                iChar.EnableRagdoll();
            }
            // start to check if the ragdoll is stable
            StartCoroutine(RagdollStabilizer(2f));

            // if(!iChar.isDead) 
            characterHips.SetParent(_ragdollContainer.transform);

            Invoke("ResetCollisionSound", 0.2f);
        }

        // ragdoll collision sound        
        public virtual void OnRagdollCollisionEnter(vRagdollCollision ragdolCollision)
        {
            if (!inApplyCollisionSound && ragdolCollision.ImpactForce > 1)
            {
                if (collisionSource)
                {
                    collisionSource.clip = collisionClip;
                    collisionSource.volume = ragdolCollision.ImpactForce * 0.05f;
                    if (!collisionSource.isPlaying)
                    {
                        inApplyCollisionSound = true;
                        collisionSource.Play();
                        Invoke("ResetCollisionSound", 0.2f);
                    }
                }
            }
        }

        // ragdoll stabilizer - wait until the ragdoll became stable based on the chest velocity.magnitude
        protected virtual IEnumerator RagdollStabilizer(float delay)
        {
            float rdStabilize = Mathf.Infinity;
            yield return new WaitForSeconds(delay);
            while (rdStabilize > (iChar != null && iChar.isDead ? 0.0001f : 0.1f))
            {
                if (animator != null && !animator.isActiveAndEnabled)
                {
                    rdStabilize = characterChest.GetComponent<Rigidbody>().velocity.magnitude;

                }
                else
                {
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            if (iChar != null && iChar.isDead)
            {
                //Destroy(iChar as Component);
                yield return new WaitForEndOfFrame();
                DestroyComponents();
            }
            inStabilize = false;
        }

        // reset player - restore control to the character	
        protected virtual IEnumerator ResetPlayer(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            //Debug.Log("Ragdoll OFF");        
            if (iChar != null)
            {
                iChar.ResetRagdoll();
            }
        }

        // ragdoll blend - code based on the script by Perttu Hämäläinen with modifications to work with this Controller        
        protected virtual void RagdollBehaviour()
        {
            var isDead = !(iChar != null && iChar.currentHealth > 0);
            if (isDead)
            {
                return;
            }
            if (iChar == null || !iChar.ragdolled)
            {
                return;
            }

            //Blending from ragdoll back to animated
            if (state == RagdollState.blendToAnim)
            {
                if (Time.time <= ragdollingEndTime + mecanimToGetUpTransitionTime)
                {
                    //If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
                    //character to the best match with the ragdoll
                    Vector3 animatedToRagdolled = ragdolledHipPosition - animator.GetBoneTransform(HumanBodyBones.Hips).position;
                    Vector3 newRootPosition = transform.position + animatedToRagdolled;

                    //Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
                    RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition + Vector3.up, Vector3.down), 1, groundLayer, QueryTriggerInteraction.Ignore);

                    foreach (RaycastHit hit in hits)
                    {
                        if (!hit.transform.IsChildOf(transform))
                        {
                            newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
                        }
                    }
                    transform.position = newRootPosition;

                    //Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
                    Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
                    ragdolledDirection.y = 0;

                    Vector3 meanFeetPosition = 0.5f * (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                    Vector3 animatedDirection = animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                    animatedDirection.y = 0;

                    //Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
                    //hence setting the y components of the vectors to zero. 
                    transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdolledDirection.normalized);
                }
                //compute the ragdoll blend amount in the range 0...1
                float ragdollBlendAmount = 1.0f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
                ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);

                //In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
                //To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
                //and slerp all the rotations towards the ones stored when ending the ragdolling
                foreach (BodyPart b in bodyParts)
                {
                    if (b.transform != transform)
                    { //this if is to prevent us from modifying the root of the character, only the actual body parts
                      //position is only interpolated for the hips
                        if (b.transform == animator.GetBoneTransform(HumanBodyBones.Hips))
                        {
                            b.transform.position = Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
                        }
                        //rotation is interpolated for all body parts
                        b.transform.rotation = Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
                    }
                }

                //if the ragdoll blend amount has decreased to zero, move to animated state

                //Debug.Log($"Blend {ragdollBlendAmount}");
                if (ragdollBlendAmount == 0)
                {
                    state = RagdollState.animated;
                    return;
                }
            }
        }

        // set all rigidbodies to kinematic
        protected virtual void setKinematic(bool newValue)
        {
            foreach (var bp in bodyParts)
            {
                if (bp.transform != null && !ignoreTags.Contains(bp.transform.tag) && bp.rigidbody)
                {
                    if (bp.rigidbody.isKinematic != newValue)
                    {
                        bp.rigidbody.isKinematic = newValue;
                        if (newValue == false)
                        {
                            var v = new Vector3(_parentRigb.velocity.x * horizontalMultiplier, _parentRigb.velocity.y * verticalMultiplier, _parentRigb.velocity.z * horizontalMultiplier);
                            bp.rigidbody.velocity = v;
                        }
                    }
                }
            }
        }

        // set all colliders to trigger
        protected virtual void setCollider(bool newValue)
        {
            //if (!disableColliders) return;
            foreach (var bp in bodyParts)
            {
                if (bp.transform != null && !ignoreTags.Contains(bp.transform.tag))
                {
                    if (!bp.transform.Equals(transform) && bp.collider)
                    {
                        if (disableColliders)
                        {
                            bp.collider.enabled = !newValue;
                        }
                        else
                        {
                            bp.collider.isTrigger = newValue;
                        }
                    }
                }
            }
        }

        // destroy the components if the character is dead
        protected virtual void DestroyComponents()
        {
            if (removePhysicsAfterDie)
            {
                var comps = GetComponentsInChildren<MonoBehaviour>();
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i].transform != transform)
                    {
                        Destroy(comps[i]);
                    }
                }
                var joints = GetComponentsInChildren<CharacterJoint>();
                if (joints != null)
                {
                    foreach (CharacterJoint comp in joints)
                    {
                        if (!ignoreTags.Contains(comp.gameObject.tag) && comp.transform != transform)
                        {
                            Destroy(comp);
                        }
                    }
                }

                var rigidbodies = GetComponentsInChildren<Rigidbody>();
                if (rigidbodies != null)
                {
                    foreach (Rigidbody comp in rigidbodies)
                    {
                        if (!ignoreTags.Contains(comp.gameObject.tag) && comp.transform != transform)
                        {
                            Destroy(comp);
                        }
                    }
                }

                var colliders = GetComponentsInChildren<Collider>();
                if (colliders != null)
                {
                    foreach (Collider comp in colliders)
                    {
                        if (!ignoreTags.Contains(comp.gameObject.tag) && comp.transform != transform)
                        {
                            Destroy(comp);
                        }
                    }
                }
            }
        }
    }
}