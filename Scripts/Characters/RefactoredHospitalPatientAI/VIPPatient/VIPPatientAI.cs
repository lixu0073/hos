using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using IsoEngine;

namespace Hospital
{
	//TAK BARDZO DO ZMIANY
	public class VIPPatientAI : RefactoredHospitalPatientAI
	{
		public string definition;

		public override void Awake(){
			goToChangeRoom = new VIPGoToChangeRoom(this, ResourcesHolder.GetHospital().VIPspots[1]); // przy łóżku
			inBed = new VIPInBed(this);
			goToDiagRoom = new VIPGoToDiagRoom(this);
			healing = new VIPHealing(this);
			returnToBed = new VIPReturnToBed(this);
			goHome = new VIPGoHome(this, HospitalDataHolder.Instance.Emergency.entrance);
			goToRoom = new VIPGoToRoom(this);
		}

		public override string SaveToString()
		{
			var z = GetComponent<HospitalCharacterInfo>();
			var p = HospitalDataHolder.Instance.QueueContainsPatient(this);
			return position  + "!" + state + "!" + p + "!" + z.SaveToString() + "!" + stateManager.State.SaveToString() + "^" + z.personalBIO;
		}

		public override void Initialize(string info)
		{
			//Debug.Log(info);
			var strs = info.Split('!');
			base.Initialize(Vector2i.Parse(strs[0]));
			state = (CharacterStatus)Enum.Parse(typeof(CharacterStatus), strs[1]);
			//print(info);
			stateManager = new RefactoredStateManager();
			var queue = bool.Parse(strs[2]);
			GetComponent<HospitalCharacterInfo>().FromString(strs[3]);
			if (queue)
				HospitalDataHolder.Instance.AddLastToDiagnosticQueue(this);
			if (strs.Length > 5)
			{
				switch (strs[4])
				{
				case "GTCR":
					stateManager.State = goToChangeRoom;
					break;
				case "IB":
					stateManager.State = inBed;
					break;
				case "GTDR":
					var p = (DiagnosticRoom)HospitalAreasMapController.HospitalMap.FindRotatableObject(strs[6]);
					p.SetPatient(this);
					stateManager.State = goToDiagRoom;
					break;
				case "H":
					Debug.Log(strs[6] + "    " + strs[5]);
					stateManager.State = healing;
					break;
				case "RTB":
					stateManager.State = returnToBed;
					break;
				case "GH":
					stateManager.State = goHome;
					break;
				case "GTR":
					stateManager.State = goToRoom;
					break;
				default:
					break;
				}
			}
			var tmp = GetComponent<HospitalCharacterInfo>();
			if (!Patients.Contains(tmp) && !(stateManager.State is BaseGoToRoom || stateManager.State is BaseGoHome || stateManager.State is BaseGoToChangeRoom))
				Patients.Add(tmp);
		}

		public override void GetOutOfBed(){
			
		}

		public void FreeBed(){
			
		}
		public override void UnCoverBed(){
			
		}


		public override void LayInBed(){
			base.LayInBed ();
			transform.position = new Vector3 (ResourcesHolder.GetHospital().VIPspots [1].x, transform.position.y, ResourcesHolder.GetHospital().VIPspots [1].y -0.5f);
			anim.Play (AnimHash.Bed_Reading);
			anim.SetFloat ("tile_X", -1);
			anim.SetFloat ("tile_Y", 0);
		}

		public override void ChangeToPajama()
		{
			//var p = this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts;
			List<Sprite> Pajama = ReferenceHolder.GetHospital().vipSpawner.GetPajama (definition);
			for (int i = 0; i < this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts.Count; ++i)
			{
				this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts[i].sprite = Pajama[i];
			}
			return;
		}



	}


}