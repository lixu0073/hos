using System.Collections;
using System.Collections.Generic;

namespace Hospital
{
    public class StandardEventsDeltaConfig
    {
        private static string standardEventConfigNameAWS = null;

        private const string configNameParamKey = "selectSEconfig";

        public static void Initialize(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey(configNameParamKey))
            {
                standardEventConfigNameAWS = (string)parameters[configNameParamKey];
            }
        }

        public static string GetStandardEventConfigNameAWS()
        {
            return standardEventConfigNameAWS;
        }
    }
}
