using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine.AI;

namespace Invector.vCharacterController.AI
{
    [CustomEditor(typeof(vWaypointArea))]
    [CanEditMultipleObjects]
    [InitializeOnLoad]
    public class vWaypointAreaEditor : Editor
    {
        public vWaypoint currentNode;
        public vWaypointArea pathArea;
        public SerializedObject _wayArea;
        public int indexOfWaypoint;
        public SerializedProperty editMode;
        public GUISkin skin;
        public int indexOfPatrolPoint;
        public bool hotKeys;
        public bool isPlaying;
        public Tool tool;
        private Texture2D m_Logo = null;

#if INVECTOR_AI_TEMPLATE
        [MenuItem("GameObject/Invector/FSM AI/New Waypoint Area", false, -100)]
        [MenuItem("Invector/FSM AI/Components/New Waypoint Area")]
        static void NewWaypointArea()
        {
            var wp = new GameObject("WaypointArea", typeof(vWaypointArea));

            SceneView view = SceneView.lastActiveSceneView;
            if (SceneView.lastActiveSceneView == null)
                throw new UnityException("The Scene View can't be access");

            Vector3 spawnPos = view.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            wp.transform.position = spawnPos;
        }
#endif
        [MenuItem("Invector/Melee Combat/Components/New Waypoint Area")]
        static void MeleeNewWaypointArea()
        {
            var wp = new GameObject("WaypointArea", typeof(vWaypointArea));

            SceneView view = SceneView.lastActiveSceneView;
            if (SceneView.lastActiveSceneView == null)
                throw new UnityException("The Scene View can't be access");

            Vector3 spawnPos = view.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            wp.transform.position = spawnPos;
        }

        void OnEnable()
        {
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
            tool = Tools.current;
            pathArea = (vWaypointArea)target;
            editMode = serializedObject.FindProperty("editMode");
            if (pathArea.waypoints == null)
                pathArea.waypoints = new List<vWaypoint>();
            if (pathArea.waypoints.Count > 0)
                currentNode = pathArea.waypoints[0];
            SetVisiblePoints(editMode.boolValue);
        }

        private void OnDisable()
        {
            SetVisiblePoints(editMode.boolValue);
        }

