using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Hospital;
using SimpleUI;
using TMPro;

public class ProbeTableHoverElement : MonoBehaviour //, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    MedicineRef medicine;
    GameObject pref;
    ProbeTable probTable;
    [SerializeField] private Image medicineImage = null;

    [SerializeField] private GameObject arrow = null;
    [SerializeField] GameObject badge = null;


    public void Initialize(ProbeTable table, MedicineRef medicine, GameObject prefab)
    {
        medicineImage.enabled = true;
        pref = prefab;
        medicineImage.sprite = medicine != null
            ? ResourcesHolder.Get().GetSpriteForCure(medicine)
            : ResourcesHolder.GetHospital().ProbeTableToolSprite;
        probTable = table;

        badge.SetActive(false);

        if (medicine != null)
        {
            //var bei = ResourcesHolder.Get().GetMedicineInfos(medicine) as BaseElixirInfo;
            this.medicine = medicine;
            if (pref == null)
            {
                medicineImage.color = new Color(1, 1, 1, .67f);
                arrow.SetActive(false);
            }
        }
        else
        {
            arrow.SetActive(false);
        }

        UpdateBadgeVisibility();
    }

    public void OnMouseDown()
    {
        if (pref == null)
        {
            MedicineLockedTooltip.Open(medicine);
            return;
        }

        InstantiateTool();
    }

    private void InstantiateTool()
    {
        UpdateBadgeVisibility();

        arrow.SetActive(false);
        medicineImage.enabled = false;
        var p = Instantiate(pref);
        var z = p.transform;
        z.position = Input.mousePosition;
        z.SetParent(UIController.get.canvas.transform);
        z.SetAsLastSibling();
        z.localScale = Vector3.one;

        p.GetComponent<ProbeTableTool>().Initialize(probTable, medicine, () =>
        {
            if (ProbeTableHover.GetActive() != null)
                if (gameObject && gameObject.activeSelf)
                    ShowImage();
        });
        p.SetActive(true);
    }

    public void ShowImage()
    {
        if (medicine != null)
        {
            arrow.SetActive(true);
        }

        medicineImage.enabled = true;
    }

    public void UpdateBadgeVisibility()
    {
        if (medicine != null)
        {
            if (MedicineBadgeHintsController.Get().GetMedicineNeededToHealCount(medicine) > 0)
            {
                //GameState.Get(GetMedicinesWithPrerequisite
                badge.SetActive(true);
            }
            else badge.SetActive(false);

            //Debug.LogWarning("Update bage visibility: " + badge.activeSelf + " for needed " + HintsController.Get().GetMedicineNeededToHealCount(medicine));
        }
        else badge.SetActive(false);
    }

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //	//if (shouldPropagate)
    //	transform.parent.parent.gameObject.SendMessage("OnBeginDrag", eventData);
    //}

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //	//	if (shouldPropagate)
    //	transform.parent.parent.gameObject.SendMessage("OnEndDrag", eventData);
    //	shouldBuild = true;
    //	shouldPropagate = true;
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //	if (shouldPropagate)
    //		transform.parent.parent.gameObject.SendMessage("OnDrag", eventData);
    //}

    public Image GetMedicineImage()
    {
        return medicineImage;
    }
}