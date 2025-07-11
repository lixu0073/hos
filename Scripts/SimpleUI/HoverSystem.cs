using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Hospital;

namespace SimpleUI
{
	public class HoverSystem : MonoBehaviour
	{
		public Canvas canvas;
		public Camera cameras;
		public GameObject prefab;
		public GameObject doctorHoverPrefab;
		public GameObject productionHoverPrefab;
		public GameObject thingy;
		//void Start()
		//{
		//	activeHover = null;
		//	//prefab = (GameObject)Resources.Load("SimpleUI/Prefabs/Hover");
		//}

		//public void TestFunction()
		//{
		//	CustomHover temp = CreateHover(Vector2.zero);
		//	//temp.Initialize();
		//	for (int i = 1; i < 6; i++)
		//	{
		//		var tempo = Instantiate(thingy);
		//		tempo.SetActive(true);
		//		temp.AddElement(HoverPanels.Up, tempo, i * 20, i * 20);
		//	}
		//	for (int i = 1; i < 6; i++)
		//	{
		//		var tempo = Instantiate(thingy);
		//		tempo.SetActive(true);
		//		temp.AddElement(HoverPanels.Down, tempo, 120 - i * 20, 120 - i * 20);
		//	}
		//	//	print("aje em hir");
		//}

		//public void CloseHover()
		//{
		//	if (activeHover != null)
		//	{
		//		activeHover.GetComponent<CustomHover>().Close();
		//		activeHover.gameObject.SetActive(false);
		//	}
		//}
		//public void MoveHoverTo(Vector2 ScreenPosition)
		//{
		//	if (activeHover != null)
		//	{
		//		activeHover.GetComponent<RectTransform>().anchoredPosition = ScreenPosition;
		//	}
		//}
		//public void MoveHoverBy(Vector2 ScreenDistance)
		//{
		//	if (activeHover != null)
		//	{
		//		activeHover.GetComponent<RectTransform>().anchoredPosition += ScreenDistance;
		//	}
		//}
		//public CustomHover CreateHover()
		//{
		//	return CreateHover(Vector2.zero);
		//}
		//public CustomHover CreateHover(Vector2 ScreenPosition)
		//{
		//	if (activeHover == null)
		//	{
		//		var temp = Instantiate(prefab);
		//		temp.SetActive(true);
		//		temp.transform.SetParent(canvas.transform, false);
		//		temp.transform.SetAsFirstSibling();
		//		temp.GetComponent<RectTransform>().anchoredPosition = ScreenPosition;
		//		activeHover = temp;
		//		return temp.GetComponent<CustomHover>();
		//	}
		//	else
		//	{
		//		activeHover.GetComponent<CustomHover>().Close();
		//		activeHover.gameObject.SetActive(true);
		//		activeHover.GetComponent<RectTransform>().anchoredPosition = ScreenPosition;
		//		return activeHover.GetComponent<CustomHover>();
		//	}

		//}


		//public DoctorHover CreateDoctorHoover()
		//{
		//	if(activeDoctorHover== null)
		//	{
		//		activeDoctorHover = GameObject.Instantiate(doctorHoverPrefab);
		//		activeDoctorHover.transform.SetParent(canvas.transform,false);
		//		activeDoctorHover.transform.SetAsFirstSibling();
		//		doctorHover = activeDoctorHover.GetComponent<DoctorHover>();
		//	}
		//	return doctorHover;
		//}
		//public void CloseDoctorHover()
		//{
		//	if(activeDoctorHover!= null)
		//	{
		//		doctorHover.Reset();
		//		activeDoctorHover.SetActive(false);
		//	}
		//}
		//public MedicineProductionHover CreateProductionHover()
		//{
		//	if (activeProductionHover == null)
		//	{
		//		activeProductionHover = GameObject.Instantiate(productionHoverPrefab);
		//		activeProductionHover.transform.SetParent(canvas.transform, false);
		//		activeProductionHover.transform.SetAsFirstSibling();
		//		productionHover = activeProductionHover.GetComponent<MedicineProductionHover>();
		//	}
		//	return productionHover;
		//}
		//public void CloseProductionHover()
		//{
		//	if (activeProductionHover != null)
		//	{
		//		productionHover.Reset();
		//		activeProductionHover.SetActive(false);
		//	}
		//}


		//public CustomHover CreateHover(Vector3 WorldPosition)
		//{
		//	return CreateHover(cameras.WorldToScreenPoint(WorldPosition));
		//}

	}
}