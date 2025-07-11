using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hospital;
using IsoEngine;


public class DoctorRoomInfo : ShopRoomInfo
{
#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/RoomInfo/DoctorRoomInfo")]
	public new static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<DoctorRoomInfo>();
	}
#endif

    public string DoctorDescription;
    public int cureTime = 25;
    public int cureXpReward = 1;
    public int cureCoinsReward = 1;
    public int curePositiveEnergyReward = 1;

    public int CurePositiveEnergyReward {
        get
        {
            return curePositiveEnergyReward;
        }
    }

    public Sprite CollectableImage = null;

	public DoctorMachineType MachineColor = DoctorMachineType.Blue;

    public Vector3[] ElixirTubePositions = null;   //used for position of '-1 particle' when adding elixir; North = 0, East = 1, South = 2, West = 3   
    /* pozycje przed przestawianiem:
    2.9f, 0.3f, 2.9f
    3f, 0.3f, 5f
    5f, 0.3f, 3f
    3f, 0.3f, 2.9f
    */

    public Color DoctorColor;       //used for character info clouds
    public ClinicDiseaseDatabaseEntry CuredDisease;

    public Vector3[] DocChairPos;
    public Vector3[] DocIdlePos;
    public Vector3[] HintOffset;
}

