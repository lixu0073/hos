using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatientAvatarUI : MonoBehaviour
{
    [SerializeField]
    private GameObject unknownIcon = null;
    [SerializeField]
    private GameObject timerBadge = null;
    [SerializeField]
    private GameObject onWayBadge = null;

    
    [SerializeField]
    private Image avatarHead = null;
    [SerializeField]
    private Image avatarBody = null;
    [SerializeField]
    private Image avatarBackground = null;

    public void SetPatientAvatarActive(bool setActive)
    {
        gameObject.SetActive(setActive);
    }

    public void SetAvatarView(Sprite head, Sprite body, PatientBackgroundType backgroundType = PatientBackgroundType.defaultAdult)
    {
        avatarHead.sprite = head;
        avatarBody.sprite = body;
        SetBackground(backgroundType);
        SetAvatarObjectsActive();
    }

    public void SetUnknownView()
    {
        SetUnknownObjectsActive();
    }

    public void SetTimerView()
    {
        SetTimerObjectsActive();
    }


    public void SetOnWayView()
    {
        SetOnWayObjectsActive();
    }

    private void ClearAvatar()
    {
        avatarHead.gameObject.SetActive(false);
        avatarBody.gameObject.SetActive(false);
        SetUnknownIconActive(false);
        SetTimerBadgeActive(false);
        SetOnWayBadgeActive(false);
    }

    private void SetAvatarObjectsActive()
    {
        ClearAvatar();

        avatarHead.gameObject.SetActive(true);
        avatarBody.gameObject.SetActive(true);
    }

    private void SetUnknownObjectsActive()
    {
        ClearAvatar();

        SetUnknownIconActive(true);
    }


    public void SetTimerObjectsActive()
    {
        ClearAvatar();

        SetTimerBadgeActive(true);
    }

    public void SetOnWayObjectsActive()
    {
        ClearAvatar();

        SetOnWayBadgeActive(true);
    }
    
    private void SetUnknownIconActive(bool setActive)
    {
        if (unknownIcon != null)
        {
            unknownIcon.SetActive(setActive);
        }
    }

    private void SetTimerBadgeActive(bool setActive)
    {
        if (timerBadge != null)
        {
            timerBadge.SetActive(setActive);
        }
    }

    private void SetOnWayBadgeActive(bool setActive)
    {
        if (onWayBadge != null)
        {
            onWayBadge.SetActive(setActive);
        }
    }

    private void SetBackground(PatientBackgroundType backgroundType = PatientBackgroundType.defaultAdult)
    {
        if (avatarBackground == null)
        {
            return;
        }

        switch (backgroundType)
        {
            case PatientBackgroundType.defaultAdult:
                Debug.LogWarning("Not implemented");
                break;
            case PatientBackgroundType.unknownBaby:
                avatarBackground.sprite = ResourcesHolder.GetMaternity().unknownBabyAvatarBg;
                break;
            case PatientBackgroundType.boyBaby:
                avatarBackground.sprite = ResourcesHolder.GetMaternity().boyBabyBabyAvatarBg;
                break;
            case PatientBackgroundType.girlBaby:
                avatarBackground.sprite = ResourcesHolder.GetMaternity().girlBabyAvatarBg;
                break;
            default:
                Debug.LogWarning("Not implemented");
                break;
        }
    }

    public enum PatientBackgroundType
    {
        defaultAdult,
        unknownBaby,
        boyBaby,
        girlBaby
    }
}
