using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class MhApiResponse  {
    private const string OK = "OK";

    public string message;
    public string status;

    public bool IsOk()
    {
        return status == OK;
    }

}
