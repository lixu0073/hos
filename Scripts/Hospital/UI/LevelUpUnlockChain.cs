using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Hospital;

public class LevelUpUnlockChain : MonoBehaviour {

    public Image[] icons;
    public TextMeshProUGUI[] names;
    public GameObject[] newBadges;
    public GameObject[] items;
    public GameObject[] arrows;
    public PointerDownListener[] pointerDownListeners;
    

    public void SetChain(MedicineDatabaseEntry medicine)
    {
        SetItem(0, medicine.producedIn.ShopImage, I2.Loc.ScriptLocalization.Get(medicine.producedIn.ShopTitle), IsMachineNew(medicine));
        SetListener(0, I2.Loc.ScriptLocalization.Get(medicine.producedIn.ShopTitle), I2.Loc.ScriptLocalization.Get(medicine.producedIn.ShopDescription));

        SetItem(1, medicine.image, I2.Loc.ScriptLocalization.Get(medicine.Name), IsMedicineNew(medicine));
        SetSize(1, 15, 15, 15, 15);
        SetListener(1, medicine);

        if (medicine.Disease != null)       //for medicines that heal patietnts in treatment rooms
        {
            SetItem(2, medicine.Disease.DiseasePicSmall, I2.Loc.ScriptLocalization.Get(medicine.Disease.Name), true);
            SetSize(2, 20, 20, 10, 30);
            SetListener(2, medicine.Disease, medicine);
        }
        else              //for elixirs that are used by doctors
        {
            SetItem(2, medicine.doctorRoom.ShopImage, I2.Loc.ScriptLocalization.Get(medicine.doctorRoom.ShopTitle), true);
            SetListener(2, I2.Loc.ScriptLocalization.Get(medicine.doctorRoom.ShopTitle), I2.Loc.ScriptLocalization.Get(medicine.doctorRoom.ShopDescription));
        }
    }

    public void InitiailizeMaternityChain(ShopRoomInfo info)
    {
        SetChainItemMaternity(0, info);
        SetChainItemMaternity(1, (ShopRoomInfo)info.depeningRoom);
        items[2].SetActive(false);
        arrows[1].SetActive(false);
    }



    public void SetPanaceaChainTooltips() {
        SetListener(0, I2.Loc.ScriptLocalization.Get("PANACEA_COLLECTOR_TITLE"), I2.Loc.ScriptLocalization.Get("PANACEA_COLLECTOR_INFO_SHORT_TEXT"));
        SetListener(1, I2.Loc.ScriptLocalization.Get("SHOP_TITELS/TEST_TUBE_TABLE"), I2.Loc.ScriptLocalization.Get("SHOP_DESCRIPTIONS/TEST_TUBE_TABLE"));
        SetListener(2, ResourcesHolder.Get().medicines.cures[0].medicines[1]);
    }

    private void SetChainItemMaternity(int index, ShopRoomInfo info)
    {
        SetItem(index, info.ShopImage, I2.Loc.ScriptLocalization.Get(info.ShopTitle), true);
        SetListener(index, I2.Loc.ScriptLocalization.Get(info.ShopTitle), I2.Loc.ScriptLocalization.Get(info.ShopDescription));
    }

    void SetItem(int index, Sprite sprite, string name, bool isNew)
    {
        icons[index].sprite = sprite;
        names[index].text = name;

        if (isNew)
        {
            newBadges[index].SetActive(true);
        }
        else
        {
            newBadges[index].SetActive(false);
        }
    }

    void SetSize(int index, int leftMargin, int rightMargin, int topMargin, int botMargin)
    {
        RectTransform rt = icons[index].GetComponent<RectTransform>();

        rt.offsetMin = new Vector2(leftMargin, botMargin);
        rt.offsetMax = new Vector2(-rightMargin, -topMargin);
        /*
        LayoutElement le = items[index].GetComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = height;
        le.preferredHeight = height;
        */
    }

    void SetListener(int index, string title, string description)
    {
        pointerDownListeners[index].SetDelegate(() =>
        {
            TextTooltip.Open(title, description);
        });
    }

    void SetListener(int index, MedicineDatabaseEntry med)
    {
        pointerDownListeners[index].SetDelegate(() =>
        {
            TextTooltip.Open(med.GetMedicineRef(), false, true);
        });
    }

    void SetListener(int index, DiseaseDatabaseEntry disease, MedicineDatabaseEntry med)
    {
        pointerDownListeners[index].SetDelegate(() =>
        {
            TextTooltip.Open(disease, med.GetMedicineRef());
        });
    }

    bool IsMachineNew(MedicineDatabaseEntry medicine)
    {
        if (medicine.producedIn.unlockLVL >= Game.Instance.gameState().GetHospitalLevel())
            return true;
        else
            return false;
    }

    bool IsMedicineNew(MedicineDatabaseEntry medicine)
    {
        if (medicine.minimumLevel >= Game.Instance.gameState().GetHospitalLevel())
            return true;
        else
            return false;
    }
}
