using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomizableHospitalFlagDatabase))]
public class CustomizableHospitalFlagDatabaseEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CustomizableHospitalFlagDatabase mTarget = (CustomizableHospitalFlagDatabase)target;
        if (GUILayout.Button("SetExp"))
        {
            mTarget.SetFlagsExp();
        }
    }
}
#endif