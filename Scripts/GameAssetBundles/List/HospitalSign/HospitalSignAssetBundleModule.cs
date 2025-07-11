using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HospitalSignAssetBundleModule : BaseAssetBundleModule
{

    public void GetSprite(string signName, OnSuccessSprite onSuccess, OnFailure onFailure)
    {
        HospitalSignInfo info = GetInfo(signName);

        if (info == null)
        {
            if (onFailure != null)
                onFailure.Invoke(new System.Exception("Sign info " + signName + " not found. Probably game was lauched from mainScene. Schould from LoadingScene."));
            return;
        }
        GetSprite(signName, info.route, info.assetbundleVersion, onSuccess, onFailure);
    }

    public void GetMiniatureSprite(string signName, OnSuccessSprite onSuccess, OnFailure onFailure)
    {
        HospitalSignInfo info = GetInfo(signName);

        if (info == null)
        {
            if (onFailure != null)
                onFailure.Invoke(new System.Exception("Sign info " + signName + " not found. Probably game was lauched from mainScene. Schould from LoadingScene."));
            return;
        }
        GetSprite(signName, info.route, info.assetbundleVersion, onSuccess, onFailure);
    }

    public HospitalSignInfo GetInfo(string signName)
    {
        return ResourcesHolder.GetHospital().signsDatabase.GetSignInfo(signName);
    }
	
}
