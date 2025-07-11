using UnityEngine;

public class ZzzDoc : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] SpriteRenderer elixir;
    [SerializeField] SpriteRenderer cloud;
    [SerializeField] SpriteRenderer badge;
#pragma warning restore 0649
    bool isShown;

    public bool IsShown()
    {
        return isShown;
    }

    void Awake()
    {
        Hide();
    }

    public void SetPosition(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    public void SetElixirSprite(Sprite sprite)
    {
        elixir.sprite = sprite;
    }

    public void Show()
    {
        isShown = true;
        elixir.enabled = true;
        cloud.enabled = true;
        badge.enabled = true;
    }

    public void Hide()
    {
        isShown = false;
        elixir.enabled = false;
        cloud.enabled = false;
        badge.enabled = false;
    }
}
