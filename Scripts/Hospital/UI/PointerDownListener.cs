using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using SimpleUI;

namespace Hospital
{
	public class PointerDownListener : MonoBehaviour, IPointerDownHandler
	{
		OnEvent onPointerDown;
		public void OnPointerDown(PointerEventData eventData)
		{
			onPointerDown?.Invoke();
		}
		public void SetDelegate(OnEvent onPointerDown)
		{
			this.onPointerDown = onPointerDown;
		}

        public void ClearDelegate() {
            this.onPointerDown = null;
        }
	}
}