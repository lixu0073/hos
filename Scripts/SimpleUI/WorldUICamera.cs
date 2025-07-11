using UnityEngine;
using System.Collections;


//this camera renders items which are in world space but have to be rendered in front (like Collectables)
public class WorldUICamera : MonoBehaviour {

    public Camera mainCam;
    Camera thisCam;

    void Awake() {
        thisCam = GetComponent<Camera>();
    }

    void LateUpdate() {
        SyncSize();
    }
    
    void SyncSize() {
        thisCam.orthographicSize = mainCam.orthographicSize;
    }
}
