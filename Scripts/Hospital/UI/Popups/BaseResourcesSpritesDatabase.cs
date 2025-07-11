using System.Collections.Generic;
using UnityEngine;

public class BaseResourcesSpritesDatabase : ScriptableObject
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/BaseResourcesSpritesDatabase")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<BaseResourcesSpritesDatabase>();
    }
#endif
#pragma warning disable 0649
    [SerializeField]
    List<BaseResourceSpriteData> database;
#pragma warning restore 0649

    public Sprite GetSprite(BaseResourceSpriteData.SpriteType spriteType)
    {
        foreach (var item in database)
        {
            if (item.spriteType == spriteType)
            {
                return item.sprite;
            }
        }
        return null;
    }
}
