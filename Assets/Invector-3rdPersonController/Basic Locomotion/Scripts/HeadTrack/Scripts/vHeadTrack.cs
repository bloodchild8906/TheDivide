using Invector.vEventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController
{
    [vClassHeader("HEAD TRACK", iconName = "headTrackIcon")]
    public class vHeadTrack : vMonoBehaviour
    {
        #region variables

        [vEditorToolbar("Settings")]

        [vHelpBox("If your character is not looking up/down, try changing the axis", vHelpBoxAttribute.MessageType.Info)]
        public Vector3 upDownAxis = Vector3.right;

        [Header("Head & Body Weight")]
        public float strafeHeadWeight = 0.6f;
        public float strafeBodyWeight = 0.6f;
        public float aimingHeadWeight = 0.8f;
        public float aimingBodyWeight = 0.8f;
        public float freeHeadWeight = 0.6f;
        public float freeBodyWeight = 0.6f;

        [SerializeField] protected float smooth = 10f;

        [Header("Default Offsets ")]
        [SerializeField] protected Vector2 defaultOffsetSpine;
        [SerializeField] protected Vector2 defaultOffsetHead;

        [vReadOnly(true)]
        public Vector2 offsetSpine;
        [vReadOnly(true)]
        public Vector2 offsetHead;

        [Header("Tracking")]
        [Tooltip("Follow the Camera Forward")]
        public bool followCamera = true;
        public bool _freezeLookPoint = false;
        [vHideInInspector("followCamera")]
        [Tooltip("Force to follow camera")]
        [SerializeField] protected bool _alwaysFollowCamera = false;
        public virtual bool alwaysFollowCamera { get { return _alwaysFollowCamera; } set { _alwaysFollowCamera = value; } }
        [Tooltip("Ignore the Limits and continue to follow the camera")]
        public bool cancelTrackOutOfAngle = true;
        [Tooltip("Considerer the head animation forward while tracking, try it to see different results")]
        public bool considerHeadAnimationForward;

        [Header("Limits")]
        [vMinMax(minLimit = -180f, maxLimit = 180f)] public Vector2 horizontalAngleLimit = new Vector2(-100, 100);
        [vMinMax(minLimit = -90f, maxLimit = 90f)] public Vector2 verticalAngleLimit = new Vector2(-80, 80);

        [vHelpBox("Animations with vAnimatorTag Behavior will ignore the HeadTrack while is being played")]
        [Header("Ignore AnimatorTags")]
        public List<string> animatorIgnoreTags = new List<string>() { "Attack", "LockMovement", "CustomAction", "IsEquipping", "IgnoreHeadtrack" };

        [vEditorToolbar("Bones")]
        [vHelpBox("Auto Find Bones using Humanoid")]
        public bool autoFindBones = true;
        public Transform head;
        public List<Transform> spine = new List<Transform>();

        [vEditorToolbar("Detection")]
        public float updateTargetInteraction = 1;
        public float distanceToDetect = 10f;
        public LayerMask obstacleLayer = 1 << 0;
        [vHelpBox("Gameobjects Tags to detect")]
        public List<string> tagsToDetect = new List<string>() { "LookAt" };
     
        [vEditorToolbar("HeadTrack Angles")]
      
        [vHelpBox("Angle between character forward and camera forward")]
        [SerializeField, vReadOnly(false)]
        protected Vector2 _desiredlookAngle;
        [vHelpBox("Angle between head forward and character forward")]
        [SerializeField, vReadOnly(false)]
        protected Vector2 _currentLookAngle;
        [vHelpBox("Angle between head forward and camera forward")]
        [SerializeField, vReadOnly(false)]
        protected Vector2 _relativeLookAngle;

        [vSeparator("Optinal Animator paramenters")]
        [vHelpBox("This is an optional to use and update paramenters in the animator using the look angles of the headtrack")]     

        

        [Tooltip("When enable this. The headtrack will update the angles relative to the character forward and camera forward")]
        public bool useDesiredLookAngle;
        [vHideInInspector("useDesiredLookAngle")]
        public string desiredLookAngleH = "HorizontalLookAngle";
        [vHideInInspector("useDesiredLookAngle")]
        public string desiredLookAngleV = "VerticalLookAngle";

        [Tooltip("When enable this. The headtrack will update the angles relative to the Head forward and Character forward")]
        public bool useCurrentAngle;
        [vHideInInspector("useCurrentAngle")]
        public string currentAngleH = "CurrentHorizontalLookAngle";
        [vHideInInspector("useCurrentAngle")]
        public string currentAngleV = "CurrentVerticalLookAngle";

        [Tooltip("When enable this. The headtrack will update the angles relative to the character head forward and camera forward")]
        public bool useRelativeLookAngle;
        [vHideInInspector("useRelativeLookAngle")]
        public string relativeLookAngleH ="HorizontalLookAngleRelative";
        [vHideInInspector("useRelativeLookAngle")]
        public string relativeLookAngleV = "VerticalLookAngleRelative";



        internal UnityEvent onInitUpdate = new UnityEvent();
        internal UnityEvent onFinishUpdate = new UnityEvent();
        internal List<vLookTarget> targetsInArea = new List<vLookTarget>();

        public virtual Camera cameraMain { get; set; }
        public virtual vLookTarget currentLookTarget { get; set; }
        public virtual vLookTarget lastLookTarget { get; set; }
        public virtual Quaternion currentLookRotation { get; set; }
        public virtual bool ignoreSmooth { get; set; }

        protected virtual float yRotation { get; set; }
        protected virtual float xRotation { get; set; }
        protected virtual float _currentHeadWeight { get; set; }
        protected virtual float _currentBodyWeight { get; set; }
        protected virtual Animator animator { get; set; }
        protected virtual vIAnimatorStateInfoController animatorStateInfo { get; set; }
        protected virtual float headHeight { get; set; }
        protected virtual Transform simpleTarget { get; set; }
        protected virtual Vector3 temporaryLookPoint { get; set; }
        protected virtual float temporaryLookTime { get; set; }
        protected virtual vHeadTrackSensor sensor { get; set; }
        protected virtual float interaction { get; set; }
        protected virtual vICharacter vChar { get; set; }
        protected virtual Transform forwardReference { get; set; }

        protected int currentLookAngleH_Hash = -1;
        protected int currentLookAngleV_Hash = -1;
        protected int desiredLookAngleH_Hash = -1;
        protected int desiredLookAngleV_Hash = -1;
        protected int relativeLookAngleH_Hash = -1;
        protected int relativeLookAngleV_Hash = -1;
        

        #endregion

        public virtual float Smooth
        {
            get
            {
                return ignoreSmooth ? 1f : smooth * Time.deltaTime;
            }
        }

        protected virtual Vector3 _currentLocalLookPosition { get; set; }
        protected virtual Vector3 _lastLocalLookPosition { get; set; }

        public virtual float currentVerticalLookAngle { get => _currentLookAngle.x; protected set => _currentLookAngle.x = value; }
        public virtual float currentHorizontalLookAngle { get => _currentLookAngle.y; protected set => _currentLookAngle.y = value; }


        public virtual float desiredVerticalLookAngle{ get => _desiredlookAngle.x; protected set => _desiredlookAngle.x = value; }
        public virtual float desiredHorizontalLookAngle { get => _desiredlookAngle.y; protected set => _desiredlookAngle.y = value; }

        public virtual float relativeVerticalLookAngle{ get => _relativeLookAngle.x; protected set => _relativeLookAngle.x = value; }
        public virtual float relativeHorizontalLookAngle { get => _relativeLookAngle.y; protected set => _relativeLookAngle.y = value; }

        public virtual bool freezeLookPoint { get => _freezeLookPoint; set => _freezeLookPoint = value; }

        public virtual Vector3 currentLookPosition
        {
            get => freezeLookPoint ? transform.TransformPoint(_lastLocalLookPosition) : transform.TransformPoint(_currentLocalLookPosition);
            protected set
            {
                _currentLocalLookPosition = transform.InverseTransformPoint(value);
                if (!freezeLookPoint)
                {
                    _lastLocalLookPosition = _currentLocalLookPosition;
                }
            }
        }

        protected virtual void Start()
        {
            if (!sensor)
            {
                var sensorObj = new GameObject("HeadTrackSensor");
                sensor = sensorObj.AddComponent<vHeadTrackSensor>();
            }

            // updates the headtrack using the late update of the tpinput so we don't need to create another one
            var tpInput = GetComponent<vThirdPersonInput>();
            if (tpInput)
            {
                tpInput.onLateUpdate -= UpdateHeadTrack;
                tpInput.onLateUpdate += UpdateHeadTrack;
            }

            vChar = GetComponent<vICharacter>();
            sensor.headTrack = this;
            cameraMain = Camera.main;
            var layer = LayerMask.NameToLayer("HeadTrack");
            sensor.transform.parent = transform;
            sensor.gameObject.layer = layer;
            sensor.gameObject.tag = transform.tag;
            animatorStateInfo = GetComponent<vIAnimatorStateInfoController>();

           

            Init();
        }

        public virtual void Init()
        {
            currentLookPosition = GetLookPoint();
            _lastLocalLookPosition = _currentLocalLookPosition;
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            if (animator.GetValidParameter(currentAngleH, out AnimatorControllerParameter p)) currentLookAngleH_Hash = p.nameHash;
            if (animator.GetValidParameter(currentAngleV, out p)) currentLookAngleV_Hash = p.nameHash;

            if (animator.GetValidParameter(desiredLookAngleH, out p)) desiredLookAngleH_Hash = p.nameHash;
            if (animator.GetValidParameter(desiredLookAngleV, out p)) desiredLookAngleV_Hash = p.nameHash;

            if (animator.GetValidParameter(relativeLookAngleH, out p)) relativeLookAngleH_Hash = p.nameHash;
            if (animator.GetValidParameter(relativeLookAngleV, out p)) relativeLookAngleV_Hash = p.nameHash;
            if (autoFindBones)
            {
                spine.Clear();
                head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head)
                {
                    if (!forwardReference)
                    {
                        forwardReference = new GameObject("FWRF").transform;
                    }

                    forwardReference.SetParent(head);
                    forwardReference.transform.localPosition = Vector3.zero;
                    forwardReference.transform.rotation = transform.rotation;
                    var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                    if (hips)
                    {
                        var target = head;
                        for (int i = 0; i < 4; i++)
                        {
                            if (target.parent && target.parent.gameObject != hips.gameObject)
                            {
                                spine.Add(target.parent);
                                target = target.parent;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }


            if (head)
            {
                headHeight = Vector3.Distance(transform.position, head.position);
                sensor.transform.position = head.transform.position;
            }
            else
            {
                headHeight = 1f;
                sensor.transform.position = transform.position;
            }
            if (spine.Count == 0)
            {
                Debug.Log("Headtrack Spines missing");
            }

            spine.Reverse();
        }

        protected virtual Vector3 headPoint { get { return transform.position + (transform.up * headHeight); } }

        public virtual void UpdateHeadTrack()
        {
            if (animator == null || !animator.enabled)
            {
                return;
            }

            if (vChar != null && vChar.currentHealth > 0f && animator != null && !vChar.ragdolled)
            {
                onInitUpdate.Invoke();
                if (!freezeLookPoint)
                {
                    currentLookPosition = GetLookPoint();
                }

                SetLookAtPosition(currentLookPosition, _currentHeadWeight, _currentBodyWeight);
                UpdateAngles();
                onFinishUpdate.Invoke();
            }
        }

        public virtual void SetLookAtPosition(Vector3 point, float headWeight, float spineWeight)
        {
            var lookRotation = Quaternion.LookRotation(point - headPoint);
            currentLookRotation = lookRotation;
            var euler = lookRotation.eulerAngles - transform.rotation.eulerAngles;
            var y = NormalizeAngle(euler.y);
            var x = NormalizeAngle(euler.x);
            var eulerB = considerHeadAnimationForward ? forwardReference.eulerAngles - transform.eulerAngles : Vector3.zero;
            currentVerticalLookAngle = Mathf.Clamp(Mathf.Lerp(currentVerticalLookAngle, ((x) - eulerB.NormalizeAngle().x) + Quaternion.Euler(offsetSpine + defaultOffsetSpine).eulerAngles.NormalizeAngle().x, Smooth), verticalAngleLimit.x, verticalAngleLimit.y);
            currentHorizontalLookAngle = Mathf.Clamp(Mathf.Lerp(currentHorizontalLookAngle, ((y) - eulerB.NormalizeAngle().y) + Quaternion.Euler(offsetSpine + defaultOffsetSpine).eulerAngles.NormalizeAngle().y, Smooth), horizontalAngleLimit.x, horizontalAngleLimit.y);

            var xSpine = NormalizeAngle(currentVerticalLookAngle);
            var ySpine = NormalizeAngle(currentHorizontalLookAngle);

            foreach (Transform segment in spine)
            {
                var rotY = Quaternion.AngleAxis((ySpine * spineWeight) / spine.Count, segment.InverseTransformDirection(transform.up));
                segment.rotation *= rotY;
                var rotX = Quaternion.AngleAxis((xSpine * spineWeight) / spine.Count, segment.InverseTransformDirection(transform.TransformDirection(upDownAxis)));
                segment.rotation *= rotX;
            }
            if (head)
            {
                var xHead = NormalizeAngle(currentVerticalLookAngle - (xSpine * spineWeight) + Quaternion.Euler(offsetHead + defaultOffsetHead).eulerAngles.NormalizeAngle().x);
                var yHead = NormalizeAngle(currentHorizontalLookAngle - (ySpine * spineWeight) + Quaternion.Euler(offsetHead + defaultOffsetHead).eulerAngles.NormalizeAngle().y);
                var _rotY = Quaternion.AngleAxis(yHead * headWeight, head.InverseTransformDirection(transform.up));
                head.rotation *= _rotY;
                var _rotX = Quaternion.AngleAxis(xHead * headWeight, head.InverseTransformDirection(transform.TransformDirection(upDownAxis)));
                head.rotation *= _rotX;
            }
        }

        public virtual void UpdateAngles()
        {           
            if (useCurrentAngle)
            {
                if (currentLookAngleH_Hash != -1) animator.SetFloat(currentLookAngleH_Hash, currentHorizontalLookAngle);
                if (currentLookAngleV_Hash != -1) animator.SetFloat(currentLookAngleV_Hash, currentVerticalLookAngle);
            }

            if (useDesiredLookAngle)
            {
                if (desiredLookAngleH_Hash != -1) animator.SetFloat(desiredLookAngleH_Hash, desiredHorizontalLookAngle);
              
                if (desiredLookAngleV_Hash != -1) animator.SetFloat(desiredLookAngleV_Hash, desiredVerticalLookAngle);
            }

            if(useRelativeLookAngle)
            {
                if (relativeLookAngleH_Hash != -1) animator.SetFloat(relativeLookAngleH_Hash, relativeHorizontalLookAngle);
                if (relativeLookAngleV_Hash != -1) animator.SetFloat(relativeLookAngleV_Hash, relativeVerticalLookAngle);
            }
        }

        /// <summary>
        /// Rotate the spine using angles
        /// </summary>
        /// <param name="angleX">Vertical angle</param>
        /// <param name="angleY">Horizontal angle</param>
        public virtual void RotateSpine(float angleX, float angleY)
        {
            var xSpine = NormalizeAngle(angleX);
            var ySpine = NormalizeAngle(angleY);

            foreach (Transform segment in spine)
            {
                var rotY = Quaternion.AngleAxis((ySpine) / spine.Count, segment.InverseTransformDirection(transform.up));
                segment.rotation *= rotY;
                var rotX = Quaternion.AngleAxis((xSpine) / spine.Count, segment.InverseTransformDirection(transform.TransformDirection(upDownAxis)));
                segment.rotation *= rotX;
            }
        }

        public virtual Vector3 desiredLookDirection { get; protected set; }

        public virtual Vector3 LookDirection { get; protected set; }

        protected virtual bool lookConditions
        {
            get
            {
                if (!cameraMain)
                {
                    cameraMain = Camera.main;
                }
                return head != null && (followCamera && cameraMain != null) || (!followCamera && (currentLookTarget || simpleTarget)) || temporaryLookTime > 0;
            }
        }

        protected virtual Vector3 GetLookPoint()
        {
            if (animator == null)
            {
                return Vector3.zero;
            }

            var distanceToLook = 100;
            if (lookConditions && !IgnoreHeadTrack())
            {
                desiredLookDirection = transform.forward;
                if (temporaryLookTime <= 0)
                {
                    var lookPosition = headPoint + (transform.forward * distanceToLook);
                    if (followCamera)
                    {
                        lookPosition = (cameraMain.transform.position + (cameraMain.transform.forward * distanceToLook));
                    }

                    desiredLookDirection = lookPosition - headPoint;

                    if ((followCamera && !alwaysFollowCamera) || !followCamera)
                    {

                        if (simpleTarget != null)
                        {
                            desiredLookDirection = simpleTarget.position - headPoint;
                            if (currentLookTarget && currentLookTarget == lastLookTarget)
                            {
                                currentLookTarget.ExitLook(this);
                                lastLookTarget = null;
                            }
                        }
                        else if (currentLookTarget != null && (currentLookTarget.ignoreHeadTrackAngle || TargetIsOnRange(currentLookTarget.lookPoint - headPoint)) && currentLookTarget.IsVisible(headPoint, obstacleLayer))
                        {
                            desiredLookDirection = currentLookTarget.lookPoint - headPoint;
                            if (currentLookTarget != lastLookTarget)
                            {
                                currentLookTarget.EnterLook(this);
                                lastLookTarget = currentLookTarget;
                            }
                        }
                        else if (currentLookTarget && currentLookTarget == lastLookTarget)
                        {
                            currentLookTarget.ExitLook(this);
                            lastLookTarget = null;
                        }
                    }
                }
                else
                {
                    desiredLookDirection = temporaryLookPoint - headPoint;
                    temporaryLookTime -= Time.deltaTime;
                    if (currentLookTarget && currentLookTarget == lastLookTarget)
                    {
                        currentLookTarget.ExitLook(this);
                        lastLookTarget = null;
                    }
                }

                var angle = GetTargetAngle(desiredLookDirection);
                if (cancelTrackOutOfAngle && (lastLookTarget == null || !lastLookTarget.ignoreHeadTrackAngle))
                {
                    if (TargetIsOnRange(desiredLookDirection))
                    {
                        if (animator.GetBool("IsStrafing") && !IsAnimatorTag("Upperbody Pose"))
                        {
                            SmoothValues(strafeHeadWeight, strafeBodyWeight, angle.x, angle.y);
                        }
                        else if (animator.GetBool("IsStrafing") && IsAnimatorTag("Upperbody Pose"))
                        {
                            SmoothValues(aimingHeadWeight, aimingBodyWeight, angle.x, angle.y);
                        }
                        else
                        {
                            SmoothValues(freeHeadWeight, freeBodyWeight, angle.x, angle.y);
                        }
                    }
                    else
                    {
                        SmoothValues();
                    }
                }
                else
                {
                    if (animator.GetBool("IsStrafing") && !IsAnimatorTag("Upperbody Pose"))
                    {
                        SmoothValues(strafeHeadWeight, strafeBodyWeight, angle.x, angle.y);
                    }
                    else if (animator.GetBool("IsStrafing") && IsAnimatorTag("Upperbody Pose"))
                    {
                        SmoothValues(aimingHeadWeight, aimingBodyWeight, angle.x, angle.y);
                    }
                    else
                    {
                        SmoothValues(freeHeadWeight, freeBodyWeight, angle.x, angle.y);
                    }
                }
                if (targetsInArea.Count > 1)
                {
                    SortTargets();
                }
            }
            else
            {
                SmoothValues();
                if (targetsInArea.Count > 1)
                {
                    SortTargets();
                }
            }

            var rotA = Quaternion.AngleAxis(yRotation, transform.up);
            var rotB = Quaternion.AngleAxis(xRotation, transform.right);
            var finalRotation = (rotA * rotB);
            var lookDirection = finalRotation * transform.forward;
            LookDirection = lookDirection;
            _desiredlookAngle = GetTargetAngle(cameraMain.transform.forward);
            _relativeLookAngle = -(GetTargetAngle(lookDirection) - _desiredlookAngle);


            return headPoint + (lookDirection * distanceToLook);
        }

        protected virtual Vector2 GetTargetAngle(Vector3 direction)
        {
            if (direction.magnitude == 0) return Vector2.zero;
            var lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            var angleResult = lookRotation.eulerAngles - transform.eulerAngles;

            return new Vector2(angleResult.NormalizeAngle().x, angleResult.NormalizeAngle().y);
        }

        protected virtual bool TargetIsOnRange(Vector3 direction)
        {
            var angle = GetTargetAngle(direction);
            return (angle.x >= verticalAngleLimit.x && angle.x <= verticalAngleLimit.y && angle.y >= horizontalAngleLimit.x && angle.y <= horizontalAngleLimit.y);
        }

        public virtual void SetAlwaysFollowCamera(bool value)
        {
            alwaysFollowCamera = value;
        }

        /// <summary>
        /// Set vLookTarget
        /// </summary>
        /// <param name="target"></param>
        public virtual void SetLookTarget(vLookTarget target, bool priority = false)
        {
            if (!targetsInArea.Contains(target))
            {
                targetsInArea.Add(target);
            }

            if (priority)
            {
                currentLookTarget = target;
            }
        }

        /// <summary>
        /// Set Simple target
        /// </summary>
        /// <param name="target"></param>
        public virtual void SetLookTarget(Transform target)
        {
            simpleTarget = target;
        }

        /// <summary>
        /// Set a temporary look point to headtrack   
        /// </summary>
        /// <param name="point">look point</param>
        /// <param name="time">time to stay looking</param>
        public virtual void SetTemporaryLookPoint(Vector3 point, float time = 1f)
        {
            temporaryLookPoint = point;
            temporaryLookTime = time;
        }

        public virtual void RemoveLookTarget(vLookTarget target)
        {
            if (targetsInArea.Contains(target))
            {
                targetsInArea.Remove(target);
            }

            if (currentLookTarget == target)
            {
                currentLookTarget = null;
            }
        }

        public virtual void RemoveLookTarget(Transform target)
        {
            if (simpleTarget == target)
            {
                simpleTarget = null;
            }
        }

        /// <summary>
        /// Make angle to work with -180 and 180 
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        protected virtual float NormalizeAngle(float angle)
        {
            if (angle > 180)
            {
                angle -= 360;
            }
            else if (angle < -180)
            {
                angle += 360;
            }

            return angle;
        }

        protected virtual void ResetValues()
        {
            _currentHeadWeight = 0;
            _currentBodyWeight = 0;
            yRotation = 0;
            xRotation = 0;
        }

        protected virtual void SmoothValues(float _headWeight = 0, float _bodyWeight = 0, float _x = 0, float _y = 0)
        {
            _currentHeadWeight = Mathf.Lerp(_currentHeadWeight, _headWeight, Smooth);
            _currentBodyWeight = Mathf.Lerp(_currentBodyWeight, _bodyWeight, Smooth);
            yRotation = Mathf.Lerp(yRotation, _y, Smooth);
            xRotation = Mathf.Lerp(xRotation, _x, Smooth);
            yRotation = Mathf.Clamp(yRotation, horizontalAngleLimit.x, horizontalAngleLimit.y);
            xRotation = Mathf.Clamp(xRotation, verticalAngleLimit.x, verticalAngleLimit.y);
        }

        protected virtual void SortTargets()
        {
            interaction += Time.deltaTime;
            if (interaction > updateTargetInteraction)
            {
                interaction -= updateTargetInteraction;
                if (targetsInArea == null || targetsInArea.Count < 2)
                {
                    if (targetsInArea != null && targetsInArea.Count > 0)
                    {
                        currentLookTarget = targetsInArea[0];
                    }

                    return;
                }

                for (int i = targetsInArea.Count - 1; i >= 0; i--)
                {
                    if (targetsInArea[i] == null)
                    {
                        targetsInArea.RemoveAt(i);
                    }
                }
                targetsInArea.Sort(delegate (vLookTarget c1, vLookTarget c2)
                {
                    return Vector3.Distance(this.transform.position, c1 != null ? c1.transform.position : Vector3.one * Mathf.Infinity).CompareTo
                        ((Vector3.Distance(this.transform.position, c2 != null ? c2.transform.position : Vector3.one * Mathf.Infinity)));
                });
                if (targetsInArea.Count > 0)
                {
                    currentLookTarget = targetsInArea[0];
                }
            }
        }

        public virtual void OnDetect(Collider other)
        {
            if (tagsToDetect.Contains(other.gameObject.tag) && other.GetComponent<vLookTarget>() != null)
            {
                currentLookTarget = other.GetComponent<vLookTarget>();
                var headTrack = other.GetComponentInParent<vHeadTrack>();
                if (!targetsInArea.Contains(currentLookTarget) && (headTrack == null || headTrack != this))
                {
                    targetsInArea.Add(currentLookTarget);
                    SortTargets();
                    currentLookTarget = targetsInArea[0];
                }
            }
        }

        public virtual void OnLost(Collider other)
        {
            if (tagsToDetect.Contains(other.gameObject.tag) && other.GetComponentInParent<vLookTarget>() != null)
            {
                var _currentLookTarget = other.GetComponentInParent<vLookTarget>();

                if (targetsInArea.Contains(_currentLookTarget))
                {
                    targetsInArea.Remove(_currentLookTarget);


                    if (_currentLookTarget == lastLookTarget)
                    {
                        _currentLookTarget.ExitLook(this);
                    }
                }
                SortTargets();
                if (targetsInArea.Count > 0)
                {
                    currentLookTarget = targetsInArea[0];
                }
                else
                {
                    currentLookTarget = null;
                }
            }
        }

        public virtual bool IgnoreHeadTrack()
        {
            if (animatorIgnoreTags.Exists(tag => IsAnimatorTag(tag)))
            {
                return true;
            }
            return false;
        }

        public virtual bool IsAnimatorTag(string tag)
        {
            if (animator == null)
            {
                return false;
            }

            if (animatorStateInfo.isValid())
            {
                if (animatorStateInfo.animatorStateInfos.HasTag(tag))
                {
                    return true;
                }
            }
            return false;
        }
    }
}