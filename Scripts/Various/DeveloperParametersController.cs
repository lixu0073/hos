using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DeveloperParametersController : MonoBehaviour
{
    public DevelopGameParameters parameters;
    private static DeveloperParametersController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static DeveloperParametersController Instance()
    {
        return instance;
    }
}
