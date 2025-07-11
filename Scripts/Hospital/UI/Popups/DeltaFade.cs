using UnityEngine;
using UnityEngine.UI;

public class DeltaFade : MonoBehaviour
{
    float checkInterval = 3f;
#pragma warning disable 0649
    [SerializeField]
    Image img;
#pragma warning restore 0649

    public void Show()
    {
        //Debug.LogError("DeltaFade Show");
        img.raycastTarget = true;

        CancelInvoke("Check");
        Invoke("Check", checkInterval);
    }

    void Check()
    {
        if (img.raycastTarget)
        {
            Hide();
            throw new System.TimeoutException("Delta fade has been shown when no message is active.");
        }

        CancelInvoke("Check");
        Invoke("Check", checkInterval);
    }

    public void Hide()
    {
        //Debug.LogError("DeltaFade Hide");
        img.raycastTarget = false;

        CancelInvoke("Check");
    }
}
