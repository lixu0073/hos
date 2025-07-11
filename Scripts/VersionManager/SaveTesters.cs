using UnityEngine;
using Hospital;

public class SaveTesters : MonoBehaviour
{
	public static void TestSave_v1(Save save)
    {
		int saveVersion = 1;
		try
		{
			TestID_v1 (save.ID);
			TestFacebookID_v1 (save.FacebookID);
			TestVersion_v1 (save.version, 1, 2, saveVersion);
			TestLastBuyByWise_v1(save.lastBuyByWise, 0, 0, saveVersion);// 0 is strange but it is how it is 

		}
		catch(SaveErrorTestPositive /*saveErrorTestPositive*/)
		{
			Debug.Log ("Save Error Test Positive means that something went incredibly wrong");
		}

	}

	private static bool TestID_v1(string ID)
    {
		return true;
	}

	private static bool TestFacebookID_v1(string FacebookID)
    {
		return true;
	}

	private static bool TestVersion_v1(string version, int minValue, int maxValue, int saveVersion)
    {
		int iVersion = int.Parse (version); // Is version format correct?
		return TestNumberValue(iVersion, minValue, maxValue, "Version", saveVersion);
	}

	private static bool TestLastBuyByWise_v1(long lastBuyByWise,  int minValue, int maxValue, int saveVersion)
    {
		return TestNumberValue(lastBuyByWise, minValue, maxValue, "lastBuyByWise", saveVersion); 
	}


	#region basicTests

	private static bool TestNumberValue(int value, int minValue, int maxValue, string valueName, int saveVersion)
    {
		if (value < minValue || value > maxValue)
        {
			throw new SaveErrorTestPositive (valueName + ": " + value + " - is out of range <" + minValue + "," + maxValue + ">" + " Tested save version: " + saveVersion);
			//return false;
		}
		return true;
	}

	private static bool TestNumberValue(long value, long minValue, long maxValue, string valueName, int saveVersion)
    {
		if (value < minValue || value > maxValue)
        {
			throw new SaveErrorTestPositive (valueName + ": " + value + " - is out of range <" + minValue + "," + maxValue + ">" + " Tested save version: " + saveVersion);
			//return false;
		}
		return true;
	}

	private static bool TestNumberValue(float value, float minValue, float maxValue, string valueName, int saveVersion)
    {
		if (value < minValue || value > maxValue)
        {
			throw new SaveErrorTestPositive (valueName + ": " + value + " - is out of range <" + minValue + "," + maxValue + ">" + " Tested save version: " + saveVersion);
			//return false;
		}
		return true;
	}

	#endregion

}
