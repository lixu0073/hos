using UnityEngine;
using System.Collections;

public class PersonCreator : MonoBehaviour {
	//public GameObject pooler;
	//public GameObject personPrefab;
	//public bool isDoctor;
	//public bool isWoman;
	//public bool isPatient;
	//public bool isReceptionist;

	//private int index;
	//private GameObject person;

	//private Sprite[] HeadFront;
	//private Sprite[] HeadBack;
	//private Sprite[] TorsoFront;
	//private Sprite[] TorsoBack;
	//private Sprite[] Stomach;
	//private Sprite[] Arms;
	//private Sprite[] Hands;
	//private Sprite[] Thigh;
	//private Sprite[] Calf;
	//private Sprite[] Foot;
	//private Sprite[] Apron;


	////void Start()
	////{
	////	person = Instantiate (personPrefab, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(new Vector3(Screen.width/2,Screen.height/2,10)), Quaternion.identity) as GameObject;
	////	HeadFront = pooler.GetComponent<PersonParts> ().HeadListFront;
	////	HeadBack = pooler.GetComponent<PersonParts> ().HeadListBack;
	////	TorsoFront = pooler.GetComponent<PersonParts> ().TorsoListFront;
	////	TorsoBack = pooler.GetComponent<PersonParts> ().TorsoListBack;
	////	Stomach = pooler.GetComponent<PersonParts> ().StomachList;
	////	Arms = pooler.GetComponent<PersonParts> ().ArmsList;
	////	Hands = pooler.GetComponent<PersonParts> ().HandsList;
	////	Thigh = pooler.GetComponent<PersonParts> ().ThighList;
	////	Calf = pooler.GetComponent<PersonParts> ().CalfList;
	////	Foot = pooler.GetComponent<PersonParts> ().FootList;
	////}

	//public void changeHead()
	//{
	//	index = Random.Range (0, HeadFront.Length);
	//	person.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = HeadFront[index];
	//}

	//public void changeTorso()
	//{
	//	index = Random.Range (0, TorsoFront.Length);
	//	person.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>().sprite = TorsoFront[index];
	//}

	//public void changeStomach()
	//{
	//	index = Random.Range (0, Stomach.Length);
	//	person.transform.GetChild(4).gameObject.GetComponent<SpriteRenderer>().sprite = Stomach[index];
	//}

	//public void changeHands()
	//{
	//	index = Random.Range (0, Arms.Length);
	//	person.transform.GetChild(5).gameObject.GetComponent<SpriteRenderer>().sprite = Arms[index];
	//	person.transform.GetChild (5).gameObject.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = Hands[index];
	//	person.transform.GetChild(6).gameObject.GetComponent<SpriteRenderer>().sprite = Arms[index];
	//	person.transform.GetChild (6).gameObject.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = Hands[index];
	//}

	//public void changeLegs()
	//{
	//	index = Random.Range (0, Arms.Length);
	//	person.transform.GetChild(7).gameObject.GetComponent<SpriteRenderer>().sprite = Thigh[index];
	//	person.transform.GetChild (7).gameObject.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = Calf[index];
	//	person.transform.GetChild(8).gameObject.GetComponent<SpriteRenderer>().sprite = Thigh[index];
	//	person.transform.GetChild (8).gameObject.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = Calf[index];
	//}

}
