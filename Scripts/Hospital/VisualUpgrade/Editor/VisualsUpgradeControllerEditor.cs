using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VisualsUpgradeController))]
public class VisualsUpgradeControllerEditor : Editor
{
    int levelToSet = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        VisualsUpgradeController mTarget = (VisualsUpgradeController)target;

        levelToSet = EditorGUILayout.IntField("LevelToSet:", levelToSet);

        if (GUILayout.Button("Test: Set Level Visuals"))
        {
            mTarget.SetLevel(levelToSet);
        }
    }
}
