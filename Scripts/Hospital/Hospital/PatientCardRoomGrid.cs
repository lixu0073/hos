using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class PatientCardRoomGrid : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] GameObject roomPrefab;
    [SerializeField] Transform duplicatedTransform;
#pragma warning restore 0649
    List<GameObject> rooms;
    bool isVip;
    [SerializeField] float vipOffset = 55;
    int lastPatientCount;

    void Awake()
    {
        rooms = new List<GameObject>();
    }

    void Update()
    {
        if (isVip)
            transform.localPosition = new Vector3(duplicatedTransform.localPosition.x + vipOffset, transform.localPosition.y, transform.localPosition.z);
        else
            transform.localPosition = new Vector3(duplicatedTransform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
    }

    public void UpdateGrid()
    {
        int patientCount = 0;
        for (int i = 0; i < duplicatedTransform.childCount; i++)
        {
            if (duplicatedTransform.GetChild(i).gameObject.activeSelf)
                patientCount++;
        }

        //on level 3 in tutorial we dont want to show room panels.
        if (!TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.cure_bed_patient))
            patientCount = 0;

        //when there's vip there is odd number of portraits.
        isVip = patientCount % 2 == 1 && Hospital.HospitalAreasMapController.HospitalMap.vipRoom.ExternalHouseState == Hospital.ExternalRoom.EExternalHouseState.enabled;

        if (rooms == null)
            rooms = new List<GameObject>();

        if (patientCount > lastPatientCount)
        {
            //for some reason too many room grids are spawned. Lets reset and spawn new ones.
            //Debug.LogError("This stuff");
            rooms.Clear();
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }
        else if (rooms.Count == Mathf.FloorToInt(patientCount / 2))
            return; //all room grids are already spawned

        //spawn rooms below portraits
        for (int i = 1; i < patientCount; i += 2)
        {
            GameObject go = Instantiate(roomPrefab, transform) as GameObject;
            go.transform.localScale = Vector3.one;
            rooms.Add(go);
        }

        lastPatientCount = patientCount;
    }
}