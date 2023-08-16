using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class vGenericInputSelector : PopupWindowContent
{
    public vGenericInputSelector (string label,string lastInput, bool drawKeys,bool isUnityInput, UnityEngine.Events.UnityAction<string, bool> onSelectInput)
    {
        this.label = label;
        this.drawKeys = drawKeys;
        this.onSelectInput = onSelectInput;
        this.isUnityInput = isUnityInput;
        this.lastInput = lastInput;
        if(drawKeys)
        {
            toolBar = new GUIContent[] { new GUIContent("Unity Inputs", unityInputToolip), new GUIContent("KeyCodes", keyCodeTooltip) };
            selectedToolBar = isUnityInput?0:1;
        }          
        else toolBar = new GUIContent[] { new GUIContent("Unity Inputs", unityInputToolip)};
       

    }
    const string keyCodeTooltip = "KeyCode use a  Enum (GetKey, GetKeyDown,GetKeyUp)";
    const string unityInputToolip = "Unity input use input names created in InputManager (GetButton, GetButtonDown, GetButtonUp";
    public static UnityEngine.Object inputManager;
    public  static string[] _unityInputs = new string[0];
    public static string[] _unityKeys = new string[0];
    public static float minHeight = EditorGUIUtility.singleLineHeight*10;
    public static float maxHeight = EditorGUIUtility.singleLineHeight * 50;
    public static float height= EditorGUIUtility.singleLineHeight;
    public string label;
    public string search;  
   
    protected Vector2 scrollView;
    protected Vector2 scrollView2;
    protected int selectedToolBar = 0;
    protected bool drawKeys =true;
    protected bool isUnityInput;
    protected string lastInput;
    protected GUIContent[] toolBar;  
    /// <summary>
    /// Event to return the select input and if the input is an  keycode
    /// </summary>
    public UnityEngine.Events.UnityAction<string, bool> onSelectInput;

    public override Vector2 GetWindowSize()
    {
        return new Vector2(300, Mathf.Clamp((height * UnityInputs.Length),minHeight,maxHeight));
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.BeginArea(rect,"","box");
        GUILayout.Box(label,GUILayout.ExpandWidth(true));
        DrawFilter();

        selectedToolBar = GUILayout.Toolbar(selectedToolBar, toolBar);
        if(selectedToolBar==0) DrawInput( UnityInputs, false, ref scrollView,isUnityInput );
        else if (selectedToolBar ==1) DrawInput( UnityKeys, true, ref scrollView2,!isUnityInput);
        GUILayout.EndArea();
     //   if(GUI.GetNameOfFocusedControl()!= "Search Field") EditorGUI.FocusTextInControl("Search Field");

    }
    void DrawInput( string[] inputs,bool isKey,ref  Vector2 scrollView,bool hightlightName = false)
    {
        GUILayout.BeginVertical();       
        GUILayout.Space(10);
        scrollView = GUILayout.BeginScrollView(scrollView);
            for (int i=0;i< inputs.Length;i++)
            {            
                var input = inputs[i];
            if (string.IsNullOrEmpty(search) || input.Trim().StartsWith(search) || input.Trim().ToLower().StartsWith(search.ToLower()))
            {
                if (hightlightName && input.Equals(lastInput)) GUI.color = Color.green;
                if (GUILayout.Button(input, EditorStyles.toolbarButton))
                {
                    onSelectInput?.Invoke(input, isKey);
                    editorWindow.Close();
                }
            }
                GUI.color = Color.white;
            }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void DrawFilter()
    {
       
        GUILayout.BeginHorizontal();

        GUI.SetNextControlName("Search Field");
        search = GUILayout.TextField(search, EditorStyles.toolbarSearchField);
        if(string.IsNullOrEmpty(search))
        {
            var fieldRect = GUILayoutUtility.GetLastRect();
            GUI.Label(fieldRect, "  ....Search for input",EditorStyles.centeredGreyMiniLabel);
        }
          
        
        GUILayout.EndHorizontal();
    }

    static string[] UnityKeys
    {
        get
        {
            if (_unityKeys != null && _unityKeys.Length > 0) return _unityKeys;
            _unityKeys = Enum.GetNames(typeof(KeyCode));
            return _unityKeys;
        }
    }

    static string[] UnityInputs
    {
        get
        {
            if (_unityInputs != null && _unityInputs.Length > 0) return _unityInputs;

            if (!inputManager)
                inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];

            SerializedObject obj = new SerializedObject(inputManager);
            SerializedProperty axisArray = obj.FindProperty("m_Axes");

            if (_unityInputs.Length != axisArray.arraySize)
                _unityInputs = new string[axisArray.arraySize];
            for (int i = 0; i < axisArray.arraySize; ++i)
            {
                var axis = axisArray.GetArrayElementAtIndex(i);
                var name = axis.FindPropertyRelative("m_Name").stringValue;
                _unityInputs[i] = name;
            }

            return _unityInputs;
        }
    }
}
