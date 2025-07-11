using UnityEngine;
using System.Collections.Generic;
using System;
using IsoEngine;
using SimpleUI;
using TMPro;
using MovementEffects;
using System.Text;
using Hospital;
using System.Globalization;

public class CustomizableFloor : CustomizableItem
{
    public Vector4 color
    {
        get;
        private set;
    }

    internal HospitalArea floorArea;
    Vector3 paintPos;

    // Use this for initialization
    public CustomizableFloor()
    {
        customizableItemType = CutomizableItemType.Floor;
    }

    public CustomizableFloor(HospitalArea floorArea, Vector3 paintPos, Vector4 color)
    {
        customizableItemType = CutomizableItemType.Floor;
        this.floorArea = floorArea;
        this.color = color;
        this.paintPos = paintPos;
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("!");
        builder.Append(floorArea.ToString());
        builder.Append("!");
        builder.Append(Vector4ToString(this.color));
        builder.Append("!");
        builder.Append(ReferenceHolder.Get().floorControllable.GetCurrentFloorColorName(floorArea));
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        if (!string.IsNullOrEmpty(saveString))
        {
            var save = saveString.Split('!');
            this.floorArea = (HospitalArea)Enum.Parse(typeof(HospitalArea), save[1]);
            this.color = ParseVector4(save[2]);
            ReferenceHolder.Get().floorControllable.SetCurrentFloorColor(save[3], this.floorArea);
            RefreshCustomization(true);
        }
    }

    public Vector4 ParseVector4(string str)
    {
        var tmp = str.Split(',');
        return new Vector4(float.Parse(tmp[0], CultureInfo.InvariantCulture), float.Parse(tmp[1], CultureInfo.InvariantCulture), float.Parse(tmp[2], CultureInfo.InvariantCulture), float.Parse(tmp[3], CultureInfo.InvariantCulture));
    }

    public string Vector4ToString(Vector4 vec)
    {
        return vec.x + "," + vec.y + "," + vec.z + "," + vec.w;
    }

    public override bool UpdateWith(CustomizableItem item)
    {
        var tmp = (item as CustomizableFloor);

        if (tmp != null && this.floorArea == tmp.floorArea)
        {
            this.color = tmp.color;
            this.paintPos = tmp.paintPos;
            return true;
        }

        return false;
    }


    public CustomizableFloor GetCustomizationItem(HospitalArea floorArea)
    {
        if (floorArea == this.floorArea)
            return this;

        return null;
    }

    public override void RefreshCustomization(bool fromSave = false)
    {
        ReferenceHolder.Get().floorControllable.RefreshFloorColor(floorArea, color, paintPos, fromSave);
    }
}
