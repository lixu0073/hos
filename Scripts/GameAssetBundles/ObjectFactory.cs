using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectFactory
{

    public static BaseGameAssetBundle GetGameAssetBundelObject(string className)
    {
        Type type = Type.GetType(className);
        if (type == null)
        {
            return null;
        }
        System.Object obj = Activator.CreateInstance(type);
        if (obj == null)
        {
            return null;
        }
        return (BaseGameAssetBundle)obj;
    }

    public static ILocalNotificationController GetLocalNotificationController()
    {
        string controllerName = GetClassNameOnScene("LocalNotificationController");
        ILocalNotificationController controller = GetObject<ILocalNotificationController>(controllerName);
        if (controller == null)
        {
            Debug.LogError("Class " + controllerName + " not exist!");
        }
        return controller;
    }

    public static IPublicSaveManager GetPublicSaveManager()
    {
        string managerName = GetClassNameOnScene("PublicSaveManager");
        IPublicSaveManager manager = GetObject<IPublicSaveManager>(managerName);
        if(manager == null)
        {
            Debug.LogError("Class " + managerName + " not exist!");
        }
        return manager;
    }

    public static IMainUIAdapter GetMainUIAdapter()
    {
        string managerName = GetClassNameOnScene("MainUIAdapter");
        IMainUIAdapter manager = GetObject<IMainUIAdapter>(managerName);
        if (manager == null)
        {
            Debug.LogError("Class " + managerName + " not exist!");
        }
        return manager;
    }

    private static string GetClassNameOnScene(string className)
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.name + "_" + className;
    }

    private static T GetObject<T>(string className)
    {

        Type type = Type.GetType(className);
        if (type == null)
        {
            return default(T);
        }
        System.Object obj = Activator.CreateInstance(type);
        if (obj == null)
        {
            return default(T);
        }
        return (T)obj;
    }

}
