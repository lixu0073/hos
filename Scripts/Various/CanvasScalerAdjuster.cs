using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerAdjuster : MonoBehaviour
{
    private static CanvasScaler _canvasScaler = null;

    void Awake()
    {
        _canvasScaler = GetComponent<CanvasScaler>();
        if (_canvasScaler != null)
            _canvasScaler.matchWidthOrHeight = 0.0f;
    }

    public static void BalanceWidthHeight(float newValue)
    {
        if (_canvasScaler != null)
            _canvasScaler.matchWidthOrHeight = newValue;
    }
}
