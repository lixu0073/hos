using System;

namespace IsoEngine
{
	/// <summary>
	/// Pair of integers. Frequently used to represent tile position.
	/// </summary>
	[Serializable]
	public struct Vector2i
	{
		public static Vector2i zero = new Vector2i(0, 0);
        public static Vector2i empty = new Vector2i(-1, -1);
        public int x;
		public int y;

		public Vector2i(int X, int Y)
		{
			x = X;
			y = Y;
		}
		public override string ToString()
		{
			return string.Format("({0},{1})", x, y);
		}
		public static Vector2i operator +(Vector2i one, Vector2i two)
		{
			return new Vector2i(one.x + two.x, one.y + two.y);
		}
		public static Vector2i operator -(Vector2i one, Vector2i two)
		{
			return new Vector2i(one.x - two.x, one.y - two.y);
		}
		public static bool operator==(Vector2i one, Vector2i two)
		{
			return one.x == two.x && one.y == two.y;
		}
		public static bool operator!=(Vector2i one, Vector2i two)
		{
			return one.x != two.x || one.y != two.y;
		}
		public override bool Equals(object obj)
		{
			return ((Vector2i)obj).x == x && ((Vector2i)obj).y == y;
		}
		public override int GetHashCode()
		{
			return 125 * x % 13 * y;
		}
		public static Vector2i Parse(string str)
		{
			//UnityEngine.Debug.Log(str);
            var g = str.Substring(1, str.Length - 2).Split(',',' ');
			return new Vector2i(int.Parse(g[0], System.Globalization.CultureInfo.InvariantCulture), int.Parse(g[1], System.Globalization.CultureInfo.InvariantCulture));
		}
    }
}