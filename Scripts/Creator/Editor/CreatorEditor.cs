using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CreatorController))]
class CreatorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		CreatorController creatorController = target as CreatorController;

        GUILayout.Space(10);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Space(10);
        if (GUILayout.Button("Reset"))
        {
            creatorController.Reset();
        }

        if (GUILayout.Button("Generate Walls"))
		{
			creatorController.GeneraterWalls();
		}

		if (GUILayout.Button("Create new object"))
		{
			creatorController.CreateObject();
		}

		if (GUILayout.Button("Allign"))
		{
			creatorController.Allign();
		}

        GUILayout.Space(10);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Space(10);
        creatorController.LoadingPrefab = (GameObject)UnityEditor.EditorGUILayout.ObjectField("Prefab", creatorController.LoadingPrefab, typeof(GameObject), true);

        if (GUILayout.Button("Load object"))
		{
			creatorController.LoadPrefab();
		}

        GUILayout.Space(10);
        creatorController.PrefabName = UnityEditor.EditorGUILayout.TextField("Prefab name", creatorController.PrefabName);

        if (GUILayout.Button("Save object"))
        {
            creatorController.SavePrefab();
        }

        GUILayout.Space(10);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Space(10);

        creatorController.transformPos[1] = UnityEditor.EditorGUILayout.IntField("X: ", creatorController.transformPos[1]);
        creatorController.transformPos[0] = UnityEditor.EditorGUILayout.IntField("Y: ", creatorController.transformPos[0]);

        GUILayout.Space(10);
        if (GUILayout.Button("Transform"))
        {
            creatorController.TransformContent(-creatorController.transformPos[0], creatorController.transformPos[1]);
        }

        GUILayout.Space(10);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1),});
        GUILayout.Space(10);

        creatorController.addSide = (Side)(UnityEditor.EditorGUILayout.EnumPopup("Line: ", creatorController.addSide));

        GUILayout.Space(5);

        if (GUILayout.Button("Add"))
        {
            creatorController.ChangeContentLineOnSide(creatorController.addSide, false);
        }
        if (GUILayout.Button("Remove"))
        {
            creatorController.ChangeContentLineOnSide(creatorController.addSide, true);
        }

        GUILayout.Space(10);
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Space(10);

        if (GUILayout.Button("Update Prefab"))
        {
            creatorController.SavePrefab(1);
        }

        GUILayout.Space(10);
    }
}