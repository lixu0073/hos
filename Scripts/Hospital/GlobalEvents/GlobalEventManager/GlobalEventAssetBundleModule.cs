using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalEventAssetBundleModule : BaseAssetBundleModule
{
    private GameObject root = null;

    private Dictionary<SpriteName, GameObject> sprites = new Dictionary<SpriteName, GameObject>();
    private Dictionary<GameObjectName, GameObject> gameObjects = new Dictionary<GameObjectName, GameObject>();

    public delegate void OnGameObjectGet(GameObject go);
    public delegate void OnSpriteGet(Sprite sprite);
    public delegate void OnSuccess();

    private string AssetBundleName = "global_event_asset_bundle";
    private string route = "global_event_asset_bundle";
    private int version = 3;

    public enum GameObjectName
    {
        MarieSpine,
        MoleculesSpine,
        Hamster,
        HamsterGlobal
    };

    public enum SpriteName
    {
        GE_bg_city,
        GE_bar_top_slice,
        GE_bar_top,
        Glow_edited,
        GE_board,
        Table,
        PersonalCityBackground,
        PersonalBarOutline,
        GlowPersonal,
        GlowPersonal2,
        PersonalHorizontalSlice,
        PersonalIndicator,
        PersonalBarFill
    };

    public void LoadResources(OnSuccess onSuccess, OnFailure onFailure)
    {
        if (root != null)
        {
            onSuccess?.Invoke();
            return;
        }
        GetGameObject(AssetBundleName, route, version, (go) =>
        {
            root = go;
            onSuccess?.Invoke();
        }, (ex) =>
        {
            onFailure?.Invoke(ex);
        });

    }

    public void GetGameObject(GameObjectName name, OnGameObjectGet onSuccess, OnFailure onFailure, Transform parent)
    {
        if (root == null)
        {
            onFailure?.Invoke(new Exception("resources not loaded"));
            return;
        }
        GameObject go = GetGameObjectByName(name);
        if (go == null)
        {
            go = InstantiateByName(name, parent);
        }
        if (go == null)
        {
            onFailure?.Invoke(new Exception("Cannot load gameObject"));
        }
        else
        {
            onSuccess?.Invoke(go);
        }
    }

    public void GetSprite(SpriteName name, OnSpriteGet onSuccess, OnFailure onFailure)
    {
        if (root == null)
        {
            onFailure?.Invoke(new Exception("resources not loaded"));
            return;
        }
        GameObject go = GetGameObjectWithSpriteByName(name);
        if (go == null)
        {
            go = InstantiateByName(name);
        }
        if (go == null)
        {
            onFailure?.Invoke(new Exception("Cannot load sprite"));
        }
        else
        {
            onSuccess?.Invoke(go.GetComponent<Image>().sprite);
        }
    }

    public void DestroyObjects()
    {
    }

    #region Helpers

    private GameObject GetGameObjectByName(string name, Transform parent = null)
    {
        if (parent == null)
        {
            return Instantiate(root.transform.Find(name).gameObject) as GameObject;
        }
        else
        {
            return Instantiate(root.transform.Find(name).gameObject, parent) as GameObject;
        }
    }

    private GameObject InstantiateByName(SpriteName name)
    {
        if (sprites.ContainsKey(name) && sprites[name] != null)
        {
            return sprites[name];
        }
        else
        {
            sprites.Remove(name);
        }
        GameObject go = GetGameObjectByName(name.ToString());
        if (go != null)
        {
            sprites.Add(name, go);
        }
        return go;
    }

    private GameObject InstantiateByName(GameObjectName name, Transform parent)
    {
        if (gameObjects.ContainsKey(name) && gameObjects[name] != null)
        {
            return gameObjects[name];
        }
        else
        {
            gameObjects.Remove(name);
        }
        GameObject go = GetGameObjectByName(name.ToString(), parent);
        if (go != null)
        {
            gameObjects.Add(name, go);
        }
        return go;
    }

    private GameObject GetGameObjectWithSpriteByName(SpriteName name)
    {
        if (sprites.ContainsKey(name))
            return sprites[name];
        return null;
    }

    private GameObject GetGameObjectByName(GameObjectName name)
    {
        if (gameObjects.ContainsKey(name))
            return gameObjects[name];
        return null;
    }

    #endregion

}
