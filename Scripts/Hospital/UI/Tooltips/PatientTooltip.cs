using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Hospital;
using TMPro;

public class PatientTooltip : SimpleUI.Tooltip
{
	[SerializeField]
	TextMeshProUGUI NameText = null;
    [SerializeField]
    TextMeshProUGUI SexText = null;
    [SerializeField]
    TextMeshProUGUI AgeText = null;
    [SerializeField]
    TextMeshProUGUI BloodText = null;


    public static PatientTooltip Instance
    {
        get; private set;
    }

    public static PatientTooltip Open(DoctorRoom room, BaseCharacterInfo patientInfo, int type = 0)
    {

        if (Instance == null)
            Instantiate();
        Instance.Initialize(room, patientInfo, type);

        return Instance;
    }
    
    private static void Instantiate()
    {
        Instance = GameObject.Instantiate(ResourcesHolder.Get().PatientTooltipPrefab).GetComponent<PatientTooltip>();
        Instance.gameObject.transform.SetParent(UIController.get.canvas.transform);
        Instance.transform.localScale = Vector3.one;
    }

    private void Initialize(DoctorRoom room, BaseCharacterInfo patientInfo, int type)
	{
		base.Initialize();
		var rh = ResourcesHolder.Get();
        if (!patientInfo.Name.Contains("_"))
        {
            NameText.text = patientInfo.Name + " " + patientInfo.Surname;
        } else {
            NameText.text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + patientInfo.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + patientInfo.Surname);
        }

        CharacterCreator characterCreator = ReferenceHolder.GetHospital().PersonCreator;
        SexText.text = CharacterCreator.GetSexString(patientInfo.Sex);
        AgeText.text = patientInfo.Age.ToString();
        BloodText.text = CharacterCreator.GetBloodTypeString(patientInfo.BloodType);

        gameObject.SetActive(true);
		transform.SetAsLastSibling();
		SetPosition();
	}

	public override void Close()
	{
		base.Close();
	}
}
