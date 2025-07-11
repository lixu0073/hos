using UnityEngine;
using System.Collections;

namespace Hospital
{

	public class MedicineProductionMachineInfo : ShopRoomInfo
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/RoomInfo/MedicineProductionMachineInfo")]
		public new  static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<MedicineProductionMachineInfo>();
		}

#endif
        public string MachineDescription;
        public MedicineType productedMedicine;
        public Sprite FirstMedicineSprite;
	}
}