using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesHolder : MonoBehaviour
{

    private static ResourcesHolder resourcesHolder = null;

    public static BaseResourcesHolder Get()
    {
        if (resourcesHolder == null)
            throw new IsoException("Fatal Failure of BaseResourcesHolder system. Delete your project and start again :v ");
        return resourcesHolder.GetBase();
    }

    public static HospitalResourcesHolder GetHospital()
    {
        if (resourcesHolder.hospitalResourcesHolder != null)
            return resourcesHolder.hospitalResourcesHolder;
        return null;
    }

    public static MaternityResourcesHolder GetMaternity()
    {
        if (resourcesHolder.maternityResourcesHolder != null)
            return resourcesHolder.maternityResourcesHolder;
        return null;
    }

    private HospitalResourcesHolder hospitalResourcesHolder = null;
    private MaternityResourcesHolder maternityResourcesHolder = null;

    void Awake()
    {
        resourcesHolder = this;
        hospitalResourcesHolder = GetComponent<HospitalResourcesHolder>();
        maternityResourcesHolder = GetComponent<MaternityResourcesHolder>();
    }

    public BaseResourcesHolder GetBase()
    {
        if (hospitalResourcesHolder != null)
            return hospitalResourcesHolder;
        return maternityResourcesHolder;
    }
}
