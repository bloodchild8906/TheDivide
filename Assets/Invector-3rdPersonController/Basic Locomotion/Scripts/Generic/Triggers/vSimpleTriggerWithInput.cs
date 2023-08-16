using Invector;
using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[vClassHeader("Simple Trigger Input")]
public class vSimpleTriggerWithInput : vSimpleTrigger
{
    public InputType inputType = InputType.GetButtonDown;

    [Tooltip("Input to make the action")]
    public GenericInput actionInput = new GenericInput("E", "A", "A");

    public enum InputType
    {
        GetButtonDown,
        GetDoubleButton,
        GetButtonTimer
    };

    [vHelpBox("Time you have to hold the button *Only for GetButtonTimer*")]
    public float buttonTimer = 3f;
    [vHelpBox("Add delay to start the input count *Only for GetButtonTimer*")]
    public float inputDelay = 0.1f;
    [vHelpBox("Time to press the button twice *Only for GetDoubleButton*")]
    public float doubleButtomTime = 0.25f;

    public float _currentInputDelay;
    public float currentButtonTimer;

    public UnityEvent OnPressButton;
    public UnityEvent OnCancelButtonTimer;
    public OnUpdateValue OnUpdateButtonTimer;
    
    void Update()
    {
        if (!other)
        {
            _currentInputDelay = inputDelay;
            return;
        }

        // GetButtonDown
        if (inputType == InputType.GetButtonDown)
        {
            if (actionInput.GetButtonDown())
            {
                OnPressButton.Invoke();
            }
        }
        // GetDoubleButton
        else if (inputType == InputType.GetDoubleButton)
        {
            if (actionInput.GetDoubleButtonDown(doubleButtomTime))
            {
                OnPressButton.Invoke();
            }
        }
        // GetButtonTimer (Hold Button)
        else if (inputType == InputType.GetButtonTimer)
        {
            if (_currentInputDelay <= 0)
            {
                var up = false;
                var t = 0f;

                // call the OnPressButton event after the buttomTimer is finished
                if (actionInput.GetButtonTimer(ref t, ref up, buttonTimer))
                {
                    _currentInputDelay = inputDelay;
                    OnPressButton.Invoke();
                }

                // update the button timer
                if (actionInput.inButtomTimer)
                {
                    UpdateButtonTimer(t);
                }

                // reset the buttonTimer if you release the button before finishing
                if (up)
                    CancelButtonTimer();
            }
            else
            {
                _currentInputDelay -= Time.deltaTime;
            }
        }
    }

    public void UpdateButtonTimer(float value)
    {
        if (value != currentButtonTimer)
        {
            currentButtonTimer = value;
            OnUpdateButtonTimer.Invoke(value);
        }
    }

    private void CancelButtonTimer()
    {
        OnCancelButtonTimer.Invoke();
        _currentInputDelay = inputDelay;
        UpdateButtonTimer(0);
    }

    [System.Serializable]
    public class OnUpdateValue : UnityEvent<float>
    {

    }
}
