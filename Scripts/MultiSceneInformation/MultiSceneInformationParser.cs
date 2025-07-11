using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <remarks></remarks>
public class MultiSceneInformationParser
{
    private static MultiSceneInformationParser instance;

    private MultiSceneInformationParser()
    {

    }

    public static MultiSceneInformationParser Instance()
    {
        if (instance == null)
        {
            instance = new MultiSceneInformationParser();
        }
        return instance;
    }

    public MultiSceneInformation Parse(string data)
    {
        throw new System.NotImplementedException();
    }
}
