using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsoEngine
{
	[Serializable]
	public class PathInfo
	{
		/// <summary>
		/// Number of level
		/// </summary>
		public int lvlID;
		/// <summary>
		/// List of next steps on path(coordinates of tiles).
		/// </summary>
		public List<Vector2i> path;
	}

	/// <summary>
	/// tri of integers representing tile coordinates in level space(!). Not in world space!
	/// </summary>
	public struct TilePosition
	{
		public int lvl, x, y;
		public TilePosition(int Lvl, int X, int Y)
		{
			lvl = Lvl;
			x = X;
			y = Y;
		}
		public TilePosition(int Lvl, Vector2i pos)
		{
			lvl = Lvl;
			x = pos.x;
			y = pos.y;
		}
	}

	/// <summary>
	/// Consist about information about path on single or two levels(from starting point to elevator and from elevator to ending point).
	/// </summary>
	public class InterLevelPathInfo
	{
		public List<PathInfo> paths;

		public InterLevelPathInfo()
		{
			paths = new List<PathInfo>();
		}
	}
}
