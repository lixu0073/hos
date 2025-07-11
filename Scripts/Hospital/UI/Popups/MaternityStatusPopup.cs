using SimpleUI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MaternityStatusPopup : UIElement
{
#pragma warning disable 0649
    [SerializeField]
    private MaternityStatusVitaminPanel vitaminPanel;
    [SerializeField]
    private MaternityStatusMotherPanel motherPanel;
    [SerializeField]
    private TextMeshProUGUI NoInfoText;
#pragma warning restore 0649
    private UnityAction goTomaternityAction;

    private MaternityStatusPopup Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public MaternityStatusPopup Get()
    {
        return Instance;
    }

    public void InitializeData(MaternityStatusData data)
    {
        data.mainPopupStrategy.SetupPanel(this, data);
    }

    public MaternityStatusVitaminPanel GetVitaminPanel()
    {
        return vitaminPanel;
    }

    public MaternityStatusMotherPanel GetMotherPanel()
    {
        return motherPanel;
    }

    public void OnGoTomaternityButtonClick()
    {
        goTomaternityAction?.Invoke();
    }

    public void ButtonExit()
    {
        base.Exit();
    }

    public void SetEmptyPanel()
    {
        NoInfoText.gameObject.SetActive(true);
    }

    public void SetGoToMaternityButton(UnityAction action)
    {
        goTomaternityAction = action;
    }

    public void SetPanelWithInfo()
    {
        motherPanel.gameObject.SetActive(true);
        vitaminPanel.gameObject.SetActive(true);
    }

    public void ClearPanel()
    {
        vitaminPanel.gameObject.SetActive(false);
        motherPanel.gameObject.SetActive(false);
        NoInfoText.gameObject.SetActive(false);
    }
}
