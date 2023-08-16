using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
    public static class vCharacterExtensions
    {

        /// <summary>
        /// Load all <see cref="vCharacterController.vActions.IActionController"/> and derivatives  in character gameObject to register to events <see cref="vCharacterController.vCharacter.onActionEnter"/>,<see cref="vCharacterController.vCharacter.onActionStay"/> and <see cref="vCharacterController.vCharacter.onActionExit"/>.
        /// </summary>
        /// <param name="character">Target <seealso cref="vCharacterController.vCharacter>"/></param>
        public static void LoadActionControllers(this vCharacterController.vCharacter character, bool debug = false)
        {
            var actionControllers = character.GetComponentsInChildren<vCharacterController.vActions.IActionController>();
            for (int i = 0; i < actionControllers.Length; i++)
            {
                if (actionControllers[i].enabled)
                {
                    if (actionControllers[i] is vCharacterController.vActions.IActionListener)
                    {
                        var actionListener = actionControllers[i] as vCharacterController.vActions.IActionListener;

                        {
                            if (actionListener.actionEnter)
                            {
                                character.onActionEnter.RemoveListener(actionListener.OnActionEnter);
                                character.onActionEnter.AddListener(actionListener.OnActionEnter);
                                if (debug) Debug.Log("Register Action Enter event to the " + actionListener.GetType().Name);
                            }

                            if (actionListener.actionStay)
                            {
                                character.onActionStay.RemoveListener(actionListener.OnActionStay);
                                character.onActionStay.AddListener(actionListener.OnActionStay);
                                if (debug) Debug.Log("Register Action Stay event to the " + actionListener.GetType().Name);
                            }

                            if (actionListener.actionExit)
                            {
                                character.onActionExit.RemoveListener(actionListener.OnActionExit);
                                character.onActionExit.AddListener(actionListener.OnActionExit);
                                if (debug) Debug.Log("Register action Exit event to the " + actionListener.GetType().Name);
                            }

                        }
                    }
                    else
                    {
                        if (actionControllers[i] is vCharacterController.vActions.IActionEnterListener)
                        {
                            character.onActionEnter.RemoveListener((actionControllers[i] as vCharacterController.vActions.IActionEnterListener).OnActionEnter);
                            character.onActionEnter.AddListener((actionControllers[i] as vCharacterController.vActions.IActionEnterListener).OnActionEnter);
                            if (debug) Debug.Log("Register Action Enter event to the " + actionControllers[i].GetType().Name);
                        }

                        if (actionControllers[i] is vCharacterController.vActions.IActionStayListener)
                        {
                            character.onActionStay.RemoveListener((actionControllers[i] as vCharacterController.vActions.IActionStayListener).OnActionStay);
                            character.onActionStay.AddListener((actionControllers[i] as vCharacterController.vActions.IActionStayListener).OnActionStay);
                            if (debug) Debug.Log("Register Action Stay event to the " + actionControllers[i].GetType().Name);
                        }

                        if (actionControllers[i] is vCharacterController.vActions.IActionExitListener)
                        {
                            character.onActionExit.RemoveListener((actionControllers[i] as vCharacterController.vActions.IActionExitListener).OnActionExit);
                            character.onActionExit.AddListener((actionControllers[i] as vCharacterController.vActions.IActionExitListener).OnActionExit);
                            if (debug) Debug.Log("Register Action Exit event to the " + actionControllers[i].GetType().Name);
                        }
                    }
                }
            }
        }
    }
}