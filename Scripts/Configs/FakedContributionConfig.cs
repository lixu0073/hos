using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Hospital
{
    public class FakedContributionConfig
    {
        private const char POINTS_SEPARATOR = ';';
        private const char POINTS_COORD_SEPARATOR = '%';

        public const string KEY_ID = "InterpolationPoints";

        private static Dictionary<float, float> interpolationPoints;

        public static void InstantiateConfig(Dictionary<string, object> parametres)
        {
            if (parametres != null && parametres.Count > 0)
            {
                foreach (KeyValuePair<string, object> pair in parametres)
                {
                    string key = pair.Key;
                    string val = pair.Value as string;

                    if (!string.IsNullOrEmpty(key) && key.StartsWith(KEY_ID) && !string.IsNullOrEmpty(val))
                    {
                        string sansKey = key.Substring(KEY_ID.Length);
                        int temp;

                        if (!string.IsNullOrEmpty(sansKey) && int.TryParse(sansKey, out temp))
                        {
                            ParseInterpolationPoints(val);
                        }
                    }
                }
            }
        }

        public static void GetInterpolationPointsLists(out List<float> xs, out List<float> ys)
        {
            xs = new List<float>();
            ys = new List<float>();

            if (interpolationPoints != null)
            {
                foreach (KeyValuePair<float, float> pair in interpolationPoints)
                {
                    xs.Add(pair.Key);
                    ys.Add(pair.Value);
                }
            }
            else
            {
                foreach(FakedContributionConfigFallback.InterpolationPair pair in ResourcesHolder.Get().FakedContributionFallback.GetPair())
                {
                    xs.Add(pair.x);
                    ys.Add(pair.y);
                }
            }
        }

        private static void ParseInterpolationPoints(string arg)
        {
            if(interpolationPoints == null) interpolationPoints = new Dictionary<float, float>();

            string[] points = arg.Split(POINTS_SEPARATOR);
            string[] details;
            float x, y;

            if (points != null && points.Length > 0)
            {
                foreach (string point in points)
                {
                    details = point.Split(POINTS_COORD_SEPARATOR);
                    /*
                    x = float.Parse(details[0], CultureInfo.InvariantCulture);
                    y = float.Parse(details[1], CultureInfo.InvariantCulture);
                    */

                    if (details != null && details.Length == 2 && float.TryParse(details[0], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out x) && float.TryParse(details[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                    {
                        interpolationPoints.Add(x, y);
                    }
                }
            }
        }
    }
}