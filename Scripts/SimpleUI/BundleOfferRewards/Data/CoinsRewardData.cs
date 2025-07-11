using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class CoinsRewardData : BaseRewardData
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Reward/Coins")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<CoinsRewardData>();
        }
#endif

        public override BubbleBoyReward GetReward()
        {
            return new SuperBundleRewardCoin(amount);
        }
    }
}