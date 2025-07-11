using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

namespace IsoEngine
{
    /// <summary>
    /// More avanced version of mapController. Provides pathfinding implemented with A* on Tiles.
    /// </summary>
    public class PFMapController : BaseMapController
    {
        [Serializable]
        public class PFMapAdditionalData : IsoMapAdditionalData
        {
            public Vector2i elevatorEntry;


            public PFMapAdditionalData(IsoMapAdditionalData previousData)
                : base(previousData)
            {

            }
        }


        private Vector2i elevatorEntry;
        private Vector2i[] elevatorEntries;

        private bool PathfinderRunning;
        private Thread Pathfinder;
        private List<OrderInfo> orders;
        private System.Object ordersLock = new System.Object();


        #region METHODS

        /*
		public void UpdateObstruction(IObstructable item)
		{
			GetLevel<PFLevelController>(item.LevelID).RemoveSpotQueue(item);
			GetLevel<PFLevelController>(item.LevelID).AddSpotQueue(item);
		}
*/

        public void AddQueueSpot(QueueableSpot.SpotData spot)
        {
            GetLevel<PFLevelController>(spot.LevelID).AddSpotQueue(spot);
        }

        public void RemoveQueueSpot(QueueableSpot.SpotData spot)
        {
            GetLevel<PFLevelController>(spot.LevelID).RemoveSpotQueue(spot);
        }

        public override void IsoDestroy()
        {
            base.IsoDestroy();

            DisablePathfindingThread();
        }

        protected override Type GetIsoLevelControllerType()
        {
            return typeof(PFLevelController);
        }
        protected override bool CheckIsoLevelControllerType(Type isoLevelControllerType)
        {
            if (!base.CheckIsoLevelControllerType(isoLevelControllerType))
                return false;

            return typeof(PFLevelController).IsAssignableFrom(isoLevelControllerType);
        }

        public virtual void UpdatePatientPosition(Hospital.BasePatientAI patient, Vector2i old, Vector2i oldDest)
        {
            if (patient != null && GetLevel<PFLevelController>(0) != null)
            {
                GetLevel<PFLevelController>(0).UpdatePatientPosition(patient, old, oldDest);
            }
        }

        public virtual bool isAnyPersonOnTile(Vector2i pos)
        {
            if (GetLevel<PFLevelController>(0).isAnyPersonOnTile(pos))
                return true;
            else return false;
        }

        public virtual Hospital.BasePatientAI GetNearPatient(Vector2i pos, int range)
        {
            if (GetLevel<PFLevelController>(0) != null)
            {
                return GetLevel<PFLevelController>(0).GetNearPatient(pos, range);
            }
            else return null;
        }

        /// <summary>
        /// Creates map from data provided in IsoMapData object and starts pathfinding thread.
        /// </summary>
        /// <param name="mapData">Map will by created from data in this object</param>
        public override void CreateMap(IsoMapData mapData)
        {
            //print("creating map in pfmapcontroller. map size is: " + mapData.levelData[0].tileData.GetLength(0) + " " + mapData.levelData[0].tileData.GetLength(1));
            var additionalData = IsoMapAdditionalData.UnpackData<PFMapAdditionalData>(mapData);
            base.CreateMap(mapData);

            elevatorEntry = additionalData.elevatorEntry;

            int elevatorX = additionalData.elevatorEntry.x;
            int elevatorY = additionalData.elevatorEntry.y;

            elevatorEntries = new Vector2i[Levels.Length];

            // Run pathfinder
            PathfinderRunning = false;
            EnablePathFindingThread();

            for (int i = 0; i < Levels.Length; i++)
            {
                int x = elevatorX - Levels[i].X;
                int y = elevatorY - Levels[i].Y;

                if (Levels[i].IsPointInterior(x, y))
                {
                    elevatorEntries[i] = new Vector2i(elevatorX - Levels[i].X, elevatorY - Levels[i].Y);
                }
                else
                {
                    throw new IsoException("Elevator not inside level " + i);
                }
            }
        }

        public override IsoMapData GetData()
        {
            IsoMapData mapData = base.GetData();

            var additionalData = IsoMapAdditionalData.PackData<PFMapAdditionalData>(mapData);

            additionalData.elevatorEntry = elevatorEntry;

            return mapData;
        }
        /// <summary>
        /// Get path between tiles. Remember that this function IS NOT executed in pathfinding thread, but invoking thread.
        /// </summary>
        /// <param name="lvlFrom">ID of level of starting tile</param>
        /// <param name="xFrom"> X coordinate of starting tile</param>
        /// <param name="yFrom"> Y coordinate of starting tile</param>
        /// <param name="lvlTo">ID of level of ending tile</param>
        /// <param name="xTo"> X coordinate of ending tile</param>
        /// <param name="yTo"> Y coordinate of ending tile</param>
        /// <returns></returns>
        public InterLevelPathInfo GetPath(int lvlFrom, Vector2i fromPos, int lvlTo, Vector2i toPos, Hospital.PathType pathType, bool isPathForPatient)
        {
            InterLevelPathInfo temp = null;

            if (lvlFrom == lvlTo)
            {
                var path = GetLevel<PFLevelController>(lvlFrom).GetPath(fromPos, toPos, pathType, isPathForPatient);

                if (path != null)
                {
                    temp = new InterLevelPathInfo();
                    temp.paths.Add(path);
                }
            }
            else
            {
                var path1 = GetLevel<PFLevelController>(lvlFrom).GetPath(fromPos, elevatorEntries[lvlFrom], pathType, isPathForPatient);

                if (path1 != null)
                {
                    var path2 = GetLevel<PFLevelController>(lvlTo).GetPath(elevatorEntries[lvlTo], toPos, pathType, isPathForPatient);

                    if (path2 != null)
                    {
                        temp = new InterLevelPathInfo();
                        temp.paths.Add(path1);
                        temp.paths.Add(path2);
                    }
                }
            }

            return temp;
        }

