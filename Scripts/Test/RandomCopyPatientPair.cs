using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    [System.Serializable]
    public class RandomCopyPatientPair
    {
        public GameObject random;
        public GameObject copy;

        public RandomCopyPatientPair()
        {
            random = null;
            copy = null;
        }

        public RandomCopyPatientPair(RandomCopyPatientPair rcp)
        {
            random = rcp.random;
            copy = rcp.copy;
        }

        public RandomCopyPatientPair(GameObject _random, GameObject _copy)
        {
            random = _random;
            copy = _copy;
        }

        public void Set(RandomCopyPatientPair rcp)
        {
            Set(rcp.random, rcp.copy);
        }

        public void Set(GameObject _random, GameObject _copy)
        {
            random = _random;
            copy = _copy;
        }
    }
}