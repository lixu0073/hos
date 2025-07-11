using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HospitalCustomizationSaveData
{ 
    public List<CustomizableItem> customizableItems = new List<CustomizableItem>();

    public string currentSignName;
    public string currentFlagName;

    public string previousSignName;
    public string previousFlagName;

    public string currentHospitalFloorColorName;
    public string currentLaboratoryFloorColorName;
    public string currentMaternityFloorColorName;

    public List<string> ownedSigns;
    public List<string> ownedFlags;
    public List<string> ownedFloorColors;
    public int PremiumFloorColorCounter;

    string lastRetrievedSign;

    public HospitalCustomizationSaveData()
    {
        ownedSigns = new List<string>();
        ownedFlags = new List<string>();
        ownedFloorColors = new List<string>();
        PremiumFloorColorCounter = 0;
    }
}
