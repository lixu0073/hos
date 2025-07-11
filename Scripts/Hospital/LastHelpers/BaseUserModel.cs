using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;
using System;

public class BaseUserModel : BaseFollower
{

    public BaseUserModel(string SaveID) : base()
    {
        saveID = SaveID;
    }

    public override string ToString()
    {
        return GetSaveID();
    }

    

}
