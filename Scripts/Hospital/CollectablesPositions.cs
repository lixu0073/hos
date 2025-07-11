using UnityEngine;
using System.Collections;

public class CollectablesPositions : MonoBehaviour {

    public Transform[] positions;

    private void Awake()
    {
        Canvas canva = GetComponent<Canvas>();
        if (canva != null)
        {
            canva.worldCamera = ReferenceHolder.Get().worldUICamera;
            canva = null;
        }
    }
}
