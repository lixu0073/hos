using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundleSpritesReferences : ScriptableObject {

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/BundleSpritesReferences")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<BundleSpritesReferences>();
    }
#endif
    public List<SingleBoxSpriteData> boxesData;


    public Sprite GetIconForBox(SingleBoxSpriteData.BundledResourceSprite boxType)
    {
        foreach (SingleBoxSpriteData box in boxesData)
        {
            if (box.bundleReourceType == boxType)
            {
                return box.GeneralIcon;
            }
        }
        return null;
    }

    public Sprite GetTopBoxIconForBox(SingleBoxSpriteData.BundledResourceSprite boxType)
    {
        foreach (SingleBoxSpriteData box in boxesData)
        {
            if (box.bundleReourceType == boxType)
            {
                return box.TopCover;
            }
        }
        return null;
    }

    public Sprite GetBottomBoxIconForBox(SingleBoxSpriteData.BundledResourceSprite boxType)
    {
        foreach (SingleBoxSpriteData box in boxesData)
        {
            if (box.bundleReourceType == boxType)
            {
                return box.BottomBox;
            }
        }
        return null;
    }
}
