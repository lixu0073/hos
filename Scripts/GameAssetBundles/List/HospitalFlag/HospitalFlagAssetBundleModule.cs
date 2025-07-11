using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HospitalFlagAssetBundleModule : BaseAssetBundleModule
{
    
    public void GetSprite(string flagName, OnSuccessSprite onSuccess, OnFailure onFailure)
    {
        HospitalFlagInfo info = GetInfo(flagName);
        if (info == null)
        {
            onFailure?.Invoke(new System.Exception("Sign info " + flagName + " not found. Probably game was lauched from mainScene. Schould from LoadingScene."));
            return;
        }
        onSuccess(info.ingameTexture);
    }

    public void GetMiniatureSprite(string flagName, OnSuccessSprite onSuccess, OnFailure onFailure)
    {
        HospitalFlagInfo info = GetInfo(flagName);
        if (info == null)
        {
            onFailure?.Invoke(new System.Exception("Sign info " + flagName + " not found. Probably game was lauched from mainScene. Schould from LoadingScene."));
            return;
        }
        GetSprite(info.miniature, info.route, info.assetbundleVersion, onSuccess, onFailure);
    }

    public void UnloadFlag(string flagName)
    {
        HospitalFlagInfo flagToUnload = GetInfo(flagName);
        if (flagToUnload != null)
            UnloadAssetBundle(flagToUnload.miniature);
    }

    private HospitalFlagInfo GetInfo(string flagName)
    {
        return ResourcesHolder.GetHospital().flagsDatabase.GetFlagInfo(flagName);
    }
	
}
