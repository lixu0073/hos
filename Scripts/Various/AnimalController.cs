using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;

public class AnimalController : MonoBehaviour {

    public List<GameObject> fishes;
    public List<Transform> fishSpawnSpots;
    public List<GameObject> deer;

    private Hospital.AnimalAI currentDeer;

    void Start()
    {
        DisableAllAnimals();
        SpawnRandomDeer();
        Timing.RunCoroutine(SpawnFishes());
    }

    void DisableAllAnimals()
    {
        int fishCount = fishes.Count;
        for (int i = 0; i < fishCount; i++)
            fishes[i].SetActive(false);

        int deerCount = deer.Count;
        for (int i = 0; i < deerCount; i++)
            deer[i].SetActive(false);
    }

    void SpawnRandomDeer()
    {
        currentDeer = deer[Random.Range(0, deer.Count)].GetComponent<Hospital.AnimalAI>();
        currentDeer.gameObject.SetActive(true);
    }

    public void InitializeDeer()
    {
        if (currentDeer!=null)
            currentDeer.Initialize();
    }

    IEnumerator<float> SpawnFishes()
    {
        while (true)
        {
            SpawnFish();
            yield return Timing.WaitForSeconds(BaseGameState.RandomFloat(2f, 6f));
        }
    }

    void SpawnFish()
    {
        //Debug.LogWarning("Spawn Fish");
        GameObject fish = GetFreeFish();
        fish.transform.SetParent(fishSpawnSpots[BaseGameState.RandomNumber(0, fishSpawnSpots.Count)]);
        fish.transform.localPosition = Vector3.zero;
        fish.SetActive(true);
        Timing.RunCoroutine(DisableFish(fish, 3f));
    }

    GameObject GetFreeFish()
    {
        int fishCount = fishes.Count;
        for (int i = 0; i < fishCount; i++)
            if(!fishes[i].activeSelf)
                return fishes[i];
        return fishes[0];
    }

    IEnumerator<float> DisableFish(GameObject fish, float delay)
    {
        yield return Timing.WaitForSeconds(delay);
        fish.SetActive(false);
    }
}
