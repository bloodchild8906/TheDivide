using UnityEngine;
namespace Invector.vCharacterController
{
    public class vRagdollCollision
    {

        private GameObject sender;

        private Collision collision;

        private float impactForce;
        /// <summary>
        /// Gameobjet receiver of collision info
        /// </summary>
        public GameObject Sender { get { return sender; } }
        /// <summary>
        /// Collision info 
        /// </summary>
        public Collision Collision { get { return collision; } }
        /// <summary>
        /// Magnitude of relative linear velocity of the two colliding objects
        /// </summary>
        public float ImpactForce { get { return impactForce; } }

        /// <summary>
        /// Create a New collision info seu trouxa
        /// </summary>
        /// <param name="sender">current gameobjet</param>
        /// <param name="collision">current collision info</param>
        public vRagdollCollision(GameObject sender, Collision collision)
        {
            this.sender = sender;
            this.collision = collision;
            this.impactForce = collision.relativeVelocity.magnitude;
        }
    }
}
