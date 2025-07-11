using UnityEngine;

public class Flag : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private SpriteRenderer spriteRenderer;
#pragma warning restore 0649

    public void UpdateFlag(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void UpdateFlSetFlagActive(bool setActive)
    {
        ReferenceHolder.GetHospital().HospitalNameSign.SetPoleActive(setActive);
        spriteRenderer.gameObject.SetActive(setActive);
    }
}
