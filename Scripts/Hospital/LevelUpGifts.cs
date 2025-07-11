using UnityEngine;

namespace Hospital
{
	public class LevelUpGifts : ScriptableObject
	{
		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/LevelUpGifts")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<LevelUpGifts>();
		}
		#endif

		public LevelUpGift[] Gifts;

		[System.Serializable]
		public class LevelUpGift
		{
			public GiftedResources[] resources;
            public GiftedMedicines[] medicines;
            public GiftedDecoration[] decorations;
        }

		[System.Serializable]
		public class GiftedResources
		{
			[SerializeField] public ResourceType type;
			[SerializeField] public int amount;
		}
        
		[System.Serializable]
		public class GiftedMedicines
		{
			[SerializeField] public MedicineRef medRef;
			[SerializeField] public int amount;
		}

        [System.Serializable]
        public class GiftedDecoration
        {
            [SerializeField] public DecorationInfo medRef;
            [SerializeField] public int amount;
        }


        public void LogGiftsToString() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < Gifts.Length; i++) {
                sb.Append(string.Format("\nLEVEL {0}: ", i));

                if (Gifts[i].resources.Length > 0) {
                    for (int j = 0; j < Gifts[i].resources.Length; j++) {
                        sb.Append(string.Format(" - Resource {0} x{1}", Gifts[i].resources[j].type, Gifts[i].resources[j].amount));
                    }
                }
                if (Gifts[i].medicines.Length > 0) {
                    for (int j = 0; j < Gifts[i].medicines.Length; j++) {
                        sb.Append(string.Format(" - Medicine {0} x{1}", ResourcesHolder.Get().GetNameForCure(Gifts[i].medicines[j].medRef), Gifts[i].medicines[j].amount));
                    }
                }
                if (Gifts[i].decorations.Length > 0) {
                    for (int j = 0; j < Gifts[i].decorations.Length; j++) {
                        sb.Append(string.Format(" - Decoration {0} x{1}", I2.Loc.ScriptLocalization.Get(Gifts[i].decorations[j].medRef.ShopTitle), Gifts[i].decorations[j].amount));
                    }
                }
            }

            Debug.Log("Gifts per all level string:" + sb.ToString());
        }
    }
}
