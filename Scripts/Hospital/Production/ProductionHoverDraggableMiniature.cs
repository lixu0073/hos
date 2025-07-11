using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using MovementEffects;

namespace Hospital
{

	public class ProductionHoverDraggableMiniature : MonoBehaviour
	{
		MedicineProductionMachine room;
		MedicineRef medicine;
		RectTransform tr;
		Transform myTransform;
		float width, height;
        float heightExpansion = 0;
		float left;

		SimpleUI.OnEvent onEnd;

        bool canAddMedicineToProduction = false;
        bool tapReleased = false;
		void Start()
		{
            startSendingToProduction();
        }

        void startSendingToProduction() {
            canAddMedicineToProduction = false;
            tapReleased = false;

            Timing.RunCoroutine(PointerOverMachineCheck(), Segment.LateUpdate);
        }

		public void Initialize(MedicineProductionMachine room, RectTransform panel, MedicineRef medicine, SimpleUI.OnEvent oneEnd=null)
		{
			this.onEnd = oneEnd;
			//print("draggable initilize");
			this.room = room;
			tr = panel;
			this.medicine = medicine;
			GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);
			width = tr.sizeDelta.x * 0.8f * (tr.childCount < 6 ? (tr.childCount - 1) * 0.2f : 1) * UIController.get.canvas.transform.localScale.x;
			height = tr.sizeDelta.y * 0.8f * (tr.childCount < 5 ? 0.5f : 1) * UIController.get.canvas.transform.localScale.y;
            heightExpansion = tr.sizeDelta.y * 0.8f * 0.5f  * UIController.get.canvas.transform.localScale.y;
            left = -120 * UIController.get.canvas.transform.localScale.x;
			myTransform = transform;

			//print("setting position from " + myTransform.position + " to" + Input.mousePosition+" in initialize");
			myTransform.position = Input.mousePosition;
		}
		void Update()
		{
			myTransform.position = Input.mousePosition;

			if (Input.GetMouseButtonUp(0))
			{
                tapReleased = true;
            }

		}

		IEnumerator<float> PointerOverMachineCheck(){
			bool isOverPast = false;
			for (;;) {
				bool isOver = false;
				RaycastHit hit;
				Ray ray = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out hit)) {
				//	Debug.Log (hit.transform.name);
					if (hit.transform == room.transform) 
					{
						isOver = true;
					}
				}

				var mPos = myTransform.position;
				var pos = tr.position;

				if (mPos.x > pos.x + left && mPos.x < pos.x + width && mPos.y < pos.y + heightExpansion && mPos.y > pos.y - height)
				{
					isOver = true;

				}

				if (isOver != isOverPast)
				{
					if (isOver)
					{
                        canAddMedicineToProduction = true;
                        room.ShowMedicineInQueue(medicine);
					} else {

                        canAddMedicineToProduction = false;
						room.HideMedicineInQueue();
					}
				}

				isOverPast = isOver;

                if (tapReleased)
                {
                    // StopCoroutine(PointerOverMachineCheck());
                    room.HideMedicineInQueue();

                    if (canAddMedicineToProduction && room != null)
                    {
                        //room.HideMedicineInQueue();

                        if (medicine != null)
                            room.AddMedicineToQueue(medicine);
                    }
                    break;
                   
                }
                yield return Timing.WaitForSeconds(0.04f);
               
			}
            onEnd?.Invoke();
            GameObject.Destroy(gameObject);
        }

	}
}
