using Invector.IK;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Invector.vShooter
{
    using PlayerController;
    public class vShooterIKAdjustWindow : EditorWindow
    {
        public static vShooterIKAdjustWindow curWindow;
        public vIShooterIKController ikController;
        public SerializedObject ikList;
        public SerializedObject ik;
        public IKSolverEditorHelper leftIK, rightIK;
        public Transform selected, referenceSelected;
        public GUIStyle fontLabelStyle = new GUIStyle();
        GUISkin skin;
        public bool applicationStarted;
        public Vector2 scroll;
        Rect selectorRect;

        public bool freezeAnimator;
        public float lastAnimatorSpeed;

        public bool useLockCamera
        {
            get
            {
                return (ikController is vILockCamera);
            }
        }      

        public bool LockCamera
        {
            get
            {
                if (useLockCamera)
                    return (ikController as vILockCamera).LockCamera;
                return false;
            }
            set
            {
                if (useLockCamera)
                    (ikController as vILockCamera).LockCamera = value;
            }
        }



        [MenuItem("Invector/Shooter/Open IK Adjust Window",priority =100)]
        public static void InitEditorWindow()
        {
            if (!curWindow)
            {
                curWindow = (vShooterIKAdjustWindow)EditorWindow.GetWindow<vShooterIKAdjustWindow>("IK Adjust Window");
                curWindow.titleContent.image = Resources.Load("icon_v2") as Texture2D;
            }
        }

        protected virtual void OnEnable()
        {
            skin = Resources.Load("welcomeWindowSkin") as GUISkin;
            // Remove delegate listener if it has previously
            // been assigned.

#if UNITY_2018
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            if (SceneView.onSceneGUIDelegate != this.OnSceneGUI)
            {
                SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            }
#elif UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
#endif
        }

        void DrawSceneGizmos()
        {
            if (!Application.isPlaying) return;

            if (ikController == null)
            {
                return;
            }

            if (!ikController.CurrentActiveWeapon) return;
            applicationStarted = true;
            if (ikController.WeaponIKAdjustList && ikController.CurrentWeaponIK && ikController.LeftIK != null && ikController.RightIK != null)
            {
                DrawIKHandles();
                DrawGlobalK();
            }
        }

        void OnGUI()
        {
            GUILayout.BeginVertical(skin.GetStyle("WindowBG"));

            if (!Application.isPlaying)
            {
                DrawMessageInfo("Go to <color=green>PlayMode</color> to edit the IK Adjust List", "PlayModeIcon");
                ikController = null;
                return;
            }
            if (ikController == null)
            {
                DrawMessageInfo("Select a <color=green>Shooter Controller</color>", "ShooterControllerIcon");
                return;
            }

            if (!ikController.CurrentActiveWeapon)
            {
                DrawMessageInfo("Shooter Controller Doesn't have a <color=green>Active Weapon</color>", "WeaponIcon");
                return;
            }

            if (skin == null) skin = Resources.Load("vSkin") as GUISkin;


            if (GUILayout.Button(ikController.LockAiming ? "Unlock Aim" : "Lock Aim", EditorStyles.miniButton))
            {
                ikController.LockAiming = !ikController.LockAiming;
            }

            if (GUILayout.Button(ikController.LockAiming ? "Unlock HipFire Aim" : "Lock HipFire Aim", EditorStyles.miniButton))
            {
                ikController.LockHipFireAiming = !ikController.LockHipFireAiming;
            }

            if (GUILayout.Button(ikController.IsCrouching ? "Unlock Crouch" : "Lock Crouch", EditorStyles.miniButton))
            {
                ikController.IsCrouching = !ikController.IsCrouching;
            }

            if (useLockCamera && GUILayout.Button(LockCamera ? "Unlock Camera" : "Lock Camera", EditorStyles.miniButton))
            {
                LockCamera = !LockCamera;
            }
            if (GUILayout.Button(freezeAnimator ? "UnFreeze Animator" : "Freeze Animator", EditorStyles.miniButton))
            {
                var animator = ikController.gameObject.GetComponent<Animator>();
                if (freezeAnimator)
                {
                    freezeAnimator = false;
                    animator.speed = lastAnimatorSpeed;
                }
                else
                {
                    freezeAnimator = true;
                    lastAnimatorSpeed = animator.speed;
                    animator.speed = 0;

                }
            }



            scroll = EditorGUILayout.BeginScrollView(scroll);
            {
                {
                    if (ikController.WeaponIKAdjustList)
                    {
                        if (ikList == null || ikList.targetObject != ikController.WeaponIKAdjustList) ikList = new SerializedObject(ikController.WeaponIKAdjustList);
                        if (ikList != null) ikList.UpdateIfRequiredOrScript();

                        var weaponIKAdjustList = ikController.WeaponIKAdjustList;
                        EditorGUI.BeginChangeCheck();
                        weaponIKAdjustList = (vWeaponIKAdjustList)EditorGUILayout.ObjectField(weaponIKAdjustList, typeof(vWeaponIKAdjustList), false);

                        if (EditorGUI.EndChangeCheck())
                        {
                            ikController.WeaponIKAdjustList = weaponIKAdjustList;
                            ikController.UpdateWeaponIK();
                            return;
                        }

                        EditorGUI.BeginChangeCheck();
                       
                        GUILayout.Box("State : "+ikController.CurrentIKAdjustStateWithTag, skin.box, GUILayout.ExpandWidth(true));
                       
                       

                        if (ikController.CurrentWeaponIK && ikController.CurrentIKAdjust!=null)
                        {
                            if (ik == null || ik.targetObject != ikController.CurrentWeaponIK) ik = new SerializedObject(ikController.CurrentWeaponIK);
                            ik.Update();
                            try
                            {
                                GUILayout.Space(20);
                                DrawWeaponIKSettings(ikController.CurrentWeaponIK);
                                GUILayout.Space(20);
                            }
                            catch { }
                        }
                        else
                        {
                            if (ikController.CurrentWeaponIK && ikController.IsUsingCustomIKAdjust)
                            {
                                EditorStyles.helpBox.richText = true;
                                EditorGUILayout.HelpBox("Shooter is using a customIK Adjust for this weapon " + ikController.CurrentActiveWeapon.weaponCategory + " category,  click in the button below to create one.", MessageType.Info);
                                if (GUILayout.Button("Create Custom Adjust", skin.button))
                                {
                                    if (ikController.IsLeftWeapon)
                                    {
                                        ikController.CurrentWeaponIK.ikAdjustsLeft.Add(new IKAdjust(ikController.CurrentIKAdjustState));
                                    }
                                    else ikController.CurrentWeaponIK.ikAdjustsRight.Add(new IKAdjust(ikController.CurrentIKAdjustState));
                                  
                                   
                                    ikList.ApplyModifiedProperties();
                                    EditorUtility.SetDirty(ikList.targetObject);
                                    AssetDatabase.SaveAssets();

                                    EditorUtility.SetDirty(ikController.CurrentWeaponIK);
                                }
                            }
                            else
                            {
                                EditorStyles.helpBox.richText = true;
                                EditorGUILayout.HelpBox("This weapon doesn't have a IKAdjust for the '" + ikController.CurrentActiveWeapon.weaponCategory + "' category,  click in the button below to create one.", MessageType.Info);
                                if (GUILayout.Button("Create New IK Adjust", skin.button))
                                {
                                    vWeaponIKAdjust ikAdjust = ScriptableObject.CreateInstance<vWeaponIKAdjust>();
                                    AssetDatabase.CreateAsset(ikAdjust, "Assets/" + ikController.gameObject.name + "@" + ikController.CurrentActiveWeapon.weaponCategory + ".asset");
                                    ikAdjust.weaponCategories = new List<string>() { ikController.CurrentActiveWeapon.weaponCategory };

                                    ikController.WeaponIKAdjustList.weaponIKAdjusts.Add(ikAdjust);
                                    ikController.UpdateWeaponIK();

                                    SerializedObject scriptableIK = new SerializedObject(ikAdjust);
                                    scriptableIK.ApplyModifiedProperties();
                                    ikList.ApplyModifiedProperties();
                                    EditorUtility.SetDirty(ikList.targetObject);
                                    AssetDatabase.SaveAssets();
                                    EditorUtility.SetDirty(scriptableIK.targetObject);
                                }
                                if (GUILayout.Button("Choose IK Adjust", skin.button))
                                {
                                    var action = new UnityEngine.Events.UnityAction<vWeaponIKAdjust>((ikAdjust) =>
                                    {
                                        ikController.WeaponIKAdjustList.weaponIKAdjusts.Add(ikAdjust);
                                        ikController.UpdateWeaponIK();

                                        ikList.ApplyModifiedProperties();
                                        EditorUtility.SetDirty(ikList.targetObject);
                                    });

                                    PopupWindow.Show(selectorRect, new vIKAjustSelector(ikController.CurrentActiveWeapon.weaponCategory, action, skin));
                                }
                                if (Event.current.type == EventType.Repaint) selectorRect = GUILayoutUtility.GetLastRect();
                                ikController.UpdateWeaponIK();
                            }
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (ikList != null)
                            {
                                ikList.ApplyModifiedProperties();
                                EditorUtility.SetDirty(ikList.targetObject);
                            }
                        }
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("You need to assign a <color=green>IK Adjust List</color> in the ShooterManager First!", skin.GetStyle("SuperTitle"));
                    }
                }
                GUILayout.Space(20);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace(); GUILayout.EndVertical();
        }
       
        private void DrawMessageInfo(string message, string icon)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(message, skin.GetStyle("SuperTitle"));
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(" ", skin.GetStyle(icon));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawGizmosAlert()
        {
            GUILayout.Label("Make sure to <color=green>Enable the Gizmos</color> button of your SceneView Window", skin.GetStyle("SuperTitle"));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(" ", skin.GetStyle("GizmosIcon"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        public void CreateNewIKAdjustList(vIShooterIKController targetShooterManager)
        {
            vWeaponIKAdjustList ikAdjust = ScriptableObject.CreateInstance<vWeaponIKAdjustList>();
            AssetDatabase.CreateAsset(ikAdjust, "Assets/" + ikController.gameObject.name + "@IKAdjustList.asset");

            targetShooterManager.WeaponIKAdjustList = ikAdjust;
            ikList = new SerializedObject(ikAdjust);
            ikList.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(ikList.targetObject);
        }

        void DrawIKHandles()
        {
            var currentWeaponIK = ikController.CurrentWeaponIK;
            if (leftIK == null || leftIK.iKSolver == null || !leftIK.iKSolver.isValidBones) leftIK = new IKSolverEditorHelper(ikController.LeftIK);
            if (rightIK == null || rightIK.iKSolver == null || !rightIK.iKSolver.isValidBones) rightIK = new IKSolverEditorHelper(ikController.RightIK);
            if (leftIK == null || rightIK == null) return;
            IKAdjust ikAdjust = IkAdjust();
            leftIK.DrawIKHandles(ref selected, ref referenceSelected, ikAdjust!=null && !ikController.EditingIKGlobalOffset? Color.blue:Color.grey);
            if(leftIK.iKSolver.middleBoneOffset!=selected)
            {
                DrawTooltipLabel(leftIK.iKSolver.middleBone.position, "Left Hint", Color.blue);
            }
            if (leftIK.iKSolver.endBoneOffset != selected)
            {
                DrawTooltipLabel(leftIK.iKSolver.endBoneOffset.position, "Left Hand", Color.blue);
            }

            rightIK.DrawIKHandles(ref selected, ref referenceSelected, ikAdjust != null && !ikController.EditingIKGlobalOffset ? Color.green : Color.grey);
            if (rightIK.iKSolver.middleBoneOffset != selected)
            {
                DrawTooltipLabel(rightIK.iKSolver.middleBone.position, "Right Hint", Color.green);
            }
            if (rightIK.iKSolver.endBoneOffset != selected)
            {
                DrawTooltipLabel(rightIK.iKSolver.endBoneOffset.position, "Right Hand",Color.green);
            }

            if (ikAdjust!=null && selected != null && referenceSelected!=null)
            {
                if (DrawTransformHandles(selected, referenceSelected))
                {
                    Undo.RecordObject(currentWeaponIK, "Change IK Bone Transform");    
                    ApplyOffsets(ikAdjust, ikController.IsLeftWeapon ? ikController.LeftIK : ikController.RightIK, ikController.IsLeftWeapon ? ikController.RightIK : ikController.LeftIK);
                }
            }
        }

        void DrawGlobalK()
        {
            var ikTarget = ikController.CurrentActiveWeapon.handIKTargetOffset;
          
            if(ikTarget)
            {
                if (selected != ikTarget)
                {
                    //Handles.color = Color.red;
                    //if(Handles.Button(ikTarget.position, Quaternion.identity, 0.02f,0.02f,Handles.SphereHandleCap))
                    //{
                    //    selected = ikTarget;
                    //    referenceSelected = null;
                    //}
                    //DrawTooltipLabel(ikTarget.position, "Global Hand Ik Adjust", Color.red);
                    //Handles.color = Color.white;
                }
                else
                {
                   
                    Vector3 position = ikTarget.position;
                    Quaternion rotation = ikTarget.rotation;
                    EditorGUI.BeginChangeCheck();
                    if (Tools.current != Tool.Rotate)
                    {
                        position = Handles.PositionHandle(position, Tools.pivotRotation == PivotRotation.Local ? ikTarget.rotation : Quaternion.identity);
                    }
                    else if (Tools.current == Tool.Rotate)
                        rotation = Handles.RotationHandle(rotation, position);
                   
                   if( EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ikTarget, "Change IK Target Transform");
                        Undo.RecordObject(ikController.WeaponIKAdjustList, "Change IK Target Transform");
                        if (ikController.IsLeftWeapon)
                        {
                            if (Tools.current != Tool.Rotate)
                            {
                                ikTarget.position = position;
                                ikController.WeaponIKAdjustList.ikTargetPositionOffsetL = ikTarget.localPosition;
                            }
                            else
                            {
                                ikTarget.rotation = rotation;
                                ikController.WeaponIKAdjustList.ikTargetRotationOffsetL = ikTarget.localEulerAngles;
                            }
                        }
                        else
                        {
                            if (Tools.current != Tool.Rotate)
                            {
                                ikTarget.position = position;
                                ikController.WeaponIKAdjustList.ikTargetPositionOffsetR = ikTarget.localPosition;
                            }
                            else
                            {
                                ikTarget.rotation = rotation;
                                ikController.WeaponIKAdjustList.ikTargetRotationOffsetR = ikTarget.localEulerAngles;
                            }
                        }
                        EditorUtility.SetDirty(ikController.WeaponIKAdjustList);
                    }
                }
            }

            ikController.EditingIKGlobalOffset = ikController.CurrentActiveWeapon.handIKTargetOffset == selected;
        }

        protected void DrawTooltipLabel(Vector3 position,string label,Color color)
        {
            
            switch (Event.current.type)
            {
                case EventType.Repaint:
                    {
                        float dist = Vector2.Distance(Event.current.mousePosition, HandleUtility.WorldToGUIPoint(position));
                        if (dist < 5f)
                        {
                            fontLabelStyle.fontSize = 15;
                            fontLabelStyle.normal.textColor = color;
                            fontLabelStyle.fontStyle = FontStyle.Bold;
                            fontLabelStyle.alignment = TextAnchor.MiddleCenter;
                            GUI.color = Color.white;
                            Handles.Label(position, label, fontLabelStyle);
                            break;
                        }
                    }
                    break;
            }
        }

        protected IKAdjust IkAdjust()
        {
            var ikAdjust = ikController.CurrentIKAdjust;
            return ikAdjust;
        }

        void ApplyOffsets(IKAdjust currentIKAdjust, vIKSolver weaponArm, vIKSolver supportWeaponArm)
        {
            currentIKAdjust.supportHandOffset.position = supportWeaponArm.endBoneOffset.localPosition;
            currentIKAdjust.supportHandOffset.eulerAngles = supportWeaponArm.endBoneOffset.localEulerAngles;
            currentIKAdjust.supportHintOffset.position = supportWeaponArm.middleBoneOffset.localPosition;
            currentIKAdjust.supportHintOffset.eulerAngles = supportWeaponArm.middleBoneOffset.localEulerAngles;

            currentIKAdjust.weaponHandOffset.position = weaponArm.endBoneOffset.localPosition;
            currentIKAdjust.weaponHandOffset.eulerAngles = weaponArm.endBoneOffset.localEulerAngles;
            currentIKAdjust.weaponHintOffset.position = weaponArm.middleBoneOffset.localPosition;
            currentIKAdjust.weaponHintOffset.eulerAngles = weaponArm.middleBoneOffset.localEulerAngles;
            ik.ApplyModifiedProperties();
            EditorUtility.SetDirty(ik.targetObject);
        }

        void DrawWeaponIKSettings(vWeaponIKAdjust currentWeaponIK)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(" ", skin.GetStyle("PopUpArrow"));

            if (GUILayout.Button(currentWeaponIK.name, skin.GetStyle("PopUp")))
            {
                var index = ikController.WeaponIKAdjustList.IndexOfIK(currentWeaponIK);
                var action = new UnityEngine.Events.UnityAction<vWeaponIKAdjust>((_ikAdjust) =>
                {
                    ikController.WeaponIKAdjustList.weaponIKAdjusts[index] = (_ikAdjust);
                    ikController.UpdateWeaponIK();

                    ikList.ApplyModifiedProperties();
                    EditorUtility.SetDirty(ikList.targetObject);
                });

                PopupWindow.Show(selectorRect, new vIKAjustSelector(ikController.CurrentActiveWeapon.weaponCategory, action, skin, currentWeaponIK));
            }

            if (Event.current.type == EventType.Repaint) selectorRect = GUILayoutUtility.GetLastRect();
            if (GUILayout.Button(new GUIContent("Copy Settings", "Copy other settings to this settings"), skin.button))
            {
                var list =ikController.IsLeftWeapon? currentWeaponIK.ikAdjustsLeft:currentWeaponIK.ikAdjustsRight;
                GenericMenu menu = new GenericMenu();
                var index = list.IndexOf(ikController.CurrentIKAdjust);

               for (int i=0;i<list.Count;i++)
                {
                    var adjust = list[i];
                    if (adjust.name != ikController.CurrentIKAdjustState) menu.AddItem(new GUIContent(list[i].name), false, () =>
                    {
                        Undo.RecordObject(currentWeaponIK, "IK Settings");
                        SerializedObject scriptableIK = new SerializedObject(currentWeaponIK);
                      
                        var copy = adjust.Copy();
                        copy.name = list[index].name;
                        list[index] = copy;
                        scriptableIK.ApplyModifiedProperties();
                        scriptableIK.Update();
                        ikController.UpdateWeaponIK();
                    });

                }
                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();
            if (ikController.LeftIK == null || ikController.RightIK == null || !ikController.LeftIK.isValidBones || !ikController.RightIK.isValidBones) return;
            GUILayout.BeginHorizontal();
            GUI.color = Color.white;
            GUI.enabled = selected != ikController.LeftIK.endBoneOffset;
            if (GUILayout.Button(new GUIContent("Left Hand", "Edit the Left Hand"), EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true)))
            {
                referenceSelected = ikController.LeftIK.endBone;
                selected = ikController.LeftIK.endBoneOffset;
            }
            GUI.enabled = selected != ikController.LeftIK.middleBoneOffset;
            if (GUILayout.Button(new GUIContent("Left Hint", "Edit the Left Hint"), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true)))
            {
                referenceSelected = ikController.LeftIK.middleBone;
                selected = ikController.LeftIK.middleBoneOffset;

            }
            GUI.color = Color.red;
            GUI.enabled = ikController.CurrentActiveWeapon.handIKTargetOffset;
            if (GUILayout.Button(new GUIContent("Hand IK Target","Edit the global offset for support hand"), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true)))
            {
                referenceSelected = null;
                selected = ikController.CurrentActiveWeapon.handIKTargetOffset;
            }
            GUI.color = Color.white;
            GUI.enabled = selected != ikController.RightIK.middleBoneOffset;
            if (GUILayout.Button(new GUIContent("Right Hint", "Edit the Right Hint"), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true)))
            {
                referenceSelected = ikController.RightIK.middleBone;
                selected = ikController.RightIK.middleBoneOffset;

            }
            GUI.enabled = selected != ikController.RightIK.endBoneOffset;
            if (GUILayout.Button(new GUIContent("Right Hand", "Edit the Right Hand"), EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true)))
            {
                referenceSelected = ikController.RightIK.endBone;
                selected = ikController.RightIK.endBoneOffset;

            }
           
            GUI.enabled = true;
            GUI.color = Color.white;

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            var ikAdjust = IkAdjust();
            if (selected != null && referenceSelected!=null)
            {
                GUILayout.Label(selected.name, EditorStyles.whiteBoldLabel);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Reset Position", EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true)))
                {
                    Undo.RecordObject(selected, "Reset Position");
                    selected.localPosition = Vector3.zero;
                    ApplyOffsets(ikAdjust, ikController.IsLeftWeapon ? ikController.LeftIK : ikController.RightIK,
                                            ikController.IsLeftWeapon ? ikController.RightIK : ikController.LeftIK);
                }
                if (GUILayout.Button("Reset", EditorStyles.miniButtonMid, GUILayout.ExpandWidth(true)))
                {
                    Undo.RecordObject(selected, "Reset ALL");
                    selected.localPosition = Vector3.zero;
                    selected.localEulerAngles = Vector3.zero;
                    ApplyOffsets(ikAdjust, ikController.IsLeftWeapon ? ikController.LeftIK : ikController.RightIK,
                                            ikController.IsLeftWeapon ? ikController.RightIK : ikController.LeftIK);
                }
                if (GUILayout.Button("Reset Rotation", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true)))
                {
                    Undo.RecordObject(selected, "Reset Rotation");
                    selected.localEulerAngles = Vector3.zero;
                    ApplyOffsets(ikAdjust, ikController.IsLeftWeapon ? ikController.LeftIK : ikController.RightIK,
                                            ikController.IsLeftWeapon ? ikController.RightIK : ikController.LeftIK);
                }
                GUILayout.EndHorizontal();
            }
            else if(ikController.CurrentActiveWeapon && ikController.CurrentActiveWeapon.handIKTargetOffset==selected)
            {
                EditorGUILayout.HelpBox($"This is a GLOBAL " +(ikController.IsLeftWeapon?"Right Arm":"Left Arm") + " OFFSET \n\n When creating a new Character, ADJUST THIS HAND IK FIRST so that all weapons and poses have a base alignment, then create the necessary IK Adjustments for each pose and weapon", MessageType.Warning);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Position", EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true)))
                {
                    Undo.RecordObject(selected, "Reset Position");
                    Undo.RecordObject(ikController.WeaponIKAdjustList, "Reset Position");
                    if (ikController.IsLeftWeapon)
                    {
                        ikController.WeaponIKAdjustList.ikTargetPositionOffsetL = Vector3.zero;
                      
                    }
                    else
                    {
                        ikController.WeaponIKAdjustList.ikTargetPositionOffsetR = Vector3.zero;                       
                    }
                    ikController.CurrentActiveWeapon.handIKTargetOffset.localPosition = Vector3.zero;
                    EditorUtility.SetDirty(ikController.WeaponIKAdjustList);
                }
                if (GUILayout.Button("Reset", EditorStyles.miniButtonMid, GUILayout.ExpandWidth(true)))
                {
                    Undo.RecordObject(selected, "Reset ALL");
                    Undo.RecordObject(ikController.WeaponIKAdjustList, "Reset Position");
                    if (ikController.IsLeftWeapon)
                    {
                        ikController.WeaponIKAdjustList.ikTargetPositionOffsetL = Vector3.zero;
                        ikController.WeaponIKAdjustList.ikTargetRotationOffsetL = Vector3.zero;
                    }
                    else
                    {
                        ikController.WeaponIKAdjustList.ikTargetPositionOffsetR = Vector3.zero;
                        ikController.WeaponIKAdjustList.ikTargetRotationOffsetR = Vector3.zero;
                    }
                    ikController.CurrentActiveWeapon.handIKTargetOffset.localPosition = Vector3.zero;
                    ikController.CurrentActiveWeapon.handIKTargetOffset.localEulerAngles = Vector3.zero;
                    EditorUtility.SetDirty(ikController.WeaponIKAdjustList);
                }
                if (GUILayout.Button("Reset Rotation", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true)))
                {
                    Undo.RecordObject(selected, "Reset Rotation");
                    Undo.RecordObject(ikController.WeaponIKAdjustList, "Reset Position");
                    if (ikController.IsLeftWeapon)
                    {                       
                        ikController.WeaponIKAdjustList.ikTargetRotationOffsetL = Vector3.zero;
                    }
                    else
                    {                       
                        ikController.WeaponIKAdjustList.ikTargetRotationOffsetR = Vector3.zero;
                    }
                    ikController.CurrentActiveWeapon.handIKTargetOffset.localEulerAngles = Vector3.zero;
                    EditorUtility.SetDirty(ikController.WeaponIKAdjustList);
                }
                GUILayout.EndHorizontal();
                //DrawGlobalIKOffsets();
            }
            DrawHeadTrackSliders(ikAdjust.spineOffset);

        }

        void DrawHeadTrackSliders(IKOffsetSpine offsetSpine)
        {
            Vector2 _spine = offsetSpine.spine;
            Vector2 _head = offsetSpine.head;
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginVertical(skin.box);
                {
                    GUILayout.BeginHorizontal(skin.box);
                    GUILayout.Label("- <b>Spine and Head Offsets</b> are applied for <color=green>each weapon and state</color>", skin.GetStyle("Description"));
                    GUILayout.EndHorizontal();

                    GUILayout.Label("Spine Offset", EditorStyles.whiteBoldLabel);

                    GUILayout.BeginHorizontal();
                    DrawSlider(ref _spine.x, "X");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    DrawSlider(ref _spine.y, "Y");
                    GUILayout.EndHorizontal();

                    GUILayout.Label("Head Offset", EditorStyles.whiteBoldLabel);

                    GUILayout.BeginHorizontal();
                    DrawSlider(ref _head.x, "X");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    DrawSlider(ref _head.y, "Y");
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(ikController.CurrentWeaponIK, "Change Offset Spine");
                offsetSpine.spine = _spine;
                offsetSpine.head = _head;

                ik.ApplyModifiedProperties();
                EditorUtility.SetDirty(ik.targetObject);
            }
        }

        //void DrawGlobalIKOffsets()
        //{
        //    if (!ikController.WeaponIKAdjustList||!ikController.CurrentActiveWeapon) return;
        //    if (selected != ikController.CurrentActiveWeapon.handIKTargetOffset) return;
        //    Vector3 _ikPosL = ikController.WeaponIKAdjustList.ikTargetPositionOffsetL;
        //    Vector3 _ikPosR = ikController.WeaponIKAdjustList.ikTargetPositionOffsetR;
        //    Vector3 _ikRotL = ikController.WeaponIKAdjustList.ikTargetRotationOffsetL;
        //    Vector3 _ikRotR = ikController.WeaponIKAdjustList.ikTargetRotationOffsetR;

        //    EditorGUI.BeginChangeCheck();
        //    {
        //        GUILayout.BeginVertical(skin.box);

        //        GUILayout.BeginHorizontal(skin.box);
        //        GUILayout.Label("- Start by aligning the <b>Left Hand IK </b>, this will have affect on <color=green>all weapons and states</color>", skin.GetStyle("Description"));
        //        GUILayout.EndHorizontal();

        //        if (ikController.CurrentActiveWeapon.isLeftWeapon)
        //        {
        //            GUILayout.Label("Left IK Position Offset", EditorStyles.whiteBoldLabel);

        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikPosL.x, "X", -1, 1);
        //            GUILayout.EndHorizontal();
        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikPosL.y, "Y", -1, 1);
        //            GUILayout.EndHorizontal();
        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikPosL.z, "Z", -1, 1);
        //            GUILayout.EndHorizontal();
        //        }
        //        else 
        //        {
        //            GUILayout.Label("Right IK Position Offset", EditorStyles.whiteBoldLabel);

        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikPosR.x, "X", -1, 1);
        //            GUILayout.EndHorizontal();
        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikPosR.y, "Y", -1, 1);
        //            GUILayout.EndHorizontal();
        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikPosR.z, "Z", -1, 1);
        //            GUILayout.EndHorizontal();
        //        }

        //        if (ikController.CurrentActiveWeapon.isLeftWeapon)
        //        {
        //            GUILayout.Label("Left IK Rotation Offset", EditorStyles.whiteBoldLabel);

        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikRotL.x, "X");
        //            GUILayout.EndHorizontal();
        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikRotL.y, "Y");
        //            GUILayout.EndHorizontal();
        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikRotL.z, "Z");
        //            GUILayout.EndHorizontal();
        //        }

        //        else
        //        {
        //            GUILayout.Label("Right IK Rotation Offset", EditorStyles.whiteBoldLabel);

        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikRotR.x, "X");
        //            GUILayout.EndHorizontal();
        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikRotR.y, "Y");
        //            GUILayout.EndHorizontal();
        //            GUILayout.BeginHorizontal();
        //            DrawSlider(ref _ikRotR.z, "Z");
        //            GUILayout.EndHorizontal();
        //        }
        //        GUILayout.EndVertical();
        //    }

        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        Undo.RecordObject(ikController.WeaponIKAdjustList, "IkOffsets");

        //        ikController.WeaponIKAdjustList.ikTargetPositionOffsetL.x = _ikPosL.x;
        //        ikController.WeaponIKAdjustList.ikTargetPositionOffsetL.y = _ikPosL.y;
        //        ikController.WeaponIKAdjustList.ikTargetPositionOffsetL.z = _ikPosL.z;
        //        ikController.WeaponIKAdjustList.ikTargetPositionOffsetR.x = _ikPosR.x;
        //        ikController.WeaponIKAdjustList.ikTargetPositionOffsetR.y = _ikPosR.y;
        //        ikController.WeaponIKAdjustList.ikTargetPositionOffsetR.z = _ikPosR.z;
        //        ikController.WeaponIKAdjustList.ikTargetRotationOffsetL.x = _ikRotL.x;
        //        ikController.WeaponIKAdjustList.ikTargetRotationOffsetL.y = _ikRotL.y;
        //        ikController.WeaponIKAdjustList.ikTargetRotationOffsetL.z = _ikRotL.z;
        //        ikController.WeaponIKAdjustList.ikTargetRotationOffsetR.x = _ikRotR.x;
        //        ikController.WeaponIKAdjustList.ikTargetRotationOffsetR.y = _ikRotR.y;
        //        ikController.WeaponIKAdjustList.ikTargetRotationOffsetR.z = _ikRotR.z;

        //        ikList.ApplyModifiedProperties();
        //        EditorUtility.SetDirty(ikList.targetObject);
        //    }
           
        //}

        void DrawSlider(ref float value, string name, float min = -180, float max = 180)
        {
            GUILayout.Label(name);
            value = EditorGUILayout.Slider(value, min, max);
        }

        bool DrawTransformHandles(Transform target, Transform reference)
        {
            if (!target||!reference) return false;
            Vector3 position = target.position;
            Quaternion rotation = target.rotation;
            Handles.DrawLine(target.position, reference.position);
            if (Tools.current != Tool.Rotate)
            {
                position = Handles.PositionHandle(position, Tools.pivotRotation == PivotRotation.Local ? rotation : Quaternion.identity);
            }

            if (Tools.current == Tool.Rotate)
                rotation = Handles.RotationHandle(rotation, position);
            if (position != target.position)
            {
                Undo.RecordObject(target, "Change IK Bone Transform");
                target.position = position;

                return true;
            }
            else if (rotation != target.rotation)
            {
                Undo.RecordObject(target, "Change IK Bone Transform");
                target.rotation = rotation;
                return true;
            }
            return false;
        }
       
        //public void OnSceneGUI()
        //{
        //    DrawSceneGizmos();
        //}

        void Update()
        {
            this.minSize = new Vector2(300, 300);
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                if (curWindow == null) curWindow = this;

                if (Selection.activeTransform && (ikController == null || (Selection.activeGameObject != ikController.gameObject && Selection.activeTransform.GetComponent<vIShooterIKController>() != null)))
                {
                    ikController = Selection.activeGameObject.GetComponent<vIShooterIKController>();
                }
                Repaint();
            }


        }

        void OnFocus()
        {
#if UNITY_2018
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            if (SceneView.onSceneGUIDelegate != this.OnSceneGUI)
            {
                SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            }
#elif UNITY_2019
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
#endif
        }

        void OnDestroy()
        {
#if UNITY_2018
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#elif UNITY_2019
            SceneView.duringSceneGui -= this.OnSceneGUI;            
#endif
        }

        void OnSceneGUI(SceneView sceneView)
        {
            DrawSceneGizmos();
        }

        [System.Serializable]
        public class IKSolverEditorHelper
        {
            public vIKSolver iKSolver;
            public IKSolverEditorHelper(vIKSolver iKSolver)
            {
                this.iKSolver = iKSolver;
            }
            public void DrawIKHandles(ref Transform selected, ref Transform referenceSelected, Color color)
            {
                if (iKSolver == null || !iKSolver.isValidBones) return;
                DrawArmLine(color);
                if (selected != iKSolver.endBoneOffset)
                { 
                    
                    if (DrawTransformButton(iKSolver.endBone, Handles.SphereHandleCap))
                    {
                        referenceSelected = iKSolver.endBone;
                        selected = iKSolver.endBoneOffset;
                        Handles.color = Color.white;
                    }
                }
                if (selected != iKSolver.middleBoneOffset)
                {
                    if (DrawTransformButton(iKSolver.middleBone, Handles.CubeHandleCap))
                    {
                        referenceSelected = iKSolver.middleBone;
                        selected = iKSolver.middleBoneOffset;
                        Handles.color = Color.white;
                    }
                }
                Handles.color = Color.white;
            }

            public bool DrawTransformButton(Transform target, Handles.CapFunction cap)
            {

                if (!target) return false;
                if (Handles.Button(target.position, target.rotation, 0.02f, 0.02f, cap))
                {
                    return true;
                }
                return false;
            }

            void DrawArmLine(Color color)
            {
                Handles.color = color;
                if (iKSolver == null || !iKSolver.isValidBones) return;

                Handles.DrawAAPolyLine(iKSolver.endBone.position, iKSolver.middleBone.position, iKSolver.rootBone.position);
            }
        }
    }

    public class vIKAjustSelector : PopupWindowContent
    {
        Vector2 scroll;
        public string weaponCategory;
        public List<vWeaponIKAdjust> weaponIKAdjusts;
        vWeaponIKAdjust selected;
        UnityEngine.Events.UnityAction<vWeaponIKAdjust> onSelect;
        GUISkin skin;
        GUIStyle selectorStyle;
        bool canSelectNull;
        public vIKAjustSelector(string weaponCategory, UnityEngine.Events.UnityAction<vWeaponIKAdjust> onSelect, GUISkin skin, vWeaponIKAdjust selected = null)
        {
            this.weaponCategory = weaponCategory;
            this.onSelect = onSelect;
            this.skin = skin;
            selectorStyle = new GUIStyle(skin.button);
            selectorStyle.border = new RectOffset(1, 1, 1, 1);
            selectorStyle.margin = new RectOffset(0, 0, 0, 0);
            selectorStyle.padding = new RectOffset(0, 0, 0, 0);
            selectorStyle.overflow = new RectOffset(0, 0, 0, 0);
            selectorStyle.alignment = TextAnchor.UpperCenter;
            selectorStyle.fontSize = 12;
            selectorStyle.clipping = TextClipping.Clip;
            selectorStyle.wordWrap = true;
            if (selected != null)
            {
                this.selected = selected;
                canSelectNull = false;
            }
            else canSelectNull = true;
        }
        public override Vector2 GetWindowSize()
        {
            Vector2 windowSize = base.GetWindowSize();
            windowSize.y = 32 + (Mathf.Clamp((weaponIKAdjusts.Count + (canSelectNull ? 1 : 0)) * 25, 57, 300));
            return windowSize;
        }
        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.Box(weaponCategory.ToUpper() + " IK Adjust Selector", skin.GetStyle("WindowBG"), GUILayout.Width(rect.width), GUILayout.Height(30));
            scroll = GUILayout.BeginScrollView(scroll, false, false, GUILayout.Width(rect.width));
            GUI.color = selected == null ? Color.green : Color.white;
            if (canSelectNull && GUILayout.Button("None", selectorStyle, GUILayout.Height(25), GUILayout.Width(rect.width)))
            {
                selected = null;
                editorWindow.Close();
            }
            for (int i = 0; i < weaponIKAdjusts.Count; i++)
            {
                GUI.color = selected == weaponIKAdjusts[i] ? Color.green : Color.white;
                if (GUILayout.Button(weaponIKAdjusts[i].name, selectorStyle, GUILayout.Height(25), GUILayout.Width(rect.width)))
                {
                    selected = weaponIKAdjusts[i];
                    EditorGUIUtility.PingObject(weaponIKAdjusts[i]);
                    editorWindow.Close();
                }
            }
            GUI.color = Color.white;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        public override void OnOpen()
        {
            weaponIKAdjusts = FindAssetsByType<vWeaponIKAdjust>();
            if (weaponIKAdjusts.Count > 0)
            {
                weaponIKAdjusts = weaponIKAdjusts.FindAll(w => w.weaponCategories.Contains(weaponCategory));
            }
        }
        public override void OnClose()
        {
            if (selected && onSelect != null) onSelect(selected);
        }
        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }
    }

    public class vIKMessage : EditorWindow
    {
        public List<vWeaponIKAdjust> ikToAddDefault = new List<vWeaponIKAdjust>();
        public static vIKMessage curWindow;
        public GUIStyle style = new GUIStyle(EditorStyles.helpBox);
        public Vector2 scroll;
        public static void InitEditorWindow(List<vWeaponIKAdjust> ikToAddDefault)
        {
            if (!curWindow)
            {
                curWindow = (vIKMessage)EditorWindow.GetWindow<vIKMessage>("IK Message Window");
                curWindow.titleContent.image = Resources.Load("icon_v2") as Texture2D;
            }
            var position = curWindow.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            curWindow.position = position;
            curWindow.ikToAddDefault = ikToAddDefault;
        }
        void OnGUI()
        {
            style.richText = true;
            GUILayout.Label("<color = yellow><b><size = 20>Some IK Adjusts doesn't have all the default states</size></b></color>",style);
            scroll = GUILayout.BeginScrollView(scroll,GUILayout.MaxHeight(500));            
            for(int i=0;i<ikToAddDefault.Count;i++)
            {
                EditorGUILayout.ObjectField(ikToAddDefault[i],typeof(vWeaponIKAdjust),false);

            }
            GUILayout.EndScrollView();
            if(GUILayout.Button("Add default states to All IK Adjusts"))
            {
                for (int i = 0; i < ikToAddDefault.Count; i++)
                {
                    ikToAddDefault[i].AddDefaultStates();
                    EditorUtility.SetDirty(ikToAddDefault[i]);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Close();
            }
          
        }
    }

    [InitializeOnLoad]
    public class vShowMessage
    {
        static vShowMessage()
        {
            var weaponIKAdjusts = vIKAjustSelector.FindAssetsByType<vWeaponIKAdjust>();
            List<vWeaponIKAdjust> ikToAddDefault = new List<vWeaponIKAdjust>();
            if (weaponIKAdjusts.Count > 0)
            {
                ikToAddDefault = weaponIKAdjusts.FindAll
                    (
                      ik => ik.HasAllDefaultStates()==false
                    );
            }
            if(ikToAddDefault.Count>0)
            {
                vIKMessage.InitEditorWindow(ikToAddDefault);
            }
          
        }
    }

}