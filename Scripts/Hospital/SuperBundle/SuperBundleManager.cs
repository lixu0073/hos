using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SuperBundleManager
{

    private Dictionary<string, SuperBundlePackage> packages = new Dictionary<string, SuperBundlePackage>();

    public void Load(Dictionary<string, string> unparsedPackages)
    {
        packages.Clear();
        if (unparsedPackages == null)
            return;
        SuperBundleParser.Execute(packages, unparsedPackages);
    }

    public List<SuperBundlePackage> GetActivePackages()
    {
        List<SuperBundlePackage> list = new List<SuperBundlePackage>();
        foreach(KeyValuePair<string, SuperBundlePackage> packagePair in packages)
        {
            if(packagePair.Value.IsActive())
            {
                list.Add(packagePair.Value);
            }
        }
        return list;
    }

    public SuperBundlePackage GetPackage(string key)
    {
        if (packages.ContainsKey(key))
            return packages[key];
        return null;
    }

}
