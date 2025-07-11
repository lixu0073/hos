using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssetBundleInfo
{
    public string key;
    public int version;
    public string url;
    public string name;
    public string className;

    public GameAssetBundleInfo(string key, string unparsedData)
    {
        string[] unparsedArray = unparsedData.Split('#');
        if (unparsedArray.Length != 4)
            throw new Exception("Invalid length of array - split by '#'");

        this.key = key;
        name = unparsedArray[0];
        className = unparsedArray[1];
        url = unparsedArray[2];
        version = int.Parse(unparsedArray[3], System.Globalization.CultureInfo.InvariantCulture);
    }

}
