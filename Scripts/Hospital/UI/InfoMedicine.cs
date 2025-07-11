using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Hospital;

public class InfoMedicine : MonoBehaviour {

    public Image img;
    public PointerDownListener pdl;
    

    public void SetSprite(Sprite sprite)
    {
        img.sprite = sprite;
    }

    public void SetMaterial(Material material)
    {
        img.material = material;
    }

    public void SetColor(Color color)
    {
        img.color = color;
    }

    public void SetUpListener(MedicineDatabaseEntry medicine, bool isUnlocked)
    {
        pdl.SetDelegate(() =>
        {
            if (isUnlocked)
                TextTooltip.Open(medicine.GetMedicineRef(), true, false);
            else
                MedicineLockedTooltip.Open(medicine.GetMedicineRef());
        });
    }
    public void ClearListener() {
        pdl.ClearDelegate();
    }
}
