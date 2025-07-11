using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ObjectiveGeneratorData {

    int lastCounterId = -1;
    string[] lastParams = new string[3];

    public ObjectiveGeneratorData(int lastCounterId = 0)
    {
        this.lastCounterId = lastCounterId;
    }

    public bool CheckParamExist(string param)
    {
        if (lastParams != null && lastParams.Length > 0)
        {
            for (int i = 0; i< lastParams.Length; i++)
            {
                if (lastParams[i] == param)
                    return true;
            }
        }

        return false;
    }

    public string[] GetParams()
    {
        return lastParams;
    }

    public int GetCounterTrigger()
    {
        return lastCounterId;
    }

    public void UpdateCounterTrigger(int counter)
    {
        lastCounterId = counter;
    }

    public void UpdateParam(string param)
    {
        // if param previously exist then remove

        for (int i = 0; i < lastParams.Length; i++)
        {
            if (lastParams[i] == param)
                return;
        }

        // if any param was empty then set

        for (int i = 0; i < lastParams.Length; i++)
        {
            if (string.IsNullOrEmpty(lastParams[i]))
            {
                lastParams[i] = param;
                return;
            }
        }

        // remove first param and set as last

        for (int i = 0; i < lastParams.Length; i++)
        {
            if (i <= lastParams.Length - 2)
                lastParams[i] = lastParams[i + 1];
            else
                lastParams[i] = param;
        }
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(Checkers.CheckedAmount(this.lastCounterId, 0, int.MaxValue, "ObjectiveGeneratorData lastCounterId: ").ToString());
        builder.Append("!");
        if (this.lastParams != null)
        {
            for (int i = 0; i < this.lastParams.Length; i++)
            {
                builder.Append(this.lastParams[i]);
                if (i< this.lastParams.Length - 1)
                    builder.Append("^");
            }
        }

        return builder.ToString();
    }

    public void LoadFromString(string str)
    {
        var save = str.Split('!');

        lastCounterId = int.Parse(save[0], System.Globalization.CultureInfo.InvariantCulture);

        if (!string.IsNullOrEmpty(save[1]))
        {
            var paramStr = save[1].Split('^');

            if (paramStr.Length>0)
            {
                for (int i = 0; i < paramStr.Length; i++)
                {
                    this.lastParams[i] = paramStr[i];
                }
            }
        }
    }
}
