using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using IsoEngine;
using Hospital;

[CustomEditor(typeof(TileController))]
[CanEditMultipleObjects]
public class TileEditor : Editor
{
	private bool toggle = false;
	private int index = 0;
	//private int oldID = 0;

	private string[] enums;
	private string[] typeEnums;
	private int[] values;

	public override void OnInspectorGUI()
	{
		if (enums == null)
		{
			enums = Enum.GetNames(typeof(TileController.SpotDirection)).Skip(1).ToArray();
		}
		if (typeEnums == null)
			typeEnums = Enum.GetNames(typeof(SpotTypes));
		if (values == null)
			values = Enumerable.Range(0, typeEnums.Length).ToArray();

		DrawDefaultInspector();

		if (targets.Length == 1)
		{

			TileController tileController = (TileController)target;

			if (tileController.GameObject == null)
			{
				EditorGUILayout.LabelField("Object", "Empty");
			}
			else
			{
				EditorGUILayout.LabelField("Object", tileController.GameObject.name);
			}

			toggle = (tileController.spotDirection != TileController.SpotDirection.None);

			if (toggle)
				index = (int)tileController.spotDirection - 1;
			else
				index = 0;

			bool newToggle = EditorGUILayout.Toggle("Set spot", toggle);

			if (newToggle != toggle)
			{
				toggle = newToggle;

				if (!toggle)
				{
					tileController.spotDirection = (TileController.SpotDirection)0;
				}
				else
				{
					tileController.spotDirection = (TileController.SpotDirection)(index + 1);
				}

				tileController.SetSpot();
			}

			if (toggle)
			{
				int newIndex = EditorGUILayout.Popup("Direction", index, enums);

				if (newIndex != index)
				{
					index = newIndex;

					tileController.spotDirection = (TileController.SpotDirection)(index + 1);

					tileController.SetSpot();
				}
                /*
				int newID = EditorGUILayout.IntPopup("id", oldID, typeEnums, values);

				if (newID != oldID)
				{
					oldID = newID;
					tileController.spotID = newID;
				}
                */
			}

			tileController.IsPassable = EditorGUILayout.Toggle("Is Passable", tileController.IsPassable);
		}
		else
		{
			bool isPassable = EditorGUILayout.Toggle("Is Passable", ((TileController)targets[0]).IsPassable);

			for (int i = 0; i < targets.Length; ++i)
			{
				((TileController)targets[i]).IsPassable = isPassable;
			}
		}
	}
}