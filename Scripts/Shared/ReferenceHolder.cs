using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceHolder : MonoBehaviour {

    private static ReferenceHolder refereneHolder = null;

    public static Base_ReferenceHolder Get()
    {
        if (refereneHolder == null)
            throw new IsoException("Fatal Failure of Base_ReferenceHolder system. Delete your project and start again :v ");
        return refereneHolder.GetBase();
    }

    public static HospitalReferenceHolder GetHospital()
    {
        if (refereneHolder.hospitalReferenceHolder != null)
            return refereneHolder.hospitalReferenceHolder;
        return null;
    }

    public static MaternityReferenceHolder GetMaternity()
    {
        if (refereneHolder.maternityReferenceHolder != null)
            return refereneHolder.maternityReferenceHolder;
        return null;
    }

    private HospitalReferenceHolder hospitalReferenceHolder = null;
    private MaternityReferenceHolder maternityReferenceHolder = null;

    void Awake()
    {
        refereneHolder = this;
        hospitalReferenceHolder = GetComponent<HospitalReferenceHolder>();
        maternityReferenceHolder = GetComponent<MaternityReferenceHolder>();
        if (hospitalReferenceHolder!=null)
        {
            hospitalReferenceHolder.Initialize();
        }
        else
        {
            maternityReferenceHolder.Initialize();
        }
    }
    void Start()
    {
        if (refereneHolder == null)
        {
            Debug.LogWarning("ReferenceHolder is null. Re setting it to this");
            refereneHolder = this;
        }
        if (hospitalReferenceHolder == null)
        {
            Debug.LogWarning("HospitalReferenceHolder is null. Re setting it to this. If we are in maternity, this is fine");
            hospitalReferenceHolder = GetComponent<HospitalReferenceHolder>();
        }
        if (maternityReferenceHolder == null)
        {
            Debug.LogWarning("MaternityReferenceHolder is null. Re setting it to this. If we are in hospital, this is fine");
            maternityReferenceHolder = GetComponent<MaternityReferenceHolder>();
        }
        if (hospitalReferenceHolder != null)
        {
            hospitalReferenceHolder.Initialize();
        }
        else if (maternityReferenceHolder != null)
        {
            maternityReferenceHolder.Initialize();
        }
    }

    public Base_ReferenceHolder GetBase()
    {
        if (hospitalReferenceHolder != null)
            return hospitalReferenceHolder;
        return maternityReferenceHolder;
    }

    private void OnDestroy()
    {
        ObjectivesSynchronizer.Instance.Deinitialize();
    }
}
