using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRendererVisualController : VisualsUpgradeController.VisualController
{
    [SerializeField]
    private SpriteRenderer spriteRenderer = null;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }
    }

    public override void HideObject()
    {
        spriteRenderer.gameObject.SetActive(false);
    }

    public override void ShowObject()
    {
        spriteRenderer.gameObject.SetActive(true);
    }

    public override void SetObject(Sprite spriteToSet)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = spriteToSet;
            return;
        }
    }
}
