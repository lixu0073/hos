using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FriendAddingResultView : MonoBehaviour 
{
#pragma warning disable 0649
    [SerializeField] private Button exit;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private TextMeshProUGUI friendFoundText;
    [SerializeField] private TextMeshProUGUI friendNotFoundText;
    [SerializeField] private GameObject friendRoot;
#pragma warning restore 0649

    public UnityAction Exit
    {
        set
        {
            exit.RemoveAllOnClickListeners();
            exit.onClick.AddListener(value);
        }
    }

    public UnityAction VisitingOkAction
    {
        set
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(value);
        }    
    }

    public bool Succes
    {
        set
        {
            friendNotFoundText.gameObject.SetActive(!value);
            friendFoundText.gameObject.SetActive(value);
            friendRoot.SetActive(value);
        }
    }

    public string ConfirmButtonText
    {
        set { confirmButtonText.GetComponent<Localize>().Term = value; }
    }
}
