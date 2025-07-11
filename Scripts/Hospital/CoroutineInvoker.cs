using UnityEngine;

public class CoroutineInvoker : MonoBehaviour
{

    public static CoroutineInvoker Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }
}
