using UnityEngine;
using System.Collections;

public class TutorialKidsClickArea : MonoBehaviour
{

    [TutorialTriggerable]
    public void SetActive(bool isActive) { this.gameObject.SetActive(isActive); }

    public void OnClick()
    {
        Debug.Log("TutorialKidsClickArea OnClick");
        NotificationCenter.Instance.KidsClicked.Invoke(new BaseNotificationEventArgs());
    }
}
