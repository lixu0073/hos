using UnityEngine;
using System.Collections;

namespace Hospital
{
	public class ExternalRoomInfo : ScriptableObject
    {
		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/External/Room")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<ExternalRoomInfo>();
		}
		#endif
		public ExternalRoomType ExternalRoomType;
		public string roomName;
		public int UnlockLvl;
		public int RenovatingTimeSeconds;
        public ResourceType costResource = ResourceType.Coin;
		public int RenovationCost;
		public int ExpRecived;
	}
}
