using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SuperBundleLocalDatabase : ScriptableObject {
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/SuperBundleLocal")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<SuperBundleLocalDatabase>();
    }
#endif


    public List<SuperBundleLocalData> localBundles;


    public Dictionary<string,string> GetBundleDictionary()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();

        for (int i = 0; i < localBundles.Count; i++)
        {
            dict.Add(localBundles[i].key, localBundles[i].value);
        }

        return dict;
    }

}

[System.Serializable]
public class SuperBundleLocalData {

    public string key;
    public string value;
    //example value: test_product_id#samplePackageName#10#2017-02-01 00:00:00#2017-03-20 00:00:00#1#Coin!!10*Booster!1!2*Diamond!!5*Medicine!BaseElixir(1)!2*Decoration!skeleton!1
}

