using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameAssetBundle
{
    public AssetBundle assetBundle;

    public void Initialize(AssetBundle assetBundle = null)
    {
        this.assetBundle = assetBundle;
        OnInitalized();
    }
    protected abstract void OnInitalized();
    public abstract bool IsLoaded();
    public abstract IEnumerator LoadContent();
    public abstract IEnumerator LoadContentFromResources();

    public abstract bool IsOnlyToCache();

}
