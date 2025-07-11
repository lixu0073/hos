using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using SimpleUI;
using MovementEffects;

public class CloudsManager : MonoBehaviour
{
    public static CloudsManager instance;

    [SerializeField] private float cloudDensity = 0.25f; // how many patients will have clouds
    [SerializeField] private int maxCloudAmount = 10; // how many clouds could be on map
    [SerializeField] private Vector2 spawnDelayrange = new Vector2(2f, 5f);

    public ComicCloudMessagesDatabase messageDatabase;

    public float smallCloudTime = 30;
    public float bigCloudTime = 5;

    private List<IPersonCloudController> notActiveClouds = new List<IPersonCloudController>();
    private List<IPersonCloudController> activeClouds = new List<IPersonCloudController>();
    private List<IPersonCloudController> disabled = new List<IPersonCloudController>();

    private List<int>[] freeIDs;
    private List<int>[] usedIDs;

    private bool cloudOpenerWorking = false;

    void Awake()
    {
        instance = this;
        int supportsCount = messageDatabase.messageGroups.Length;
        freeIDs = new List<int>[supportsCount];
        usedIDs = new List<int>[supportsCount];
        for (int i = 0; i < supportsCount; i++)
        {
            freeIDs[i] = new List<int>();
            usedIDs[i] = new List<int>();
            int supportLength = messageDatabase.messageGroups[i].messages.Length;
            for (int j = 0; j < supportLength; j++)
            {
                freeIDs[i].Add(j);
            }
        }
    }

    public void MoveToNotActiveClouds(IPersonCloudController cloudController)
    {
        if (activeClouds.Contains(cloudController))
        {
            activeClouds.Remove(cloudController);
            TurnOnOtherCloud();
        }
        disabled.Remove(cloudController);
        if (!notActiveClouds.Contains(cloudController))
        {
            notActiveClouds.Add(cloudController);
        }
        if (activeClouds.Count < maxCloudAmount)
        {
            TurnOnOtherCloud();
        }
    }

    public void MoveToActiveClouds(IPersonCloudController cloudController)
    {
        if (!activeClouds.Contains(cloudController))
        {
            activeClouds.Add(cloudController);
        }
        notActiveClouds.Remove(cloudController);
        disabled.Remove(cloudController);
    }

    public void MoveToDisabledClouds(IPersonCloudController cloudController)
    {
        if (activeClouds.Contains(cloudController))
        {
            activeClouds.Remove(cloudController);
            TurnOnOtherCloud();
        }
        notActiveClouds.Remove(cloudController);

        if (!disabled.Contains(cloudController))
        {
            disabled.Add(cloudController);
        }
        TurnOnOtherCloud();
    }

    public void RemoveFromClouds(IPersonCloudController cloudController)
    {
        activeClouds.Remove(cloudController);
        notActiveClouds.Remove(cloudController);
        disabled.Remove(cloudController);

        //TurnOnOtherCloud ();
    }

    private void TurnOnOtherCloud()
    {
        /*	int emergencyBrake = 0;
            while(activeClouds.Count <= maxCloudAmount && notActiveClouds.Count > 0 && activeClouds.Count / (float)(activeClouds.Count + notActiveClouds.Count) <= cloudDensity && emergencyBrake < 200){
                notActiveClouds [Random.Range (0, notActiveClouds.Count)].SetCloudState (CloudState.active);
                emergencyBrake++;
                if(emergencyBrake == 200){
                    Debug.Log("Could be worse!");
                }
            }*/

        //Timing.KillCoroutine (DelayedTurnOnOtherCloud ().GetType ());
        if (!cloudOpenerWorking && Game.Instance.gameState().GetHospitalLevel() > 2)
        { //to z lvlem można zrobić lepiej ale tak jest najszybciej i działa
            Timing.RunCoroutine(DelayedTurnOnOtherCloud(spawnDelayrange.x, spawnDelayrange.y));
        }
    }

    public string GetRandomMessageOfType(MessageType messageType)
    {
        string message = "";
        bool messageFound = false;
        int i = 0;
        while (!messageFound)
        {
            if (messageDatabase.messageGroups[i].messageType == messageType)
            {
                message = I2.Loc.ScriptLocalization.Get(messageDatabase.messageGroups[i].messages[Random.Range(0, messageDatabase.messageGroups[i].messages.Length)].key);
                messageFound = true;
                return message;
            }
            i++;
            if (i >= messageDatabase.messageGroups.Length)
            {
                return "No such a messageType in database"; //ta wiem powinien być jakiś exception
            }
        }

        return "Something went wrong";
    }

