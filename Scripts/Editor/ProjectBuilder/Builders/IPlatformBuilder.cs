using Facebook.Unity.Settings;
using Hospital;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public abstract class IPlatformBuilder {
    #region ObjectNames
    protected const string ANALYTICS_CONTROLLER = "AnalyticsController";
    protected const string LOADING_POPUP_CANVAS = "LoadingPopupCanvasNEW";
    protected const string TEST_OBJECTS = "HospitalUIPrefab/TestObjects";
    protected const string DAILY_QUEST_BUTTON = "PopUps/DebugButton";
    protected const string DAILY_QUEST_BUTTON1 = "PopUps/DebugButton (1)";
    protected const string DAILY_QUEST_BUTTON2 = "PopUps/DebugButton (2)";
    protected const string TENJI_CONTROLLER = "TenjinController";

    protected const string CONFIRM_BUTTON = "Canvas/ConfirmButton";
    protected const string LOADING_GAME = "LoadingGame";
    #endregion
   
    private SceneValidator sceneValidator;

    protected Dictionary<string, Type[]> ObjectTypesRequiredMainScene;
    protected Dictionary<string, Type[]> ObjectTypesRequiredLoadingScene;
    //protected List<string> FacebookLables = new List<string>(new string[] { "MyHospitalMac", "MyHospital_Androind_IOS" });
    //protected List<string> FacebookIds = new List<string>(new string[] { "273731103090804", "1142522002490156" });
    protected string[] scenes = { "Assets/_MyHospital/Scenes/MainScene.unity", "Assets/_MyHospital/Scenes/LoadingScene.unity" };

    private const string MY_HOSPITAL = "MyHospital";
    private const string BUNDLE_NO_SUPPORT = "platform does not support Boundle Building";

    public IPlatformBuilder()
    {
        ObjectTypesRequiredMainScene = new Dictionary<string, Type[]>();
        ObjectTypesRequiredLoadingScene = new Dictionary<string, Type[]>();
        sceneValidator = new SceneValidator();

        CreateRequiredTypesDictionaryMainScene();
        CreateRequiredTypesDictionaryLoadingScene();
        AddPlatformSpecificToDictionaries();

        //FacebookSettings.AppLabels = FacebookLables;
        //FacebookSettings.AppIds = FacebookIds;
    }

    protected abstract void AddPlatformSpecificToDictionaries();
    protected abstract void PlatformSpecificScriptLoadingScene(bool isRelease);
    protected abstract void PlatfromSpecificResources();
    protected abstract void BuildSpecyfics(BuildPlayerOptions options);

    public virtual void BuildBoundles()
    {
        Debug.LogError(BUNDLE_NO_SUPPORT);
    }

    public void ValidateMainScene()
    {
        sceneValidator.LoadMainScene();
        sceneValidator.ValidateScene(ObjectTypesRequiredMainScene);
    }

    public void ValidateLoadingScene()
    {
        sceneValidator.LoadLoadingScene();
        sceneValidator.ValidateScene(ObjectTypesRequiredLoadingScene);
    }

    public void SetGameObjectsMainScene(bool isRelease)
    {
        sceneValidator.LoadMainScene();
        SetDefaultGameObjectsMainScene(isRelease);
        EditorSceneManager.SaveOpenScenes();
    }

    public void SetGameObjectsMaternityScene(bool isRelease)
    {
        sceneValidator.LoadMaternityScene();
        SetDefaultGameObjectsMaternityScene(isRelease);
        EditorSceneManager.SaveOpenScenes();
    }


    public void SetScriptsLoadingScene(bool isRelease)
    {
        sceneValidator.LoadLoadingScene();
        SetDefaultScriptsLoadingScene(isRelease);
        PlatformSpecificScriptLoadingScene(isRelease);
        EditorSceneManager.SaveOpenScenes();
    }

    public void SetPlatformSpecyfic()
    {
        PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
        PlayerSettings.SplashScreen.showUnityLogo = false;
     
        PlatfromSpecificResources();
    }

    public void PipelineBuilding()
    {
        EditorUserBuildSettings.allowDebugging = false;
        EditorUserBuildSettings.development = false;

        BuildPlayerOptions options = new BuildPlayerOptions(); 
        options.scenes = scenes;
        options.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        options.target = EditorUserBuildSettings.activeBuildTarget;
        options.locationPathName = "PlatformBuild/";//GetDestinedPath();
        BuildSpecyfics(options);
        BuildPipeline.BuildPlayer(options);
    }

    private string GetDestinedPath()
    {
        //string[] splitPath = Application.dataPath.Split('/');
        StringBuilder builder = new StringBuilder();
        //for (int i = 0; i < splitPath.Length - 2; i++)
        //{
        //    builder.Append(splitPath[i]);
        //    builder.Append("/");
        //}
        builder.Append(MY_HOSPITAL);
        return builder.ToString();
    }

    private void CreateRequiredTypesDictionaryLoadingScene()
    {
        Debug.LogError("TODO add new ANALYTICS_CONTROLLER");
        //ObjectTypesRequiredLoadingScene.Add(ANALYTICS_CONTROLLER, new Type[] { typeof(DeltaDNAController) });
        ObjectTypesRequiredLoadingScene.Add(CONFIRM_BUTTON, new Type[] { });
        ObjectTypesRequiredLoadingScene.Add(TENJI_CONTROLLER, new Type[] { typeof(TenjinController) });
        ObjectTypesRequiredLoadingScene.Add(LOADING_GAME, new Type[] { typeof(LoadingGame) });
    }

    private void CreateRequiredTypesDictionaryMainScene()
    {
        ObjectTypesRequiredMainScene.Add(TEST_OBJECTS, new Type[] { });
        ObjectTypesRequiredMainScene.Add(LOADING_POPUP_CANVAS, new Type[] { });
        ObjectTypesRequiredMainScene.Add(DAILY_QUEST_BUTTON, new Type[] { });
        ObjectTypesRequiredMainScene.Add(DAILY_QUEST_BUTTON1, new Type[] { });
        ObjectTypesRequiredMainScene.Add(DAILY_QUEST_BUTTON2, new Type[] { });
    }

    private void SetDefaultGameObjectsMainScene(bool isRelease)
    {
        GameObjectFinder.FindEavenIfNotActive(LOADING_POPUP_CANVAS).SetActive(isRelease);
        GameObjectFinder.FindEavenIfNotActive(TEST_OBJECTS).SetActive(!isRelease);
        GameObjectFinder.FindEavenIfNotActive(DAILY_QUEST_BUTTON).SetActive(!isRelease);
        GameObjectFinder.FindEavenIfNotActive(DAILY_QUEST_BUTTON1).SetActive(!isRelease);
        GameObjectFinder.FindEavenIfNotActive(DAILY_QUEST_BUTTON2).SetActive(!isRelease);
    }

    private void SetDefaultGameObjectsMaternityScene(bool isRelease)
    {
        GameObjectFinder.FindEavenIfNotActive(LOADING_POPUP_CANVAS).SetActive(isRelease);
    }


    private void SetDefaultScriptsLoadingScene(bool isRelease)
    {
        Debug.LogError("TODO add new ANALYTICS_CONTROLLER");
        //DeltaDNAController dna = GameObjectFinder.FindEavenIfNotActive(ANALYTICS_CONTROLLER).GetComponent<DeltaDNAController>();
        //ReflectionSetter.setValueInScript(dna, ReflectionSetter.DNA_IS_DEV_BUILD, !isRelease);

        Button confirm = GameObjectFinder.FindEavenIfNotActive(CONFIRM_BUTTON).GetComponent<Button>();
        confirm.onClick.RemoveAllListeners();
        EditorUtility.SetDirty(confirm);

        TenjinController tjController = GameObjectFinder.FindEavenIfNotActive(TENJI_CONTROLLER).GetComponent<TenjinController>();
        ReflectionSetter.setValueInScript(tjController, ReflectionSetter.TENJI_IS_TEST_BUILD, !isRelease);

        PlatformSpecificScriptLoadingScene(isRelease);

        LoadingGame loading = GameObjectFinder.FindEavenIfNotActive(LOADING_GAME).GetComponent<LoadingGame>();
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.CHECKING_GAME_VERSIO, isRelease);
        //ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_DELTADNA_CONFIG, isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_FONTS_FROM_SERVER, !isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_INTRO_FROM_SERVER, !isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_TUTORIAL_FROM_SERVER, !isRelease);
        ReflectionSetter.setValueInScript(loading, ReflectionSetter.LOAD_GAME_BOUNDLES_FROM_SERVER, !isRelease);
    }
}
