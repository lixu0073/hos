using UnityEngine;
using UnityEditor;
using System.Collections;
using Hospital;

[CustomEditor(typeof(MedicineDatabaseEntry), true)]
class MedicineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MedicineDatabaseEntry medicine = target as MedicineDatabaseEntry;

        GUILayout.Space(10);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Space(10);

        if (GUILayout.Button("Calculate diamond price"))
        {
            medicine.CalculateDiamondPrice();
        }
    }
}