using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SuperBundleParser
{

    public static void Execute(Dictionary<string, SuperBundlePackage> packages, Dictionary<string, string> unparsedPackages)
    {
        foreach(KeyValuePair<string, string> unparsedPackage in unparsedPackages)
        {
            try
            {
                packages.Add(unparsedPackage.Key, new SuperBundlePackage(unparsedPackage.Key, unparsedPackage.Value));
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
	
}
