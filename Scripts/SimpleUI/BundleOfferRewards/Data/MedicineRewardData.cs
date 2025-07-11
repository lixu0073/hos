using UnityEngine;

namespace Hospital
{
    public class MedicineRewardData : BaseRewardData
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Reward/Medicine")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<MedicineRewardData>();
        }
#endif
#pragma warning disable 0649
        [SerializeField]
        private MedicineDatabaseEntry med;
#pragma warning restore 0649

        public override BubbleBoyReward GetReward()
        {
            return new SuperBundleRewardMedicine(med.GetMedicineRef(), amount);
        }
    }
}