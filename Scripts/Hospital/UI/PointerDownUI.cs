using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PointerDownUI : MonoBehaviour
{
    private UnityAction onPointerDownAction;

    public void SetOnPointerDownAction(UnityAction action)
    {
        onPointerDownAction = action;
    }

    public void OnPointerDown()
    {
        if (onPointerDownAction != null)
        {
            onPointerDownAction();
        }
    }
}
