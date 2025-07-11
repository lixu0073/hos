using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using IsoEngine;

public class ObjectivesSynchronizer
{
    private ObjectivesSaveData data = new ObjectivesSaveData();

    private static ObjectivesSynchronizer instance = null;

    public static ObjectivesSynchronizer Instance
    {
        get
        {
            if (instance == null)
                instance = new ObjectivesSynchronizer();

            return instance;
        }
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();

        if (data.objectives.Count > 0)
        {
            builder.Append(Checkers.CheckedAmount(data.objectivesCounter, 0, int.MaxValue, "Objectives currentObjectivesLevel: ").ToString());
            builder.Append("#");
            builder.Append(data.dynamicObjective);
            builder.Append("#");

            for (int i = 0; i < data.objectives.Count; i++)
            {
                builder.Append(data.objectives[i].SaveToString());

                if (i < data.objectives.Count - 1)
                    builder.Append("?");
            }

            builder.Append("#");

            if (data.objectivesGeneratorData != null && data.objectivesGeneratorData.Count > 0)
            {
                int id = 0;
                foreach (KeyValuePair<ObjectivesSaveData.ObjectiveGeneratorDataType, ObjectiveGeneratorData> k in data.objectivesGeneratorData)
                {
                    builder.Append(k.Key);
                    builder.Append("@");
                    builder.Append(k.Value.SaveToString());
                    if (id < data.objectivesGeneratorData.Count - 1)
                        builder.Append("?");

                    id++;
                }
            }

            builder.Append("#");

            builder.Append(data.rewardSeen.ToString());
        }

        return builder.ToString();
    }

    public void LoadFromString(string saveString, bool visitingMode)
    {
        data.objectives.Clear();

        if (!visitingMode)
        {
            if (!string.IsNullOrEmpty(saveString))
            {
                var save = saveString.Split('#');

                if (save != null && save.Length > 2)
                {
                    data.objectivesCounter = int.Parse(save[0], System.Globalization.CultureInfo.InvariantCulture);
                    data.dynamicObjective = bool.Parse(save[1]);

                    if (!string.IsNullOrEmpty(save[2]))
                    {
                        var objectivesSaveData = save[2].Split('?');

                        if (objectivesSaveData.Length > 0)
                        {
                            for (int i = 0; i < objectivesSaveData.Length; i++)
                            {
                                var objectiveDataSave = objectivesSaveData[i].Split('!');

                                Type type = Type.GetType(objectiveDataSave[0]);
                                System.Object obj = Activator.CreateInstance(type);
                                (obj as Objective).LoadFromString(objectivesSaveData[i]);
                                (obj as Objective).AddListener();
                                data.objectives.Add(obj as Objective);
                            }

                            UIController.getHospital.ObjectivesPanelUI.SetObjectives();
                        }
                        else
                        {
                            throw new IsoEngine.IsoException("Can't load Objectives Data");
                        }
                    }

                    if (save.Length > 3 && !string.IsNullOrEmpty(save[3]))
                    {
                        var objectiesGeneratorString = save[3].Split('?');

                        if (objectiesGeneratorString.Length > 0)
                        {
                            data.objectivesGeneratorData.Clear();

                            for (int i = 0; i < objectiesGeneratorString.Length; i++)
                            {
                                var stringKey = objectiesGeneratorString[i].Split('@');
                                ObjectiveGeneratorData genDat = new ObjectiveGeneratorData();
                                ObjectivesSaveData.ObjectiveGeneratorDataType gentTypeKey = (ObjectivesSaveData.ObjectiveGeneratorDataType)Enum.Parse(typeof(ObjectivesSaveData.ObjectiveGeneratorDataType), stringKey[0]);
                                genDat.LoadFromString(stringKey[1]);
                                data.objectivesGeneratorData.Add(gentTypeKey, genDat);
                            }
                        }
                        else
                        {
                            throw new IsoEngine.IsoException("Can't load Objectives Generator Data");
                        }
                    }

                    if (save.Length > 4 && !string.IsNullOrEmpty(save[4]))
                    {
                        data.rewardSeen = bool.Parse(save[4]);
                    }
                }
            }
        }
    }

    public void Initialize()
    {
        if (!ReferenceHolder.Get().objectiveController.ObjectivesSet)
            ReferenceHolder.Get().objectiveController.UpdateObjectives();
    }

    public void Deinitialize()
    {
        if (!ReferenceHolder.Get().objectiveController.ObjectivesSet && data!=null && data.objectives!=null)
        {
            for (int i = 0; i < data.objectives.Count; i++)
            {
                data.objectives[i].OnDestroy();
            }
        }
    }

