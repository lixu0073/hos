using UnityEngine;
using System.Collections.Generic;
using IsoEngine;
using Hospital;
using System.Linq;
using System;
using System.Collections;

public abstract class BaseResourcesHolder : MonoBehaviour
{

    void Start()
    {
        if (!medicines.InitializeDatabase())
        {
            throw new IsoException("MedicineDatabase was not properly initialized, look above for details");
        }
        PostStart();
    }

    public abstract void PostStart();

    public List<MedicineDatabaseEntry> GetMedicines()
    {
        return medicines.cures.SelectMany((x) => { return x.medicines.Where((y) => { return true; }); }).ToList();
    }

    public List<MedicineDatabaseEntry> EnumerateKnownMedicinesForLvl(int lvl)
    {
        return medicines.cures.SelectMany((x) => { return x.medicines.Where((y) => { return (y.minimumLevel <= lvl && y.minimumLevel != -2); }); }).ToList();
    }
    public List<MedicineDatabaseEntry> EnumerateKnownMedicines()
    {
        return EnumerateKnownMedicinesForLvl(Game.Instance.gameState().GetHospitalLevel());
    }

    public List<MedicineDatabaseEntry> FilterOutRepeatingDiseases(KeyValuePair<MedicineDatabaseEntry, int>[] requiredCures)
    {
        List<MedicineDatabaseEntry> listToReturn = new List<MedicineDatabaseEntry>();
        for (int i = 0; i < requiredCures.Length; ++i)
        {
            if (i == 0)
            {
                listToReturn.Add(requiredCures[i].Key);
            }
            else
            {
                if (!listToReturn.Exists(x => x.Disease.DiseaseType == requiredCures[i].Key.Disease.DiseaseType))
                {
                    listToReturn.Add(requiredCures[i].Key);
                }
            }
        }
        return listToReturn;
    }

    #region prefabs
    public GameObject RotateButton;
    public GameObject RemoveButton;
    public GameObject HoldingPanel;
    public GameObject ProgressBar;
    public GameObject BuyWithDiamondsButton;
    public GameObject ParticleUnpack;
    public GameObject ParticleExpand;
    public GameObject ParticleRemovableDecoration;
    public GameObject ParticleDiamondBuilding;
    public GameObject ParticleUnpackVIP;
    public GameObject DraggableDrawerItem;
    public GameObject UnanchorArrow;
    public Sprite coinSprite;
    public Sprite diamondSprite;
    public Sprite bbCoinSprite;
    public Sprite bbDiamondSprite;
    public Sprite PESprite;
    public Sprite expSprite;
    public Sprite timeSprite;
    public Sprite friendsDefaultAvatar;
    public Sprite cureBadge;
    public Sprite anyDocSprite;
    public Material GrayscaleMaterial;
    public GameObject MedicineTooltipPrefab;
    public GameObject MedicineLockedTooltipPrefab;
    public GameObject TextTooltipPrefab;
    public GameObject PatientTooltipPrefab;
    public FrameData frameData;

    #endregion
    [Header("AssetPathes")]
    public AssetPath[] assetPathes = null;

    [Header("HelpBadgeBackgrounds")]
    public Sprite plantationBadgeBackground;
    public Sprite epidemyBadgeBackground;
    public Sprite treatmentBadgeBackground;

    public Sprite yellowBackground;
    public Sprite greenBackground;
    public Sprite redBackground;

    public Sprite wiseAvatar;

