using UnityEngine;

namespace Invector
{
    [System.Serializable]
    public class OnDead : UnityEngine.Events.UnityEvent<GameObject> { }
    public interface vIHealthController : vIDamageReceiver
    {      
        /// <summary>
        /// Event called when <seealso cref="currentHealth"/>  is zero or less
        /// </summary>
        OnDead onDead { get; }
        /// <summary>
        /// Current Health value
        /// </summary>
        float currentHealth { get; }
        /// <summary>
        /// Max Health value
        /// </summary>
        int MaxHealth { get; }
        /// <summary>
        /// Check if  <seealso cref="currentHealth"/>  is zero or less
        /// </summary>
        bool isDead { get; set; }
        /// <summary>
        /// Encrease or Decrease <seealso cref="currentHealth"/>  respecting the <seealso cref="MaxHealth"/>
        /// </summary>
        /// <param name="value">value</param>
        void AddHealth(int value);
        /// <summary>
        /// Change <seealso cref="currentHealth"/> respecting the <seealso cref="MaxHealth"/>
        /// </summary>
        /// <param name="value">value</param>
        void ChangeHealth(int value);
        /// <summary>
        /// Change the Max Health value
        /// </summary>
        /// <param name="value">value</param>
        void ChangeMaxHealth(int value);
        /// <summary>
        /// Reset's current health to specific health value
        /// </summary>
        /// <param name="health">target health</param>
        void ResetHealth(float health);
        /// <summary>
        /// Reset's current health to max health
        /// </summary>
        void ResetHealth();
      
    }
  
    public static class vHealthControllerHelper
    {
        static vIHealthController GetHealthController(this GameObject gameObject)
        {
            return gameObject.GetComponent<vIHealthController>();
        }

        /// <summary>
        /// Encrease or Decrease <seealso cref="currentHealth"/>  respecting the <seealso cref="MaxHealth"/>
        /// </summary>
        /// <param name="receiver">Target to GetComponent <seealso cref="vIHealthController"/> </param>
        /// <param name="health"></param>
        public static void AddHealth(this GameObject receiver, int health)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {              
                healthController.AddHealth(health);
            }
        }
        /// <summary>
        /// Change <seealso cref="currentHealth"/> respecting the <seealso cref="MaxHealth"/>
        /// </summary>
        /// <param name="receiver">Target to GetComponent <seealso cref="vIHealthController"/> </param>
        /// <param name="health"></param>
        public static void ChangeHealth(this GameObject receiver, int health)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {
                healthController.ChangeHealth(health);
            }
        }

        /// <summary>
        /// Change the Max Health value
        /// </summary>
        /// <param name="receiver">Target to GetComponent <seealso cref="vIHealthController"/> </param>
        /// <param name="health"></param>
        public static void ChangeMaxHealth(this GameObject receiver, int health)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {
                healthController.ChangeMaxHealth(health);
            }
        }

        /// <summary>
        /// Check if GameObject Has a vIHealthController 
        /// </summary>
        /// <param name="gameObject">Target to GetComponent <seealso cref="vIHealthController"/> </param>
        /// <returns></returns>
        public static bool HasHealth(this GameObject gameObject)
        {
            return gameObject.GetHealthController() != null;
        }

        /// <summary>
        /// Check if GameObject is dead
        /// </summary>
        /// <param name="gameObject">Target to GetComponent <seealso cref="vIHealthController"/> </param>
        /// <returns>return true if GameObject does not has a vIHealthController or currentHealth is less or equals zero </returns>
        public static bool IsDead(this GameObject gameObject)
        {
            var health = gameObject.GetHealthController();
            return health == null || health.isDead;
        }

        /// <summary>
        /// Reset's current health to specific health value
        /// </summary>
        /// <param name="receiver">Target to GetComponent <seealso cref="vIHealthController"/> </param>
        /// <param name="health">target health</param>
        public static void ResetHealth(this GameObject receiver, float health)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {
                healthController.ResetHealth(health);
            }
        }
        /// <summary>
        /// Reset's current health to max health
        /// </summary>
        /// <param name="receiver">Target to GetComponent <seealso cref="vIHealthController"/> </param>
        public static void ResetHealth(this GameObject receiver)
        {
            var healthController = receiver.GetHealthController();
            if (healthController != null)
            {
                healthController.ResetHealth();
            }
        }
    }
}

