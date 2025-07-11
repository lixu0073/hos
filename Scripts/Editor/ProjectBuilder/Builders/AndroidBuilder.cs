using Facebook.Unity.Settings;
using UnityEditor;

public class AndroidBuilder : IPlatformBuilder
{
    protected override void AddPlatformSpecificToDictionaries()
    {
    }

    protected override void PlatfromSpecificResources()
    {
        FacebookSettings.SelectedAppIndex = 1;
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[0];
    }

    protected override void PlatformSpecificScriptLoadingScene(bool isRelease)
    {
    }

    protected override void BuildSpecyfics(BuildPlayerOptions options)
    {
        options.options = BuildOptions.AcceptExternalModificationsToPlayer;
    }
}
