using AssetBundles;
using Facebook.Unity.Settings;
using Hospital;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class IOSBuilder : IPlatformBuilder
{
    private const string MAIN_SCENE = "main_scene_";
    private const string FONTS = "fonts_";
    private const string INTRO = "intro_";
    private const string TUTORIAL = "tutorial_";

    private const string RESOURCE_INTRO_PANEL = "Assets/_MyHospital/Resources/IntroPanel.prefab";
    private const string RESOURCE_FONTS = "Assets/_MyHospital/Fonts/Resources";
    private const string RESOURCE_RATE_CHARACTERS = "Assets/_MyHospital/Resources/RateCharacters.prefab";
    private const string RESOURCE_SAVES = "Assets/_MyHospital/Resources/Saves";
    private const string RESOURCE_TUTORIAL_CHARACTERS = "Assets/_MyHospital/Tutorial/Resources/TutorialCharacters";
    private const string RESOURCE_TUTORIAL_ANIMATIONS = "Assets/_MyHospital/Tutorial/Resources/TutorialAnimations";
    private const string RESOURCE_MAIN_SCENE = "Assets/_MyHospital/Scenes/MainScene.unity";

    private const string META = ".meta";

    private List<string> toDelete = new List<string>(new string[]{
        RESOURCE_INTRO_PANEL,RESOURCE_FONTS,RESOURCE_RATE_CHARACTERS,
        RESOURCE_SAVES,RESOURCE_TUTORIAL_CHARACTERS,RESOURCE_TUTORIAL_ANIMATIONS
   });

    protected override void AddPlatformSpecificToDictionaries()
    {
    }

    protected override void PlatformSpecificScriptLoadingScene(bool isRelease)
    {
        LoadingGame loading = GameObjectFinder.FindEavenIfNotActive(LOADING_GAME).GetComponent<LoadingGame>();
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.CHECKING_GAME_VERSIO, isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_DELTADNA_CONFIG, isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_FONTS_FROM_SERVER, isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_INTRO_FROM_SERVER, isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_TUTORIAL_FROM_SERVER, isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_GAME_BOUNDLES_FROM_SERVER, isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_SCENE_FROM_SERVER, isRelease);

        DateTime date = DateTime.Now;
        string dateString = date.ToString("yyyyMMdd_hhmm");
        BuilderConfig.Instance.AssetBoundleTag = dateString;

        ReflectionSetter.setValueInScript(loading, ReflectionSetter.BOUNDLE_SCENE_ASSET_NAME, MAIN_SCENE + dateString);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.BOUNDLE_FONT_ASSET_NAME, FONTS + dateString);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.BOUNDLE_INTRO_ASSET_NAME, INTRO + dateString);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.BOUNDLE_TUTORIAL_ASSET_NAME, TUTORIAL + dateString);

        AssetImporter.GetAtPath(RESOURCE_MAIN_SCENE).SetAssetBundleNameAndVariant(MAIN_SCENE + BuilderConfig.Instance.AssetBoundleTag, "");
        AssetImporter.GetAtPath(RESOURCE_INTRO_PANEL).SetAssetBundleNameAndVariant(INTRO + BuilderConfig.Instance.AssetBoundleTag, "");
        ChangeTagsForFilesInFolder(RESOURCE_FONTS, META, FONTS);
        ChangeTagsForFilesInFolder(RESOURCE_TUTORIAL_CHARACTERS, META, TUTORIAL);
        ChangeTagsForFilesInFolder(RESOURCE_TUTORIAL_ANIMATIONS, META, TUTORIAL);
    }

    protected override void PlatfromSpecificResources()
    {
        FacebookSettings.SelectedAppIndex = 1;
        foreach (string path in toDelete)
        {
            FileUtil.DeleteFileOrDirectory(path);
            FileUtil.DeleteFileOrDirectory(path + META);
            Debug.Log(path + " aws deleted");
        }
        AssetDatabase.Refresh();
    }
    protected override void BuildSpecyfics(BuildPlayerOptions options)
    {
        Debug.LogWarning("NOTHING HERE IN BUILD SPECIFIC ON IOS");
    }

    public override void BuildBoundles()
    {
        AssetBundlesMenuItems.BuildAssetBundles();
    }

    private void ChangeTagsForFilesInFolder(string path, string exclude, string tag)
    {
        tag += BuilderConfig.Instance.AssetBoundleTag;
        foreach (string pathUnparsed in Directory.GetFiles(Application.dataPath.Replace("Assets","") + path, "*")
            .Where(s => !s.EndsWith(exclude)))
        {
            string pathParsed = pathUnparsed.Replace("\\", "/").Remove(0, pathUnparsed.IndexOf("Assets"));
            AssetImporter.GetAtPath(pathParsed).SetAssetBundleNameAndVariant(tag, "");
            Debug.Log("Changed asset boundle tag of " + pathParsed + " to " + tag);

        }
    }

}


