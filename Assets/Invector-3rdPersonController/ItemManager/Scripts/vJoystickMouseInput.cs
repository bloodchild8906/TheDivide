using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.EventSystems;

public class vJoystickMouseInput : BaseInput
{
    public StandaloneInputModule inputModule;
    public RectTransform cursor;
    public JoystickAxisInput joystickAxisInput;

    [System.Serializable]
    public class JoystickAxisInput
    {
        public string vertical = "LeftAnalogVertical";
        public string horizontal = "LeftAnalogHorizontal";
        public float horizontalAxis
        {
            get
            {
                return Input.GetAxis(horizontal);
            }
        }
        public float verticalAxis
        {
            get
            {
                return Input.GetAxis(vertical);
            }
        }
    }
    public BaseInput oldOverride;

    protected override void OnEnable()
    {
        if (inputModule)
        {
            inputModule.inputOverride = this;
        }
    }

    protected override void OnDisable()
    {
        if (inputModule)
        {
            inputModule.inputOverride = oldOverride;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (!inputModule)
            inputModule = FindObjectOfType<StandaloneInputModule>();
        if (inputModule)
        {
            oldOverride = inputModule.inputOverride;
            inputModule.inputOverride = this;
        }

    }

    protected Vector2 CursorPosition = Vector2.zero;
    public float mouseSpeed = 4;

    public override Vector2 mousePosition
    {
        get
        {
            if (vInput.instance.inputDevice == InputDevice.Joystick)
            {
                if (cursor && (!cursor.gameObject.activeSelf || Cursor.visible))
                {
                    Cursor.visible = false;
                    CursorPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                    cursor.gameObject.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(null);
                }
                CursorPosition.x += joystickAxisInput.horizontalAxis * mouseSpeed;
                CursorPosition.x = Mathf.Clamp(CursorPosition.x, 0, Screen.width);
                CursorPosition.y += joystickAxisInput.verticalAxis * mouseSpeed;
                CursorPosition.y = Mathf.Clamp(CursorPosition.y, 0, Screen.height);
            }
            else
            {
                if (cursor && cursor.gameObject.activeSelf)
                {
                    Cursor.visible = true;
                    cursor.gameObject.SetActive(false);
                }
                CursorPosition = base.mousePosition;
            }
            if (cursor) cursor.position = CursorPosition;
            return this.CursorPosition;
        }
    }
    public virtual string submitButton
    {
        get
        {
            return this.inputModule.submitButton;
        }
    }
    public override bool GetMouseButton(int button)
    {
        switch (vInput.instance.inputDevice)
        {
            case InputDevice.Joystick:
                if (button == 0)
                    return Input.GetButton(submitButton);
                else return base.GetMouseButton(button);

            default:
                return base.GetMouseButton(button);
        }
    }
    public override bool GetMouseButtonUp(int button)
    {
        switch (vInput.instance.inputDevice)
        {
            case InputDevice.Joystick:
                if (button == 0)
                    return Input.GetButtonUp(submitButton);
                else return base.GetMouseButtonUp(button);

            default:
                return base.GetMouseButtonUp(button);
        }
    }
    public override bool GetMouseButtonDown(int button)
    {
        switch (vInput.instance.inputDevice)
        {
            case InputDevice.Joystick:
                if (button == 0)
                    return Input.GetButtonDown(submitButton);
                else return base.GetMouseButtonDown(button);

            default:
                return base.GetMouseButtonDown(button);
        }
    }
}
