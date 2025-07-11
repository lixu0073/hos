using UnityEngine;
using System.Collections.Generic;
using System;

namespace Hospital
{
    public class VersionManager : MonoBehaviour
    {

        #region Static

        private static VersionManager instance;

        public static VersionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("There is no VersionManager instance on scene!");
                }
                return instance;
            }
        }

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("There are possibly multiple instances of VersionManager on scene!");
            }
            instance = this;
        }

        #endregion

        public Save UpgradeSave(Save save,bool visitingPurpose = false)
        {
			int currentVersion = int.Parse(LoadingGame.version, System.Globalization.CultureInfo.InvariantCulture);
            //Debug.Log("Current Version: " + currentVersion);
            int saveVersion = int.Parse(save.version, System.Globalization.CultureInfo.InvariantCulture);
            //Debug.Log("Save Version: " + saveVersion);
            if(currentVersion == saveVersion)
            {
                return save;
            }
            if(currentVersion < saveVersion)
            {
                return save;
                throw new VersionManagerException("current version lower than save version");
            }
            for(int i = saveVersion + 1; i <= currentVersion; ++i)
            {
                IUpgradeUseCase upgradeUseCase = GetUpgradeUseCaseToVersion(i);
                if(upgradeUseCase != null)
                {
                    Debug.Log("Upgrading save to version: " + i);
                    save = upgradeUseCase.Upgrade(save,visitingPurpose);
                }
                save.version = i.ToString();
            }
            return save;
        }

        private IUpgradeUseCase GetUpgradeUseCaseToVersion(int version)
        {
            Type type = Type.GetType("Hospital.UpgradeUseCase_" + version);
            if(type == null)
            {
                return null;
            }
			System.Object obj = Activator.CreateInstance(type);
            if(obj == null)
            {
                return null;
            }
            return (IUpgradeUseCase)obj;
        }

    }
}
