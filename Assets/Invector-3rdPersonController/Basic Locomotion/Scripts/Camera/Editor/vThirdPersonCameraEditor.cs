using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace Invector.vCamera
{
    [CustomEditor(typeof(vThirdPersonCamera),true)]
    [CanEditMultipleObjects]
    public class vThirdPersonCameraEditor : Editor
    {
        protected virtual GUISkin skin { get; set; }
        protected virtual vThirdPersonCamera tpCamera { get; set; }
        protected virtual bool hasPointCopy { get; set; }
        protected virtual Vector3 pointCopy { get; set; }
        protected virtual int indexSelected { get; set; }
        protected virtual Texture2D m_Logo { get; set; }

        protected virtual void OnSceneGUI()
        {
            if (Application.isPlaying)
                return;
            tpCamera = (vThirdPersonCamera)target;

            if (tpCamera.gameObject == Selection.activeGameObject)
                if (tpCamera.CameraStateList != null && tpCamera.CameraStateList.tpCameraStates != null && tpCamera.CameraStateList.tpCameraStates.Count > 0)
                {
                    if (tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].cameraMode != TPCameraMode.FixedPoint) return;
                    try
                    {
                        for (int i = 0; i < tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints.Count; i++)
                        {
                            if (indexSelected == i)
                            {
                                Handles.color = Color.blue;
                                tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint = tpCamera.transform.position;
                                tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].eulerAngle = tpCamera.transform.eulerAngles;
                                if (tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[indexSelected].freeRotation)
                                {
                                    Handles.SphereHandleCap(0, tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].eulerAngle, Quaternion.identity, 0.5f, EventType.Ignore);
                                }
                                else
                                {
                                    Handles.DrawLine(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint,
                                    tpCamera.mainTarget.position);
                                }
                            }
                            else if (Handles.Button(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint, Quaternion.identity, 0.5f, 0.3f, Handles.SphereHandleCap))
                            {
                                indexSelected = i;
                                tpCamera.indexLookPoint = i;
                                tpCamera.transform.position = tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint;
                                tpCamera.transform.rotation = Quaternion.Euler(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].eulerAngle);
                            }
                            Handles.color = Color.white;
                            Handles.Label(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint, tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].pointName);
                        }
                    }
                    catch { if (tpCamera.indexList > tpCamera.CameraStateList.tpCameraStates.Count - 1) tpCamera.indexList = tpCamera.CameraStateList.tpCameraStates.Count - 1; }
                }
        }

        protected virtual void OnEnable()
        {
            m_Logo = (Texture2D)Resources.Load("tp_camera", typeof(Texture2D));
            indexSelected = 0;
            tpCamera = (vThirdPersonCamera)target;
            tpCamera.indexLookPoint = 0;
            if (tpCamera.CameraStateList==null||tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].cameraMode != TPCameraMode.FixedPoint) return;
            if (tpCamera.CameraStateList != null && (tpCamera.indexList < tpCamera.CameraStateList.tpCameraStates.Count) && tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints.Count > 0)
            {
                tpCamera.transform.position = tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[0].positionPoint;
                tpCamera.transform.rotation = Quaternion.Euler(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[0].eulerAngle);
            }
        }

        public override void OnInspectorGUI()
        {
            var oldskin = GUI.skin;
            if (!skin) skin = Resources.Load("vSkin") as GUISkin;
            GUI.skin = skin;

            tpCamera = (vThirdPersonCamera)target;

            EditorGUILayout.Space();
            GUILayout.BeginVertical("Third Person Camera by Invector", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(5);

            if (tpCamera.cullingLayer == 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Please assign the Culling Layer to 'Default' ", MessageType.Warning);
                EditorGUILayout.Space();
            }

            EditorGUILayout.HelpBox("The target will be assign automatically to the current Character when start, check the InitialSetup method on the Motor.", MessageType.Info);
            GUI.skin = oldskin;
            base.OnInspectorGUI();
            GUI.skin = skin;
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Camera States", "window");

            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(5);

            EditorGUILayout.HelpBox("This settings will always load in this List, you can create more List's with different settings for another characters or scenes", MessageType.Info);

            tpCamera.CameraStateList = (vThirdPersonCameraListData)EditorGUILayout.ObjectField("CameraState List", tpCamera.CameraStateList, typeof(vThirdPersonCameraListData), false);
            if (tpCamera.CameraStateList == null)
            {
                GUILayout.EndVertical();
                return;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("New CameraState")))
            {
                if (tpCamera.CameraStateList.tpCameraStates == null)
                    tpCamera.CameraStateList.tpCameraStates = new List<vThirdPersonCameraState>();

                tpCamera.CameraStateList.tpCameraStates.Add(new vThirdPersonCameraState("New State" + tpCamera.CameraStateList.tpCameraStates.Count));
                tpCamera.indexList = tpCamera.CameraStateList.tpCameraStates.Count - 1;
            }

            if (GUILayout.Button(new GUIContent("Delete State")) && tpCamera.CameraStateList.tpCameraStates.Count > 1 && tpCamera.indexList != 0)
            {
                tpCamera.CameraStateList.tpCameraStates.RemoveAt(tpCamera.indexList);
                if (tpCamera.indexList - 1 >= 0)
                    tpCamera.indexList--;
            }

            GUILayout.EndHorizontal();

            if (tpCamera.CameraStateList.tpCameraStates.Count > 0)
            {
                if (tpCamera.indexList > tpCamera.CameraStateList.tpCameraStates.Count - 1) tpCamera.indexList = 0;
                tpCamera.indexList = EditorGUILayout.Popup("State", tpCamera.indexList, getListName(tpCamera.CameraStateList.tpCameraStates));
                StateData(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList]);
            }

            GUILayout.EndVertical();
            EditorGUILayout.Space();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(tpCamera);
                EditorUtility.SetDirty(tpCamera.CameraStateList);
            }
            GUI.skin = oldskin;
        }

        protected virtual void StateData(vThirdPersonCameraState camState)
        {
            EditorGUILayout.Space();

            DrawEnumField("Camera Mode", ref camState.cameraMode);           
            DrawTextField("State Name",ref camState.Name);

            if (CheckName(camState.Name, tpCamera.indexList))
            {
                EditorGUILayout.HelpBox("This name already exist, choose another one", MessageType.Error);
            }

            switch (camState.cameraMode)
            {
                case TPCameraMode.FreeDirectional:
                    FreeDirectionalMode(camState);
                    break;
                case TPCameraMode.FixedAngle:
                    FixedAngleMode(camState);
                    break;
                case TPCameraMode.FixedPoint:
                    FixedPointMode(camState);
                    break;
            }
        }

        protected virtual void DrawLookPoint(vThirdPersonCameraState camState)
        {
            if (camState.lookPoints == null) camState.lookPoints = new List<LookPoint>();
            if (camState.lookPoints.Count > 0)
            {
                EditorGUILayout.HelpBox("You can create multiple camera points and change them using the TriggerChangeCameraState script.", MessageType.Info);

                if (tpCamera.indexLookPoint > camState.lookPoints.Count - 1)
                    tpCamera.indexLookPoint = 0;
                if (tpCamera.indexLookPoint < 0)
                    tpCamera.indexLookPoint = camState.lookPoints.Count - 1;
                GUILayout.BeginHorizontal("box");
                GUILayout.Label("Fixed Points");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<", GUILayout.Width(20)))
                {
                    if (tpCamera.indexLookPoint - 1 < 0)
                        tpCamera.indexLookPoint = camState.lookPoints.Count - 1;
                    else
                        tpCamera.indexLookPoint--;
                    tpCamera.transform.position = camState.lookPoints[tpCamera.indexLookPoint].positionPoint;
                    tpCamera.transform.rotation = Quaternion.Euler(camState.lookPoints[tpCamera.indexLookPoint].eulerAngle);

                    indexSelected = tpCamera.indexLookPoint;
                }
                GUILayout.Box((tpCamera.indexLookPoint + 1).ToString("00") + "/" + camState.lookPoints.Count.ToString("00"));
                if (GUILayout.Button(">", GUILayout.Width(20)))
                {
                    if (tpCamera.indexLookPoint + 1 > camState.lookPoints.Count - 1)
                        tpCamera.indexLookPoint = 0;
                    else
                        tpCamera.indexLookPoint++;
                    tpCamera.transform.position = camState.lookPoints[tpCamera.indexLookPoint].positionPoint;
                    tpCamera.transform.rotation = Quaternion.Euler(camState.lookPoints[tpCamera.indexLookPoint].eulerAngle);
                    indexSelected = tpCamera.indexLookPoint;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("box");
                GUILayout.Label("Point Name");
                camState.lookPoints[tpCamera.indexLookPoint].pointName = GUILayout.TextField(camState.lookPoints[tpCamera.indexLookPoint].pointName, 100);
                GUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Check 'Static Camera' to create a static point and leave uncheck to look at the Player.", MessageType.Info);
                camState.lookPoints[tpCamera.indexLookPoint].freeRotation = EditorGUILayout.Toggle("Static Camera", camState.lookPoints[tpCamera.indexLookPoint].freeRotation);

                EditorGUILayout.Space();
            }

            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("New Point"))
            {
                LookPoint p = new LookPoint();
                p.pointName = "point_" + (camState.lookPoints.Count + 1).ToString("00");
                p.positionPoint = tpCamera.transform.position;
                p.eulerAngle = (tpCamera.mainTarget) ? tpCamera.mainTarget.position : (tpCamera.transform.position + tpCamera.transform.forward);
                camState.lookPoints.Add(p);
                tpCamera.indexLookPoint = camState.lookPoints.Count - 1;

                tpCamera.transform.position = camState.lookPoints[tpCamera.indexLookPoint].positionPoint;
                indexSelected = tpCamera.indexLookPoint;
            }

            if (GUILayout.Button("Remove current point "))
            {
                if (camState.lookPoints.Count > 0)
                {
                    camState.lookPoints.RemoveAt(tpCamera.indexLookPoint);
                    tpCamera.indexLookPoint--;
                    if (tpCamera.indexLookPoint > camState.lookPoints.Count - 1)
                        tpCamera.indexLookPoint = 0;
                    if (tpCamera.indexLookPoint < 0)
                        tpCamera.indexLookPoint = camState.lookPoints.Count - 1;
                    if (camState.lookPoints.Count > 0)
                        tpCamera.transform.position = camState.lookPoints[tpCamera.indexLookPoint].positionPoint;
                    indexSelected = tpCamera.indexLookPoint;
                }
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        protected virtual void FreeDirectionalMode(vThirdPersonCameraState camState)
        {

            DrawSliderField("Forward", ref camState.forward, -1f, 1f);
            DrawSliderField("Right", ref camState.right, -3f, 3f);
            DrawFloatField("Distance",ref camState.defaultDistance);
            DrawToogleField("Use Zoom",ref camState.useZoom);
            if (camState.useZoom)
            {
                DrawFloatField("Max Distance", ref camState.maxDistance);
                DrawFloatField("Min Distance",ref camState.minDistance);
            }
            DrawFloatField("Height", ref camState.height);
            DrawSliderField("Field of View",ref camState.fov, 1, 179);
            DrawFloatField("Smooth",ref camState.smooth);
            DrawFloatField("Smooth Damp",ref camState.smoothDamp);
            DrawFloatField("Culling Height",ref camState.cullingHeight);
            DrawVector3Field("Rotation OffSet",ref camState.rotationOffSet);
            DrawFloatField("MouseSensitivity X",ref camState.xMouseSensitivity);
            DrawFloatField("MouseSensitivity Y",ref camState.yMouseSensitivity);
            MinMaxSliderField("Limit Angle X", ref camState.xMinLimit, ref camState.xMaxLimit, -360, 360);
            MinMaxSliderField("Limit Angle Y", ref camState.yMinLimit, ref camState.yMaxLimit, -180, 180);
        }

        protected virtual void FixedAngleMode(vThirdPersonCameraState camState)
        {
           DrawFloatField("Distance",ref  camState.defaultDistance);
           DrawToogleField("Use Zoom",ref camState.useZoom);
            if (camState.useZoom)
            {
                DrawFloatField("Max Distance",ref camState.maxDistance);
                DrawFloatField("Min Distance",ref camState.minDistance);
            }
            DrawFloatField("Height",ref camState.height);
            DrawSliderField("Field of View",ref camState.fov, 1, 179);
            DrawFloatField("Smooth Follow",ref camState.smooth);
            DrawFloatField("Culling Height",ref camState.cullingHeight);
            DrawSliderField("Right",ref camState.right, -3f, 3f);
            DrawSliderField("Angle X",ref camState.fixedAngle.x, -360, 360);
            DrawSliderField("Angle Y",ref camState.fixedAngle.y, -360, 360);
        }

        protected virtual void FixedPointMode(vThirdPersonCameraState camState)
        {
            DrawFloatField("Smooth Follow",ref camState.smooth);
            DrawSliderField("Field of View",ref camState.fov, 1, 179);
            camState.fixedAngle.x = 0;
            camState.fixedAngle.y = 0;

            DrawLookPoint(camState);
        }

        protected virtual bool CheckName(string Name, int _index)
        {
            foreach (vThirdPersonCameraState state in tpCamera.CameraStateList.tpCameraStates)
                if (state.Name.Equals(Name) && tpCamera.CameraStateList.tpCameraStates.IndexOf(state) != _index)
                    return true;

            return false;
        }

        #region Camera State Drawers with undo

        protected virtual void DrawEnumField<T>(string name,ref T value)where T :System.Enum
        {          
            T _value = value;
            EditorGUI.BeginChangeCheck();
            _value = (T)EditorGUILayout.EnumPopup(name, _value);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tpCamera.CameraStateList, "ChangeCameraState");
                value = _value;
            }
        }

        protected virtual void DrawTextField(string name, ref string value)
        {
            string _value = value;
            EditorGUI.BeginChangeCheck();
            _value =EditorGUILayout.TextField(name, _value);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tpCamera.CameraStateList, "ChangeCameraState");
                value = _value;
            }
        }

        protected virtual void DrawVector3Field(string name, ref Vector3 value)
        {
            Vector3 _value = value;
            EditorGUI.BeginChangeCheck();
            _value = EditorGUILayout.Vector3Field("Rotation OffSet", _value);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tpCamera.CameraStateList, "ChangeCameraState");
                value = _value;
            }
        }

        protected virtual void DrawSliderField(string name, ref float value, float min, float max)
        {
            float _value = value;
            EditorGUI.BeginChangeCheck();
            _value = EditorGUILayout.Slider(name, _value, min, max);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tpCamera.CameraStateList, "ChangeCameraState");
                value = _value;
            }
        }

        protected virtual void DrawFloatField(string name, ref float value)
        {
            float _value = value;
            EditorGUI.BeginChangeCheck();
            _value = EditorGUILayout.FloatField(name, _value);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tpCamera.CameraStateList, "ChangeCameraState");
                value = _value;
            }
        }

        protected virtual void DrawToogleField(string name, ref bool value)
        {
            bool _value = value;
            EditorGUI.BeginChangeCheck();
            _value = EditorGUILayout.Toggle(name, _value);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tpCamera.CameraStateList, "ChangeCameraState");
                value = _value;
            }
        }

        protected virtual void MinMaxSliderField(string name, ref float minVal, ref float maxVal, float minLimit, float maxLimit)
        {
            float _minVal = minVal;
            float _maxVal = maxVal;
            GUILayout.BeginVertical();
            GUILayout.Label(name);
            GUILayout.BeginHorizontal("box");

            EditorGUI.BeginChangeCheck();
            _minVal = EditorGUILayout.FloatField(_minVal, GUILayout.MaxWidth(60));
            EditorGUILayout.MinMaxSlider(ref _minVal, ref _maxVal, minLimit, maxLimit);
            _maxVal = EditorGUILayout.FloatField(_maxVal, GUILayout.MaxWidth(60));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tpCamera.CameraStateList, "ChangeCameraState");
                minVal = _minVal;
                maxVal = _maxVal;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        #endregion

        [MenuItem("Invector/Basic Locomotion/Resources/New CameraState List Data")]
        protected static void NewCameraStateData()
        {
            vScriptableObjectUtility.CreateAsset<vThirdPersonCameraListData>();
        }

        protected virtual string[] getListName(List<vThirdPersonCameraState> list)
        {
            string[] names = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                names[i] = list[i].Name;
            }
            return names;
        }
    }
}