using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    [CreateAssetMenu(fileName = "FakedContributionConfigFallback", menuName = "Faked Contribution Config Fallback")]
    public class FakedContributionConfigFallback : ScriptableObject
    {
        [System.Serializable]
        public struct InterpolationPair
        {
            public float x;
            public float y;
        }

#pragma warning disable 0649
        [SerializeField] private InterpolationPair[] pairs;
#pragma warning restore 0649

        public IEnumerable<InterpolationPair> GetPair()
        {
            foreach (InterpolationPair pair in pairs)
            {
                yield return pair;
            }
        }
    }
}