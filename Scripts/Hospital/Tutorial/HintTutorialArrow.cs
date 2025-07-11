using UnityEngine;
using UnityEngine.UI;

public class HintTutorialArrow : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] Animator anim;
    [SerializeField] Image arrow;
#pragma warning restore 0649

    public void SetArrow()
    {
        if (Game.Instance.gameState() != null && Game.Instance.gameState().GetHospitalLevel() <= 5)
            ShowArrow();
        else
            HideArrow();
    }

    void ShowArrow()
    {
        //Debug.LogError("ShowArrow");
        anim.enabled = true;
        arrow.enabled = true;
    }

    void HideArrow()
    {
        //Debug.LogError("HideArrow");
        //anim.enabled = false;
        arrow.enabled = false;
    }
}
