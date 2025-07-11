using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAnalyticParams
{
    protected Dictionary<string, object> parameters = new Dictionary<string, object>();
    public Dictionary<string, object> GetParams()
    {
        return parameters;
    }

    public virtual void Initialize() { }

}
