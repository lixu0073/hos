using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterNames : ScriptableObject
{
#if UNITY_EDITOR_WIN
    [UnityEditor.MenuItem("Assets/Create/CharacterNames")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<CharacterNames>();
    }
#endif

    [System.Serializable]
    public struct CharacterData
    {
        public string Name;
        public string Surname;
        public int Gender;
        public int Race;
    }

}
