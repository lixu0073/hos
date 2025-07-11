using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class ReflectionSetter {

    public const string DNA_IS_DEV_BUILD = "isDevBuild";
    public const string COGNITO_IDENTITY_POOL = "IdentityPoolId";
    public const string TENJI_IS_TEST_BUILD = "isTestBuild";

    public const string CHECKING_GAME_VERSIO = "checkingGameVersion";
    public const string LOAD_DELTADNA_CONFIG = "loadDeltaDNAConfigs";
    public const string LOAD_FONTS_FROM_SERVER = "loadFontsFromServer";
    public const string LOAD_INTRO_FROM_SERVER = "loadIntroFromServer";
    public const string LOAD_TUTORIAL_FROM_SERVER = "loadTutorialFromServer";
    public const string LOAD_GAME_BOUNDLES_FROM_SERVER = "loadGameBundlesFromServer";
    public const string LOAD_SCENE_FROM_SERVER = "loadSceneFromServer";

    public const string BOUNDLE_SCENE_ASSET_NAME = "sceneAssetBundleName";
    public const string BOUNDLE_FONT_ASSET_NAME = "fontAssetBundleName";
    public const string BOUNDLE_INTRO_ASSET_NAME = "introAssetBundleName";
    public const string BOUNDLE_TUTORIAL_ASSET_NAME = "tutorialAssetBundleName";

    public static void setValueInScript(MonoBehaviour script, string fieldName, bool value)
    {
        FieldInfo prop = script.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        prop.SetValue(script, value);
        EditorUtility.SetDirty(script);
        Debug.Log("Set " + fieldName + " of script " + script + " to " + value);
    }

    public static void setValueInScript(MonoBehaviour script, string fieldName, string value)
    {
        FieldInfo prop = script.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        prop.SetValue(script, value);
        EditorUtility.SetDirty(script);
        Debug.Log("Set " + fieldName + " of script " + script + " to " + value);
    }

    public static void SetActive(GameObject gameobject, bool active)
    {
        gameobject.SetActive(active);
        EditorUtility.SetDirty(gameobject);
        Debug.Log("Set " + gameobject.name + "activity to " + active);
    }
}
