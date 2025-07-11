using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteAssetManager : MonoBehaviour
{
    [SerializeField]
    private bool loadOnEnable = true;
    [SerializeField]
    private bool unloadOnDisable = true;

    [SerializeField]
    private string assetPath = "";

    private bool assetLoaded = false;

    private Image targetImage = null;

    private static Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
    private static Dictionary<string, int> spriteUsersCount = new Dictionary<string, int>();
    private static Dictionary<string, UnityAction> onResourceLoaded = new Dictionary<string, UnityAction>();

    private Coroutine loadingCoroutine = null;
    private UnityAction onEnable = null;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (loadOnEnable)
        {
            LoadSpriteAsset();
        }
    }

    private void OnDisable()
    {
        StopLoadingCoroutine();

        if (unloadOnDisable)
        {
            UnloadSpriteAsset();
        }

        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void SetAssetPath(string assetPath)
    {
        if (assetLoaded)
        {
            UnloadSpriteAsset();
        }

        this.assetPath = assetPath;
    }

    public string GetAssetPath()
    {
        return assetPath;
    }

    public void LoadSpriteAsset()
    {
        if (loadedSprites.ContainsKey(assetPath))
        {
            ++spriteUsersCount[assetPath];
            targetImage.sprite = loadedSprites[assetPath];
            assetLoaded = true;
            onEnable = null;
            return;
        }

        if (onResourceLoaded.ContainsKey(assetPath))
        {
            onResourceLoaded[assetPath] += LoadSpriteAsset;
            return;
        }

        if (!loadOnEnable && !gameObject.activeSelf)
        {
            onEnable = LoadSpriteAsset;
        }
        else
        {
            StartLoadingCoroutine();
            onEnable = null;
        }
    }

    public void UnloadSpriteAsset()
    {
        targetImage.sprite = null;
        --spriteUsersCount[assetPath];

        if (spriteUsersCount[assetPath] < 1)
        {
            Resources.UnloadAsset(loadedSprites[assetPath]);

            loadedSprites.Remove(assetPath);
            spriteUsersCount.Remove(assetPath);
        }

        assetLoaded = false;
    }


    private void StopLoadingCoroutine()
    {
        if (loadingCoroutine != null)
        {
            try { 
                StopCoroutine(loadingCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            loadingCoroutine = null;
        }
    }

    private void StartLoadingCoroutine()
    {
        StopLoadingCoroutine();
        loadingCoroutine = StartCoroutine(LoadSpriteAssetAsync());
    }

    private IEnumerator LoadSpriteAssetAsync()
    {
        onResourceLoaded.Add(assetPath, new UnityAction(() => 
        {
            spriteUsersCount.Add(assetPath, 1);
            targetImage.sprite = loadedSprites[assetPath];
            assetLoaded = true;
        }));
        ResourceRequest request = Resources.LoadAsync<Sprite>(assetPath);

        while (!request.isDone)
        {
            yield return null;
        }

        loadedSprites.Add(assetPath, request.asset as Sprite);

        onResourceLoaded[assetPath].Invoke();
        onResourceLoaded[assetPath] = null;
        onResourceLoaded.Remove(assetPath);
    }
}
