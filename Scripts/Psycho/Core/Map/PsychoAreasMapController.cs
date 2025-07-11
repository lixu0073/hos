using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;
using IsoEngine;

public class PsychoAreasMapController : PFMapController
{

    #region Static

    public static PsychoAreasMapController _map;

    public static PsychoAreasMapController Map
    {
        get
        {
            if (_map == null)
            {
                Debug.Log("Scene should contain exactly one PsychoAreasMapController if you want size use this feature");
            }
            return _map;
        }
    }

    void Awake()
    {
        _map = this;
    }

    #endregion

    public override void IsoDestroy()
    {
        base.IsoDestroy();
    }

    public override void CreateMap(IsoMapData mapData)
    {
        base.CreateMap(mapData);
    }

    internal override void Initialize()
    {
        base.Initialize();
       
    }
}
