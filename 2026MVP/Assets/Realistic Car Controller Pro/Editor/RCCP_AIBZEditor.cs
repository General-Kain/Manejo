//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Container for the brake zones. Editor only.
/// </summary>
[CustomEditor(typeof(RCCP_AIBrakeZonesContainer))]
public class RCCP_AIBZEditor : Editor {

    RCCP_AIBrakeZonesContainer bzScript;
    GUISkin skin;

    private void OnEnable() {

        skin = Resources.Load<GUISkin>("RCCP_Gui");

    }

    public override void OnInspectorGUI() {

        bzScript = (RCCP_AIBrakeZonesContainer)target;
        serializedObject.Update();
        GUI.skin = skin;

        if (GUILayout.Button("Delete Brake Zones")) {

            bool isPrefab = PrefabUtility.IsPartOfAnyPrefab(bzScript.gameObject);

            if (isPrefab) {

                bool unpackPrefab = EditorUtility.DisplayDialog("Realistic Car Controller Pro | Unpacking Prefab", "This brake zone container is connected to a prefab. In order to delete brake zones, you'll need to unpack the prefab connection first.", "Unpack", "Cancel");

                if (unpackPrefab)
                    PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(bzScript.gameObject), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                else
                    return;

            }

            bool confirm = EditorUtility.DisplayDialog("Realistic Car Controller Pro | Deleting Brake Zones", "Are you sure you want to delete all brake zones? You can't undo this operation.", "Delete All", "Cancel");

            if (confirm) {
                foreach (RCCP_AIBrakeZone t in bzScript.brakeZones)
                    DestroyImmediate(t.gameObject);

                bzScript.brakeZones.Clear();
                EditorUtility.SetDirty(bzScript);
            }

        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("brakeZones"), new GUIContent("Brake Zones", "Brake Zones"), true);

        EditorGUILayout.HelpBox("Create BrakeZones By Shift + Left Mouse Button On Your Road", MessageType.Info);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(bzScript);

    }

    private void OnSceneGUI() {

        Event e = Event.current;
        bzScript = (RCCP_AIBrakeZonesContainer)target;

        if (e != null) {

            if (e.isMouse && e.shift && e.type == EventType.MouseDown) {

                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit = new RaycastHit();

                int controlId = GUIUtility.GetControlID(FocusType.Passive);

                // Tell the UI your event is the main one to use, it override the selection in  the scene view
                GUIUtility.hotControl = controlId;
                // Don't forget to use the event
                Event.current.Use();

                if (Physics.Raycast(ray, out hit, 5000.0f)) {

                    Vector3 newTilePosition = hit.point;

                    GameObject wp = new GameObject("Brake Zone " + bzScript.brakeZones.Count.ToString());

                    wp.transform.position = newTilePosition;
                    wp.transform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    wp.AddComponent<RCCP_AIBrakeZone>();
                    BoxCollider bC = wp.AddComponent<BoxCollider>();
                    bC.isTrigger = true;
                    bC.size = new Vector3(10f, 3f, 10f);
                    wp.transform.SetParent(bzScript.transform);

                    bzScript.GetAllBrakeZones();

                }

            }

        }

        bzScript.GetAllBrakeZones();

    }

}
