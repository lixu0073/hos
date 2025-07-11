using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PointerUpUI : MonoBehaviour
{

    private UnityAction onPointerUpAction;

    public void SetOnPointerUpAction(UnityAction action)
    {
        onPointerUpAction = action;
    }

    public void OnPointerUp()
    {
        if (onPointerUpAction != null)
        {
            onPointerUpAction();
        }
    }
}
