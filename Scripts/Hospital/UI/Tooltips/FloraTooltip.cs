using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class FloraTooltip : SimpleUI.Tooltip
{
	[SerializeField]
	TextMeshProUGUI medicineName = null;
	[SerializeField]
    TextMeshProUGUI medicineDescription = null;

    private const string collectFromTerm = "COLLECT_FROM";
    private const string plantationTerm = "PLANTATION";

    private void Initialize(MedicineRef medicine)
	{
		base.Initialize();
		var rh = ResourcesHolder.Get();
		gameObject.SetActive(true);
		medicineName.text = rh.GetNameForCure(medicine);
        medicineDescription.text = string.Format(I2.Loc.ScriptLocalization.Get(collectFromTerm), GetProperCaseString(I2.Loc.ScriptLocalization.Get(plantationTerm)));
        transform.SetAsLastSibling();
		SetPosition();
	}

    private string GetProperCaseString(string wrongCaseString)
    {
        string properCaseString = string.Empty;
        string[] words = wrongCaseString.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            properCaseString += (char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower());
            if (i < (words.Length - 1)) properCaseString += " ";
        }
        return properCaseString;
    }

	public static FloraTooltip Instance
	{ get; private set; }
	public static FloraTooltip Open(MedicineRef medicine)
	{

		if (Instance == null)
			Instantiate();
		Instance.Initialize(medicine);

		return Instance;

	}
	private static void Instantiate()
	{
		Instance = GameObject.Instantiate(ResourcesHolder.GetHospital().FloraTooltipPrefab).GetComponent<FloraTooltip>();
		Instance.gameObject.transform.SetParent(UIController.get.canvas.transform);
		Instance.transform.localScale = Vector3.one;
	}
}
