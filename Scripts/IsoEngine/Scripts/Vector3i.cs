using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoEngine
{
	[Serializable]
	public struct Vector3i
	{
		public int x;
		public int y;
		public int z;


		public Vector3i(int X, int Y,int Z)
		{
			x = X;
			y = Y;
			z = Z;
		}

		public override string ToString()
		{
			return string.Format("({0}, {1}, {2})", x, y,z);
		}

        public static Vector3i Parse(string str)
        {
            //UnityEngine.Debug.Log(str);
            var g = str.Substring(1, str.Length - 2).Split(',', ' ');
            return new Vector3i(int.Parse(g[0], System.Globalization.CultureInfo.InvariantCulture), int.Parse(g[1], System.Globalization.CultureInfo.InvariantCulture), int.Parse(g[2], System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