    public Sprite GetSpriteForCure(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id].image;
    }

    public Sprite GetSpriteForCure(int type, int id)
    {
        return medicines.cures[type].medicines[id].image;
    }

    public string GetNameForCure(MedicineRef cure)
    {
        return I2.Loc.ScriptLocalization.Get(medicines.cures[(int)cure.type].medicines[cure.id].Name);
    }

    public string GetKeyNameForCure(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id].Name;
    }

    public int GetLvlForCure(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id].minimumLevel;
    }

    public bool GetIsTankStorageCure(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id].isTankStorageItem;
    }

    public int GetEXPForCure(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id].Exp;
    }
    public int GetMinPriceForCure(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id].minPrice;
    }
    public int GetDefaultPriceForCure(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id].defaultPrice;
    }
    public int GetMaxPriceForCure(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id].maxPrice;
    }
    public int GetDiamondPriceForCure(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id].diamondPrice;
    }
    public Sprite GetTableSpriteForBaseElixir(MedicineRef baseElixir, float percent)
    {
        var p = medicines.cures[(int)baseElixir.type].medicines[baseElixir.id] as BaseElixirInfo;
        if (p == null)
            return null;
        else
            return p.GetSpriteForTable(percent);
    }
    public Sprite GetPlantSpriteForBasePlant(MedicineRef basePlant, float percent)
    {
        if (basePlant == null)
        {
            return null;
        }
        var p = medicines.cures[(int)basePlant.type].medicines[basePlant.id] as BasePlantInfo;
        if (p == null)
            return null;
        else
            return p.GetSpriteForPlant(percent);
    }

    public Sprite GetDeadPlantSprite(MedicineRef basePlant)
    {
        var p = medicines.cures[(int)basePlant.type].medicines[basePlant.id] as BasePlantInfo;
        if (p == null)
            return null;
        else
            return p.GetDeadSpriteForPlant();
    }


    public MedicineDatabaseEntry GetMedicineInfos(MedicineRef cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id];
    }

    public MedicineDatabaseEntry GetMedicineInfos(MedicineBadgeHintInfo cure)
    {
        return medicines.cures[(int)cure.type].medicines[cure.id];
    }

    public List<MedicineDatabaseEntry> GetMedicinesOfType(MedicineType type)
    {
        //return medicines.cures[(int)type].medicines;
        //-2 because there's one medicine on the list which is not used in the game currently
        return medicines.cures[(int)type].medicines.Where(x => (x.minimumLevel <= Game.Instance.gameState().GetHospitalLevel() && x.minimumLevel != -2)).ToList();
    }
    public MedicineDatabaseEntry GetFirstLockedMedicine(MedicineType type)
    {
        int actLvl = Game.Instance.gameState().GetHospitalLevel();
        //-2 because there's one medicine on the list which is not used in the game currently
        return medicines.cures[(int)type].medicines.Where(x => (x.minimumLevel > actLvl && x.minimumLevel != -2)).OrderBy(x => x.minimumLevel).FirstOrDefault();
    }

    public List<MedicineDatabaseEntry> GetAllMedicinesOfType(MedicineType type)
    {
        //-2 because there's one medicine on the list which is not used in the game currently
        return medicines.cures[(int)type].medicines.Where(x => x.minimumLevel != -2).OrderBy(x => x.minimumLevel).ToList();
    }


    public List<MedicineDatabaseEntry> GetAllMedicinesOfTypeForLevelLessThanSelected(MedicineType type, int lvl)
    {
        //-2 because there's one medicine on the list which is not used in the game currently
        return medicines.cures[(int)type].medicines.Where(x => (x.minimumLevel != -2 && x.minimumLevel <= lvl)).OrderBy(x => x.minimumLevel).ToList();
    }

    public virtual List<Rotations> GetMachinesForLevel(int level)
    {
        return HospitalAreasMapController.Map.rotationsIDs.Where(p => ((ShopRoomInfo)p.infos).unlockLVL == level).ToList();
    }

    public List<Rotations> GetAdditionalMachines(int level)
    {
        return HospitalAreasMapController.Map.rotationsIDs.Where(p => ((ShopRoomInfo)p.infos).GetMaxAmountOnLvl(level) > ((ShopRoomInfo)p.infos).GetMaxAmountOnLvl(level - 1)).ToList();
    }

    public List<Rotations> GetUnlockedMachines()
    {
        return HospitalAreasMapController.Map.rotationsIDs.Where(p => ((ShopRoomInfo)p.infos).unlockLVL <= Game.Instance.gameState().GetHospitalLevel()).ToList();
    }

    public ShopRoomInfo GetMachineForMedicine(MedicineRef medRef)
    {

        return null;
    }

    [SerializeField]
    public BoosterDatabase boosterDatabase;
    public MedicineDatabase medicines;
    public LevelUpGifts levelUpGifts;

    #region Materials

    public Material activeBorder;
    public Material buildableBorder;
    public Material collisionBorder;
    public Material mutalPathBorder;
    public Material drawerDragableOwnColorMaterial;
    public Material drawerDragableDefaultColorMaterial;
    #endregion

    #region Textures
    public Texture UnderConstructionTile;

    #endregion

    #region prefabs

    public GameObject bordersPrefab;

    #endregion

    #region StaticVariables
    public static readonly Vector3 AnimatorPosition = new Vector3(0.024f, -0.179f, -0.1f);
    public static readonly Vector3 ChairOffsetNorth = new Vector3(0f, 0f, 0f);
    public static readonly Vector3 ChairOffsetSouth = new Vector3(-0.25142f, 0.1641219f, 0.11313f);
    public static readonly Vector3 ChairOffsetEast = new Vector3(0.12258f, 0.1641219f, -0.2f);
    public static readonly Vector3 ChairOffsetWest = new Vector3(0.02258f, 0.1641219f, 0f);

    #endregion

    #region pooling

    private void fillPool(GameObject[] pool, GameObject prefab, Transform poolParent)
    {
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation) as GameObject;
            pool[i].transform.SetParent(poolParent);
            pool[i].SetActive(false);
        }
    }
    #endregion


    #region ScriptableObjects

    #endregion

    [Header("fallBack parameters")]
    public TweakablesDefaultValuesObject TweakablesDatabase;

    public int GetDefaultValueInt(BalancableKeys key)
    {
        return TweakablesDatabase.GetValueInt(key);
    }

    public float GetDefaultValueFloat(BalancableKeys key)
    {
        return TweakablesDatabase.GetValueFloat(key);
    }

    public float GetDefaultValueFloat(StandardEventKeys key)
    {
        return TweakablesDatabase.GetValueFloat(key);
    }

    public BundledRewardDefinitionDefaults BundledRewardsDefaults;

    public string GetDefaultBundledReward(BundledRewardTypes type)
    {
        return BundledRewardsDefaults.GetDefinition(type);
    }

    public FakedContributionConfigFallback FakedContributionFallback;
    public LevelUpGiftsConfigFallback LevelUpGiftsFallback;
    public CasePrizeFallbackConfig CasePrizeFallBack;
    public ConfigFallback VipWardConfigFallback;

    [ContextMenu("LogLevelUpGifts")]
    void DebugLogLevelUpGifts()
    {
        levelUpGifts.LogGiftsToString();
    }

    [Header("ProductionGauges")]
    public Sprite gauge0;
    public Sprite gauge1;
    public Sprite gauge2;
    public Sprite gauge3;

    [Header("LootBoxBadge")]
    public Sprite lootBoxMedically;
    public Sprite lootBoxLuxury;
    public Sprite lootBoxXmas;

    [Header("ButtonSprites")]
    public Sprite blueOvalButton;
    public Sprite yellowOvalButton;
    public Sprite greenOvalButton;
    public Sprite pinkOvalButton;

    public Sprite blue9SliceButton;
    public Sprite yellow9SliceButton;
    public Sprite green9SliceButton;
    public Sprite pink9SliceButton;

    public Sprite PositiveEnergyIcon;
    public Sprite cureIcon;

    public Hospital.BoxOpening.UI.BoxOpeningPopupUI.BoxOpeningRewardAssets boxOpeningRewardAssets;
    public Hospital.BoxOpening.UI.BoxOpeningPopupUI.BoxesAssets boxesSprites;

    public BundleSpritesReferences bundledPackagesReferences;
    public BaseResourcesSpritesDatabase baseResourcesSpritesDatabase;

    public bool TryGetAssetPath(AssetTag tag, out string assetPath)
    {
        assetPath = "";
        if (assetPathes == null)
        {
            return false;
        }

        for (int i = 0; i < assetPathes.Length; ++i)
        {
            if (assetPathes[i].assetTag == tag)
            {
                assetPath = assetPathes[i].assetPath;
                return true;
            }
        }

        return false;
    }

    [Serializable]
    public struct AssetPath
    {
        public AssetTag assetTag;
        public string assetPath;
    }

    public enum AssetTag
    {
        emma1,
        emma2,
        reporter,
        wise1,
        wise2,
        leo
    }
}
