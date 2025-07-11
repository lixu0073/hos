using UnityEngine;
using System.Collections;

[System.Serializable]
public class MedicineBadgeHintInfo
{
    public MedicineType type;
    public int id;
    public int count;
    public ShopRoomInfo producedIn;

    public MedicineBadgeHintInfo(int id = -1, int count = 0, MedicineType type = MedicineType.BaseElixir, ShopRoomInfo producedIn = null)
    {
        this.id = id;
        this.count = count;
        this.type = type;
        this.producedIn = producedIn;
    }

    public MedicineRef GetMedicineRef()
    {
        return new MedicineRef(type, id);
    }
}
