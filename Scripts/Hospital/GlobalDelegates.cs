using System;

namespace Hospital
{
    public delegate void OnSuccess();
    public delegate void OnFailure(Exception e);
    public delegate void OnMovementEnded(UnityEngine.RectTransform rect);
}