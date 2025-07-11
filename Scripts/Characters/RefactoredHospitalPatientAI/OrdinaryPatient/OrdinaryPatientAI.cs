using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using IsoEngine;

namespace Hospital
{
	public class OrdinaryPatientAI : RefactoredHospitalPatientAI
	{
		[HideInInspector] public HospitalRoom destRoom;
		[HideInInspector] public int spotID;

		public bool MoveWithRoom 
		{
			get;
			private set;
		}

		public override void Awake(){
			goToChangeRoom = new OrdinaryGoToChangeRoom(this, HospitalDataHolder.Instance.Emergency.entrance);
			inBed = new OrdinaryInBed(this);
			goToDiagRoom = new OrdinaryGoToDiagRoom(this);
			healing = new OrdinaryHealing(this);
			returnToBed = new OrdinaryReturnToBed(this);
			goHome = new OrdinaryGoHome(this, HospitalDataHolder.Instance.Emergency.entrance);
			goToRoom = new OrdinaryGoToRoom(this);
		}

		public void Initialize(Vector2i pos, HospitalRoom destRoom, bool spawned){
			base.Initialize (pos,spawned);

			this.destRoom = destRoom;

		}

		public override string SaveToString()
		{
			var z = GetComponent<HospitalCharacterInfo>();
			var p = HospitalDataHolder.Instance.QueueContainsPatient(this);
			return position + "!" + spotID + "!" + state + "!" + p + "!" + z.SaveToString() + "!" + stateManager.State.SaveToString() + "^" + z.personalBIO;
		}

		public override void Initialize(RotatableObject room, string info)
		{
			//Debug.Log(info);
			var strs = info.Split('!');
			base.Initialize(Vector2i.Parse(strs[0]));
			spotID = int.Parse(strs[1], System.Globalization.CultureInfo.InvariantCulture);
			state = (CharacterStatus)Enum.Parse(typeof(CharacterStatus), strs[2]);
			//print(info);
			destRoom = (HospitalRoom)room;
			destRoom.ReacquireTakenSpot(spotID);
			stateManager = new RefactoredStateManager();
			var queue = bool.Parse(strs[3]);
			GetComponent<HospitalCharacterInfo>().FromString(strs[4]);
			if (queue)
				HospitalDataHolder.Instance.AddLastToDiagnosticQueue(this);
			if (strs.Length > 5)
			{
				switch (strs[5])
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
					Debug.Log(strs[7] + "    " + strs[6]);
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



		public override void GetOutOfBed()
		{
			this.transform.position = new Vector3(destRoom.ReacquireTakenSpot(spotID).x, 0, destRoom.ReacquireTakenSpot(spotID).y);
			position = destRoom.ReacquireTakenSpot(spotID);
			//destRoom.UnCoverBed(spotID);
		}

		public override void LayInBed(){
			base.LayInBed(destRoom, destRoom.ReturnBed(spotID));
			base.LayInBed ();

		}

		public void FreeBed(){
			destRoom.FreeBed(spotID);
		}
		public override void UnCoverBed(){
	//		destRoom.UnCoverBed(spotID);
		}

		public string GetRoomName()
		{
			return destRoom.name;
		}

		public HospitalRoom GetDestRoom()
		{
			return destRoom;
		}

		public override void ChangeToPajama()
		{
			var p = this.gameObject.GetComponent<HospitalCharacterInfo>();
			List <Sprite> Pajama = ReferenceHolder.GetHospital().PersonCreator.GetPijama(p.Race, p.Sex, p.IsVIP);
			for (int i = 0; i < this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts.Count; ++i)
			{
				this.gameObject.GetComponent<HospitalCharacterInfo>().BodyParts[i].sprite = Pajama[i];
			}
			return;
		}



	}


}