using UnityEngine;
using Hospital;

public class VIPRoomMasterableClient : MonoBehaviour, MasterablePropertiesClient
{
    public string GetClientTag()
    {
        return "vipWard";
    }
}
