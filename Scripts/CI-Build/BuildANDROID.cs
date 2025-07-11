#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

public class BuildANDROID : MonoBehaviour
{
public static void EmptyMethod()
	{
        //do nothing
	}
 [MenuItem("Build/Build Prod Android")]
	public static void ProdBuild()
	{   
		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.scenes = new[] {
        "Assets/_MyHospital/Scenes/LoadingScene.unity",
        "Assets/_MyHospital/Scenes/MainScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMaternityScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMainScene.unity",
	};
    PlayerSettings.SplashScreen.showUnityLogo = false;
#if UNITY_EDITOR_OSX
        InputAndIncrementCurrentBuildVersion();
        OutputCurrentVersion();
#endif
        //App Bundle Settings
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false;

        buildPlayerOptions.locationPathName = Application.dataPath + "/../MyHospital.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        //buildPlayerOptions.options = BuildOptions.Development;// BuildOptions.AllowDebugging | BuildOptions.Development | BuildOptions.ConnectWithProfiler;
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        //Debug.Log(buildPlayerOptions.locationPathName);
    }
     [MenuItem("Build/Build Prod Android No Maternity Bundle")]
	public static void ProdBuildNoMaternityBundle()
	{
		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.scenes = new[] {
        "Assets/_MyHospital/Scenes/LoadingScene.unity",
        "Assets/_MyHospital/Scenes/MainScene.unity",
        "Assets/_MyHospital/Scenes/MaternityScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMaternityScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMainScene.unity",
	};
    PlayerSettings.SplashScreen.showUnityLogo = false;
#if UNITY_EDITOR_OSX
        InputAndIncrementCurrentBuildVersion();
        OutputCurrentVersion();
#endif
        //App Bundle Settings
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false;

        buildPlayerOptions.locationPathName = Application.dataPath + "/../MyHospital.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        //buildPlayerOptions.options = BuildOptions.Development;// BuildOptions.AllowDebugging | BuildOptions.Development | BuildOptions.ConnectWithProfiler;
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        //Debug.Log(buildPlayerOptions.locationPathName);
    }
	[MenuItem("Build/Build Dev Android")]
	public static void DevBuild()
	{
		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.scenes = new[] {
		"Assets/_MyHospital/Scenes/DevelopScene.unity",
        "Assets/_MyHospital/Scenes/LoadingScene.unity",
        "Assets/_MyHospital/Scenes/MainScene.unity",
        "Assets/_MyHospital/Scenes/MaternityScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMaternityScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMainScene.unity",
	};
    PlayerSettings.SplashScreen.showUnityLogo = false;
#if UNITY_EDITOR_OSX
        InputAndIncrementCurrentBuildVersion();
        OutputCurrentVersion();
#endif
        //App Bundle Settings
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false;
        
        buildPlayerOptions.locationPathName = Application.dataPath + "/../MyHospital.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        //buildPlayerOptions.options = BuildOptions.Development;// BuildOptions.AllowDebugging | BuildOptions.Development | BuildOptions.ConnectWithProfiler;
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        //Debug.Log(buildPlayerOptions.locationPathName);
    }

    [MenuItem("Build/Build Profiling Android")]
    public static void ProfilingBuild()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] {
        "Assets/_MyHospital/Scenes/DevelopScene.unity",
        "Assets/_MyHospital/Scenes/LoadingScene.unity",
        "Assets/_MyHospital/Scenes/MainScene.unity",
        "Assets/_MyHospital/Scenes/MaternityScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMaternityScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMainScene.unity",
    };
        PlayerSettings.SplashScreen.showUnityLogo = false;
#if UNITY_EDITOR_OSX
        InputAndIncrementCurrentBuildVersion();
        OutputCurrentVersion();
#endif
        //App Bundle Settings
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false;

        buildPlayerOptions.locationPathName = Application.dataPath + "/../MyHospital.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        //buildPlayerOptions.options = BuildOptions.Development;// BuildOptions.AllowDebugging | BuildOptions.Development | BuildOptions.ConnectWithProfiler;
        buildPlayerOptions.options = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        //Debug.Log(buildPlayerOptions.locationPathName);
    }
    
    [MenuItem("Build/Build Prod Android App Bundle")]
	public static void ProdBuildAppBundle()
	{
		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.scenes = new[] {
        "Assets/_MyHospital/Scenes/LoadingScene.unity",
        "Assets/_MyHospital/Scenes/MainScene.unity",
        "Assets/_MyHospital/Scenes/MaternityScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMaternityScene.unity",
        "Assets/_MyHospital/Scenes/RedirectToMainScene.unity",
	};
    PlayerSettings.SplashScreen.showUnityLogo = false;
#if UNITY_EDITOR_OSX
        InputAndIncrementCurrentBuildVersion();
        OutputCurrentVersion();
#endif
        //App Bundle Settings
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = true;

        buildPlayerOptions.locationPathName = Application.dataPath + "/../MyHospital.aab";
        buildPlayerOptions.target = BuildTarget.Android;
        //buildPlayerOptions.options = BuildOptions.Development;// BuildOptions.AllowDebugging | BuildOptions.Development | BuildOptions.ConnectWithProfiler;
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        //Debug.Log(buildPlayerOptions.locationPathName);
    }

    [MenuItem("Build/Output Current Version")]
    public static void OutputCurrentVersion()
    {
        File.AppendAllText("ver.txt",
                   "Application Version:" + Application.version + Environment.NewLine);
#if UNITY_ANDROID
        File.AppendAllText("buildver.txt",
                   "Android Build Version:" + PlayerSettings.Android.bundleVersionCode + Environment.NewLine);
#else
        File.AppendAllText("buildver.txt",
                   "Build Number Not Found" + Environment.NewLine);
#endif
    }

    [MenuItem("Build/Input And Increment Current Build Version")]
    public static void InputAndIncrementCurrentBuildVersion()
    {
        //string line1 = File.ReadLines("newbuildver.txt").First();//newer c#
        try
        {
            string line1 = File.ReadAllLines("buildver.txt").First();
            UnityEngine.Debug.Log("buildver.txt file found for incrementing build number.");
            int ver = 0;
            line1 = Regex.Replace(line1, "[^0-9]+", string.Empty);
            Int32.TryParse(line1, out ver);
            UnityEngine.Debug.Log("Current (old)build version read from file " + ver.ToString());
            ver++;
            PlayerSettings.Android.bundleVersionCode = ver;

        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Setting build number failed in BuildAndroid: " + e.Message);
            UnityEngine.Debug.Log("File with version not found or something went wrong, incrementing what's currently there.");
            UnityEngine.Debug.Log("Current (old)build version is: " + PlayerSettings.Android.bundleVersionCode);
            int newBNumber = PlayerSettings.Android.bundleVersionCode;
            newBNumber++;
            PlayerSettings.Android.bundleVersionCode = newBNumber;
            //UnityEngine.Debug.Log("Current (new)build version is: " + PlayerSettings.iOS.buildNumber);
        }
        finally
        {
            UnityEngine.Debug.Log("Current build version is: " + PlayerSettings.Android.bundleVersionCode);
        }

    }
}

#endif