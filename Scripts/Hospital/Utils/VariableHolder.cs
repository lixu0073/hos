using UnityEngine;
using System.Collections.Generic;
using IsoEngine;

namespace Hospital
{
    public class VariableHolder : MonoBehaviour
    {
        private static VariableHolder val;
        public static VariableHolder Get()
        {
			if (val == null)
				throw new IsoException("Fatal Failure of Variable Holder system. Delete your project and start again :v ");
			return val;
        }

        void Awake()
        {
            val = this;
        }
        
		public float BedDelay = 5f; //było 300
        
	}
}

