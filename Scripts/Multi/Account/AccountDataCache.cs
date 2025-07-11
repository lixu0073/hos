using UnityEngine;
using System.Collections;

namespace Hospital
{

    public class AccountDataCache : MonoBehaviour
    {

        public enum Providers {
            DEFAULT = 1,
            FACEBOOK = 2,
            GAMECENTER = 3
        };

        private static AccountDataCache instance;

        public static AccountDataCache Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("There is no AccountDataCache instance on scene!");
                }
                return instance;
            }
        }


        void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("There are possibly multiple instances of AccountDataCache on scene!");
            }
            instance = this;
        }

        public Providers GetCurrentProvider()
        {
            return Providers.DEFAULT;
        }

        public void SaveCurrentProvider(Providers provider)
        {
            
        }

        public void DisconnectProvider(Providers provider)
        {
            if(provider == Providers.DEFAULT)
            {
                return;
            }

        }

    }

}
