using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hospital
{
    class SaveCacheSingleton
    {
        private static SaveCacheSingleton instance = null;

        private Save cachedSave;

        /// <summary>
        /// Returns reference on the cached save from singleton instance.
        /// </summary>
        /// <returns></returns>
        public static Save GetCachedSave(bool updateSave = true)
        {
            Save result = GetInstance().cachedSave;

            if(updateSave)
            {                
                //CacheSave(result); //just in case that does *not* work
            }

            return result;
        }

        /// <summary>
        /// Caches reference to save in the singleton instance.
        /// </summary>
        /// <param name="saveToCache"></param>
        public static void CacheSave(Save saveToCache)
        {
            GetInstance().cachedSave = saveToCache;

            UnlinkFromBaseGameStateEvent();
            BaseGameState.OnLevelUp += instance.BaseGameState_OnLevelUp;
        }

        public static void UnlinkFromBaseGameStateEvent()
        {
            BaseGameState.OnLevelUp -= instance.BaseGameState_OnLevelUp;
        }

        //I'll be honest - I ain't got no fucking clue what this is supposed to do. it was originally, for some reason, in MaternitySceneSaveMerger exclusively,
        //to my liking should be definitely somewhere else. It's here to preserve the functionality lack of which may potentially screw up something.
        private void BaseGameState_OnLevelUp()
        {
            Save temp = GetInstance().cachedSave;

            if (Game.Instance.gameState().GetHospitalLevel() == 8)
            {
                temp.ShowSignIndicator = true;
            }
            if (Game.Instance.gameState().GetHospitalLevel() == 9)
            {
                temp.ShowPaintBadgeClinic = true;
                temp.ShowPaintBadgeLab = true;
            }
        }

        private static SaveCacheSingleton GetInstance()
        {
            InstantiateIfNeeded();

            return instance;
        }

        private static bool InstanceExists()
        {
            return instance != null;
        }

        private static void CreateInstance()
        {
            instance = new SaveCacheSingleton();
        }

        /// <summary>
        /// Checks if Singleton Instance exists, and if not - creates one.
        /// </summary>
        private static void InstantiateIfNeeded()
        {
            if(!InstanceExists())
            {
                CreateInstance();
            }
        }
    }
}
