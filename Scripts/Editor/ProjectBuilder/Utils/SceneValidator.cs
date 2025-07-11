using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneValidator
{

    public const string SCENE_MAIN_PATH = "Assets/_MyHospital/Scenes/MainScene.unity";
    public const string SCENE_LOADING_PATH = "Assets/_MyHospital/Scenes/LoadingScene.unity";
    public const string SCENE_DEVELOP_PATH = "Assets/_MyHospital/Scenes/DevelopScene.unity";
    public const string SCENE_MATERNITY_PATH = "Assets/_MyHospital/Scenes/MaternityScene.unity";

    private const string SCENE_MAIN = "MainScene";
    private const string SCENE_LOADING = "LoadingScene";
    private const string SCENE_MATERNITY = "MaternityScene";


    private GameObject current;

    public void ValidateScene(Dictionary<string, Type[]> dict)
    {
        CheckDictionary(dict);
    }

    public void LoadMainScene()
    {
        LoadSceneIfNotActive(SCENE_MAIN_PATH, SCENE_MAIN);
    }

    public void LoadMaternityScene()
    {
        LoadSceneIfNotActive(SCENE_MATERNITY_PATH, SCENE_MATERNITY);
    }

    public void LoadLoadingScene()
    {
        LoadSceneIfNotActive(SCENE_LOADING_PATH, SCENE_LOADING);
    }

    private void LoadSceneIfNotActive(string path, string sceneName)
    {
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }
    }

    private void CheckDictionary(Dictionary<string, Type[]> dict)
    {
        foreach (string key in dict.Keys)
        {
            current = GameObjectFinder.FindEavenIfNotActive(key);
            LogNotFound(key);
            SearchComponents(dict, key);

        }
    }

    private void SearchComponents(Dictionary<string, Type[]> dict, string key)
    {
        foreach (Type componentType in dict[key])
        {
            if (current.GetComponent(componentType) == null)
            {
                Debug.LogError("there is no component: " + componentType.ToString() + " on object: " + key);
            }
        }
    }

    private void LogNotFound(string key)
    {
        if (current == null)
        {
            Debug.LogError("there is no object: " + key);
        }
        else
        {
            Debug.Log("Object " + key + " was found");
        }
    }

}
