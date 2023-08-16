using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public partial class vMenuComponent
{
    public const string path = "Invector/Utils/";

    [MenuItem(path + "SimpleTrigger")]
    public static void AddSimpleTrigger()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)
            currentObject.AddComponent<Invector.vSimpleTrigger>();
    }

    [MenuItem(path + "AnimatorEventReceiver")]
    public static void AddAnimatorEventReceiver()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)        
            currentObject.AddComponent<Invector.vEventSystems.vAnimatorEventReceiver>();        
    }

    [MenuItem(path + "MessageReceiver")]
    public static void AddMessageReceiver()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)        
            currentObject.AddComponent<Invector.vMessageReceiver>();        
    }

    [MenuItem(path + "MessageSender")]
    public static void AddMessageSender()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)        
            currentObject.AddComponent<Invector.vMessageSender>();        
    }

    [MenuItem(path + "EventWithDelay")]
    public static void AddEventWithDelay()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)        
            currentObject.AddComponent<Invector.Utils.vEventWithDelay>();        
    }

    [MenuItem(path + "DestroyGameObject")]
    public static void AddDestroyGameObject()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)        
            currentObject.AddComponent<Invector.vDestroyGameObject>();        
    }

    [MenuItem(path + "DestroyOnTrigger")]
    public static void AddDestroyOnTrigger()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)
            currentObject.AddComponent<Invector.vDestroyOnTrigger>();
    }

    [MenuItem(path + "PlayRandomClip")]
    public static void AddPlayRandomClip()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)
            currentObject.AddComponent<Invector.vPlayRandomClip>();
    }

    [MenuItem(path + "RotateObject")]
    public static void AddRotateObject()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)
            currentObject.AddComponent<Invector.vRotateObject>();
    }

    [MenuItem(path + "LookAtCamera")]
    public static void AddLookAtCamera()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)
            currentObject.AddComponent<Invector.vLookAtCamera>();
    }

    [MenuItem(path + "Instantiate")]
    public static void AddInstantiate()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)
            currentObject.AddComponent<Invector.Utils.vInstantiate>();
    }

    [MenuItem(path + "SetParent")]
    public static void AddSetParent()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)
            currentObject.AddComponent<Invector.Utils.vSetParent>();
    }

    [MenuItem(path + "ResetTransform")]
    public static void AddResetTransform()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)
            currentObject.AddComponent<Invector.Utils.vResetTransform>();
    }

    [MenuItem(path + "DestroyChildrens")]
    public static void AddDestroyChildrens()
    {
        var currentObject = Selection.activeGameObject;
        if (currentObject)
            currentObject.AddComponent<Invector.Utils.vDestroyChildrens>();
    }
}