    public bool SetObjectives(List<Objective> objectives, bool isDynamic)
    {
        if (data.objectives != null && data.objectives.Count>0)
        {
            for (int i = 0; i < data.objectives.Count; i++)
                data.objectives[i].OnDestroy();

            data.objectives.Clear();
        }

        if (objectives != null && objectives.Count > 0)
        {
            for (int i = 0; i < objectives.Count; i++)
                data.objectives.Add(objectives[i]);

            if (isDynamic)
                data.objectivesCounter++;
            else
                data.objectivesCounter = Game.Instance.gameState().GetHospitalLevel();

            data.dynamicObjective = isDynamic;
            return true;
        }

        return false;
    }

    public bool IsDynamicObjective()
    {
        return data.dynamicObjective;
    }

    public string[] GetAllObjectiveParams(ObjectivesSaveData.ObjectiveGeneratorDataType objectiveGeneratorDataType)
    {
        ObjectiveGeneratorData objectiveData;

        if (data.objectivesGeneratorData.TryGetValue(objectiveGeneratorDataType, out objectiveData))
            return objectiveData.GetParams();

        return null;
    }

    public int GetObjectiveCounterTriger(ObjectivesSaveData.ObjectiveGeneratorDataType objectiveGeneratorDataType)
    {
        ObjectiveGeneratorData objectiveData;

        if (data.objectivesGeneratorData.TryGetValue(objectiveGeneratorDataType, out objectiveData))
            return objectiveData.GetCounterTrigger();

        return -1;
    }

    public void UpdateCounterTrigger(ObjectivesSaveData.ObjectiveGeneratorDataType objectiveGeneratorDataType, int counter)
    {
        ObjectiveGeneratorData objectiveData;

        if (data.objectivesGeneratorData.TryGetValue(objectiveGeneratorDataType, out objectiveData))
        {
            objectiveData.UpdateCounterTrigger(counter);
            return;
        }

        data.objectivesGeneratorData.Add(objectiveGeneratorDataType, new ObjectiveGeneratorData());

        if (data.objectivesGeneratorData.TryGetValue(objectiveGeneratorDataType, out objectiveData))
            objectiveData.UpdateCounterTrigger(counter);
    }

    public void UpdateParam(ObjectivesSaveData.ObjectiveGeneratorDataType objectiveGeneratorDataType, string param)
    {
        ObjectiveGeneratorData objectiveData;

        if (data.objectivesGeneratorData.TryGetValue(objectiveGeneratorDataType, out objectiveData))
        {
            objectiveData.UpdateParam(param);
            return;
        }

        data.objectivesGeneratorData.Add(objectiveGeneratorDataType, new ObjectiveGeneratorData());

        if (data.objectivesGeneratorData.TryGetValue(objectiveGeneratorDataType, out objectiveData))
            objectiveData.UpdateParam(param);
    }

    public List<Objective> GetAllObjectives()
    {
        return data.objectives;
    }

    public int ObjectivesCounter
    {
        get
        {
            return data.objectivesCounter;
        }
        private set { }
    }

    public void SetObjectivesCounter(int counter)
    {
        data.objectivesCounter = counter;
    }

    public bool ObjectivesCompleted
    {
        get
        {
            if (data.objectives != null && data.objectives.Count > 0)
            {
                for (int i = 0; i < data.objectives.Count; i++)
                {
                    if (!data.objectives[i].IsCompleted)
                        return false;
                }
                return true;
            }
            return false;
            //return true;
        }
        private set
        {
        }
    }

    public bool ObjectivesCompletedAndClaimed
    {
        get
        {
            if (data.objectives != null && data.objectives.Count > 0)
            {
                for (int i = 0; i < data.objectives.Count; i++)
                {
                    if (!data.objectives[i].IsCompleted || !data.objectives[i].isRewardClaimed)
                        return false;
                }
            }
            return true;
        }
        private set
        {
        }
    }

    public bool ObjectivesSet
    {
        get
        {
            if (data.objectives != null && data.objectives.Count > 0)
                return true;
            else return false;
        }
        private set { }
    }
    public int ObjectivesReward
    {
        get
        {
            if (data.objectives != null && data.objectives.Count > 0)
                    return data.objectives[0].Reward.Amount;

            return 0;
        }
        private set { }
    }

    public void SetRewardSeen(bool seen)
    {
        data.rewardSeen = seen;
    }

    public bool GetRewardSeen()
    {
        return data.rewardSeen;
    }

   
}
