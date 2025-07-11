using UnityEngine;
using UnityEngine.Events;

public class CollectOnMapGEGraphicsManager : MonoBehaviour
{
    private Sprite ItemIconActivitySprite;
    private Sprite ItemIconPreviousActivitySprite;
    private Sprite CollectActivityMainArtSprite;
    private Sprite CollectibleMapSprite;
    private Sprite CollectibleParticlesSprite;

    private bool ItemIconActivitySpriteLoaded;
    //private bool ItemIconPreviousActivitySpriteLoaded;
    private bool CollectActivityMainArtLoaded;
    private bool CollectibleMapSpriteLoaded;
    private bool CollectibleParticlesSpriteLoaded;
#pragma warning disable 0649
    [Header("References")]  
    [SerializeField] private GameObject CollectiblePrefab;
    [SerializeField] private ParticleSystem CollectibleParticleSystem;
#pragma warning restore 0649
    public static UnityAction onInstanceBinded = null;

    private static CollectOnMapGEGraphicsManager instance;
    public static CollectOnMapGEGraphicsManager GetInstance { get { return instance; } }

    //private bool collectibleParticleSystemTextureApplied;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There is more than one instance of the CollectOnMapGEGraphicsManager!");
        }
        else
        {
            instance = this;
            onInstanceBinded?.Invoke();
            onInstanceBinded = null;
        }
    }

    public enum GlobalEventTypes
    {
        NoActive,
        CollectOnMapActivityGlobalEvent,
        CurePatientActivityGlobalEvent,
        DecorationContributeGlobalEvent,
        MedicineContributeGlobalEvent,
    }

    private static GlobalEventTypes ActiveGlobalEvent = GlobalEventTypes.NoActive;

    public static void SetGlobalEventType(GlobalEventTypes typeOfGE)
    {
        ActiveGlobalEvent = typeOfGE;
    }

    public static bool LoadedFromDelta
    {
        get
        {
            return
                GetInstance != null &&
                GetInstance.GetIfLoadedFromDelta();
        }
    }

    public bool GetIfLoadedFromDelta()
    {
        bool result = false;

        switch (ActiveGlobalEvent)
        {
            case GlobalEventTypes.NoActive:
                result = false;
                break;

            case GlobalEventTypes.CollectOnMapActivityGlobalEvent:
                result = CollectActivityMainArtLoaded && CollectibleMapSpriteLoaded && CollectibleParticlesSpriteLoaded && ItemIconActivitySpriteLoaded;
                break;

            default:
                result = true;
                break;
        }

        return result;
    }

    public void ApplyCollectibleParticleSystem()
    {
        //if (!collectibleParticleSystemTextureApplied)
        //{
            CollectibleParticleSystem.gameObject.GetComponent<ParticleSystemRenderer>().material.mainTexture = CollectibleParticlesSprite.texture;
        //}
    }

    public GameObject SpawnCollectible()
    {
        GameObject collectible = GameObject.Instantiate(CollectiblePrefab);

        collectible.GetComponentInChildren<SpriteRenderer>().sprite = CollectibleMapSprite;

        return collectible;
    }

    public void LoadItemIconPreviousActivitySprite(string decisionPoint)
    {
        LoadSprite(decisionPoint, SpriteLoadTarget.ItemIconPreviousActivity);
    }

    public void LoadItemIconActivitySprite(string decisionPoint)
    {
        LoadSprite(decisionPoint, SpriteLoadTarget.ItemIconActivity);
    }

    public void LoadCollectActivityMainArtSprite(string decisionPoint)
    {
        LoadSprite(decisionPoint, SpriteLoadTarget.ActivityMainArt);
    }

    public void LoadCollectibleMapSprite(string decisionPoint)
    {
        LoadSprite(decisionPoint, SpriteLoadTarget.CollectibleMapSprite);
    }

    public void LoadCollectibleParticleSprite(string decisionPoint)
    {
        LoadSprite(decisionPoint, SpriteLoadTarget.CollectibleParticlesSprite);
    }

    private void LoadSprite(string decisionPoint, SpriteLoadTarget target)
    {
        Debug.LogError("GE sprites were in DDNA");
        //DecisionPointCalss.RequestSprite(decisionPoint, (sprite) => { 
        //    if(sprite != null)
        //    {
        //        LoadSprite(sprite, target);
        //    }
        //}, null);
    }

    private void LoadSprite(Sprite sprite, SpriteLoadTarget target)
    {
        switch (target)
        {
            case SpriteLoadTarget.ItemIconPreviousActivity:
                ItemIconPreviousActivitySprite = sprite;
                //ItemIconPreviousActivitySpriteLoaded = true;
                break;
            case SpriteLoadTarget.ActivityMainArt:
                CollectActivityMainArtSprite = sprite;
                CollectActivityMainArtLoaded = true;
                break;

            case SpriteLoadTarget.CollectibleMapSprite:
                CollectibleMapSprite = sprite;
                CollectibleMapSpriteLoaded = true;
                break;

            case SpriteLoadTarget.CollectibleParticlesSprite:
                CollectibleParticlesSprite = sprite;
                CollectibleParticlesSpriteLoaded = true;
                break;

            case SpriteLoadTarget.ItemIconActivity:
                ItemIconActivitySprite = sprite;
                ItemIconActivitySpriteLoaded = true;
                break;
        }
    }

    public Sprite GetItemIconPreviousActivitySprite()
    {
        return ItemIconPreviousActivitySprite;
    }

    public Sprite GetItemIconActivitySprite()
    {
        return ItemIconActivitySprite;
    }

    public Sprite GetCollectActivityMainArtSprite()
    {
        return CollectActivityMainArtSprite;
    }

    public Sprite GetCollectibleMapSprite()
    {
        return CollectibleMapSprite;
    }

    public Sprite GetCollectibleParticlesSprite()
    {
        return ItemIconActivitySprite;
    }

    private enum SpriteLoadTarget
    {
        ItemIconActivity,
        ActivityMainArt,
        CollectibleMapSprite,
        CollectibleParticlesSprite,
        ItemIconPreviousActivity
    }
}
