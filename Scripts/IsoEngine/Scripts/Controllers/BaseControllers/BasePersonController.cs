using UnityEngine;
using System.Collections.Generic;

namespace IsoEngine
{
	public class BasePersonController : ComponentController, IPathRequester, ILoadable
	{
		// Fields
		private int x;
		private int y;
		private int levelID;

		protected bool ListeningForPath;

		private bool isInterior;
		private Animator animator;
		private StateManager stateManager;
		private RecievedPathState nextState;
		private QueueableSpot.SpotData currentSpot;
		private PFLevelController.TilePersons[] neighbourTiles;

		// Properties
		public bool IsCreated
		{
			get;
			private set;
		}

		public int PrefabID
		{
			get;
			private set;
		}

		public int LevelID
		{
			get
			{
				return levelID;
			}

			private set
			{
				if (levelID == value)
					return;

				int oldLevelID = levelID;
				levelID = value;
				//engineController.GetMap<PFMapController>().UpdatePersonLevel(this, oldLevelID);
			}
		}

		public int X
		{
			get
			{
				return x;
			}

			private set
			{
				if (x == value)
					return;

				int oldX = x;
				x = value;
				//engineController.GetMap<PFMapController>().UpdatePersonPosition(this, oldX, y);
			}
		}

		public int Y
		{
			get
			{
				return y;
			}

			private set
			{
				if (y == value)
					return;

				int oldY = y;
				y = value;
				//engineController.GetMap<PFMapController>().UpdatePersonPosition(this, x, oldY);
			}
		}

		public bool IsInterior
		{
			get
			{
				return isInterior;
			}

			protected set
			{
				if (isInterior == value)
					return;

				isInterior = value;
				engineController.Map.GetLevel<PFLevelController>(LevelID).UpdatePersonInterior(this);
			}
		}

		public bool IsLoaded
		{
			get;
			private set;
		}



		// Methods
		public virtual void CreatePerson(IsoPersonData personData)
		{
			if (IsCreated)
				throw new IsoException("Person is already created");

			this.LevelID = personData.levelID;

			this.X = personData.x;
			this.Y = personData.y;

			this.PrefabID = personData.prefabID;

			var level = engineController.Map.GetLevel(LevelID);

			this.IsLoaded = false;
			this.nextState = null;
			this.currentSpot = null;

			this.ListeningForPath = true;

			this.transform.position = new Vector3(level.X + X, level.transform.position.y, level.Y + Y);

			this.stateManager = new StateManager();

			this.animator = GetComponent<Animator>();

			this.neighbourTiles = new PFLevelController.TilePersons[0];

			this.IsCreated = true;
		}

		public virtual void Load()
		{
			if (IsLoaded)
				throw new IsoException("Person is already loaded");

			// TO DO
			gameObject.GetComponentInChildren<Renderer>().enabled = true;
			gameObject.GetComponent<Collider>().enabled = true;

			IsLoaded = true;
		}

		public virtual void Unload()
		{
			if (!IsLoaded)
				throw new IsoException("Person is already unloaded");

			// TO DO
			gameObject.GetComponentInChildren<Renderer>().enabled = false;
			gameObject.GetComponent<Collider>().enabled = false;

			IsLoaded = false;
		}

		public override void IsoDestroy()
		{
			if (!IsCreated)
				throw new IsoException("Person is already destroyed");

			ListeningForPath = false;

			IsCreated = false;
		}

		public virtual IsoPersonData GetData()
		{
			IsoPersonData data = new IsoPersonData();

			data.levelID = LevelID;
			data.x = X;
			data.y = Y;
			data.prefabID = PrefabID;
			data.directionX = (int)transform.forward.x;
			data.directionY = (int)transform.forward.z;

			data.controller = typeof(BasePersonController);

			return data;
		}

		protected virtual void ReachedDestinationNotification()
		{

		}

		protected virtual void Update()
		{
			stateManager.Update();
		}

		public void UpdatePosition()
		{
			var level = engineController.Map.GetLevel(LevelID);

			X = (int)(transform.position.x + 0.5f) - level.X;
			Y = (int)(transform.position.z + 0.5f) - level.Y;

			neighbourTiles = engineController.Map.GetLevel<PFLevelController>(LevelID).GetNeighbourTiles(X, Y);
		}



		public void GoTo(QueueableSpot.SpotData spot)
		{
			currentSpot = spot;
			engineController.GetMap<PFMapController>().OrderPath(this, LevelID, new Vector2i(X, Y), spot.LevelID, new Vector2i(spot.X, spot.Y), Hospital.PathType.Default);
		}

		public void StopMovement()
		{
			if (stateManager.State is RecievedPathState)
				stateManager.State = null;
		}

		public void SetPath(InterLevelPathInfo path)
		{
			if (!ListeningForPath)
				return;

            engineController.AddTask(delegate()
			{
				if (this == null || gameObject == null)
					return;

				if (stateManager.State == null)
				{
					stateManager.State = new RecievedPathState(this, path, currentSpot);
				}
				else
				{
					nextState = new RecievedPathState(this, path, currentSpot);
				}

				//if (path == null)
					//Debug.LogError("Path not found");
			});
		}


		private Vector3 MovementVector = new Vector3(0, 0, 1);
		private Vector3 LookingVector = new Vector3(0, 0, 1);

		#region States

		public class RecievedPathState : IState
		{
			private BasePersonController parent;

			private InterLevelPathInfo pathInfo;
			private QueueableSpot.SpotData spot;

