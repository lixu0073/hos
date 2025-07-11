using UnityEditor;
using UnityEngine;

public class BuilderConfig : ScriptableObject {

    private const string PATH = "Assets/_MyHospital/Scripts/Editor/ProjectBuilder/BuilderConfig.asset";

    public string AssetBoundleTag;

    private static BuilderConfig instance;

    public static BuilderConfig Instance
    {
        get
        {
            if (instance == null)
            {
                if (System.IO.File.Exists(PATH))
                {
                    instance = AssetDatabase.LoadAssetAtPath<BuilderConfig>(PATH);
                }
                else
                {
                    instance = CreateInstance<BuilderConfig>();
                    AssetDatabase.CreateAsset(instance, PATH);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            return instance;
        }
    }
}

