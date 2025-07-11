using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Hospital;

public class EpidemyBox : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject boxOpen;
    [SerializeField] private GameObject boxClosed;
    [SerializeField] private GameObject boxDisabled;
    [SerializeField] private GameObject boxSelection;
    [SerializeField] private Color colorAlpha;

    public Animator anim;

    [Header("Open box parameters")]
    [SerializeField] private Image openBoxImage;
    [SerializeField] private Image medicineIcon;
    [SerializeField] private TextMeshProUGUI medicineQuantity;
    [SerializeField] private GameObject helpBadge;

    [Header("Closed box parameters")]
    [SerializeField] private Image closedBoxImage;
    [SerializeField] private GameObject helpingPersonInfo;
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI hospitalName;
#pragma warning restore 0649

    public void SetHelperInfoInactive()
    {
        helpingPersonInfo.SetActive(false);
    }

    public void SetBounceAnimation()
    {
        helpingPersonInfo.GetComponent<Animator>().SetTrigger("Bounce");
    }

    public void SetClosedBoxWithHelpLook(Sprite avatar, string helperLevel, string helperHospitalName, bool hasConfirmHelper, UnityAction confirmHelper)
    {
        Animator anim = helpingPersonInfo.GetComponent<Animator>();
        anim.SetTrigger("Idle");
        avatarImage.sprite = avatar;
        level.text = helperLevel;
        hospitalName.text = helperHospitalName;
        helpingPersonInfo.gameObject.SetActive(true);
        helpingPersonInfo.GetComponent<Button>().onClick.RemoveAllListeners();

        if (!VisitingController.Instance.IsVisiting && !hasConfirmHelper)
        {
            helpingPersonInfo.GetComponent<Button>().onClick.AddListener(confirmHelper);
            helpingPersonInfo.GetComponent<Button>().onClick.AddListener(() =>
            {
                anim.SetTrigger("Tap");
                MessageController.instance.ShowMessage(60);
                helpingPersonInfo.GetComponent<Button>().onClick.RemoveAllListeners();
            });
        }
        CloseBox();
    }

    public void SetClosedBoxWithoutHelpLook()
    {
        CloseBox();
    }

    private void CloseBox()
    {
        boxOpen.SetActive(false);
        boxClosed.SetActive(true);
        boxDisabled.SetActive(false);
        helpBadge.SetActive(false);
        anim.SetTrigger("Full");

        if (Hospital.VisitingController.Instance.IsVisiting)
        {
            closedBoxImage.material = ResourcesHolder.Get().GrayscaleMaterial;
            closedBoxImage.color = colorAlpha;
        }
        else
        {
            closedBoxImage.material = null;
            closedBoxImage.color = Color.white;
        }

        boxOpen.GetComponent<Button>().onClick.RemoveAllListeners();
    }

    public void SetOpenBoxLook(Sprite medicineSprite, string medicineStatus, bool helpRequested, bool isVisiting, UnityAction onBoxClickAction)
    {
        boxOpen.SetActive(true);
        boxClosed.SetActive(false);
        boxDisabled.SetActive(false);
        helpingPersonInfo.gameObject.SetActive(false);

        if (helpRequested && medicineSprite != null)
            helpBadge.SetActive(true);
        else
            helpBadge.SetActive(false);

        medicineIcon.sprite = medicineSprite;
        if (medicineSprite == null)
            medicineIcon.gameObject.SetActive(false);
        if (isVisiting && (!helpRequested || medicineSprite == null))
        {
            medicineQuantity.text = "";
            medicineIcon.material = ResourcesHolder.Get().GrayscaleMaterial;
            openBoxImage.material = ResourcesHolder.Get().GrayscaleMaterial;
            //medicineIcon.color = colorAlpha;
            openBoxImage.color = colorAlpha;
            boxOpen.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        else
        {
            medicineQuantity.text = medicineStatus;
            medicineIcon.material = null;
            openBoxImage.material = null;
            //medicineIcon.color = Color.white;
            openBoxImage.color = Color.white;
            boxOpen.GetComponent<Button>().onClick.RemoveAllListeners();
            boxOpen.GetComponent<Button>().onClick.AddListener(onBoxClickAction);
        }
    }

    public void SetDisabled()
    {
        boxOpen.SetActive(false);
        boxClosed.SetActive(false);
        boxDisabled.SetActive(true);
    }

    public void RefreshMedicineStatus(string medicineStatus)
    {
        //only refresh when not visiting or when visiting on objects which are requested for help
        if (!Hospital.VisitingController.Instance.IsVisiting ||
            (Hospital.VisitingController.Instance.IsVisiting && helpBadge.activeInHierarchy))
            medicineQuantity.text = medicineStatus;
    }

    public void MakeHelpButtonVisible()
    {
        helpBadge.SetActive(true);
    }

    public void SetSelected(bool isSelected)
    {
        boxSelection.SetActive(isSelected);
    }
}
