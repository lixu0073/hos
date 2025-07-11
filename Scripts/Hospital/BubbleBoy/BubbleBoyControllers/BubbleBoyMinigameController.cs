using UnityEngine;
using System.Collections.Generic;
using Hospital;
using System.Linq;

public class BubbleBoyMinigameController : MonoBehaviour
{
    private List<BubbleBoyReward> firstSpecialList;
    private List<MedicineDatabaseEntry> secondMedicineList, thirdFillersList;
    private List<BubbleBoyReward> bubbleBoyReward = new List<BubbleBoyReward>();

    List<int> fourthCoinsList;

    private int randomizedSize = 7;
    //private int specialItemsToWin = 0;
    //private bool secondSpecialIsDiamond = false;

    public void GenerateMinigame()
    {
        if (firstSpecialList!=null && firstSpecialList.Count>0)
            firstSpecialList.Clear();

        if (secondMedicineList != null && secondMedicineList.Count > 0)
            secondMedicineList.Clear();

        if (thirdFillersList != null && thirdFillersList.Count > 0)
            thirdFillersList.Clear();

        if (fourthCoinsList != null && fourthCoinsList.Count > 0)
            fourthCoinsList.Clear();

        firstSpecialList = GetListForFirstSpecialList();
        secondMedicineList = GetListForSecondMedicineList(Game.Instance.gameState().GetHospitalLevel());
        thirdFillersList = GetListForThirdFillersList(secondMedicineList);
        if ((firstSpecialList == null || firstSpecialList.Count == 0) && (secondMedicineList == null || secondMedicineList.Count == 0) && (thirdFillersList == null || thirdFillersList.Count == 0))
        {
            Debug.LogError("Problem with getting lists for minigame");
            return;
        }
        fourthCoinsList = GetCoinList();

        RandomizeRewards();
        Debug.LogWarning("tmp");
    }

    public void RandomizeRewards()
    {
        bubbleBoyReward.Clear();

        Random.InitState(Animator.StringToHash(CognitoEntry.UserID));

        int tmpRandom = Random.Range(0, 100);

        int rewardsRandomized = 0;

        // FIRST LIST SPECIAL OBJECT RANDOMIZATION
        if (tmpRandom >= 85) 
        {
            RandomSettings(1);
            tmpRandom = BaseGameState.RandomNumber(100);

            if (tmpRandom >= 90)
            {
                RandomSettings(2);
                AddRandomizedDiamondReward(); 
                rewardsRandomized++;
            }
            else
            {
                RandomSettings(2);
                AddRandomizedSpecialItemReward(); 
                rewardsRandomized++;
            }
            RandomSettings(3);
            AddRandomizedSpecialItemReward(); 
            rewardsRandomized++;
        }
        else
        {
            RandomSettings(1);
            AddRandomizedSpecialItemReward();
            rewardsRandomized++;
        }

        // SECOND LIST MEDICINE RANDOMIZATION
        RandomSettings(4);
        AddRandomizedMedicineRewardFromMedicineList();
        RandomSettings(5);
        AddRandomizedMedicineRewardFromMedicineList();
        rewardsRandomized += 2;

        // THIRD LIST RANDOMIZATION
        RandomSettings(6);
        AddRandomizedMedicineRewardFromFrillersList();
        RandomSettings(7);
        AddRandomizedMedicineRewardFromFrillersList();

        rewardsRandomized += 2;

        int tmp = 8;
        while(rewardsRandomized < randomizedSize)
        {
            RandomSettings(tmp);
            AddRandomizedCoinsReward();
            rewardsRandomized++;
            ++tmp;
        }

        // Randomized item at start
        int n = bubbleBoyReward.Count;

        while (n > 1)
        {
            int k = BaseGameState.RandomNumber(n);
            --n;
            BubbleBoyReward temp = bubbleBoyReward[n];
            BubbleBoyReward temp2 = bubbleBoyReward[k];
            bubbleBoyReward[n] = temp2;
            bubbleBoyReward[k] = temp; ;
        }
    }

