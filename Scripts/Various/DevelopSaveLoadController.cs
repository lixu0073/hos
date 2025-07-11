using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class DevelopSaveLoadController
{

	public string[] GetVersions(DevelopSaveController.VersionSave[] versions)
    {
        List<string> list = new List<string>();
        foreach (var version in versions)
        {
            list.Add(version.Version);
        }
        return list.ToArray();
    }

    public string[] GetSaves(string version, DevelopSaveController.VersionSave[] versions)
    {
        List<string> saves = new List<string>();
        foreach (DevelopSaveController.VersionSave ver in versions)
        {
            if(ver.Version.Equals(version))
            {
                foreach(string save in ver.Saves)
                {
                    saves.Add(save);
                }
                break;
            }
        }
        return saves.ToArray();
    }

}
