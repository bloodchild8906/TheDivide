using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;
public class vEnableDisableByInputDevice : MonoBehaviour
{
    public InputDevice inputDevice;
    public CheckMethod methodToCheck;
    public enum CheckMethod
    {
        Equals,
        Different,
    }
    void Start()
    {
        vInput.instance.onChangeInputType -= OnChangeInput;
        vInput.instance.onChangeInputType += OnChangeInput;
        OnChangeInput(vInput.instance.inputDevice);
    }

    public void OnChangeInput(InputDevice type)
    {       
        bool validate = methodToCheck == CheckMethod.Different ? type != inputDevice : type == inputDevice;
        if(gameObject.activeSelf!=validate)gameObject.SetActive(validate);      
    }
  
}
