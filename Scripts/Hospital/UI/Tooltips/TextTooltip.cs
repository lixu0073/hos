using UnityEngine;
using Hospital;
using TMPro;

public class TextTooltip : SimpleUI.Tooltip
{
    public static TextTooltip Instance { get; private set; }

    public TextMeshProUGUI medicineName;
    public TextMeshProUGUI medicineDescription;
    public TextMeshProUGUI medicineProductionTime;
    RectTransform tr;

    private void Initialize(MedicineRef medicine, bool cures = false, bool producedIn = true, bool showProductionTime = true)
    {
        base.Initialize();
        var rh = ResourcesHolder.Get();
        gameObject.SetActive(true);
        medicineName.text = rh.GetNameForCure(medicine);

        if (medicine.type != MedicineType.Special && medicine.type != MedicineType.Fake)
        {
            if (producedIn)
            {
                medicineDescription.gameObject.SetActive(true);
                if (medicine.type == MedicineType.BasePlant)
                    medicineDescription.text = I2.Loc.ScriptLocalization.Get("TOOLTIP_GROW_ON_PLANTATION");
                else
                    medicineDescription.text = I2.Loc.ScriptLocalization.Get("TOOLTIP_PRODUCED_IN") + " " + I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().medicines.cures[(int)medicine.type].ProducedBy.ShopTitle);
            }

            if (cures)
            {
                medicineDescription.text = "";
                //tr.sizeDelta = new Vector2(tr.sizeDelta.x, tr.sizeDelta.y + 20);
                var p = rh.GetMedicineInfos(medicine);
                if (p.Disease == null)
                    medicineDescription.text = I2.Loc.ScriptLocalization.Get("TOOLTIP_PRODUCED_IN") + " " + I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().medicines.cures[(int)medicine.type].ProducedBy.ShopTitle);
                else
                {
                    medicineDescription.gameObject.SetActive(true);
                    medicineDescription.text += I2.Loc.ScriptLocalization.Get("TOOLTIP_CURES") + " " + I2.Loc.ScriptLocalization.Get(p.Disease.Name);
                }
            }

            medicineProductionTime.text = UIController.GetFormattedTime((int)rh.GetMedicineInfos(medicine).ProductionTime);
        }
        else if (medicine.type == MedicineType.Fake && medicine.id == 0)
        {
            medicineDescription.gameObject.SetActive(true);
            medicineDescription.text = I2.Loc.ScriptLocalization.Get("CHILDREN_REWARD");
            showProductionTime = false;
        }
        else if (medicine.type == MedicineType.Special)
        {
            medicineDescription.gameObject.SetActive(true);
            if (medicine.id == 3)    //shovel
                medicineDescription.text = I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SHOVEL_TOOLTIP");
            else if (medicine.id < 3)
                medicineDescription.text = I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SPECIAL_STORAGE_TOOLTIP");
            else
                medicineDescription.text = I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/SPECIAL_TANK_TOOLTIP");

            showProductionTime = false;
        }
        else
        {
            Debug.LogError("Nieobsluzony case tooltipa");
            showProductionTime = false;
        }

        if (showProductionTime)
        {
            medicineProductionTime.gameObject.SetActive(true);
        }
        else
        {
            medicineProductionTime.gameObject.SetActive(false);
        }

        if (medicineProductionTime.gameObject.activeSelf)
            tr.sizeDelta = new Vector2(150, 125);
        else
            tr.sizeDelta = new Vector2(150, 80);


        tr.pivot = Vector2.zero;


        transform.SetAsLastSibling();
        SetPosition();
    }

    private void Initialize(DiseaseDatabaseEntry disease, MedicineRef medicine)
    {
        base.Initialize();
        var rh = ResourcesHolder.Get();
        gameObject.SetActive(true);
        medicineName.text = I2.Loc.ScriptLocalization.Get(disease.Name);

        medicineDescription.gameObject.SetActive(true);
        medicineDescription.text = I2.Loc.ScriptLocalization.Get("TOOLTIP_CURED_WITH") + " " + I2.Loc.ScriptLocalization.Get(rh.GetMedicineInfos(medicine).Name);
        medicineProductionTime.gameObject.SetActive(false);

        tr.sizeDelta = new Vector2(150, 80);
        tr.pivot = Vector2.zero;
        transform.SetAsLastSibling();
        SetPosition();
    }

    private void Initialize(string title, string desc)
    {
        base.Initialize();
        var rh = ResourcesHolder.Get();
        gameObject.SetActive(true);
        medicineName.text = title;
        if (string.IsNullOrEmpty(desc))
        {
            medicineDescription.gameObject.SetActive(false);
            tr.sizeDelta = new Vector2(150, 60);
            tr.pivot = new Vector2(0, -0.75f);
        }
        else
        {
            medicineDescription.gameObject.SetActive(true);
            medicineDescription.text = desc;
            tr.sizeDelta = new Vector2(150, 80);
            tr.pivot = Vector2.zero;
        }
        medicineProductionTime.gameObject.SetActive(false);


        transform.SetAsLastSibling();
        SetPosition();
    }

    private void Initialize(string singleText, bool shifted)
    {
        base.Initialize();
        var rh = ResourcesHolder.Get();
        gameObject.SetActive(true);
        medicineName.text = "";

        medicineDescription.gameObject.SetActive(true);
        medicineDescription.text = singleText;
        medicineProductionTime.gameObject.SetActive(false);

        tr.sizeDelta = new Vector2(100, 50);
        if (shifted)
        {
            tr.pivot = new Vector2(-0.5f, -0.75f);
        }
        else
        {
            tr.pivot = new Vector2(0, -0.75f);
        }
        transform.SetAsLastSibling();
        SetPosition();
    }

    public static TextTooltip Open(MedicineRef medicine, bool cures = false, bool producedIn = true, bool showProductionTime = true)
    {
        if (Instance == null)
            Instantiate();
        Instance.Initialize(medicine, cures, producedIn, showProductionTime);

        return Instance;
    }

    public static TextTooltip Open(DiseaseDatabaseEntry disease, MedicineRef medicine)
    {
        if (Instance == null)
            Instantiate();
        Instance.Initialize(disease, medicine);

        return Instance;
    }

    public static TextTooltip Open(string title, string desc)
    {
        if (Instance == null)
            Instantiate();
        Instance.Initialize(title, desc);

        return Instance;
    }

    public static TextTooltip Open(string singleText, bool shifted = true)
    {
        if (Instance == null)
            Instantiate();
        Instance.Initialize(singleText, shifted);

        return Instance;
    }

    public static TextTooltip OpenSingleText(string singleText)
    {
        if (Instance == null)
            Instantiate();
        Instance.Initialize(singleText, "");

        return Instance;
    }

    private static void Instantiate()
    {
        Instance = GameObject.Instantiate(ResourcesHolder.Get().TextTooltipPrefab).GetComponent<TextTooltip>();
        Instance.gameObject.transform.SetParent(UIController.get.canvas.transform);
        Instance.transform.localScale = Vector3.one;
        Instance.tr = Instance.GetComponent<RectTransform>();
    }
}
