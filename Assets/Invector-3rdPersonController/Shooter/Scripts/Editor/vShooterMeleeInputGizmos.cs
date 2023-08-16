using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Invector.vCharacterController;
using Invector;
using System;
using Invector.IK;
using Invector.vShooter;

public static class vShooterMeleeInputGizmos
{
    public static vShooterMeleeInput shooter;

    public static bool showBlockAim = true;
    public static bool showAimReference = true;
    public static bool showAimPosition = true;

    [InitializeOnLoadMethod]
    public  static void AddSceneGizmos()
    {       
        SceneView.duringSceneGui -= DrawHandlers;
        SceneView.duringSceneGui += DrawHandlers;
    }
    static GameObject lastSelection;
    public static void DrawHandlers(SceneView obj)
    {
        if (!shooter || Selection.activeGameObject != lastSelection)
        {
            lastSelection = Selection.activeGameObject;
            if (Selection.activeGameObject && Selection.activeGameObject.scene != null && Selection.activeGameObject.TryGetComponent<vShooterMeleeInput>(out vShooterMeleeInput _shooter))
            {
                shooter = _shooter;
            }
        }

        if (!shooter || !shooter.shooterManager || !shooter.shooterManager.showCheckAimGizmos)
        {
            return;
        }

        if (shooter.CurrentActiveWeapon)
        {
            DrawCheckAimConditions(shooter);
            DrawAimPoints();
            DrawAimReference();
        }

        Handles.BeginGUI();
        windowRect = GUI.Window(0, windowRect, DrawOptionsWindow, "Shooter Options");
        windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
        windowRect.y = Mathf.Clamp(windowRect.y, 20, Screen.height- windowRect.height);
        Handles.EndGUI();
    }
    static Rect windowRect = new Rect(20, 20, 180, 20);

    static void DrawOptionsWindow(int id)
    {
        GUILayout.Box("Gizmos", GUILayout.ExpandWidth(true));
        showAimPosition =EditorGUILayout.Toggle("Show Aim",showAimPosition);
        showBlockAim = EditorGUILayout.Toggle("Show Block Aim ", showBlockAim);
        showAimReference = EditorGUILayout.Toggle("Show Aim Reference", showAimReference);

        if (shooter.CurrentActiveWeapon != null && GUILayout.Button("Select Weapon"))
        {
            Selection.activeObject = shooter.CurrentActiveWeapon;

        }


        if (shooter.shooterManager.weaponIKAdjustList != null && GUILayout.Button("Open IK Window"))
        {
            vShooterIKAdjustWindow.InitEditorWindow();
        }
        GUILayout.Space(5);
        if (Event.current.type == EventType.Repaint) windowRect.height = GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height+ EditorGUIUtility.standardVerticalSpacing;
        GUI.DragWindow();
    }
    public static void DrawCheckAimConditions(vShooterMeleeInput shooter)
    {
        if (!showBlockAim) return;
        Vector3 startPoint = Vector3.zero;
        Vector3 endPoint = Vector3.zero;
        shooter.UpdateCheckAimPoints(ref startPoint, ref endPoint);

        var color = shooter.shooterManager.useCheckAim? (shooter.aimConditions ? Color.green : Color.red):Color.gray;
        Handles.color = color * 0.25f;


        var normal = SceneView.currentDrawingSceneView.camera.transform.forward;
        Handles.DrawSolidDisc(startPoint, normal, shooter.shooterManager.checkAimRadius);
        Handles.DrawSolidDisc(endPoint, normal, shooter.shooterManager.checkAimRadius);
        Handles.color = color * 0.5f;
        Handles.DrawWireDisc(startPoint, normal, shooter.shooterManager.checkAimRadius);
        Handles.DrawWireDisc(endPoint, normal, shooter.shooterManager.checkAimRadius);
        var right = Quaternion.AngleAxis(90, shooter.tpCamera.transform.up) * (endPoint - startPoint).normalized;
        var up = Quaternion.AngleAxis(90, right) * (endPoint - startPoint).normalized;
        var p1 = startPoint + up * shooter.shooterManager.checkAimRadius;
        var p2 = startPoint - up * shooter.shooterManager.checkAimRadius;
        var p3 = endPoint - up * shooter.shooterManager.checkAimRadius;
        var p4 = endPoint + up * shooter.shooterManager.checkAimRadius;
        Handles.DrawSolidRectangleWithOutline(new Vector3[] { p1, p2, p3, p4 }, Color.white * 0.5f, Color.white);

        p1 = startPoint + right * shooter.shooterManager.checkAimRadius;
        p2 = startPoint - right * shooter.shooterManager.checkAimRadius;
        p3 = endPoint - right * shooter.shooterManager.checkAimRadius;
        p4 = endPoint + right * shooter.shooterManager.checkAimRadius;
        Handles.DrawSolidRectangleWithOutline(new Vector3[] { p1, p2, p3, p4 }, Color.white*0.5f, Color.white);
    }
    public static void DrawAimPoints()
    {
        if (!showAimPosition || !shooter.IsAiming || !shooter.aimConditions || System.Math.Round(shooter.armAlignmentWeight,1)<.8) return;
        Handles.color = shooter.shooterManager.alignArmToHitPoint? Color.green:Color.yellow;
        var muzzlePosition = shooter.CurrentActiveWeapon.muzzle.position;
        var muzzleForward = shooter.CurrentActiveWeapon.muzzle.forward;
        var aimPosition = shooter.AimPosition;
        Handles.DrawLine(muzzlePosition, aimPosition);
       
        Handles.ConeHandleCap(0, aimPosition, Quaternion.LookRotation(aimPosition-muzzlePosition), .05f, EventType.Repaint);

    }
    public static void DrawAimReference()
    {
        if (!showAimReference) return;
        Handles.color = Color.green;
        var aimReference = shooter.CurrentActiveWeapon.aimReference.position;
        var aimReferenceEndPoint = aimReference + shooter.CurrentActiveWeapon.aimReference.forward * Vector3.Distance(aimReference,shooter.CurrentActiveWeapon.muzzle.position);
      
        Handles.DrawLine(aimReference, aimReferenceEndPoint);      
        Handles.ConeHandleCap(0, aimReferenceEndPoint, Quaternion.LookRotation(shooter.CurrentActiveWeapon.aimReference.forward), .05f, EventType.Repaint);
       
        if (Handles.Button(aimReference, Quaternion.identity, 0.02f, 0.02f, Handles.SphereHandleCap))
        {
            Selection.activeObject = shooter.CurrentActiveWeapon.aimReference;
        }
        Handles.Label(aimReference, "Weapon Aim Reference");
    }
}
