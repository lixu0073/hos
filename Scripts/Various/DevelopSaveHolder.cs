using UnityEngine;
using System.Collections;

public class DevelopSaveHolder : MonoBehaviour
{
    public static DevelopSaveHolder Instance;

    public string ForcedCognito;
    public string SaveName;
    public string Version;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
