using UnityEngine;
using System.Collections;
using Hospital;
using IsoEngine;
using SimpleUI;
using Hospital.TreatmentRoomHelpRequest;
using Hospital.LootBox;

public class Base_ReferenceHolder : MonoBehaviour
{
    #region systems
    [Header("Systems")]
    public GiftSystem giftSystem;
    public EngineController engine;

    #endregion

    #region SpecialObjects
    [Header("SpecialObjects and Other")]

    public BaseResourcesHolder elements;
    public Camera worldUICamera;
    public IAPFade iapFade;
    public ObjectiveController objectiveController;
    public AnimatorChecker iphoneXAnimatorChecker;
    public LootBoxManager lootBoxManager;
    public SaveLoadManager saveLoadManager;
    public IAPShopController IAPShopController;
    public InGameFriendsProvider inGameFriendsProvider;
    public GameObject customizationControllerHolder;
    public IFloorControllable floorControllable;
    public MultiSceneInformationController multiSceneInformationController;
    public PersonalFriendCodeProvider personalFriendCodeProvider;
    public GameObject plusOneVitaminMaker;

    public virtual void Initialize()
    {
        floorControllable = customizationControllerHolder.GetComponent<IFloorControllable>();
    }
    #endregion

    public void Start()
    {
        inGameFriendsProvider = new InGameFriendsProvider();
        personalFriendCodeProvider = new PersonalFriendCodeProvider();
        FriendsDataZipper.Intialize();
    }

}
