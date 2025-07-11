using Hospital;
using UnityEngine;
using UnityEngine.UI;

public class VitaminModule : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    Image VitaminDeficiency;
    [SerializeField]
    GameObject CheckIcon;
#pragma warning restore 0649
    HospitalCharacterInfo patient;
    PointerDownListener listener;

    public void SetInfos(HospitalCharacterInfo patient)
    {
        VitaminDeficiency.gameObject.SetActive(false);
        CheckIcon.SetActive(true);
        this.patient = patient;
        listener = VitaminDeficiency.gameObject.GetComponent<PointerDownListener>();
    }

    public void SetupVitamin()
    {
        if (patient.HasVitamin())
        {
            for (int i = 0; i < patient.requiredMedicines.Length; ++i)
            {
                if (patient.requiredMedicines[i].Key.GetMedicineRef().type == MedicineType.Vitamins)
                {
                    VitaminDeficiency.sprite = patient.requiredMedicines[i].Key.Disease.DiseasePic;
                    listener.ClearDelegate();
                    listener.SetDelegate(() =>
                    {
                        TextTooltip.Open(I2.Loc.ScriptLocalization.Get(patient.requiredMedicines[i].Key.Disease.Name), I2.Loc.ScriptLocalization.Get("TOOLTIP_CURED_WITH") + " " + I2.Loc.ScriptLocalization.Get(patient.requiredMedicines[i].Key.Name));
                    });
                    break;
                }
            }
            CheckIcon.SetActive(false);
            VitaminDeficiency.gameObject.SetActive(true);

        }
    }

    public void InfoButtonDown()
    {
        if (!patient.HasVitamin())
        {
            MessageController.instance.ShowMessage(66);
        }
    }

    public void HideVitaminDeficiency()
    {
        VitaminDeficiency.gameObject.SetActive(false);
    }
}
