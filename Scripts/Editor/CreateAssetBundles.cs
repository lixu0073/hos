using UnityEngine;
using System.Collections;
using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles/", BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}