    public string GetRandomNoRepMessageOfType(MessageType messageType)
    {
        string message = "";
        bool messageFound = false;
        int i = 0;
        while (!messageFound)
        {
            if (messageDatabase.messageGroups[i].messageType == messageType)
            {
                int ID = freeIDs[i][Random.Range(0, freeIDs[i].Count)];
                freeIDs[i].Remove(ID);

                if (freeIDs[i].Count == 0)
                {
                    RefreshSupport(i);
                }
                usedIDs[i].Add(ID);

                message = I2.Loc.ScriptLocalization.Get(messageDatabase.messageGroups[i].messages[ID].key);
                GiveGift(messageDatabase.messageGroups[i].messages[ID].giftType);
                messageFound = true;
                return message;
            }
            i++;
            if (i >= messageDatabase.messageGroups.Length)
            {
                return "No such a messageType in database"; //ta wiem powinien być jakiś exception
            }
        }
        return "Something went wrong";
    }

    public void GiveGift(MessageGiftType giftType)
    {
        if (giftType == MessageGiftType.none)
        {
            return;
        }
        if (giftType == MessageGiftType.random)
        {
            GiveRandomGift();
            SoundsController.Instance.PlayReward();
            return;
        }
    }

    public void GiveRandomGift()
    {
        float prizeMoveDuration = 2;
        int giftTypeID = Random.Range(1, Game.Instance.gameState().GetHospitalLevel() > 14 ? 6 : 5); // no diamonds from clouds

        Vector3 startPoint;
        startPoint = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition);




