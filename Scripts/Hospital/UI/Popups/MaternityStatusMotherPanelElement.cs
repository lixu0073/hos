using TMPro;
using UnityEngine;

public class MaternityStatusMotherPanelElement : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private TextMeshProUGUI amountText;
    [SerializeField]
    private GameObject maleBabyPresent;
    [SerializeField]
    private GameObject femaleBabyPresent;
    [SerializeField]
    private Maternity.PatientStates.MaternityPatientStateTag StatusType;
#pragma warning restore 0649

    public void SetAmountText(int amount)
    {
        amountText.text = amount.ToString();
    }

    public Maternity.PatientStates.MaternityPatientStateTag GetStatusType()
    {
        return StatusType;
    }

    public void SetActive(bool setActive)
    {
        gameObject.SetActive(setActive);
    }

    public void SetPresent(GenderTypes gender)
    {
        maleBabyPresent.SetActive(false);
        femaleBabyPresent.SetActive(false);
        if (gender == GenderTypes.Man)
        {
            maleBabyPresent.SetActive(true);
        }
        else
        {
            femaleBabyPresent.SetActive(true);
        }
    }
}
