using UnityEngine;
using System.Collections.Generic;

namespace Hospital
{

	public class FloraProductionMachineInfo : ShopRoomInfo
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/RoomInfo/FloraMachineInfo")]
		public new static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<FloraProductionMachineInfo	>();
		}

#endif
		public MedicineRef producedFlora;
		public List<Sprite> levels;

		public Sprite GetSprite(float percent)
		{
			percent=Mathf.Clamp01(percent);
			if (levels == null || levels.Count < 1)
				throw new IsoEngine.IsoException("Flora machine should have at least one sprite!");
			int per = (int)(percent * 100);
			return levels[per / (100 / levels.Count) - (per == 100 ? 1 : 0)];
		}
	}
}