        /// <summary>
        /// Disable running pathfinder. If there are batched orders for paths they will be lost!
        /// </summary>
        public void DisablePathfindingThread()
        {
            if (PathfinderRunning == true)
            {
                PathfinderRunning = false;
                Pathfinder.Join();
            }
        }

        /// <summary>
        /// Starts pathfinder. Use only if you used DisablePathfindingThread before.
        /// </summary>
        public void EnablePathFindingThread()
        {
            if (PathfinderRunning == false)
            {
                if (orders != null && orders.Count > 0)
                    orders.Clear();

                orders = new List<OrderInfo>();
                Pathfinder = new Thread(PathfinderThread) { IsBackground = true };
                PathfinderRunning = true;
                Pathfinder.Start();
            }
        }

        /// <summary>
        /// Make order or path between tiles. This function will be executed by another thread
        ///  and results wil be delivered by invoking method from IPathRequester. If pathfinder is disabled this function will do nothing.
        /// </summary>
        /// <param name="player">object that will receive path.</param>
        /// <param name="lvlFrom">lvlID of starting tile</param>
        /// <param name="from">coordinates of starting tile</param>
        /// <param name="lvlTo">lvlID of ending tile</param>
        /// <param name="to">coordinates of ending tile</param>
        public void OrderPath(IPathRequester player, int lvlFrom, Vector2i from, int lvlTo, Vector2i to, Hospital.PathType pathType, bool isPathForPatient = true)
        {
            if (PathfinderRunning)
                lock (ordersLock)
                {
                    // orders.Add(new OrderInfo(player, lvlFrom, from, lvlTo, to, pathType, isPathForPatient));
                    AddOrUpdateOrderPath(player, lvlFrom, from, lvlTo, to, pathType, isPathForPatient);
                }
        }

        public void AddOrUpdateOrderPath(IPathRequester player, int lvlFrom, Vector2i from, int lvlTo, Vector2i to, Hospital.PathType pathType, bool isPathForPatient = true)
        {
            bool exist = false;


            if (orders != null && orders.Count > 0)
            {
                for (int i = 0; i < orders.Count; i++)
                {
                    if (orders[i].player == player)
                    {
                        orders[i].lvlTo = lvlTo;
                        orders[i].to = to;
                        orders[i].pathType = pathType;
                        orders[i].isPathForPatient = isPathForPatient;

                        exist = true;
                    }
                }
            }

            if (exist == false)
                orders.Add(new OrderInfo(player, lvlFrom, from, lvlTo, to, pathType, isPathForPatient));
        }

        /// <summary>
        /// This function is runned by designed thread.
        /// </summary>
        private void PathfinderThread()
        {
            OrderInfo temp = null;

            while (PathfinderRunning)
            {
                lock (ordersLock)
                {
                    if (orders.Count > 0)
                    {
                        temp = orders[0];
                        orders.Remove(temp);
                    }
                }

                if (temp != null)
                {
                    try
                    {
                        InterLevelPathInfo info = GetPath(temp.lvlFrom, temp.from, temp.lvlTo, temp.to, temp.pathType, temp.isPathForPatient);
                        temp.player.SetPath(info);
                    }
                    catch (Exception e)
                    {
                        lock (ordersLock)
                        {
                            if (!orders.Contains(temp))
                                orders.Add(temp);
                        }

                        Debug.LogError("PathfinderThread stuck problem ! I tried to stop him. If your game loop then it's 'cuz of me!\n"
                            + e.Message + "\n" + e.StackTrace);
                        DisablePathfindingThread();
                        return;
                    }
                }
                else
                    Thread.Sleep(250);
                temp = null;
            }
        }

        #endregion

    }

    class OrderInfo
    {
        public int lvlFrom, lvlTo;
        public Vector2i from;
        public Vector2i to;
        public IPathRequester player;
        public Hospital.PathType pathType;
        public bool isPathForPatient;

        public OrderInfo(IPathRequester Player, int LvlFrom, Vector2i From, int LvlTo, Vector2i To, Hospital.PathType pathtype, bool isPathForPatient)
        {
            player = Player;
            lvlFrom = LvlFrom;
            lvlTo = LvlTo;
            from = From;
            to = To;
            pathType = pathtype;
            this.isPathForPatient = isPathForPatient;
        }
    }
}
