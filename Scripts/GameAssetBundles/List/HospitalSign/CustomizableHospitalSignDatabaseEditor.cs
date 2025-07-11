using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomizableHospitalSignDatabase))]
public class CustomizableHospitalSignDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CustomizableHospitalSignDatabase mTarget = (CustomizableHospitalSignDatabase)target;
        if (GUILayout.Button("SetExp"))
        {
            mTarget.SetSignExp();
        }
    }
}
#endif