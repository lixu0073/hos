using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DrawerDatabase))]
public class DrawerDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DrawerDatabase db = target as DrawerDatabase;

        if (GUILayout.Button("GetAllDataAboutBuildings"))
        {
            db.GetAllData();
        }
    }
}
