using UnityEngine;

namespace Invector.vCharacterController
{
    [vClassHeader("DAMAGE RECEIVER", "You can add damage multiplier for example causing twice damage on Headshots", openClose = false)]
    public partial class vDamageReceiver : vMonoBehaviour, vIDamageReceiver
    {

        [vEditorToolbar("Default")]
        public GameObject targetReceiver;
        public vIHealthController healthController;
        public float damageMultiplier = 1f;
        [HideInInspector]
        public vRagdoll ragdoll;
        public bool overrideReactionID;
        [vHideInInspector("overrideReactionID")]
        public int reactionID;
        [vEditorToolbar("Random")]
        public bool useRandomValues;
        [vHideInInspector("useRandomValues")]
        public bool fixedValues;
        [vHideInInspector("useRandomValues")]
        public float minDamageMultiplier, maxDamageMultiplier;
        [vHideInInspector("useRandomValues")]
        public int minReactionID, maxReactionID;
        [vHideInInspector("useRandomValues;fixedValues"), Tooltip("Change Between 0 and 100")]
        public float changeToMaxValue;
        public UnityEngine.Events.UnityEvent OnGetMaxValue;
        
        [vEditorToolbar("Events")]
        [SerializeField] protected OnReceiveDamage _onStartReceiveDamage = new OnReceiveDamage();
        [SerializeField] protected OnReceiveDamage _onReceiveDamage = new OnReceiveDamage();
       
        public OnReceiveDamage onStartReceiveDamage { get { return _onStartReceiveDamage; } protected set { _onStartReceiveDamage = value; } }
        public OnReceiveDamage onReceiveDamage { get { return _onReceiveDamage; } protected set { _onReceiveDamage = value; } }

        protected virtual void Start()
        {
            ragdoll = GetComponentInParent<vRagdoll>();
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collision != null)
            {
                if (ragdoll && ragdoll.isActive)
                {
                    ragdoll.OnRagdollCollisionEnter(new vRagdollCollision(this.gameObject, collision));
                }
            }
        }

        public virtual void TakeDamage(vDamage damage)
        {
          
            if (healthController == null && targetReceiver)
                healthController = targetReceiver.GetComponent<vIHealthController>();
            else if (healthController == null)
                healthController = GetComponentInParent<vIHealthController>();

            if (healthController != null)
            {
                onStartReceiveDamage.Invoke(damage);
                var _damage = ApplyDamageModifiers(damage);
                healthController.TakeDamage(_damage);
                onReceiveDamage.Invoke(_damage);               
            }
        }

        public virtual vDamage ApplyDamageModifiers(vDamage damage)
        {          
            float multiplier = (useRandomValues && !fixedValues) ? Random.Range(minDamageMultiplier, maxDamageMultiplier) :
                               (useRandomValues && fixedValues) ? randomChange ? maxDamageMultiplier : minDamageMultiplier : damageMultiplier;
            var _damage = new vDamage(damage);
            _damage.damageValue *= multiplier;
            if (multiplier == maxDamageMultiplier) OnGetMaxValue.Invoke();

            OverrideReaction(ref _damage);
            return _damage;
        }

        protected virtual void OverrideReaction(ref vDamage damage)
        {
            if (overrideReactionID)
            {
                if (useRandomValues && !fixedValues) damage.reaction_id = Random.Range(minReactionID, maxReactionID);
                else if (useRandomValues && fixedValues) damage.reaction_id = randomChange ? maxReactionID : minReactionID;
                else
                    damage.reaction_id = reactionID;
            }
        }

        protected virtual bool randomChange
        {
            get
            {
                return Random.Range(0f, 100f) < changeToMaxValue;
            }
        }
    }
}