using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

namespace Hospital
{

    public class ProductionHoverDraggableElement : MonoBehaviour//, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        MedicineRef medicine;
        GameObject pref;
        Transform tr;
        MedicineProductionMachine machine;
        [SerializeField]
        Text amount = null;
        [SerializeField]
        GameObject arrow = null;
        [SerializeField]
        GameObject badge = null;
        [SerializeField]
        GameObject amountBG = null;
        [SerializeField]
        Image medicineImage = null;
        public void Initialize(MedicineRef medicine, GameObject prefab, Transform panelTransform, MedicineProductionMachine machine)
        {

            this.medicine = medicine;

            UpdateBadgeVisibility();
            pref = prefab;
            ActualiseMedicineCount();
            //medicineImage = GetComponent<Image>();
            medicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);
            if (pref == null)
            {
                arrow.SetActive(false);
                medicineImage.color = new Color(1, 1, 1, .67f);
                //medicineImage.material = ResourcesHolder.Get().GrayscaleMaterial;
            }
            tr = panelTransform;
            this.machine = machine;
        }
        public void ActualiseMedicineCount()
        {

            amount.gameObject.SetActive(true);
            if (pref != null)
                amount.text = GameState.Get().GetCureCount(medicine).ToString();
        }

        public void OnMouseDown()
        {
            InstantiateTool();
        }
        private void InstantiateTool()
        {
            UpdateBadgeVisibility();

            if (pref == null)
            {
                MedicineLockedTooltip.Open(medicine);
                return;
            }
            MedicineTooltip.Open(medicine, true);
            var p = GameObject.Instantiate(pref);
            var z = p.transform;
            z.position = Input.mousePosition;
            z.SetParent(transform.parent.parent.parent.parent);
            z.SetAsLastSibling();
            MedicineTooltip.Instance.gameObject.transform.SetAsLastSibling();
            z.localScale = Vector3.one;
            medicineImage.enabled = false;
            amountBG.SetActive(false);
            arrow.SetActive(false);
            p.GetComponent<ProductionHoverDraggableMiniature>().Initialize(machine, (RectTransform)tr, medicine, () =>
            {
                ShowImage();
            });
            p.SetActive(true);

            z.position = Input.mousePosition;
        }
        private void ShowImage()
        {
            try
            {
                arrow.SetActive(true);
                medicineImage.enabled = true;
            }
            catch
            {
                Debug.LogError("Something got destroyed");
            }
        }


        public void UpdateBadgeVisibility()
        {
            if (MedicineBadgeHintsController.Get().GetMedicineNeededToHealCount(medicine) > 0)
            {
                if (!badge.gameObject.activeSelf)
                {
                    badge.SetActive(true);
                }
            }
            else
            {
                if (badge.gameObject.activeSelf)
                {
                    badge.SetActive(false);
                }
            }

            // Debug.LogWarning("Update bage visibility: " + badge.activeSelf + " for needed " + HintsController.Get().GetMedicineNeededToHealCount(medicine));
            // Debug.LogWarning("Update bage visibility: " + badge.activeSelf + " for needed " + medicine.type + " " + GameState.Get().GetMedicineNeededToHealCount(medicine));
        }

        public Image GetMedicineImage() {
            return medicineImage;
        }

        public MedicineRef GetMedicine()
        {
            return medicine;
        }
    }
}