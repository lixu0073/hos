using UnityEngine;
using Hospital;

public class VIPHelipadMasterableClient : MonoBehaviour, MasterablePropertiesClient
{
    public string GetClientTag()
    {
        return "vipHelipad";
    }
}
