using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Hospital.TreatmentRoomHelpController))]
public class TreatmentRoomHelpControllerEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Hospital.TreatmentRoomHelpController myTarget = (Hospital.TreatmentRoomHelpController)target;

        if (GUILayout.Button("devRequestHelp"))
        {
            myTarget.devRequestHelp();
        }
    }
}
