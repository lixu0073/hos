using UnityEngine;
using System.Collections.Generic;
using System;
using IsoEngine;
using SimpleUI;
using TMPro;
using System.Text;
using Hospital;

public abstract class CustomizableItem{

    internal CutomizableItemType customizableItemType;

    // Use this for initialization
    internal CustomizableItem() {
        customizableItemType = CutomizableItemType.Default;
    }

    public virtual string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(this.GetType());
        return builder.ToString();
    }

    public virtual void LoadFromString(string saveString)
    {
        // ..
    }

    public virtual bool UpdateWith(CustomizableItem item)
    {
        return false;
    }

    public enum CutomizableItemType
    {
        Default,
        Floor,
        Sign,
        Flag
    }

    public virtual void RefreshCustomization(bool fromSave = false)
    {
        // ..
    }
}
