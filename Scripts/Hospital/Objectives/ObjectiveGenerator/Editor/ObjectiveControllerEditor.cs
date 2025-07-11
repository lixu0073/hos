using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectiveController))]
public class ObjectiveControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ObjectiveController myTarget = (ObjectiveController)target;

        if (GUILayout.Button("Set New Objectives"))
        {
            myTarget.RefreshObjectives();
            UIController.getHospital.ObjectivesPanelUI.SlideOutWithCoroutine();
        }

        if (GUILayout.Button("Completed Objectives"))
        {
            myTarget.CompleteAllObjectives();
            UIController.getHospital.ObjectivesPanelUI.SlideOutWithCoroutine();
        }
    }
}

