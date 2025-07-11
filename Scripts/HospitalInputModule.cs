using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 自定义输入模块，用于处理医院场景中的UI交互，忽略多点触控。
/// </summary>
public class HospitalInputModule : StandaloneInputModule
{
    /// <summary>
    /// 重写Process方法，处理输入事件。此函数复制自StandaloneInputModule，因为ProcessTouchEvents无法被重写。
    /// </summary>
    public override void Process()
    {
        bool selectedObject = this.SendUpdateEventToSelectedObject();
        if (this.eventSystem.sendNavigationEvents)
        {
            if (!selectedObject)
                selectedObject |= this.SendMoveEventToSelectedObject();
            if (!selectedObject)
                this.SendSubmitEventToSelectedObject();
        }
        if (this.ProcessTouchEvents())
            return;
        this.ProcessMouseEvent();
    }

    /// <summary>
    /// 处理触摸事件。此方法是StandaloneInputModule.ProcessTouchEvents的复制，但只处理第一个触摸点。
    /// </summary>
    /// <returns>如果处理了触摸事件，则返回true；否则返回false。</returns>
    private bool ProcessTouchEvents()
    {
        if (Input.touchCount == 0)
            return false;

        // 这是StandaloneInputModule.ProcessTouchEvents中循环体的复制，
        // 但没有循环，因此它只处理第一个触摸点
        Touch touch = Input.GetTouch(0);
        if (touch.type != TouchType.Indirect)
        {
            bool pressed;
            bool released;
            PointerEventData pointerEventData = this.GetTouchPointerEventData(touch, out pressed, out released);
            this.ProcessTouchPress(pointerEventData, pressed, released);
            if (!released)
            {
                this.ProcessMove(pointerEventData);
                this.ProcessDrag(pointerEventData);
            }
            else
                this.RemovePointerData(pointerEventData);
        }
        return true;
    }
}