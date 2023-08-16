using UnityEngine;
namespace Invector.vCharacterController.vActions
{
    /// <summary>
    /// Define all the Action Controllers.
    /// </summary>
    public interface IActionController
    {
        bool enabled { get; set; }
        GameObject gameObject { get; }
        Transform transform { get; }
        string name { get; }
        System.Type GetType();
    }
    /// <summary>
    /// Used to receive Event when do action.
    /// Ps. Need implementation 
    /// </summary>
    public interface IActionReceiver : IActionController
    {
        void OnReceiveAction(vTriggerGenericAction actionInfo);
    }

   
    /// <summary>
    /// Used to register  the event <see cref="vCharacter.onActionEnter"/> in the <see cref="vCharacter"/>. Ps.Need implementation  
    /// </summary>
    public interface IActionEnterListener : IActionController
    {
        void OnActionEnter(Collider actionCollider);
    }
    /// <summary>
    /// Used to register  the event <see cref="vCharacter.onActionExit"/> in the <see cref="vCharacter"/>. Ps.Need implementation 
    /// </summary>
    public interface IActionExitListener : IActionController
    {
        void OnActionExit(Collider actionCollider);
    }
    /// <summary> 
    /// Used to register  the event <see cref="vCharacter.onActionStay"/> in the <see cref="vCharacter"/>. Ps. Need implementation 
    /// </summary>
    public interface IActionStayListener : IActionController
    {
        void OnActionStay(Collider actionCollider);

    }
    /// <summary>
    /// Used to register  the events <see cref="vCharacter.onActionEnter"/>,<see cref="vCharacter.onActionStay"/> and <see cref="vCharacter.onActionExit"/> in the <see cref="vCharacter"/>. Depending of options enabled (<see cref="vActionListener.actionEnter"/>,  <see cref="vActionListener.actionStay"/> and <see cref="vActionListener.actionExit"/>). Ps. Need implementation 
    /// </summary>
    public interface IActionListener : IActionEnterListener, IActionExitListener, IActionStayListener
    {
        bool actionEnter { get; set; }
        bool actionExit { get; set; }
        bool actionStay { get; set; }       

    }
    /// <summary>
    /// Implementation of the <see cref="IActionListener"/>.
    /// Used to register  the events <see cref="vCharacter.onActionEnter"/>,<see cref="vCharacter.onActionStay"/> and <see cref="vCharacter.onActionExit"/> in the <see cref="vCharacter"/>. 
    /// Depending of options enabled (<see cref="actionEnter"/>,  <see cref="actionStay"/> and <see cref="actionExit"/>). Override <seealso cref="SetUpListener"/> to change default options (all enabled)
    /// </summary>   
    public abstract class vActionListener : vMonoBehaviour, IActionListener
    {
        public bool actionEnter { get; set; }
        public bool actionExit { get; set; }
        public bool actionStay { get; set; }
        protected virtual void Awake()
        {
            SetUpListener();
        }

        /// <summary>
        /// Called On Awake
        /// </summary>
        protected virtual void SetUpListener()
        {
            ///Set what kind of action need to use;
            actionEnter = true;///Use Trigger Enter
            actionExit = true; ///Use Trigger Exit 
            actionStay = true; ///Use Trigger Stay
        }

        protected virtual void Start()
        {
           
        }

        public virtual void OnActionEnter(Collider other)
        {

        }

        public virtual void OnActionStay(Collider other)
        {

        }

        public virtual void OnActionExit(Collider other)
        {

        }


    }
    
    [System.Serializable]
    public class vOnActionHandle : UnityEngine.Events.UnityEvent<vTriggerGenericAction>
    {

    }   
}