using Hospital;
using UnityEngine;

public class GiftsMailBoxStandObject : SuperObject
{
    #region SerializedFields
#pragma warning disable 0649
    [SerializeField] private GameObject Indicator;
    [SerializeField] private Animator animator;
#pragma warning restore 0649
    #endregion

    #region privateFields
    private delegate void MBoxClick();
    private MBoxClick onClick;
    int IdleTrigger = Animator.StringToHash("Idle");
    int FilledTrigger = Animator.StringToHash("Filled");
    int MaxFilledTrigger = Animator.StringToHash("MaxFilled");
    #endregion
    
    public void UpdateMailBox(GiftsMailBoxStandController.MailBoxState state)
    {
        ResetAllTriggers();
        switch (state)
        {
            case GiftsMailBoxStandController.MailBoxState.Empty:
                onClick = OnUnlockedMailboxClick;
                SetIndicatorActive(false);
                animator.SetTrigger(IdleTrigger);
                break;
            case GiftsMailBoxStandController.MailBoxState.Fill:
                onClick = OnUnlockedMailboxClick;
                SetIndicatorActive(true);
                animator.SetTrigger(FilledTrigger);
                break;
            case GiftsMailBoxStandController.MailBoxState.MaxFull:
                onClick = OnUnlockedMailboxClick;
                SetIndicatorActive(true);
                animator.SetTrigger(MaxFilledTrigger);
                break;
            default:
                break;
        }
    }
    
    private void ResetAllTriggers()
    {
        animator.ResetTrigger(IdleTrigger);
        animator.ResetTrigger(FilledTrigger);
        animator.ResetTrigger(MaxFilledTrigger);
    }

    public override void OnClick()
    {
        if (visitingMode)
            return;

        if (UIController.get.drawer.IsVisible)
        {
            UIController.get.drawer.SetVisible(false);
            return;
        }
        if (UIController.get.FriendsDrawer.IsVisible)
        {
            UIController.get.FriendsDrawer.SetVisible(false);
            return;
        }

        onClick?.Invoke();
    }

    public override void IsoDestroy() { }

    private void OnUnlockedMailboxClick()
    {
        gameObject.SetActive(true);
        StartCoroutine(UIController.getHospital.mailboxPopup.Open());
    }

    private void onFeatureLockedBoxClicked()
    {
        string message = I2.Loc.ScriptLocalization.Get("GIFT_SYSTEM/GIFT_FLOAT_RECEIVE").Replace("{0}", GiftsAPI.Instance.GiftsFeatureMinLevel.ToString());
        MessageController.instance.ShowMessage(message);
        Debug.LogError("onFeatureLockedBoxClicked");
    }

    private void onLockedMailBoxClicked()
    {
        MessageController.instance.ShowMessage(59);
    }

    private void SetIndicatorActive(bool setActive)
    {
        if (Indicator == null)
        {
            Debug.LogError("Indicator is null");
            return;
        }
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            Indicator.SetActive(false);
            return;
        }
        Indicator.SetActive(setActive);
    }

}
