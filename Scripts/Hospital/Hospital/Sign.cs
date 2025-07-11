using UnityEngine;

public class Sign : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private SpriteRenderer spriteRenderer;
#pragma warning restore 0649

    public void UpdateSign(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}
