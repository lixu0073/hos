using UnityEngine;
using System.Collections.Generic;
using Hospital.BoxOpening.UI;
using Maternity.UI;

public class MaternityUIController : BaseUIController
{

    public static MaternityUIController get;

    [Header("PopUps")]

    [Header("Hovers")]

    [Header("Other")]
    public GameObject bloodTestHoverPrefab;

    [Header("Presenters")]
    public BoxOpeningPopupUI boxOpeningPopupUI;
    public MaternityPatientCardController patientCardController;
    public MaternityInfoPopup maternityInfoPopup;
    public MaternityNurseRoomCardController nurseRoomCardController;
    public BabyPopupUI babyPopup;
    public MaternityVitaminCollectorPopup maternityVitaminCollectorPopup;
    public MaternityVitaminMakerButton vitaminMakerButton;

    void Awake()
    {
        get = this;
        PoPUpArtsFromResources = new Dictionary<string, GameObject>();
    }

    public override void SetBoosterAndBoxButtons() { }
    public override void SetDailyQuestsButton() { }

    public override void FadeClicked()
    {
        int count = ActivePopUps.Count;
        if (count == 0)
            return;
        if (ActivePopUps.Contains(alertPopUp))
            return;
        if (ActivePopUps.Contains(RatePopUp))
            return;
        ActivePopUps[count - 1].Exit();
        Fade.UpdateFadePosition(this.transform.GetSiblingIndex());
    }

    public void onSub()
    {

    }
}