using UnityEngine;
using System.Collections.Generic;

namespace IsoEngine
{
	public class QueueableSpot : IsoElement
	{
		// Types
		public delegate void RemovedNotification();

		public class SpotData
		{
			public readonly int LevelID;
			public readonly int X;
			public readonly int Y;

			public readonly Vector2 Direction;

			public SpotData(int levelID, int x, int y, Vector2 direction)
			{
				this.LevelID = levelID;
				this.X = x;
				this.Y = y;
				this.Direction = direction;
			}
		}

		private class QueuedPerson
		{
			public SpotData spot;
			public BasePersonController person;

			public RemovedNotification onRemoved;

			public QueuedPerson(BasePersonController person, SpotData spot, RemovedNotification onRemoved)
			{
				this.person = person;
				this.spot = spot;
				this.onRemoved = onRemoved;
			}
		}



		// Fields
		private IsoObjectPrefabData.SpotData spotData;
		private PFLevelController levelController;
		private LinkedList<QueuedPerson> persons;



		// Properties
		public IsoObject IsoObject
		{
			get;
			private set;
		}

		public int LevelID
		{
			get
			{
				return IsoObject.LevelID;
			}
		}

		public int X
		{
			get
			{
				return spotData.x;
			}
		}

		public int Y
		{
			get
			{
				return spotData.y;
			}
		}

		public int ID
		{
			get
			{
				return spotData.id;
			}
		}

		public Vector2 Direction
		{
			get
			{
				return spotData.direction;
			}
		}

		public int PersonsCount
		{
			get
			{
				return persons.Count;
			}
		}

		public bool IsFree
		{
			get
			{
				return persons.Count == 0;
			}
		}



		// Methods
		internal QueueableSpot(EngineController engineController, IsoObject isoObject, IsoObjectPrefabData.SpotData spotData)
			: base(engineController)
		{
			this.IsoObject = isoObject;
			this.spotData = spotData;
			this.levelController = engineController.Map.GetLevel<PFLevelController>(isoObject.LevelID);
			this.persons = new LinkedList<QueuedPerson>();
		}

		public bool IsFirst(BasePersonController person)
		{
			if (persons.Count == 0)
				return false;

			return persons.First.Value.person == person;
		}

		public SpotData GetSpotData(BasePersonController person)
		{
			foreach (var node in persons)
			{
				if (node.person == person)
					return node.spot;
			}

			throw new IsoException("Person not found in queue");
		}

		public SpotData AddPerson(BasePersonController person, RemovedNotification onRemoved = null)
		{
			SpotData ret = null;

			// First person to queue
			if (persons.Count == 0)
			{
				ret = new SpotData(IsoObject.LevelID, IsoObject.X + X, IsoObject.Y + Y, Direction);

				var node = new QueuedPerson(person, ret, onRemoved);
				persons.AddLast(node);
			}
			else
			{
				var last = persons.Last;

				SpotData lastSpot = last.Value.spot;

				int dir = GameState.RandomNumber(1, 3);

				// Check all spaces around last spot
				for (int i = 0; i < 3; ++i)
				{
					Vector2 currentDirection = lastSpot.Direction;

					if (i > 0)
					{
						switch (dir)
						{
							case 1:
							currentDirection.x = lastSpot.Direction.y;
							currentDirection.y = -lastSpot.Direction.x;
							break;

							case 2:
							currentDirection.x = -lastSpot.Direction.y;
							currentDirection.y = lastSpot.Direction.x;
							break;

							default:
							break;
						}

						dir = 3 - dir;
					}



					int x = lastSpot.X - (int)currentDirection.x;
					int y = lastSpot.Y - (int)currentDirection.y;

					// Is free
					if (levelController.IsPointInBounds(x, y) && levelController.IsFree(x, y))
					{
						ret = new SpotData(IsoObject.LevelID, x, y, currentDirection);
						engineController.GetMap<PFMapController>().AddQueueSpot(ret);

						var node = new QueuedPerson(person, ret, onRemoved);
						persons.AddLast(node);

						break;
					}
				}
			}

			if (ret == null)
			{
				int x = X;
				int y = Y;

				if (FindClosestFreeTile(ref x, ref y))
				{
					ret = new SpotData(this.LevelID, x, y, new Vector2(x - X, y - Y));
					engineController.GetMap<PFMapController>().AddQueueSpot(ret);

					var node = new QueuedPerson(person, ret, onRemoved);
					persons.AddLast(node);
				}
				else
				{
					Debug.LogError("No spot found " + person.name + "\nfor object " + IsoObject.LevelID + " (" + IsoObject.X + ", " + IsoObject.Y + ")");
				}
			}

			return ret;
		}

		public void RemovePerson(BasePersonController person)
		{
			var node = persons.First;

			while (node != null)
			{
				if (node.Value.person == person)
					break;

				node = node.Next;
			}

			if (node == null)
				throw new IsoException("Person not found in queue");


			var nextNode = node.Next;

			if (persons.Last != persons.First)
				engineController.GetMap<PFMapController>().RemoveQueueSpot(persons.Last.Value.spot);

			while (nextNode != null)
			{
				node.Value.onRemoved = nextNode.Value.onRemoved;
				node.Value.person = nextNode.Value.person;
				node.Value.person.GoTo(node.Value.spot);

				node = nextNode;
				nextNode = nextNode.Next;
			}

			persons.RemoveLast();

			//	if (notification != null)
			//	{
			//		Debug.LogErrorFormat(person.name + " removed");
			//		notification();
			//	}
		}

		public void RemoveAllPersons()
		{
			var node = persons.First;

			if (node.Value.onRemoved != null)
				node.Value.onRemoved();

			for (node = node.Next; node != null; node = node.Next)
			{
				if (node.Value.onRemoved != null)
					node.Value.onRemoved();

				engineController.GetMap<PFMapController>().RemoveQueueSpot(node.Value.spot);
			}

			persons.Clear();
		}

		private bool FindClosestFreeTile(ref int x, ref int y)
		{
			int px = 0;
			int py = 0;

			bool isInterior = levelController.IsPointInterior(x, y);
			bool perform = true;
			bool tmp;

			for (int dist = 1; perform; ++dist)
			{
				perform = false;

				for (int i = -dist; i < dist; ++i)
				{
					for (int j = 0; j < 4; ++j)
					{
						switch (j)
						{
							case 0:
							px = x - dist;
							py = y + i;
							break;

							case 1:
							px = x + i;
							py = y + dist;
							break;

							case 2:
							px = x + dist;
							py = y - i;
							break;

							case 3:
							px = x - i;
							py = y - dist;
							break;
						}
					}

					tmp = levelController.IsPointInBounds(px, py);
					if (tmp && levelController.IsFree(px, py) && levelController.IsPointInterior(px, py) == isInterior)
					{
						x = px;
						y = py;
						return true;
					}

					perform = (perform || tmp);
				}
			}

			return false;
		}
	}
}