        void OnSceneGUI()
        {
            if (!pathArea) return;
            CheckNodes(ref pathArea.waypoints);

            Event e = Event.current;
            if (editMode.boolValue)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
                {
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(worldRay, out hitInfo, Mathf.Infinity))
                    {
                        CreateNode(pathArea, hitInfo.point);
                        EditorUtility.SetDirty(pathArea);
                    }
                }
                else if (e.type == EventType.MouseDown && e.button == 0 && e.control && currentNode)
                {
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(worldRay, out hitInfo, Mathf.Infinity))
                    {
                        CreatePatrolPoint(pathArea, hitInfo.point);
                        EditorUtility.SetDirty(pathArea);
                    }
                }
            }
            else
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (editMode.boolValue)
                Handles.color = Color.white;
            else
                Handles.color = new Color(1, 1, 1, 0.2f);

            if (pathArea.waypoints.Count > 0)
            {
                var node0 = pathArea.waypoints[0];
                foreach (vWaypoint node in pathArea.waypoints)
                {
                    if (node != null)
                    {
                        if (editMode.boolValue && currentNode != null)
                            Handles.color = node.isValid ? (currentNode.Equals(node) ? Color.green : Color.white) : Color.red;
                        if (node.rotateTo)
                        {
                            var color = Handles.color;
                            Handles.color = editMode.boolValue && currentNode == node ? Color.blue : new Color(1, 1, 1, 0.2f);
                            Handles.ArrowHandleCap(0, node.transform.position, node.transform.rotation, 1f, EventType.Repaint);
                            Handles.color = color;
                        }
                        if (!editMode.boolValue)
                            Handles.SphereHandleCap(0, node.transform.position, Quaternion.identity, 0.25f, EventType.Repaint);
                        else if (Handles.Button(node.transform.position, Quaternion.identity, currentNode ? (currentNode == node ? .25f : 0.15f) : .25f, currentNode ? (currentNode == node ? .25f : 0.15f) : .25f, Handles.SphereHandleCap))
                        {
                            indexOfWaypoint = pathArea.waypoints.IndexOf(node);
                            currentNode = node;
                            indexOfPatrolPoint = 0;
                            Repaint();
                        }

                        if (editMode.boolValue)
                            Handles.color = new Color(0, 0, 1, .2f);

                        Handles.DrawLine(node0.transform.position, node.transform.position);
                        node0 = node;
                        var index = pathArea.waypoints.IndexOf(node) + 1;

                        if (currentNode == null || !currentNode.Equals(node))
                        {
                            Handles.Label(node.transform.position, new GUIContent("WP-" + index.ToString("00")));
                            if (node.subPoints != null && node.subPoints.Count > 0)
                            {
                                var patrolPoint0 = node.subPoints[0];
                                Handles.DrawLine(node.transform.position, patrolPoint0.position);
                                foreach (vPoint patrolPoint in node.subPoints)
                                {
                                    if (patrolPoint != null)
                                    {
                                        Handles.DrawLine(patrolPoint0.transform.position, patrolPoint.position);
                                        patrolPoint0 = patrolPoint;
                                        Handles.CubeHandleCap(0, patrolPoint.transform.position, Quaternion.identity, 0.15f, EventType.Repaint);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Handles.color = Color.white;
            if (currentNode && editMode.boolValue)
            {
                currentNode.transform.position = Handles.DoPositionHandle(currentNode.transform.position, currentNode.transform.rotation);
                EditorGUI.BeginChangeCheck();
                var position = Handles.Slider(currentNode.transform.position - (currentNode.transform.forward * currentNode.areaRadius), -currentNode.transform.forward, 0.1f, Handles.CubeHandleCap, 0.1f);
                var dist = Vector3.Distance(currentNode.transform.position, position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(currentNode, "Change Look At Target Position");
                    currentNode.areaRadius = dist;
                }
                Handles.Label(currentNode.transform.position - (currentNode.transform.forward * currentNode.areaRadius), dist.ToString("0.00"));

                if (currentNode.rotateTo)
                {
                    Handles.color = Color.blue;
                    currentNode.transform.rotation = Handles.Disc(currentNode.transform.rotation, currentNode.transform.position, Vector3.up, .9f, true, 0.1f);
                }

                Handles.color = Color.green;
                Handles.DrawWireDisc(currentNode.transform.position, Vector3.up, currentNode.areaRadius);
                e = Event.current;
                if (e.type == EventType.MouseDown && e.button == 1 && e.shift)
                {
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(worldRay, out hitInfo, Mathf.Infinity))
                    {
                        currentNode.transform.position = hitInfo.point;
                        EditorUtility.SetDirty(pathArea);
                    }
                }
                if (currentNode.subPoints == null) currentNode.subPoints = new List<vPoint>();
                if (currentNode.subPoints.Count > 0)
                {
                    var patrolPoint0 = currentNode.subPoints[0];

                    Handles.color = Color.cyan;
                    Handles.DrawLine(currentNode.transform.position, patrolPoint0.position);
                    foreach (vPoint patrolPoint in currentNode.subPoints)
                    {
                        Handles.color = Color.cyan;
                        Handles.DrawLine(patrolPoint0.transform.position, patrolPoint.position);
                        patrolPoint0 = patrolPoint;
                        var index = currentNode.subPoints.IndexOf(patrolPoint);
                        Handles.color = patrolPoint.isValid ? Color.cyan : Color.red;
                        if (patrolPoint != null)
                        {
                            if (Handles.Button(patrolPoint.transform.position, Quaternion.Euler(0, 0, 0), .25f, .25f, Handles.CubeHandleCap))
                            {
                                indexOfPatrolPoint = currentNode.subPoints.IndexOf(patrolPoint);
                                Repaint();
                            }
                            Handles.color = new Color(1, 1, 1, 0.1f);
                            Handles.Label(patrolPoint.position, new GUIContent("P-" + (index + 1).ToString("00")));
                        }
                    }

                    Handles.color = Color.green;
                    if (currentNode.subPoints.Count > 0 && indexOfPatrolPoint < currentNode.subPoints.Count)
                    {
                        currentNode.subPoints[indexOfPatrolPoint].position = Handles.DoPositionHandle(currentNode.subPoints[indexOfPatrolPoint].position, currentNode.subPoints[indexOfPatrolPoint].transform.rotation);
                        Handles.DrawWireDisc(currentNode.subPoints[indexOfPatrolPoint].transform.position, Vector3.up, currentNode.subPoints[indexOfPatrolPoint].areaRadius);
                        if (e.type == EventType.MouseDown && e.button == 1 && e.control && currentNode)
                        {
                            Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                            RaycastHit hitInfo;
                            if (Physics.Raycast(worldRay, out hitInfo, Mathf.Infinity))
                            {
                                currentNode.subPoints[indexOfPatrolPoint].position = hitInfo.point;
                                EditorUtility.SetDirty(pathArea);
                            }
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (skin == null) skin = Resources.Load("vSkin") as GUISkin;
            GUI.skin = skin;

            _wayArea = new SerializedObject(target);
            editMode = _wayArea.FindProperty("editMode");
            var waypoints = _wayArea.FindProperty("waypoints");
            _wayArea.Update();
            pathArea = (vWaypointArea)target;

            GUILayout.BeginVertical("WAYPOINT AREA", "window", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            if (GUILayout.Button(editMode.boolValue ? "Exit Edit Mode" : "Enter Edit Mode", GUILayout.ExpandWidth(true)))
            {
                editMode.boolValue = !editMode.boolValue;
                SetVisiblePoints(editMode.boolValue);
                Repaint();
            }
            GUI.color = Color.white;
            GUI.enabled = editMode.boolValue;
            EditorGUILayout.Space();

#if UNITY_2019_1_OR_NEWER
            EditorGUILayout.HelpBox("Make sure your SceneView Gizmos is enable", MessageType.Warning);
#endif

            if (editMode.boolValue && pathArea.waypoints.Count == 0)
                EditorGUILayout.HelpBox("Starting by holding Shift and Left Click on any surface with a collider.", MessageType.Info);

            if (pathArea.waypoints != null && pathArea.waypoints.Count > 0)
            {
                for (int i = 0; i < waypoints.arraySize; i++)
                {
                    GUI.color = (i.Equals(indexOfWaypoint) ? Color.green : Color.white);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Waypoint " + (i + 1).ToString("00"), "box", GUILayout.ExpandWidth(true)))
                    {
                        indexOfWaypoint = i;
                        currentNode = pathArea.waypoints[i];
                        Selection.activeGameObject = currentNode.gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                        Selection.activeGameObject = pathArea.gameObject;
                    }

                    if (pathArea.waypoints[i] != null && !PointIsInNavMesh(pathArea.waypoints[i].transform.position))
                    {
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Out of NavMesh", MessageType.Error);
                        Repaint();
                    }

                    if (GUILayout.Button("X", GUILayout.MaxWidth(20), GUILayout.MaxHeight(25)))
                    {
                        RemoveNode(ref waypoints, i);
                        GUILayout.EndVertical();
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUI.color = Color.white;
            if (indexOfWaypoint >= waypoints.arraySize) indexOfWaypoint = waypoints.arraySize - 1;
            try
            {
                if (waypoints != null && waypoints.arraySize > 0)
                {
                    var vPoint = new SerializedObject(waypoints.GetArrayElementAtIndex(indexOfWaypoint).objectReferenceValue as vWaypoint);
                    DrawWaypoint(vPoint, waypoints);
                }
            }
            catch { }
            GUILayout.EndVertical();
            GUI.enabled = true;
            GUILayout.BeginVertical("HOT KEYS", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            hotKeys = GUILayout.Toggle(hotKeys, hotKeys ? "Hide Hot Keys" : "Show Hot Keys", "button", GUILayout.ExpandWidth(true));
            if (hotKeys)
            {
                GUILayout.BeginVertical("box");
                GUI.color = Color.green;
                GUILayout.Label("Shift + Left Mouse Click", "box");
                GUI.color = Color.white;
                GUILayout.Label("Create New Way Point");
                GUILayout.EndVertical();
                EditorGUILayout.Separator();

                GUILayout.BeginVertical("box");
                GUI.color = Color.green;
                GUILayout.Label("Ctrl + Left Mouse Click", "box");
                GUI.color = Color.white;
                GUILayout.Label("Create New Patrol Point to selected way point");
                GUILayout.EndVertical();
                EditorGUILayout.Separator();

                GUILayout.BeginVertical("box");
                GUI.color = Color.green;
                GUILayout.Label("Shift + Right Mouse Click", "box");
                GUI.color = Color.white;
                GUILayout.Label("Set click point position to way point selected");
                GUILayout.EndVertical();
                EditorGUILayout.Separator();

                GUILayout.BeginVertical("box");
                GUI.color = Color.green;
                GUILayout.Label("Ctrl + Right Mouse Click", "box");
                GUI.color = Color.white;
                GUILayout.Label("Set click point position to patrol point selected");
                GUILayout.EndVertical();
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Separator();

            GUILayout.EndVertical();
            if (Event.current.commandName == "UndoRedoPerformed")
            {
                Repaint();
            }
            if (GUI.changed)
            {
                _wayArea.ApplyModifiedProperties();
                EditorUtility.SetDirty(pathArea);
            }
        }

        public void SetVisiblePoints(bool value)
        {
            if (pathArea == null) return;
            if (pathArea.waypoints != null && pathArea.waypoints.Count > 0)
            {
                foreach (vWaypoint wP in pathArea.waypoints)
                {
                    if (wP != null)
                    {
                        if (value == true)
                            wP.transform.hideFlags = HideFlags.None;
                        else
                            wP.transform.hideFlags = HideFlags.HideInHierarchy;

                    }
                }
                if (value) Tools.current = Tool.None;
                else Tools.current = tool;
                EditorUtility.SetDirty(pathArea.gameObject);
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        bool PointIsInNavMesh(Vector3 position)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 0.5f, NavMesh.AllAreas))
            {
                if (Vector3.Distance(position, hit.position) > 0.4f)
                {
                    return false;
                }
                Repaint();
                return true;
            }
            return false;
        }

        void DrawWaypoint(SerializedObject waypoint, SerializedProperty waypoints)
        {
            if (waypoint != null)
            {
                GUILayout.BeginVertical("window", GUILayout.ExpandHeight(false));
                GUI.color = Color.green;
                GUILayout.BeginHorizontal("box");
                GUILayout.Label("Selected WP " + (indexOfWaypoint + 1).ToString("00"), GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                if (GUILayout.Button("X", GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
                {
                    RemoveNode(ref waypoints, indexOfWaypoint);
                    GUILayout.EndVertical();
                    return;
                }

                GUILayout.EndHorizontal();
                GUI.color = Color.white;

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(waypoint.FindProperty("isValid"));
                EditorGUILayout.PropertyField(waypoint.FindProperty("randomPatrolPoint"));
                EditorGUILayout.PropertyField(waypoint.FindProperty("timeToStay"));
                EditorGUILayout.PropertyField(waypoint.FindProperty("maxVisitors"));
                EditorGUILayout.PropertyField(waypoint.FindProperty("rotateTo"));
                waypoint.FindProperty("areaRadius").floatValue = EditorGUILayout.Slider("Area Radius", waypoint.FindProperty("areaRadius").floatValue, .1f, 10f);
                var vPoints = waypoint.FindProperty("subPoints");
                DrawVPoint(vPoints);
                GUILayout.EndVertical();
                if (GUI.changed)
                {
                    waypoint.ApplyModifiedProperties();
                }
            }
        }

        void DrawVPoint(SerializedProperty vPoints)
        {
            if (vPoints != null && vPoints.arraySize > 0)
            {
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal("box");
                GUILayout.Label("Patrol Points", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                for (int i = 0; i < vPoints.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();
                    if (indexOfPatrolPoint.Equals(i))
                        GUI.color = Color.cyan;
                    else
                        GUI.color = Color.white;
                    if (GUILayout.Button("P-" + (i + 1).ToString("00"), "box", GUILayout.ExpandWidth(true)))
                    {
                        indexOfPatrolPoint = i;
                        //Selection.activeGameObject = (vPoints.GetArrayElementAtIndex(i).objectReferenceValue as vPoint).gameObject;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                    if (!PointIsInNavMesh((vPoints.GetArrayElementAtIndex(i).objectReferenceValue as vPoint).transform.position))
                    {
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Out of NavMesh", MessageType.Error);
                        Repaint();
                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
                    {
                        RemoveNode(ref vPoints, i);
                        Repaint();
                        break;
                    }
                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                GUI.color = Color.white;
                if (indexOfPatrolPoint >= vPoints.arraySize) indexOfPatrolPoint = vPoints.arraySize - 1;
                var prop = vPoints.GetArrayElementAtIndex(indexOfPatrolPoint);
                if (prop != null)
                {
                    var vPoint = new SerializedObject(prop.objectReferenceValue);
                    GUILayout.BeginVertical("box");
                    GUI.color = Color.cyan;
                    GUILayout.Box("P-" + (indexOfPatrolPoint + 1).ToString("00"), GUILayout.ExpandWidth(true));
                    GUI.color = Color.white;
                    EditorGUILayout.PropertyField(vPoint.FindProperty("isValid"));
                    EditorGUILayout.PropertyField(vPoint.FindProperty("timeToStay"));
                    EditorGUILayout.PropertyField(vPoint.FindProperty("maxVisitors"));
                    vPoint.FindProperty("areaRadius").floatValue = EditorGUILayout.Slider("Area Radius", vPoint.FindProperty("areaRadius").floatValue, 1f, 10f);

                    GUILayout.EndVertical();
                    EditorGUILayout.Space();
                    GUILayout.EndVertical();

                    if (GUI.changed)
                    {
                        vPoint.ApplyModifiedProperties();
                    }
                }
            }
        }

        void RemoveNode(ref SerializedProperty waypoints, int index)
        {
            var obj = (waypoints.GetArrayElementAtIndex(index).objectReferenceValue as vPoint).gameObject;
            waypoints.DeleteArrayElementAtIndex(index);

            DestroyImmediate(obj);
        }

        void CreateNode(vWaypointArea wayArea, Vector3 position)
        {
            var nodeObj = new GameObject("node");
            var node = nodeObj.AddComponent<vWaypoint>();
            node.subPoints = new List<vPoint>();
            nodeObj.transform.position = position;
            nodeObj.transform.parent = wayArea.transform;
            wayArea.waypoints.Add(node);
            currentNode = node;
            indexOfWaypoint = wayArea.waypoints.IndexOf(currentNode);
        }

        void CreatePatrolPoint(vWaypointArea wayArea, Vector3 position)
        {
            if (currentNode)
            {
                if (currentNode.subPoints == null) currentNode.subPoints = new List<vPoint>();
                var nodeObj = new GameObject("patrolPoint");
                var node = nodeObj.AddComponent<vPoint>();
                nodeObj.transform.position = position;
                nodeObj.transform.parent = currentNode.transform;
                currentNode.subPoints.Add(node);
                indexOfPatrolPoint = currentNode.subPoints.IndexOf(node);
            }
        }

        void CheckNodes(ref List<vWaypoint> waypoints)
        {
            var wP = ((vWaypointArea)target).transform.GetComponentsInChildren<vWaypoint>();

            if (waypoints.Count != wP.Length)
                waypoints = wP.ToList();
            foreach (vWaypoint waypoint in waypoints)
            {
                var vP = waypoint.transform.GetComponentsInChildren<vPoint>();
                var _vp = vP.ToList().FindAll(p => p.transform != waypoint.transform);
                if (waypoint.subPoints.Count != _vp.Count)
                    waypoint.subPoints = _vp;
            }
        }
    }
}