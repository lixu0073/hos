using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

public class CustomizableFlag : CustomizableItem {
    public Sprite currentSprite;
    private Sprite customSprite;

    public CustomizableFlag()
    {
        customizableItemType = CutomizableItemType.Flag;

    }

    public CustomizableFlag(Sprite sprite) {
        customizableItemType = CutomizableItemType.Flag;
        currentSprite = sprite;
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("!");
        builder.Append(ReferenceHolder.GetHospital().flagControllable.GetCurrentFlagName());
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split('!');
            ReferenceHolder.GetHospital().flagControllable.SetCurrentFlagName(save[1]);
            RetrieveTextureFromAssetBundle();
        }
    }

    public override bool UpdateWith(CustomizableItem item)
    {
        var tmp = (item as CustomizableFlag);

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
        if(String.Compare(ReferenceHolder.GetHospital().flagControllable.GetCurrentFlagName(), "noflag") == 0) {
            currentSprite = null;
            ReferenceHolder.GetHospital().flagControllable.SetFlagActive(false);
        } else {
            GameAssetBundleManager.instance.hospitalFlag.GetSprite(ReferenceHolder.GetHospital().flagControllable.GetCurrentFlagName(), OnRetrieveSpriteSucces, OnRetrieveSpriteFailure);
        }
    }

    void OnRetrieveSpriteSucces(Sprite sprite)
    {
        currentSprite = sprite;
        ReferenceHolder.GetHospital().flagControllable.SetFlagActive(true);
        ReferenceHolder.GetHospital().flagControllable.UpdateFlagSprite(currentSprite);
    }

    void OnRetrieveSpriteFailure(Exception exception)
    {

    }
}
