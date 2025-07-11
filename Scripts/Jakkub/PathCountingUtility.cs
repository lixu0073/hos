using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

using Hospital;

[System.Serializable]
public class PathCountingUtility : ScriptableObject
{
#if UNITY_EDITOR_WIN
	[UnityEditor.MenuItem("Assets/Create/PathCountingUtility")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<DoctorRoomInfo>();
	}
#endif
	public List<IsoEngine.Vector2i> RotationesCornurs;

	public List<IsoEngine.Vector2i> ListOfPathPoints;

	public List<IsoEngine.PathInfo> ListOfPaths;

	public void RotatePath(Rotation rot)
	{
		for (int i = 0; i < this.ListOfPaths.Count; ++i)
		{
			for (int j = 0; j < this.ListOfPaths[i].path.Count; j++)
			{
				Debug.Log("Old point: " + this.ListOfPaths[i].path[j].x + ", " + this.ListOfPaths[i].path[j].y);
				this.ListOfPaths[i].path[j] = this.Rotate(rot, this.ListOfPaths[i].path[j]);
				Debug.Log("New point: " + this.ListOfPaths[i].path[j].x + ", " + this.ListOfPaths[i].path[j].y);
			}
		}
	}

	public IsoEngine.Vector2i Rotate(Rotation rot, IsoEngine.Vector2i ver)
	{
		for (int i = 0; i < (int)rot; ++i)
		{
			Debug.Log("Starting from: " + ver.x + ", " + ver.y);
			ver = new IsoEngine.Vector2i(ver.y, -ver.x);
		}
		return new IsoEngine.Vector2i(RotationesCornurs[(int)rot].x + ver.x, RotationesCornurs[(int)rot].y + ver.y);
	}
}
