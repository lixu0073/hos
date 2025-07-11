using UnityEngine;
using UnityEngine.UI;
using Hospital;
using SimpleUI;

public class RequiredCureUI : MonoBehaviour
{
    [SerializeField]
    private GameObject questionmark = null;

    [SerializeField]
    private Image cureImage = null;

    [SerializeField]
    private PointerDownListener pointerDownListener = null;

    public void SetObjectActive(bool setActive)
    {
        gameObject.SetActive(setActive);
    }

    public void SetQuestionmarkActive(bool setActive)
    {
        questionmark.SetActive(setActive);
    }

    public void SetCureImageActive(bool setActive)
    {
        cureImage.gameObject.SetActive(setActive);
    }

    public void SetCureImage(Sprite cureSprite)
    {
        cureImage.sprite = cureSprite;
    }

    public void SetPointerDownListener(OnEvent onPointerDown)
    {
        pointerDownListener.SetDelegate(onPointerDown);
    }
}
