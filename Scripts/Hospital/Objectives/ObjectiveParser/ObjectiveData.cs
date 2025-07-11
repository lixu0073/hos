using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveData{

    public int Level
    {
        get;
        private set;
    }

    public string Reward
    {
        get;
        private set;
    }

    public int Progress
    {
        get;
        private set;
    }

    public string Type
    {
        get;
        private set;
    }

    public string[] OtherParameters
    {
        get;
        private set;
    }

    public ObjectiveData(string type, int level, string reward, int progress, string[] otherParameters)
    {
        this.Type = type;
        this.Level = level;
        this.Reward = reward;
        this.Progress = progress;
        this.OtherParameters = otherParameters;
    }

    public static ObjectiveData Parse(string str)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        var parts = str.Split(';');
        int paramSize = parts.Length - 4;

        string[] otherParams = null;

        if (paramSize>0)
        {
            otherParams = new string[paramSize];
            int id = 0;

            for (int i = 4; i < parts.Length; i++)
            {
                otherParams[id] = parts[i];
                id++;
            }
        }

        return new ObjectiveData(parts[0],int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), parts[2], int.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture), otherParams);
    }
}
