using UnityEngine;
using UnityEditor;
using System.Collections;
using Hospital;

[CustomEditor(typeof(MedicineDatabase))]
class MedicineDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MedicineDatabase db = target as MedicineDatabase;

        GUILayout.Space(10);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Space(10);

        if (GUILayout.Button("Calculate diamond price for ALL(except special and fake)"))
        {
            db.CalculateDiamondPriceForAll();
        }

        GUILayout.Space(10);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Space(10);

        if (GUILayout.Button("Log medicine -name-minLvl-prodTime-exp-gold-diamond"))
        {
            db.CheckPharmacyPriceForAll();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Print out medicine type/index table"))
        {
            db.LogMedicines();
        }
    }
}