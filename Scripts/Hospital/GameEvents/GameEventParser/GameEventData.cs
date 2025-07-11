using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameEventData {

    public DateTime StartTime
    {
        get;
        private set;
    }

    public DateTime EndTime
    {
        get;
        private set;
    }

    public GameEventData(DateTime startTime, DateTime endTime)
    {
        this.StartTime = startTime;
        this.EndTime = endTime;
    }

    public static GameEventData Parse(string str)
    {
        if (string.IsNullOrEmpty(str))
            return null;

        var parts = str.Split('#');

        DateTime startTime = DateTime.ParseExact(parts[0] + " UTC", "yyyy-MM-dd HH:mm:ss UTC", System.Globalization.CultureInfo.InvariantCulture);

        DateTime endTime = DateTime.ParseExact(parts[1] + " UTC", "yyyy-MM-dd HH:mm:ss UTC", System.Globalization.CultureInfo.InvariantCulture);

        return new GameEventData(startTime, endTime);
    }
}
