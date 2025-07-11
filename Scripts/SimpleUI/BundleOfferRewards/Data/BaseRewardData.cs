using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public abstract class BaseRewardData : ScriptableObject
    {
        [SerializeField]
        protected int amount;

        public abstract BubbleBoyReward GetReward();

    }
}