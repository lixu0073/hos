using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderFactory {

    public static IPlatformBuilder GetBuilder()
    {
        IPlatformBuilder targetPlatform = null;
#if UNITY_STANDALONE_OSX
        targetPlatform = new OsxBuilder();
#elif UNITY_IOS
        targetPlatform = new IOSBuilder();
#elif UNITY_ANDROID
        targetPlatform = new AndroidBuilder();
#endif
        return targetPlatform;

    }
}
