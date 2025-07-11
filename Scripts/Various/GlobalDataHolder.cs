using UnityEngine;
using System.Collections;

namespace Hospital
{
    public class GlobalDataHolder : MonoBehaviour
    {

        public static GlobalDataHolder instance = null;

        public bool IsCriticalErrorOccured = false;

        public AnalyticsModel analiticData;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }
}