        switch (giftTypeID)
        {
            case 0:
                //give Diamonds
                int diamondsAmount = 1;
                int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
                GameState.Get().AddResource(ResourceType.Diamonds, diamondsAmount, EconomySource.ComicCloud, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Diamond, startPoint, diamondsAmount, 0f, prizeMoveDuration, new Vector3(1f, 1f, 1), new Vector3(1, 1, 1), ResourcesHolder.Get().diamondSprite, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Diamonds, diamondsAmount, currentDiamondAmount);
            });
                break;
            case 1:
                //give Coins
                int coinsAmount = Random.Range(2, 11) * 5;
                int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
                GameState.Get().AddResource(ResourceType.Coin, coinsAmount, EconomySource.ComicCloud, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, startPoint, coinsAmount, 0f, prizeMoveDuration, new Vector3(1f, 1f, 1), new Vector3(1, 1, 1), ResourcesHolder.Get().coinSprite, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Coin, coinsAmount, currentCoinAmount);
            });
                break;
            case 2:
                //give Screwdriver
                int screwdriverAmount = 1;
                MedicineRef screwdriver = new MedicineRef(MedicineType.Special, 0);

                UIController.get.storageCounter.Add(false);

                GameState.Get().AddResource(screwdriver, screwdriverAmount, true, EconomySource.ComicCloud);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, startPoint, screwdriverAmount, 0, prizeMoveDuration, new Vector3(1f, 1f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(screwdriver), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(screwdriverAmount, false);
                });
                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(screwdriver, GameState.Get().GetCureCount(screwdriver)));
                break;
            case 3:
                //give Hammer
                int hammerAmount = 1;
                MedicineRef hammer = new MedicineRef(MedicineType.Special, 1);

                UIController.get.storageCounter.Add(false);

                GameState.Get().AddResource(hammer, hammerAmount, true, EconomySource.ComicCloud);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, startPoint, hammerAmount, 0, prizeMoveDuration, new Vector3(1f, 1f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(hammer), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(hammerAmount, false);
                });
                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(hammer, GameState.Get().GetCureCount(hammer)));
                break;
            case 4:
                //give Spanner

                int spannerAmount = 1;
                MedicineRef spanner = new MedicineRef(MedicineType.Special, 2);

                UIController.get.storageCounter.Add(false);

                GameState.Get().AddResource(spanner, spannerAmount, true, EconomySource.ComicCloud);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, startPoint, spannerAmount, 0, prizeMoveDuration, new Vector3(1f, 1f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(spanner), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(spannerAmount, false);
                });
                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(spanner, GameState.Get().GetCureCount(spanner)));
                break;
            case 5:
                //give Shovel
                int shovelAmount = 1;
                MedicineRef shovel = new MedicineRef(MedicineType.Special, 3);

                UIController.get.storageCounter.Add(false);

                GameState.Get().AddResource(shovel, shovelAmount, true, EconomySource.ComicCloud);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, startPoint, shovelAmount, 0, prizeMoveDuration, new Vector3(1f, 1f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(shovel), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(shovelAmount, false);
                });
                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(shovel, GameState.Get().GetCureCount(shovel)));
                break;
            case 6:
                //give Gum
                int gumAmount = 1;
                MedicineRef gum = new MedicineRef(MedicineType.Special, 4);

                UIController.get.storageCounter.Add(false);

                GameState.Get().AddResource(gum, gumAmount, true, EconomySource.ComicCloud);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, startPoint, gumAmount, 0, prizeMoveDuration, new Vector3(1f, 1f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(gum), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(gumAmount, false);
                });
                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(gum, GameState.Get().GetCureCount(gum)));
                break;
            case 7:
                //give Metal
                int metalAmount = 1;
                MedicineRef metal = new MedicineRef(MedicineType.Special, 5);

                UIController.get.storageCounter.Add(false);

                GameState.Get().AddResource(metal, metalAmount, true, EconomySource.ComicCloud);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, startPoint, metalAmount, 0, prizeMoveDuration, new Vector3(1f, 1f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(metal), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(metalAmount, false);
                });
                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(metal, GameState.Get().GetCureCount(metal)));
                break;
            case 8:
                //give Pipe
                int pipeAmount = 1;
                MedicineRef pipe = new MedicineRef(MedicineType.Special, 6);

                UIController.get.storageCounter.Add(false);

                GameState.Get().AddResource(pipe, pipeAmount, true, EconomySource.ComicCloud);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, startPoint, pipeAmount, 0, prizeMoveDuration, new Vector3(1f, 1f, 1), new Vector3(1.5f, 1.5f, 1), ResourcesHolder.Get().GetSpriteForCure(pipe), () =>
                {
                }, () =>
                {
                    UIController.get.storageCounter.Remove(pipeAmount, false);
                });
                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(pipe, GameState.Get().GetCureCount(pipe)));
                break;
        }

    }


    private void RefreshSupport(int ID)
    {
        freeIDs[ID].AddRange(usedIDs[ID]);
        usedIDs[ID].Clear();
    }

    IEnumerator<float> DelayedTurnOnOtherCloud(float min = 1, float max = 2)
    {
        cloudOpenerWorking = true;
        int emergencyBrake = 0;
        float delay = Random.Range(min, max);

        if (notActiveClouds.Count > 0)
        {
            while (activeClouds.Count < maxCloudAmount && notActiveClouds.Count > 0 && activeClouds.Count / (float)(activeClouds.Count + notActiveClouds.Count) < cloudDensity && emergencyBrake < 200)
            {
                yield return Timing.WaitForSeconds(delay);
                if (!(activeClouds.Count < maxCloudAmount && notActiveClouds.Count > 0 && activeClouds.Count / (float)(activeClouds.Count + notActiveClouds.Count) < cloudDensity))
                {
                    continue;
                }
                if (notActiveClouds.Count > 0)
                { //trzeba sprawdzac bo w czasie dzialania coroutiny mmoze sié zmienic z notactive na disabled
                    IPersonCloudController cloudController = notActiveClouds[Random.Range(0, notActiveClouds.Count)];
                    if (cloudController != null)
                    {
                        cloudController.SetCloudState(CloudState.active);
                    }
                }
                emergencyBrake++;
                if (emergencyBrake == 200)
                {
                    Debug.Log("Could be worse!");
                }

            }
        }
        cloudOpenerWorking = false;
    }

    /*	private void OnDestroy(){
            for(int i = 0; i < freeIDs.Length; i++){
                freeIDs [i].Clear ();
                usedIDs [i].Clear ();
            }
        }*/

    public enum CloudState
    {
        active,
        notActive,
        disabled
    }

    public enum MessageType
    {
        aDocQueue,
        cDocQueue,
        wandering,
        decoration,
        inBed,
        cured,
        newRoomWait,
        patio,
        diagnosis,
        doctor,
        nurse,
        vipbed,
        vipcured
    }

    public enum MessageGiftType
    {
        none,
        random,
        diamond,
        coin,
        tool
    }
}
