using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagnosticRoomStars : MonoBehaviour {
    [SerializeField]
    private GameObject[] stars = null;

    private Vector3[] positions = new Vector3[3];

    void Awake()
    {
        for (int i = 0; i < positions.Length && i < stars.Length; ++i)
        {
            positions[i] = stars[i].transform.localPosition;
        }
    }

    public void SetAppearance(int masteryLevel)
    {
        for (int i = 0; i < stars.Length; ++i)
        {
            stars[i].SetActive(false);
        }
        
        for (int i = 0; i < masteryLevel; ++i)
        {
            stars[i].SetActive(true);
        }

        SetStarsPositions(masteryLevel);
    }

    private void SetStarsPositions(int masteryLevel)
    {
        if (masteryLevel == 1)
        {
            stars[0].transform.localPosition = positions[1];
        }
        if (masteryLevel == 2)
        {
            stars[0].transform.localPosition = (positions[0] + positions[1]) / 2.0f;
            stars[1].transform.localPosition = (positions[1] + positions[2]) / 2.0f;
        }
        if (masteryLevel == 3)
        {
            stars[0].transform.localPosition = positions[0];
            stars[1].transform.localPosition = positions[1];
            stars[2].transform.localPosition = positions[2];
        }
    }
}
