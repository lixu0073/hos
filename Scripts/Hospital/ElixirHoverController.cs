using UnityEngine;
using System.Collections;
using Hospital;
using System.Collections.Generic;

public class ElixirHoverController : MonoBehaviour {

	private Vector3 firstSpotPosition;
	private Vector3 secondSpotPosition;
	private Vector3 thirdSpotPosition;

	private int totalPages = 1;
	private int currentPage = 1;

	private GameObject leftArraw;
	private GameObject rightArrow;
	private List<MedicineDatabaseEntry> medicines;

	void Start () {
		InitializeGUI ();

		UpdateCurrentPage ();
	}

	private void InitializeData(){
		medicines = ResourcesHolder.Get().GetMedicinesOfType(MedicineType.BaseElixir);
	}

	private void InitializeGUI(){
		firstSpotPosition = transform.Find("FirstSpot").gameObject.transform.localPosition;
		secondSpotPosition = transform.Find("SecondSpot").gameObject.transform.localPosition;
		thirdSpotPosition = transform.Find("ThirdSpot").gameObject.transform.localPosition;

		leftArraw = transform.Find ("LeftArrow").gameObject;
		rightArrow = transform.Find ("RightArrow").gameObject;
	}

	private void UpdateCurrentPage(){

		//for(int i = (currentPage - 1) * itemsPerPage; i < )

		UpdateArrows ();
	}



	private void InitializeItemOnSpot(Vector3 localPosition, GameObject item){
		
	}

	private void OnLeftArrowClick(){
		if (currentPage == 1) {
			return;
		}
		--currentPage;
		UpdateCurrentPage ();
	}

	private void OnRightArrowClick(){
		if (currentPage == totalPages) {
			return;
		}
		++currentPage;
		UpdateCurrentPage ();
	}

	private void UpdateArrows(){
		leftArraw.SetActive (currentPage > 1);
		rightArrow.SetActive (currentPage < totalPages);
	}
	
}
