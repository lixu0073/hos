using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

public class CustomizableSign : CustomizableItem
{
    public Sprite currentSprite;
    private Sprite customSprite;

    public CustomizableSign()
    {
        customizableItemType = CutomizableItemType.Sign;

    }

    public CustomizableSign(Sprite sprite)
    {
        customizableItemType = CutomizableItemType.Sign;
        currentSprite = sprite;
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("!");
        builder.Append(ReferenceHolder.GetHospital().signControllable.GetCurrentSignName());
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split('!');
            ReferenceHolder.GetHospital().signControllable.SetCurrentSignName(save[1]);
            RetrieveTextureFromAssetBundle();
        }
    }

    public override bool UpdateWith(CustomizableItem item)
    {
        var tmp = (item as CustomizableSign);

        if (tmp != null)
        {
            return true;
        }

        return false;
    }

    public override void RefreshCustomization(bool fromSave = false)
    {
        RetrieveTextureFromAssetBundle();
    }

    public void DeactivateCallbacks()
    {

    }

    void RetrieveTextureFromAssetBundle()
    {
        if (string.Compare("Lvl1", ReferenceHolder.GetHospital().signControllable.GetCurrentSignName()) == 0)
        {
            OnRetrieveSpriteSucces(ResourcesHolder.GetHospital().defaultSign);
        }
        else
        {
            GameAssetBundleManager.instance.hospitalSign.GetSprite(ReferenceHolder.GetHospital().signControllable.GetCurrentSignName(), OnRetrieveSpriteSucces, OnRetrieveSpriteFailure);
        }
    }


    void OnRetrieveSpriteSucces(Sprite sprite)
    {
        currentSprite = sprite;
        ReferenceHolder.GetHospital().signControllable.UpdateSignSprite(currentSprite);
        string currentSignName = ReferenceHolder.GetHospital().signControllable.GetCurrentSignName();
        string previousSignName = ReferenceHolder.GetHospital().signControllable.GetPreviousSignName();
        if (currentSignName != null && previousSignName != null && String.Compare(previousSignName, currentSignName) != 0)
        {
            ReferenceHolder.GetHospital().signControllable.UnloadSignAssetFromBundle(previousSignName);
        }
    }

    void OnRetrieveSpriteFailure(Exception exception)
    {

    }
}
