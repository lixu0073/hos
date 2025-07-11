using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CacheOnlyAssetBundle : BaseGameAssetBundle
{
    public override bool IsLoaded()
    {
        return false;
    }

    public override bool IsOnlyToCache()
    {
        return true;
    }

    public override IEnumerator LoadContent()
    {
        yield return null;
    }

    public override IEnumerator LoadContentFromResources()
    {
        yield return null;
    }

    protected override void OnInitalized() {}
}
