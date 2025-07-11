using Hospital.LootBox;
using UnityEngine;

public class ButtonAnchorController : MonoBehaviour
{
    [SerializeField]
    private Animator buttonAnchorAnimator = null;

    private bool buttonVisible = true;

    public void SetEventButtonVisible(bool setVisible)
    {
        if (transform != null && transform.GetChild(0) != null && transform.GetChild(0).gameObject != null)
        {
            if (!transform.GetChild(0).gameObject.activeInHierarchy)
                return;
        }
        else
        {
            Debug.LogError("SetEventButtonVisible ERROR: object info is null");
            return;
        }

        transform.GetChild(0).gameObject.TryGetComponent<Hospital.LootBox.LootBoxButtonUI>(out LootBoxButtonUI butt);
        if (butt && butt.IsLootBoxInCooldown)
        {
            return; // Preventing the change the LootBoxButtonUI cooldown
        }

        if (setVisible)
            ShowEventButton();
        else
            HideEventButton();
    }

    private void HideEventButton()
    {
        if (!buttonVisible)    
            return;

        if (buttonAnchorAnimator == null)
        {
            Debug.LogError("buttonAnchorAnimator is null");
            return;
        }

        buttonVisible = false;
        ResetAllTriggers();
        buttonAnchorAnimator.SetTrigger("Hide");
    }

    private void ShowEventButton()
    {
        if (buttonVisible)
            return;

        if (buttonAnchorAnimator == null)
        {
            Debug.LogError("buttonAnchorAnimator is null");
            return;
        }

        buttonVisible = true;
        ResetAllTriggers();
        buttonAnchorAnimator.SetTrigger("Show");
    }

    private void ResetAllTriggers()
    {
        buttonAnchorAnimator.ResetTrigger("Hide");
        buttonAnchorAnimator.ResetTrigger("Show");
    }
}
