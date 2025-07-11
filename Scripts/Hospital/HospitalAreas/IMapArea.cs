using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Text;
using IsoEngine;

namespace Hospital
{
	public interface IMapArea
	{
		IEnumerable<RectWallInfo> GetRectangles();
		Vector3 GetRectPos();
        Vector3 GetRectSize();
        Vector3 GetRectStartPoint();
        void SetParent(Transform trans);
		bool CanBuy();
		bool ContainsPoint(int x, int y);
		bool ContainsPoint(Vector2i position);
        string GetAreaName();
        ExpansionType GetExpansionType();
        //AreaToBuyData GetData();
        void IsoDestroy();
		int GetID();

	}
	[Serializable]
	public class RectWallInfo
	{
		public Rectangle rect;
		public IsoWallData data;
		public List<Vector3i> treePositions;
		public RectWallInfo(Rectangle Rect, IsoWallData Data, List<Vector3i> TreePositions)
		{
			rect = Rect;
			data = Data;
			treePositions = TreePositions;
		}

		public RectWallInfo(Area area, WallType innerWall,WallType outerWall)
		{
			rect = new Rectangle(area.position, area.size);
			if (area.doorPositions.Count == 0) {
				data = new IsoWallData (new Vector2i (0, 0), area.size, innerWall, outerWall, area.windowType, area.windowType, area.doorType, new List<int> () {area.doorPosition }, area.windowPositions, area.doorPathTypes);
			} else {
				data = new IsoWallData (new Vector2i (0, 0), area.size, innerWall, outerWall, area.windowType, area.windowType, area.doorType, area.doorPositions, area.windowPositions, area.doorPathTypes);
			}
			treePositions = area.trees;
		}
		public static RectWallInfo GenerateRectWallInfo(Vector2i position, Vector2i size,int doorID,List<int> doorPosition, WallType innerWallID, WallType outerWallID, int innerWindowID,int outerWindowID,List<int> windowPositions, List<Vector3i> treePositions =null)
		{
			Rectangle temp = new Rectangle(position, size);
			IsoWallData tempo = new IsoWallData(new Vector2i(0, 0), size, innerWallID, outerWallID, innerWindowID, outerWindowID, doorID, doorPosition, windowPositions);
			return new RectWallInfo(temp, tempo,treePositions);
		}

		public Vector3 GetPosition()
		{
			return new Vector3 (rect.x, 0, rect.y);
		}
	}
}
