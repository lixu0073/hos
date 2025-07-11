using UnityEngine;
using System.Collections.Generic;
using MovementEffects;

public class ParticlesController : MonoBehaviour
{
    public List<ParticleSystem> orbs;
    public List<Transform> orbsSpawnSpots;
    
    void Start()
    {
        DisableAllOrbs();
        Timing.RunCoroutine(SpawnOrbs());
    }

    void DisableAllOrbs()
    {
        ParticleSystem.EmissionModule em;
        for (int i = 0; i < orbs.Count; i++)
        {
            em = orbs[i].emission;
            em.enabled = false;
            //orbs[i].enableEmission = false;
            orbs[i].gameObject.SetActive(false);
        }
    }

    IEnumerator<float> SpawnOrbs()
    {
        while (true)
        {
            SpawnOrb();
            yield return Timing.WaitForSeconds(GameState.RandomFloat(3f, 6f));
        }
    }

    void SpawnOrb()
    {
        ParticleSystem orb = GetFreeOrb();
        //Debug.LogError("Spawn orb: " + orb.gameObject.name);
        orb.transform.SetParent(orbsSpawnSpots[GameState.RandomNumber(0, orbsSpawnSpots.Count)]);
        orb.transform.localPosition = Vector3.zero;
        orb.gameObject.SetActive(true);
        var em = orb.emission;
        em.enabled = true;
        //orb.enableEmission = true;
        Timing.RunCoroutine(DisableOrb(orb, 30f));
    }

    ParticleSystem GetFreeOrb()
    {
        for (int i = 0; i < orbs.Count; i++)
            if(!orbs[i].gameObject.activeSelf)
                return orbs[i];
        return orbs[0];
    }

    public void ShowDailyTaskParticle()
    {
        UIController.getHospital.DailyTaskUpdatedParticle.GetComponent<RectTransform>().position = Input.mousePosition;
        //UIController.get.DailyTaskUpdatedParticle.Play();
#if UNITY_EDITOR
        Debug.Log("ParticleSpawned: " + Input.mousePosition.ToString());
#endif
    }

    IEnumerator<float> DisableOrb(ParticleSystem orb, float delay)
    {
        yield return Timing.WaitForSeconds(delay);
        var em = orb.emission;
        em.enabled = false;
        //orb.enableEmission = false;
        yield return Timing.WaitForSeconds(10);
        orb.gameObject.SetActive(false);
    }
}
