using System;
using UnityEngine;

public class MaternityLabourRoomObjects : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private Transform parent;
    [SerializeField]
    private GameObject coverPrefab;
    [SerializeField]
    private GameObject balloonsPrefab;
#pragma warning restore 0649

    private GameObject coverGO = null;
    private GameObject balloonsGO = null;

    public void SpawnCover()
    {
        if (coverGO == null)
        {
            coverGO = Instantiate(coverPrefab, parent);
            try { 
                coverGO.transform.GetChild(0).GetComponent<Animator>().Play("Labor_cover_unfold", 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }
    }

    public void SetIdleCover()
    {
        if (coverGO == null)
        {
            coverGO = Instantiate(coverPrefab, parent);
            try
            {
                coverGO.transform.GetChild(0).GetComponent<Animator>().Play("Base Layer.Labor_cover_unfolded", 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }
    }

    public void SpawnBalloons()
    {
        RemoveObjects();
        if (balloonsGO == null)
        {
            balloonsGO = Instantiate(balloonsPrefab, parent);
            try { 
                balloonsGO.GetComponent<ParticleSystem>().Play();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            SoundsController.Instance.BabyBorn();
        }
    }

    public void RemoveObjects()
    {
        if (coverGO != null)
        {
            Destroy(coverGO);
            coverGO = null;
        }
        if (balloonsGO != null)
        {
            Destroy(balloonsGO);
            balloonsGO = null;
        }
    }

}
