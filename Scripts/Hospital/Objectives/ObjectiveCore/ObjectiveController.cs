using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveController : MonoBehaviour
{
    private const int toolsPercentageChance = 20;
    private ObjectivesGenerator objectivesGenerator;

    public List<Objective> GetAllObjectives()
    {
        return ObjectivesSynchronizer.Instance.GetAllObjectives();
    }

    public bool ObjectivesSet
    {
        get { return ObjectivesSynchronizer.Instance.ObjectivesSet; }
        private set { }
    }

    public bool IsTODOsActive()
    {
        return Game.Instance.gameState().GetHospitalLevel() >= 10;
    }

    public int ObjectivesListReward
    {
        get { return (int)(RewardForTODOsDiamonds); }
        private set { }
    }

    private Hospital.BalanceableFloat rewardForTODOsDiamondsBalanceable;
    private float RewardForTODOsDiamonds
    {
        get
        {
            if (rewardForTODOsDiamondsBalanceable == null)
            {
                rewardForTODOsDiamondsBalanceable = Hospital.BalanceableFactory.CreateRewardForTODOsDiamondsBalanceable();
            }
            return rewardForTODOsDiamondsBalanceable.GetBalancedValue();
        }
    }

    public bool ObjectivesCompleted
    {
        get { return ObjectivesSynchronizer.Instance.ObjectivesCompleted; }
        private set { }
    }

    public int ToDoCounter
    {
        get { return ObjectivesSynchronizer.Instance.ObjectivesCounter - 10; }
        private set { }
    }

    public bool ObjectivesCompletedAndClaimed
    {
        get { return ObjectivesSynchronizer.Instance.ObjectivesCompletedAndClaimed; }
        private set { }
    }

    public int ObjectivesReward
    {
        get { return ObjectivesSynchronizer.Instance.ObjectivesReward; }
        private set { }
    }

    List<Hospital.MedicineDatabaseEntry> tools;
    public void CollectObjectivesListReward(Vector2 startPos)//
    {
        if (tools == null)
        {
            tools = new List<Hospital.MedicineDatabaseEntry>();
            foreach (CureTypeInfo item in ResourcesHolder.Get().medicines.cures)
            {
                if (item.type == MedicineType.Special)
                {
                    tools.AddRange(item.medicines);
                    break;
                }
            }
        }
        int random = Random.Range(0, tools.Count);
        GameState.Get().AddResource(tools[random].GetMedicineRef(), 1, true, EconomySource.LevelGoalReward);
        UIController.get.storageCounter.AddLater(1, false);
        ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Special, startPos, 1, 0.75f, 1.75f, new Vector3(1.2f, 1.2f, 1.2f), new Vector3(1, 1, 1), tools[random].image, null, () =>
        {
            UIController.get.storageCounter.Remove(1, false);
        });
    }

    public void UpdateObjectives()
    {
        if (objectivesGenerator == null)
            objectivesGenerator = new ObjectivesGenerator();

        if (!ObjectivesSynchronizer.Instance.SetObjectives(objectivesGenerator.GetObjectives(Game.Instance.gameState().GetHospitalLevel()), false))
            ObjectivesSynchronizer.Instance.SetObjectives(objectivesGenerator.GetDynamicObjectives(), true);

        if (UIController.getHospital != null)
        {
            UIController.getHospital.SetObjectivesPanelUI();
        }
    }


    public bool IsDynamicObjective()
    {
        return ObjectivesSynchronizer.Instance.IsDynamicObjective();
    }

    public string OBJECTIVES_INFO()
    {
        if (ObjectiveParser.Instance == null)
        {
            return "";
        }

        var canBeObjectivesGenerated = ObjectiveParser.Instance.IsObjectivesCanBeDynamicGenerates();

        if (canBeObjectivesGenerated)
            return "DYNAMIC OBJECTIVES";
        else
            return "STATIC OBJECTIVES";
    }

    public void CheckCompletedBadges()
    {
        if (IsTODOsActive() && ObjectivesCompleted && !Hospital.HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            UIController.getHospital.ObjectivesPanelUI.SetTaskCompletedBadgeActive(false);
            UIController.getHospital.ObjectivesPanelUI.SetListCompletedBadgeActive(true);
            return;
        }
        else
        {
            UIController.getHospital.ObjectivesPanelUI.SetListCompletedBadgeActive(false);
        }

        if (IsTODOsActive() && !GetRewardSeen() && !Hospital.HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            UIController.getHospital.ObjectivesPanelUI.SetTaskCompletedBadgeActive(true);
            return;
        }
        else
        {
            UIController.getHospital.ObjectivesPanelUI.SetTaskCompletedBadgeActive(false);
        }
    }

    public void SetRewardSeen(bool seen)
    {
        ObjectivesSynchronizer.Instance.SetRewardSeen(seen);
    }

    public bool GetRewardSeen()
    {
        return ObjectivesSynchronizer.Instance.GetRewardSeen();
    }

    #region FakeMethods
#if UNITY_EDITOR
    public void RefreshObjectives()
    {
        UpdateObjectives();
    }
#endif

    public void CompleteAllObjectives()
    {
        List<Objective> listObject = GetAllObjectives();

        if (listObject != null && listObject.Count > 0)
        {

            for (int i = 0; i < listObject.Count; i++)
                listObject[i].CompleteGoal();
        }

        UIController.getHospital.SetObjectivesPanelUI();
    }

    #endregion
}
