using UnityEngine;

/// <summary>
/// sets medicine icon on building production machine
/// </summary>
public class ProductionMachineMedicine : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] SpriteRenderer medicineIconRenderer;
#pragma warning restore 0649

    void Awake()
    {
        medicineIconRenderer.enabled = false;
    }

    public void SetMedicineIcon(Sprite sprite)
    {
        medicineIconRenderer.sprite = sprite;
        medicineIconRenderer.enabled = true;
    }
}
