using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectiveDatabase {

    Dictionary<int, ObjectivePackageData> dict = new Dictionary<int, ObjectivePackageData>();
    bool dynamic;

    public void Parse(string str)
    {
        dict.Clear();

        string[] packages = str.Split('!');

        dynamic = bool.Parse(packages[0]);

        for (int i = 1; i < packages.Length; i++)
        {
            var lgpd = ObjectivePackageData.Parse(packages[i]);
            dict.Add(lgpd.Level, lgpd);
        }
    }

    public List<ObjectiveData> GetObjectiveOnLVL(int i)
    {
        ObjectivePackageData package = GetObjectivePackage(i);
        if (package != null)
            return package.GetObjectives();
        else return null;
    }

    private ObjectivePackageData GetObjectivePackage(int i)
    {
        var tmp = dict.Where((x) => { return x.Key == i; }).ToList();

        if (tmp != null && tmp.Count>0)
            return tmp.First().Value;
        return null;
    }

    public bool IsObjectivesCanBeDynamicGenerates()
    {
        return dynamic;
    }
}