    public List<BubbleBoyReward> GetAllRandomizedRewards()
    {
        return bubbleBoyReward;
    }

    private void RandomSettings(int id)
    {
        Random.InitState(Animator.StringToHash(CognitoEntry.UserID) + id * 100 + BubbleBoyDataSynchronizer.Instance.TotalEntries);
    }

    private List<BubbleBoyReward> GetListForFirstSpecialList()
    {
        List<BubbleBoyReward> tmpList = new List<BubbleBoyReward>();

        // ADD ALL BUBBLEBOY DECORATION TO FIRST LIST

        for (int i = 0; i < ResourcesHolder.GetHospital().bubbleBoyDatabase.DecorationSize(); i++)
            tmpList.Add(new BubbleBoyRewardDecoration(ResourcesHolder.GetHospital().bubbleBoyDatabase.GetDecoration(i)));

        // ADD ALL BOOSTERS TO FIRST LIST ONLY WHEN PLAYER IS ON 8 LVL
        if (Game.Instance.gameState().GetHospitalLevel() >= 8)
        {
            for (int i = 0; i < ResourcesHolder.Get().boosterDatabase.boosters.Length; ++i)
            {
                if (ResourcesHolder.Get().boosterDatabase.boosters[i].canBuy)
                    tmpList.Add(new BubbleBoyRewardBooster(i));
            }
        }

        // ADD ALL SPECIAL ITEMS TO FIRST LIST
        var tmpMedicineList = new List<MedicineDatabaseEntry>();
        tmpMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Special));

        for (int i = 0; i < tmpMedicineList.Count; ++i)
        {
            if (tmpMedicineList[i].minimumLevel <= Game.Instance.gameState().GetHospitalLevel())
                tmpList.Add(new BubbleBoyRewardMedicine(tmpMedicineList[i].GetMedicineRef()));
        }

         return tmpList;
    }

    private List<MedicineDatabaseEntry> GetListForSecondMedicineList(int playerLevel)
    {
        var tmpMedicineList = new List<MedicineDatabaseEntry>();

        // Get All Needed List of Meds
        for (int i = 1; i <= (int)MedicineType.Balm; ++i)
            tmpMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType((MedicineType)i));

        // Add bacteria
        tmpMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Bacteria));

        var medicineComparerByMinimumLevel = new MedicineComparer();
        var tempList = new List<MedicineDatabaseEntry>();
        tempList = tmpMedicineList.FindAll(x => x.minimumLevel <= playerLevel);

        // Sort list of Meds
        tempList.Sort(medicineComparerByMinimumLevel); 

        var levelsIncluded = new HashSet<int>();

        for (int i = 0; i < tempList.Count; ++i)
            levelsIncluded.Add(tempList[i].minimumLevel);

        if (tempList.Count > 0)
        {
            if (tempList.Count > 3)
                tempList.RemoveAll(x => x.minimumLevel < Enumerable.ElementAt<int>(levelsIncluded, 2));
            else tempList.RemoveAll(x => x.minimumLevel < Enumerable.ElementAt<int>(levelsIncluded, 1));
        }

        if (tempList.Count == 0)
        {
            Debug.Log("Something went wrong. Medicine list is empty.");

            return null;
        }

        return tempList;
    }

    private List<MedicineDatabaseEntry> GetListForThirdFillersList(List<MedicineDatabaseEntry> list)
    {
        var tmpMedicineList = new List<MedicineDatabaseEntry>();
        for (int i = 0; i <= (int)MedicineType.BasePlant; i++)
            tmpMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfTypeForLevelLessThanSelected((MedicineType)i, GameState.Get().hospitalLevel));
        tmpMedicineList.AddRange(ResourcesHolder.Get().GetAllMedicinesOfTypeForLevelLessThanSelected(MedicineType.Vitamins, GameState.Get().hospitalLevel));
        if (list != null && list.Count>0)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                tmpMedicineList.Remove(list[i]);
            }
        }

        return tmpMedicineList;
    }

    private List<int> GetCoinList()
    {
        List<int> tmpCount = new List<int>();

        var x = ResourcesHolder.GetHospital().bubbleBoyDatabase.GetCoinsList();

        for (int i = 0; i < x.Count; ++i)
        {
            tmpCount.Add(x[i]);
        }

        return tmpCount;
    }

    public void AddRandomizedMedicineRewardFromMedicineList()
    {
        int rnd = BaseGameState.RandomNumber(100);
        if (secondMedicineList != null)
        {
            if (secondMedicineList.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, secondMedicineList.Count);
                bubbleBoyReward.Add(new BubbleBoyRewardMedicine(secondMedicineList[index].GetMedicineRef(), 1));

                secondMedicineList.RemoveAt(index);
            }
        }
    }

    public void AddRandomizedMedicineRewardFromFrillersList()
    {
        int rnd = BaseGameState.RandomNumber(100);
        if (thirdFillersList != null)
        {
            int index = UnityEngine.Random.Range(0, thirdFillersList.Count);

            if (thirdFillersList.Count > 0)
            {
                if (thirdFillersList[index].GetMedicineRef().type == MedicineType.BaseElixir || thirdFillersList[index].GetMedicineRef().type == MedicineType.AdvancedElixir ||
                    thirdFillersList[index].GetMedicineRef().type == MedicineType.BasePlant)
                    bubbleBoyReward.Add(new BubbleBoyRewardMedicine(thirdFillersList[index].GetMedicineRef(), 3));
                else
                    bubbleBoyReward.Add(new BubbleBoyRewardMedicine(thirdFillersList[index].GetMedicineRef(), 1));

                thirdFillersList.RemoveAt(index);
            }
        }
    }

    public void AddRandomizedCoinsReward()
    {
        int rnd = BaseGameState.RandomNumber(100);
        int index = UnityEngine.Random.Range(0, fourthCoinsList.Count);
        if (fourthCoinsList.Contains(500))
        {
            if (rnd >= 11)
            {
                fourthCoinsList.Remove(500);
                index = UnityEngine.Random.Range(0, fourthCoinsList.Count);
                bubbleBoyReward.Add(new BubbleBoyRewardCoin(fourthCoinsList[index]));
                fourthCoinsList.Remove(fourthCoinsList[index]);
            }
            else
            {
                fourthCoinsList.Remove(500);
                bubbleBoyReward.Add(new BubbleBoyRewardCoin(500));
            }
        }
        else
        {
            bubbleBoyReward.Add(new BubbleBoyRewardCoin(fourthCoinsList[index]));
            fourthCoinsList.Remove(fourthCoinsList[index]);
        }
    }

    // SPECIAL LIST METHODS
    public void AddRandomizedDiamondReward()
    {
        int rnd = BaseGameState.RandomNumber(100);
        int index = UnityEngine.Random.Range(0, secondMedicineList.Count);

        if (rnd >= 90)
            bubbleBoyReward.Add(new BubbleBoyRewardDiamond(3));
        else if (rnd >= 70 && rnd < 90)
            bubbleBoyReward.Add(new BubbleBoyRewardDiamond(2));
        else
            bubbleBoyReward.Add(new BubbleBoyRewardDiamond(1));
    }

    public void AddRandomizedSpecialItemReward()
    {
        int rnd = BaseGameState.RandomNumber(100);
        if (firstSpecialList != null && firstSpecialList.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, firstSpecialList.Count);
            BubbleBoyReward tmpReward = firstSpecialList[index];
            tmpReward.SetAmount(1);

            bubbleBoyReward.Add(tmpReward);
            firstSpecialList.RemoveAt(index);
        }
        else
            Debug.LogError("Problem with getting special item");
    }
}
