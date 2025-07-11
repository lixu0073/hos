using UnityEngine;

public class IAPFade : MonoBehaviour
{
    float timeoutDelay = 99999f;
#pragma warning disable 0649
    [SerializeField]
    GameObject textObject;
#pragma warning restore 0649

    public void Show()
    {
        //Debug.LogError("IAPFade Show");
        gameObject.SetActive(true);

        CancelInvoke("TimeOut");
        Invoke("TimeOut", timeoutDelay);
    }

    public void ToggleText()
    {
        textObject.SetActive(!textObject.activeSelf);
    }   

    void TimeOut()
    {
        if (gameObject.activeSelf)
        {
            Hide();
            throw new System.TimeoutException("IAP server did not respond in a timely manner. Timeout delay = " + timeoutDelay);
        }
    }

    public void Hide()
    {
        //Debug.LogError("IAPFade Hide");
        gameObject.SetActive(false);
    }
}
