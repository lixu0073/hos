using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
[RequireComponent(typeof(Image))]
public class UIImageVisualController : VisualsUpgradeController.VisualController
{
    [SerializeField]
    private Image image = null;

    private void Awake()
    {
        if (image == null)
        {
            image = gameObject.GetComponent<Image>();
        }
    }

    public override void HideObject()
    {
        image.gameObject.SetActive(false);
    }

    public override void ShowObject()
    {
        image.gameObject.SetActive(true);
    }

    public override void SetObject(Sprite spriteToSet)
    {
        if (image != null)
        {
            image.sprite = spriteToSet;
            return;
        }
    }
}
