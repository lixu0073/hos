using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using IsoEngine;

[RequireComponent(typeof(Canvas))]
public class RotationSetter : MonoBehaviour
{
    public Camera gameCamera;

    private void Start()
    {
        GetComponent<Canvas>().worldCamera = gameCamera.GetComponent<Camera>();
        transform.rotation = gameCamera.transform.rotation;
    }

    private void Update()
    {
        transform.rotation = gameCamera.transform.rotation;
    }
}
