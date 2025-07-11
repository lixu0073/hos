using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleUI;
using UnityEngine.EventSystems;

namespace Hospital
{
	public class DraggableElixirImage : MonoBehaviour
	{
        MedicineRef medicine;

        [SerializeField]
		Image img = null;
		DoctorRoom room;
		RectTransform tr;
		float width, height;
        float heightExpansion = 0;
        float left;
		OnEvent onEnd;

        Vector3 pos;
        Vector3 mPos;
        bool addPossible = false;
        bool addPossibleLastFrame = false;

		void Start()
		{
			transform.position = Input.mousePosition;
		}

		public void Initialize(DoctorRoom room, RectTransform panel, MedicineRef medicine, OnEvent onEnd = null)
		{
            this.medicine = medicine;
            addPossible = false;
            addPossibleLastFrame = false;
            this.onEnd = onEnd;
			this.room = room;
			tr = panel;
            MedicineTooltip.Open(medicine);
            img.sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);
            width = tr.sizeDelta.x * 0.8f * (tr.childCount < 6 ? (tr.childCount - 1) * 0.2f : 1) * UIController.get.canvas.transform.localScale.x;
			height = tr.sizeDelta.y * 0.8f * (tr.childCount < 5 ? 0.5f : 1) * UIController.get.canvas.transform.localScale.y;
            heightExpansion = tr.sizeDelta.y * 0.8f * 0.5f * UIController.get.canvas.transform.localScale.y;
            left = -90 * UIController.get.canvas.transform.localScale.x;
            transform.SetAsLastSibling();
        }

        void Update()
		{
			transform.position = Input.mousePosition;
            //transform.SetAsLastSibling();
            MedicineTooltip.Instance.gameObject.transform.SetAsLastSibling();

            CheckAddPossible();
            if (Input.GetMouseButtonUp(0))
            {
                room.GetHover().HideTempElixirAdded();
                if (addPossible)
                    room.AddCure();
                onEnd?.Invoke();

                Destroy(gameObject);
                return;
            }
            SetTempElixirAdded();
        }

        void CheckAddPossible()
        {
            addPossible = false;
            pos = tr.position;
            mPos = transform.position;

            RaycastHit hit;
            Ray ray = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) && room!=null && hit.transform == room.transform)
                addPossible = true;

            if (mPos.x > pos.x + left && mPos.x < pos.x + width && mPos.y < pos.y + heightExpansion && mPos.y > pos.y - height)
                addPossible = true;
        }

        void SetTempElixirAdded()
        {
            if (addPossible && !addPossibleLastFrame)
            {
                room.GetHover().ShowTempElixirAdded();
            }
            else if (!addPossible && addPossibleLastFrame)
            {
                room.GetHover().HideTempElixirAdded();
            }

            addPossibleLastFrame = addPossible;
        }
    }
}
