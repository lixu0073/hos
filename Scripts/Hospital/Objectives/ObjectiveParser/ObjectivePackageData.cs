using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivePackageData {

    private List <ObjectiveData> objectives;

    public ObjectivePackageData(List<ObjectiveData> objectives)
    {
        this.objectives = objectives;
    }

    public int Level
    {
        get
        {
            if (objectives != null && objectives.Count > 0)
                return objectives[0].Level;
            else return -1;
        }
        private set { }
    }

    public List<ObjectiveData> GetObjectives()
    {
        return objectives;
    }

    public static ObjectivePackageData Parse(string str)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        var goals_str = str.Split('?');
        var goals = new List<ObjectiveData>();

        for (int i = 0; i < goals_str.Length; i++)
            goals.Add(ObjectiveData.Parse(goals_str[i]));


        return new ObjectivePackageData(goals);
    }
}
