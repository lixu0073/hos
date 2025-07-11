using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAssetBundleModule : MonoBehaviour
{

    public delegate void OnSuccessSprite(Sprite sprite);
    public delegate void OnSuccessGameObject(GameObject go);
    public delegate void OnFailure(Exception exception);

    protected void GetSprite(string name, string route, int version, OnSuccessSprite onSuccess, OnFailure onFailure)
    {
        GetSpriteFromResources(name, onSuccess, onFailure);
    }

    public void GetGameObject(string name, string route, int version, OnSuccessGameObject onSuccess, OnFailure onFailure)
    {
        GetGameObjectFromResources(name, onSuccess,onFailure);
    }

    public void UnloadAssetBundle(string name)
    {
        GameAssetBundleManager.UnloadAssetBundle(name);
    }

    private void OnSuccessSpriteGet(Sprite sprite, string name, OnSuccessSprite onSuccess, OnFailure onFailure)
    {
        if (sprite == null)
        {
            onFailure?.Invoke(new Exception("Can not load " + name + " from resources"));
        }
        else
        {
            onSuccess?.Invoke(sprite);
        }
    }

    private void OnSuccessGameObjectGet(GameObject go, string name, OnSuccessGameObject onSuccess, OnFailure onFailure)
    {
        if (go == null)
        {
            onFailure?.Invoke(new Exception("Can not load gameObject " + name + " from resources"));
        }
        else
        {
            onSuccess?.Invoke(go);
        }
    }

    private void GetGameObjectFromResources(string name, OnSuccessGameObject onSuccess, OnFailure onFailure)
    {
        GameObject go = Resources.Load(name) as GameObject;
        OnSuccessGameObjectGet(go, name, onSuccess, onFailure);
    }

    private void GetSpriteFromResources(string name, OnSuccessSprite onSuccess, OnFailure onFailure)
    {
        Sprite sprite = Resources.Load<Sprite>(name) as Sprite;
        OnSuccessSpriteGet(sprite, name, onSuccess, onFailure);
    }

    private void GetGameObjectFromAssetBundle(string name, string route, int version, OnSuccessGameObject onSuccess, OnFailure onFailure)
    {
        GameAssetBundleManager.GetAssetBundle(name, route, version, (assetBundle) => {
            GameObject go = assetBundle.LoadAsset<GameObject>(name) as GameObject;
            OnSuccessGameObjectGet(go, name, onSuccess, onFailure);
        }, (exception) => {
            onFailure?.Invoke(exception);
        });
    }

    private void GetSpriteFromAssetBundle(string name, string route, int version, OnSuccessSprite onSuccess, OnFailure onFailure)
    {
        GameAssetBundleManager.GetAssetBundle(name, route, version, (assetBundle) => {
            Sprite sprite = assetBundle.LoadAsset<Sprite>(name) as Sprite;
            OnSuccessSpriteGet(sprite, name, onSuccess, onFailure);
        }, (exception) => {
            onFailure?.Invoke(exception);
        });
    }
    
}
