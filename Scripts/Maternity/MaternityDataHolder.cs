using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityDataHolder : MonoBehaviour
{

    public static MaternityDataHolder Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Reset()
    {
        //Debug.LogError("Resetuje maternity data holder. Jeszcze nie zrobione.");
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
