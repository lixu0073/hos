using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class PositiveEnergyRewardData : BaseRewardData
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Reward/PositiveEnergy")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<PositiveEnergyRewardData>();
        }
#endif

        public override BubbleBoyReward GetReward()
        {
            return new SuperBundleRewardPositiveEnergy(amount);
        }
    }
}