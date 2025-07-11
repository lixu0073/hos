using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PathCounter : MonoBehaviour {
	public RoomCharacterInfo Room;
	public IsoEngine.IsoObjectPrefabData roomData;
	private List<IsoEngine.Vector2i>[,] AllCombinations;
	private List<IsoEngine.Vector2i> ListOfPathPoints;
	private bool[,] roomTileData;
	private int[,] distance;
	private bool[,] closed;
	private IsoEngine.Vector2i[,] previous;


	void Start()
	{
		Room.RotationesCornurs.Add (new IsoEngine.Vector2i (0, 0));
		Room.RotationesCornurs.Add (new IsoEngine.Vector2i (0, roomData.tilesY-1));
		Room.RotationesCornurs.Add (new IsoEngine.Vector2i (roomData.tilesX-1, roomData.tilesY-1));
		Room.RotationesCornurs.Add (new IsoEngine.Vector2i (roomData.tilesX-1, 0));
		if(roomData != null){
		ListOfPathPoints = Room.ListOfPathPoints;
		AllCombinations = new List<IsoEngine.Vector2i>[ListOfPathPoints.Count, ListOfPathPoints.Count];

		roomTileData = new bool[roomData.tilesX, roomData.tilesY];

		distance = new int[roomData.tilesX, roomData.tilesY];
		previous = new IsoEngine.Vector2i[roomData.tilesX, roomData.tilesY];
		closed = new bool[roomData.tilesX, roomData.tilesY];

		for (int i=0; i< roomData.tilesX; ++i) {
			for (int j=0; j<roomData.tilesY; ++j) {
				roomTileData [i, j] = roomData.tilesData [i * roomData.tilesY + j].isPassable;
			}
		}

		for (int i=0; i< ListOfPathPoints.Count; ++i) {
			for (int j=0; j<ListOfPathPoints.Count; ++j) {
				AllCombinations[i,j] = new List<IsoEngine.Vector2i>();
			}
		}

		for (int i=0; i< ListOfPathPoints.Count; ++i) {
			for(int j=0; j< ListOfPathPoints.Count; j++) {
				IsoEngine.PathInfo tempPath = GetPath(ListOfPathPoints[i], ListOfPathPoints[j]);
				Room.ListOfPaths.Add(tempPath);
			}
		}
		}
	}

	#region AStar
	public IsoEngine.PathInfo GetPath(IsoEngine.Vector2i From, IsoEngine.Vector2i To)
	{
		//A* pathfinding
		int i = roomTileData.GetLength(0);
		int j = roomTileData.GetLength(1);
		
		for (int ii = 0; ii < i; ii++)
			for (int jj = 0; jj < j; jj++)
		{
			closed[ii, jj] = false;
			distance[ii, jj] = int.MaxValue;
		}
		
		List<IsoEngine.Vector2i> open = new List<IsoEngine.Vector2i>();
		distance[From.x, From.y] = 0;
		open.Add(From);
		IsoEngine.Vector2i found = open[0];
		int dist;
		while (open.Count > 0)
		{
			found = open[0];
			
			int foundDistance = int.MaxValue;
			foreach (IsoEngine.Vector2i p in open)
			{
				if (closed[p.x, p.y])
					continue;
				dist = distance[p.x, p.y] + HowFar(p, To);
				if (dist <= foundDistance)
				{
					if (HowFar(p, To) < HowFar(found, To))
					{
						found = p;
						foundDistance = dist;
					}
					else if (HowFar(p, To) < HowFar(found, To))
					{
						found = p;
						foundDistance = dist;
					}
				}
			}
			
			if (found.x == To.x && found.y == To.y)
				break;
			open.Remove(found);
			closed[found.x, found.y] = true;
			foreach (var p in GetNeighbours(found))
			{
				if (!closed[p.x, p.y])
				{
					if (!open.Contains(p))
						open.Add(p);
					if (distance[p.x, p.y] == int.MaxValue ||
					    distance[p.x, p.y] > distance[found.x, found.y] + HowFar(found, p))
					{
						distance[p.x, p.y] = distance[found.x, found.y] + HowFar(found, p);
						previous[p.x, p.y] = found;
					}
				}
			}
			
			
		}
		if (!(found.x == To.x && found.y == To.y))
			return null;
		var path = new List<IsoEngine.Vector2i>();
		IsoEngine.Vector2i temp = To;
		while (true)
		{
			if (temp.x == From.x && temp.y == From.y)
				break;
			path.Add(temp);
			temp = previous[temp.x, temp.y];
		}
		IsoEngine.PathInfo pathe = new IsoEngine.PathInfo();
		path.Reverse();
		pathe.path = path;
		return pathe;
	}
	
	private IEnumerable<IsoEngine.Vector2i> GetNeighbours (IsoEngine.Vector2i elem)
	{
		if (Exists (elem.x, elem.y + 1) && roomTileData [elem.x, elem.y + 1])
			yield return new IsoEngine.Vector2i (elem.x, elem.y + 1);
		if (Exists (elem.x + 1, elem.y) && roomTileData [elem.x + 1, elem.y])
			yield return new IsoEngine.Vector2i (elem.x + 1, elem.y);
		if (Exists (elem.x, elem.y - 1) && roomTileData [elem.x, elem.y - 1])
			yield return new IsoEngine.Vector2i (elem.x, elem.y - 1);
		if (Exists (elem.x - 1, elem.y) && roomTileData [elem.x - 1, elem.y])
			yield return new IsoEngine.Vector2i (elem.x - 1, elem.y);
		yield break;
		
	}
	
	private bool Exists (int x, int y)
	{
		return (x >= 0 && y >= 0 && x < roomData.tilesX && y < roomData.tilesY);
	}
	
	private int HowFar (IsoEngine.Vector2i from, IsoEngine.Vector2i to)
	{
		int minDiff = Mathf.Min (Mathf.Abs (to.x - from.x), Mathf.Abs (to.y - from.y));
		return (Mathf.Max (Mathf.Abs (to.x - from.x), Mathf.Abs (to.y - from.y)) - minDiff) * 2 + 3 * minDiff;
	}
	#endregion


}
