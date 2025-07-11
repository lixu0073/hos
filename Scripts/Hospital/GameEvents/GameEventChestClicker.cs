using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GameEventChestClicker : MonoBehaviour, IPointerClickHandler
{

    public delegate void OnClick();

    public OnClick onClickEvent;

    public void AddOnClickListener(OnClick onClickEvent)
    {
        this.onClickEvent = onClickEvent;
    }

    public void OnPointerClick(PointerEventData pointerEvent)
    {
        onClickEvent?.Invoke();
    }
}
