using UnityEngine;

#if MOBILE_INPUT
using UnityStandardAssets.CrossPlatformInput;
#endif

namespace Invector.vCharacterController
{
    public class vMousePositionHandler : MonoBehaviour
    {
        public Camera mainCamera;
        protected static vMousePositionHandler _instance;
        public static vMousePositionHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<vMousePositionHandler>();
                }

                if (_instance == null)
                {
                    var go = new GameObject("MousePositionHandler");
                    _instance = go.AddComponent<vMousePositionHandler>();
                    _instance.mainCamera = Camera.main;
                }
                return _instance;
            }
        }

        public string joystickHorizontalAxis = "RightAnalogHorizontal";
        public string joystickVerticalAxis = "RightAnalogVertical";
        public float joystickSensitivity = 25f;
        public bool clampScreen = true;

        public Vector2 joystickMousePos;

        public virtual void SetMousePosition(Vector2 pos)
        {
            joystickMousePos = pos;
        }

        public virtual Vector2 mousePosition
        {
            get
            {
                var inputDevice = vInput.instance.inputDevice;
                switch (inputDevice)
                {
                    case InputDevice.MouseKeyboard:
                        return Input.mousePosition;
                    case InputDevice.Joystick:
                        joystickMousePos.x += Input.GetAxis("RightAnalogHorizontal") * joystickSensitivity;
                        joystickMousePos.x = Mathf.Clamp(joystickMousePos.x, -(Screen.width * 0.5f), (Screen.width * 0.5f));
                        joystickMousePos.y += Input.GetAxis("RightAnalogVertical") * joystickSensitivity;
                        joystickMousePos.y = Mathf.Clamp(joystickMousePos.y, -(Screen.height * 0.5f), (Screen.height * 0.5f));
                        var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                        var result = joystickMousePos + screenCenter;
                        result.x = Mathf.Clamp(result.x, 0, Screen.width);
                        result.y = Mathf.Clamp(result.y, 0, Screen.height);
                        return result;
                    case InputDevice.Mobile:
                        if (clampScreen)
                        {
#if MOBILE_INPUT
                            joystickMousePos.x += CrossPlatformInputManager.GetAxis("RightAnalogHorizontal") * joystickSensitivity;
                            joystickMousePos.x = Mathf.Clamp(joystickMousePos.x, -(Screen.width * 0.5f), (Screen.width * 0.5f));
                            joystickMousePos.y += CrossPlatformInputManager.GetAxis("RightAnalogVertical") * joystickSensitivity;
                            joystickMousePos.y = Mathf.Clamp(joystickMousePos.y, -(Screen.height * 0.5f), (Screen.height * 0.5f));
                            var mobileScreenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                            var mobileResult = joystickMousePos + mobileScreenCenter;
                            mobileResult.x = Mathf.Clamp(mobileResult.x, 0, Screen.width);
                            mobileResult.y = Mathf.Clamp(mobileResult.y, 0, Screen.height);
                            return mobileResult;
#else
                                                    return Input.GetTouch(0).deltaPosition;
#endif
                        }
                        else
                        {
                            return joystickMousePos;
                        }

                    default: return Input.mousePosition;
                }
            }
        }

        public virtual Vector3 WorldMousePosition(LayerMask castLayer, out Collider collider)
        {
            if (!mainCamera)
            {
                if (!Camera.main)
                {
                    Debug.LogWarning("Trying to get the world mouse position but a MainCamera is missing from the scene");
                    collider = null;
                    return Vector3.zero;
                }
                else
                {
                    mainCamera = Camera.main;
                    collider = null;
                    return Vector3.zero;
                }
            }
            else
            {
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, mainCamera.farClipPlane, castLayer))
                {
                    collider = hit.collider;
                    return hit.point;
                }
                else
                {
                    collider = null;
                    return ray.GetPoint(mainCamera.farClipPlane);
                }
            }
        }

        public virtual bool CastWorldMousePosition(LayerMask castLayer,out RaycastHit hit,float distance =0)
        {
            if (!mainCamera)
            {
                if (!Camera.main)
                {
                    Debug.LogWarning("Trying to get the world mouse position but a MainCamera is missing from the scene");
                    hit = default;
                    return false;
                }
                else
                {
                    mainCamera = Camera.main;
                    hit = default;
                    return false;
                }
            }
            else
            {
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
               

                if (Physics.Raycast(ray, out hit,distance==0? mainCamera.farClipPlane:distance, castLayer))
                {
                    return true;
                }
                else
                {
                    hit = default;
                    return false;
                }
            }
        }

        public virtual bool CastWorldMousePosition(LayerMask castLayer, out RaycastHit hit, float distance = 0,float radius =0f)
        {
            if (!mainCamera)
            {
                if (!Camera.main)
                {
                    Debug.LogWarning("Trying to get the world mouse position but a MainCamera is missing from the scene");
                    hit = default;
                    return false;
                }
                else
                {
                    mainCamera = Camera.main;
                    hit = default;
                    return false;
                }
            }
            else
            {
                if(radius==0)
                {
                    return CastWorldMousePosition(castLayer, out hit, distance);
                }
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);

                if (Physics.SphereCast(ray,radius, out hit, distance == 0 ? mainCamera.farClipPlane : distance, castLayer))
                {
                    return true;
                }
                else
                {
                    hit = default;
                    return false;
                }
            }
        }

        public virtual Vector3 WorldMousePosition(LayerMask castLayer)
        {
            if (!mainCamera)
            {
                if (!Camera.main)
                {
                    Debug.LogWarning("Trying to get the world mouse position but a MainCamera is missing from the scene");
                    return Vector3.zero;
                }
                else
                {
                    mainCamera = Camera.main;
                    return Vector3.zero;
                }
            }
            else
            {
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, mainCamera.farClipPlane, castLayer))
                {
                    return hit.point;
                }
                else
                {
                    return ray.GetPoint(mainCamera.farClipPlane);
                }
            }
        }
    }
}