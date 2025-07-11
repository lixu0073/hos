using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameObjectFinder {

    /// <summary>
    ///  it is SLOW only use in editor
    /// </summary>
    /// <param name="gameobject"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject FindEavenIfNotActive(string name)
    {
        GameObject current = GameObject.Find(name);
        if (current)
        {
            return current;
        }

        current = SearchInActiveParentObjects(name);
        if (current)
        {
            return current;
        }

        current = SearchHierarchyRoot(name);
        return current;
    }

    private static GameObject SearchInActiveParentObjects(string key)
    {
        if (key.Contains("/"))
        {
            string[] splitKey = key.Split('/');
            GameObject parent = GameObject.Find(splitKey[0]);
            return FindInActiveParent(parent, splitKey.Last());

        }
        return null;
    }

    private static GameObject SearchHierarchyRoot(string key)
    {
        GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject current = Array.Find(objects, gObj => gObj.name == key);
        return current;
    }

    private static GameObject FindInActiveParent(GameObject parent, string name)
    {
        Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }
        return null;
    }
}