			public virtual string SaveToString()
			{
				return "";
			}
			public RecievedPathState(BasePersonController parent, InterLevelPathInfo pathInfo, QueueableSpot.SpotData spot)
			{
				this.parent = parent;
				this.pathInfo = pathInfo;
				this.spot = spot;
			}

			private int closestTile = 0;

			public void OnEnter()
			{

				if (pathInfo == null)
				{
					parent.transform.LookAt(parent.transform.position + new Vector3(spot.Direction.x, 0, spot.Direction.y), Vector3.up);
					parent.ReachedDestinationNotification();
					parent.stateManager.State = null;
					return;
				}

				if(pathInfo.paths[0].path.Count == 1)
				{
					parent.transform.LookAt(parent.transform.position + new Vector3(spot.Direction.x, 0, spot.Direction.y), Vector3.up);
					parent.ReachedDestinationNotification();
					parent.stateManager.State = null;
					return;
				}

				
				float minDist = 0;
				closestTile = currentTile = 0;
				int closestLevel = currentLevel = 0;

				while (currentLevel < pathInfo.paths.Count)
				{
					Vector2i tile = pathInfo.paths[currentLevel].path[currentTile];
					//BaseIsoLevelController level = parent.engineController.Map.GetLevel(pathInfo.paths[currentLevel].lvlID);

					float dist = (new Vector2(tile.x - parent.X, tile.y - parent.Y)).magnitude;
					if (dist < 1.5f)
					{
						minDist = dist;
						closestTile = currentTile;
						closestLevel = currentLevel;
					}

					currentTile += 1;
					if (currentTile >= pathInfo.paths[currentLevel].path.Count)
					{
						currentTile = 0;
						currentLevel += 1;
					}
				}

				if (minDist == 0)
				{
					closestTile = 0;
					currentTile = 0;
					currentLevel = 0;
				}
				else
				{
					currentTile = closestTile;
					currentLevel = closestLevel;
				}

			//	currentTile = 0;
			//	currentLevel = 0;

				CreateCurve();

				//nextPoint = curve.GetPoint(currentTile - closestTile, 0.0f);

				elapsedTime = 0.0f;
				//timeLimit = curve.GetSegment(currentTile - closestTile).Length / movingSpeed;
				travelPercent = 0.0f;

				//parent.animator.SetFloat("Speed", 0.1f);
			}

			private int currentTile;
			private int currentLevel;
			private void CreateCurve()
			{
				BaseIsoLevelController level = parent.engineController.Map.GetLevel(pathInfo.paths[currentLevel].lvlID);

				Vector3[] points = new Vector3[pathInfo.paths[currentLevel].path.Count - closestTile + 1];
				Vector3 lastPosition;
				Vector3 position = parent.transform.position;

				points[0] = parent.transform.position;

				for(int i = closestTile; i < pathInfo.paths[currentLevel].path.Count; ++i)
				{
					Vector2i tile = pathInfo.paths[currentLevel].path[i];

					lastPosition = position;
					position = new Vector3(tile.x + level.X, level.transform.position.y, tile.y + level.Y);
					
					Vector3 direction = position - lastPosition;
					Vector3 offset = Quaternion.LookRotation(direction) * Quaternion.AngleAxis(BaseGameState.RandomFloat(0,1) * 90 - 45, Vector3.up) * new Vector3(BaseGameState.RandomFloat(0, 1) * 0.15f + 0.2f, 0, 0);

					points[i - closestTile + 1] = position + offset;
				}

				//curve = new BezierCurve(points[0] - parent.LookingVector, points, points[points.Length - 1] + new Vector3(spot.Direction.x, 0, spot.Direction.y), 0.2f, true);
			}

			public void OnExit()
			{
				if (parent.nextState == null)
					parent.animator.SetFloat("Speed", 0.0f);
			}

			public void Notify(int id, object parameters)
			{
			}



			private float travelPercent;
			private float elapsedTime;
			private float timeLimit=1;
			//private float movingSpeed = 1;

			private Vector3 point;
			private Vector3 nextPoint=Vector3.one;

			public void OnUpdate()
			{
				if(travelPercent < 1.0f)
				{
					elapsedTime += Time.deltaTime;
					travelPercent = elapsedTime / timeLimit;

					point = nextPoint;
					//nextPoint = curve.GetPoint(currentTile - closestTile, travelPercent);

					parent.transform.position = point;

					parent.LookingVector = (nextPoint - point).normalized;
					parent.transform.LookAt(nextPoint);

					parent.UpdatePosition();
				}

				if(travelPercent >= 1.0f)
				{
					elapsedTime -= timeLimit;
					
					if (parent.nextState != null)
					{
						parent.stateManager.State = parent.nextState;
						parent.nextState = null;
						return;
					}

					currentTile += 1;
					if (currentTile >= pathInfo.paths[currentLevel].path.Count)
					{
						currentTile = 0;
						closestTile = 0;
						currentLevel += 1;

						if (currentLevel >= pathInfo.paths.Count)
						{
							parent.transform.LookAt(parent.transform.position + new Vector3(spot.Direction.x, 0, spot.Direction.y), Vector3.up);
							parent.ReachedDestinationNotification();
							parent.stateManager.State = null;
							return;
						}

						CreateCurve();
					}

					//timeLimit = curve.GetSegment(currentTile - closestTile).Length / movingSpeed;
					travelPercent = elapsedTime / timeLimit;

					//nextPoint = curve.GetPoint(currentTile - closestTile, travelPercent);
				}

			}
		}

		#endregion States
	}
}