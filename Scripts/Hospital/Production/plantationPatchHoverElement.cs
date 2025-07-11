using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Hospital;
using SimpleUI;

public class plantationPatchHoverElement : MonoBehaviour//, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	MedicineRef medicine;
	GameObject pref;
	PlantationPatch plantationPatch;
	[SerializeField]
	private Image medicineImage = null;
    [SerializeField]
	private GameObject arrow = null;
    [SerializeField]
    GameObject badge = null;

    private PlantationPatchToolType toolType;

	public void Initialize(PlantationPatch patch, MedicineRef medicine, GameObject prefab, PlantationPatchToolType toolType)
	{
		medicineImage.enabled = true;
		pref = prefab;

        badge.SetActive(false);

        if (medicine != null)
        {
            medicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);
        }
        else
        {
            switch (toolType)
            {
                case PlantationPatchToolType.collect:
                    medicineImage.sprite = ResourcesHolder.GetHospital().PlantHarvestingSprite;
                    break;
                case PlantationPatchToolType.help:
                    medicineImage.sprite = ResourcesHolder.GetHospital().PatchHelpSignSprite;
                    break;
                case PlantationPatchToolType.renew:
                    medicineImage.sprite = ResourcesHolder.GetHospital().PatchCultivatorSprite;
                    break;
                default:
                    medicineImage.sprite = ResourcesHolder.GetHospital().PlantHarvestingSprite;
                    break;
            }
        }
		plantationPatch = patch;
		this.toolType = toolType;

		if (medicine != null)
		{
			//var bei = ResourcesHolder.Get().GetMedicineInfos(medicine) as BasePlantInfo;
			//amountGameObject.GetComponent<Text>().text = bei.Price.ToString();
			this.medicine = medicine;
			if(pref== null)
			{

				//medicineImage.material = ResourcesHolder.Get().GrayscaleMaterial;
                medicineImage.color = new Color(1, 1, 1, .67f);
                //amountContainerGameObject.SetActive(false);
                arrow.SetActive(false);
			}

            UpdateBadgeVisibility();
            /*upperPart.SetActive(true);
			this.medicine = medicine;
			var g = ResourcesHolder.Get().GetMedicineInfos(medicine) as BaseElixirInfo;
			panaceaAmount.text = "x" + g.PanaceaAmount;
			panaceaAmount.color = g.PanaceaAmount <= GameState.Get().CheckPanaceaAmount() ? Color.white : Color.red;*/

        }
		else
		{
			//amountContainerGameObject.SetActive(false);
			arrow.SetActive(false);
		}
		//upperPart.SetActive(false);
		time = -1;
	}
	private float time;
	Vector3 firstPos;

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

    }
    public void OnMouseDown()
	{

        if (pref == null) {
            MedicineLockedTooltip.Open(medicine);
            return;
        }/* else if(toolType == PlantationPatchToolType.renew){
			
			TextTooltip.Open(ResourcesHolder.Get().medicines.cures[15].medicines[3].GetMedicineRef(), true);
		}*/
        else if (toolType == PlantationPatchToolType.help) {
                        TextTooltip.OpenSingleText(I2.Loc.ScriptLocalization.Get("HELP"));
        }

        time = Time.time;
		firstPos = Input.mousePosition;
		InstantiateTool();
	}
	public void OnMouseUp()
	{
		time = -1;
	}
	public void OnMouseExit()
	{
		time = -1;
	}
	public void OnMouseEnter()
	{
		time = -1;
	}

	private void InstantiateTool()
	{
		arrow.SetActive(false);
		//amountContainerGameObject.SetActive(false);
		medicineImage.enabled = false;
		var p = GameObject.Instantiate(pref);
		var z = p.transform;
		z.position = Input.mousePosition;
		z.SetParent(UIController.get.canvas.transform);
		z.SetAsLastSibling();
		z.localScale = Vector3.one;

		p.GetComponent<PlantationPatchTool>().Initialize(plantationPatch, toolType, medicine, () =>
			{
				if(plantationPatchHover.GetActive()!=null)
				if (gameObject && gameObject.activeSelf)
					Invoke("ShowImage", 0.1f);
			});
		p.SetActive(true);
		time = -1;
		//ProbeTableHover.GetActive().Close();
	}
	public void ShowImage()
	{
		if (medicine != null)
		{
			arrow.SetActive(true);
			//amountContainerGameObject.SetActive(true);
		}
		medicineImage.enabled = true;

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

	public Image GetMedicineImage() {
		return medicineImage;
	}
}