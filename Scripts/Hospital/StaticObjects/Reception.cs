using UnityEngine;
using System.Collections.Generic;
using System;
using IsoEngine;
using SimpleUI;
using TutorialSystem;

namespace Hospital
{
    public class Reception : SuperObject
    {
        bool initialized = false;
        private Vector2i position;
        //int wallID=0;
        public GameObject entrance;
        private GameObject go;
        public List<Vector2i> spots;
        public bool IsClickable = false;
        public bool isBusy = false;

        public GameObject receptionist;

        [SerializeField] private Vector2i entranceSpot = Vector2i.zero;
        public Vector2i EntranceSpot
        {
            get
            {
                return position + entranceSpot;
            }
        }
        [SerializeField] private Vector2i chairPos = Vector2i.zero;

        // spots on chairs
        private List<IsoObjectPrefabData.SpotData> waitingChairSpots;
        //private bool[] spotsChairTaken;
        private List<bool> spotsChairTaken = new List<bool>();
        // spots in queue
        private List<IsoObjectPrefabData.SpotData> waitingQueueSpots;
        // private bool[] spotsQueueTaken;
        private List<bool> spotsQueueTaken = new List<bool>();

        public List<Level> levelPrefabs = null;
        private List<ClinicPatientAI> patientsInReception;
        public List<BasePatientAI> waitingForChairSpot;
        [SerializeField] private GameObject impassable3x2 = null;
        [SerializeField] private GameObject impassable2x1 = null;
        [SerializeField] private GameObject impassable1x2 = null;
        [SerializeField] private GameObject impassable15x1 = null;
        [SerializeField] private GameObject trueImpassable1x2 = null;
        public int impassable3x2ID;
        public int impassable1x2ID;
        public int impassable2x1ID;
        public int impassable15x1ID;
        public int trueImpassable1x2ID;

        [SerializeField] int[] unlockNumbers = null;

        private bool notFirst = true;
        [SerializeField] public bool canSpawnPatients = true;
        public bool CanSpawnPatients
        {
            get;
            private set;
        }

        [TutorialTrigger]
        public event EventHandler receptionClicked;


        public void Initialize(Vector2i position)
        {
            this.position = position;

            spotsChairTaken = new List<bool>();
            spotsQueueTaken = new List<bool>();

            var eng = ReferenceHolder.Get().engine;
            foreach (var p in levelPrefabs)
            {
                p.chairsID = eng.AddObjectToEngine(p.chairs);
            }
            impassable3x2ID = eng.AddObjectToEngine(impassable3x2);
            impassable1x2ID = eng.AddObjectToEngine(impassable1x2);
            impassable2x1ID = eng.AddObjectToEngine(impassable2x1);
            impassable15x1ID = eng.AddObjectToEngine(impassable15x1);
            trueImpassable1x2ID = eng.AddObjectToEngine(trueImpassable1x2);
            isBusy = false;

            //if (Game.Instance.gameState().GetHospitalLevel() > 1)
            //    IsClickable = true;
        }

        public override void IsoDestroy()
        {
            initialized = false;
            if (go != null)
                GameObject.Destroy(go);
            actualLevel = -1;
            if (waitingChairSpots != null)
                waitingChairSpots.Clear();

            if (waitingQueueSpots != null)
                waitingQueueSpots.Clear();
        }

        [TutorialTriggerable]
        public void SetClickable(bool clickable)
        {
            IsClickable = clickable;
        }

        [TutorialCondition]
        public bool AlreadyClicked() { return actualLevel > 0; }

        public override void OnClick()
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;
           
            if (UIController.get.drawer.IsVisible)
            {
                UIController.get.drawer.SetVisible(false);
                return;
            }
            if (UIController.get.FriendsDrawer.IsVisible)
            {
                UIController.get.FriendsDrawer.SetVisible(false);
                return;
            }

            Debug.Log("ReceptionOnClick(). Is clickable " + IsClickable + ". actual Level = " + actualLevel);
            if (IsClickable)
            {
                if (actualLevel == 0)
                {
                    Upgrade();
                    NotificationCenter.Instance.ReceptionBuilt.Invoke(new ReceptionBuiltEventArgs(this));
                    receptionClicked?.Invoke(this, null);
                }
                else if (actualLevel > 0)
                {
                    if (Game.Instance.gameState().GetHospitalLevel() > 2)
                    {
                        Debug.Log("Open Achievements PopUp");
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(position.x + 2, 0, position.y + 3), 1.0f, false);
                        StartCoroutine(UIController.getHospital.AchievementsPopUp.Open( () =>
                        {
                            SoundsController.Instance.PlayButtonClick2();
                            return;
                        }));
                    }
                    else
                    {
                        Debug.Log("Achievements Unlock on level 3");
                        MessageController.instance.ShowMessage(39);
                    }
                }
            }
            SoundsController.Instance.PlayButtonClick2();
        }

        public void Upgrade()
        {
            NotificationCenter.Instance.ReceptionBuilt.Invoke(new ReceptionBuiltEventArgs(this));
            if (actualLevel < 4)
                SetLevel(actualLevel + 1);
        }

        public Vector2i GetReceptionSpot()
        {
            if (spots == null || spots.Count < 1)
                throw new IsoException("Reception spots not initialized!");

            var i = BaseGameState.RandomNumber(((actualLevel - 1) / 2) + 1);
            if (i >= spots.Count)
                i = spots.Count - 1;

            isBusy = true;

            if (receptionist.transform.GetChild(0).GetComponent<Animator>() != null)
                receptionist.transform.GetChild(0).GetComponent<Animator>().SetBool("talk", true);

            return spots[i] + position;
        }

        public void FreeReceptionSpot()
        {
            isBusy = false;

            if (receptionist.transform.GetChild(0).GetComponent<Animator>() != null)
                receptionist.transform.GetChild(0).GetComponent<Animator>().SetBool("talk", false);
        }

        // ...
        // SPOTS ON CHAIRS FOR WAITING AFTER CHECK IN
        // ...

        public int GetWaitingSpotOnChairs(out Vector2i pos)
        {
            for (int i = 0; i < spotsChairTaken.Count; i++)
            {
                if (!spotsChairTaken[i])
                {
                    pos = new Vector2i(waitingChairSpots[i].x, waitingChairSpots[i].y) + chairPos + position;
                    spotsChairTaken[i] = true;
                    return i;
                }
            }
            pos = new Vector2i(-1, -1);
            return -1;
        }

        public Vector2i GetWaitingSpotOnChairs(int id)
        {
            if (id < 0 || id >= waitingChairSpots.Count || spotsChairTaken[id])
            {
                throw new IsoException("Wrong ID");
            }
            spotsChairTaken[id] = true;
            return new Vector2i(waitingChairSpots[id].x, waitingChairSpots[id].y) + chairPos + position;
        }

        public Vector2 GetWaitingSpotOnChairsDirection(int id)
        {
            return waitingChairSpots[id].direction;
        }

        public void ReturnTakenSpotOnChairs(int id)
        {
            if (id >= spotsChairTaken.Count || id < 0 || !spotsChairTaken[id])
                return;
            spotsChairTaken[id] = false;
            if (waitingForChairSpot.Count > 0)
            {
                var p = waitingForChairSpot[0];
                waitingForChairSpot.Remove(p);
                p.Notify((int)StateNotifications.ReceptionSpotFree, null);
            }
        }

        // ...
        // SPOTS IN QUEUE FOR WAITING BEFORE CHECK IN
        // ...

        public int GetWaitingSpotInQueue(out Vector2i pos) // fil in spot queue from end of queue
        {
            Vector2i tmp = Vector2i.zero;
            int takenID = -1;

            if (spotsQueueTaken != null && spotsQueueTaken.Count > 0)
            {
                for (int i = spotsQueueTaken.Count - 1; i >= 0; i--)
                {
                    if (!spotsQueueTaken[i])
                    {
                        tmp = new Vector2i(waitingQueueSpots[i].x, waitingQueueSpots[i].y);
                        takenID = i;
                    }
                    else if (tmp.x != -1 && tmp.y != -1)
                    {
                        if (takenID != -1)
                            spotsQueueTaken[takenID] = true;

                        pos = tmp;
                        return takenID;
                    }
                }

                if (takenID > -1)
                {
                    pos = tmp;
                    spotsQueueTaken[takenID] = true;
                    return takenID;
                }
                else
                {
                    pos = Vector2i.zero;
                    return -1;
                }
            }
            else
            {
                pos = Vector2i.zero;
                // Debug.LogError("Reception is close so there isn't reception queue");
                return -1;
            }
        }

        public bool IsWaitingQueueEmpty()
        {
            if (spotsQueueTaken != null && spotsQueueTaken.Count > 0)
            {
                if (!spotsQueueTaken[0]) return true;
            }

            return false;
        }

        public bool IsWaitingQueueFull() // fill in spot queue from end of queue
        {
            if (spotsQueueTaken != null)
            {
                foreach (bool spot in spotsQueueTaken)
                {
                    if (!spot) return false;
                }
            }
            return true;
        }

        public bool GetWaitingSpotInQueue(int id, out Vector2i pos)
        {
            if (id < 0 || id >= waitingQueueSpots.Count || spotsQueueTaken[id] == true)
            {
                pos = Vector2i.zero;
                return false;
            }

            TakeSpotInQueue(id);
            pos = new Vector2i(waitingQueueSpots[id].x, waitingQueueSpots[id].y);
            return true;
        }

        public Vector2 GetWaitingSpotInQueueDirection(int id)
        {
            if (id < 0 || id >= waitingQueueSpots.Count)
                return Vector2.zero;

            return waitingQueueSpots[id].direction;
        }

        public Vector2i GetWaitingSpotPosInQueueFromID(int id)
        {
            if (id < 0 || id >= waitingQueueSpots.Count)
            {
                return Vector2i.zero;
            }
            TakeSpotInQueue(id);
            return new Vector2i(waitingQueueSpots[id].x, waitingQueueSpots[id].y);
        }

        public void ReturnTakenSpotInQueue(int id)
        {
            if (id >= spotsQueueTaken.Count || id < 0 || !spotsQueueTaken[id])
                return;

            spotsQueueTaken[id] = false;
        }

        public void TakeSpotInQueue(int id)
        {
            if (spotsQueueTaken == null || id >= spotsQueueTaken.Count || id < 0)
                return;
            
            spotsQueueTaken[id] = true;
        }


        public void EnableSpawning()
        {
            //Debug.Log("Uruchomienie spawnowania ludzików");
            canSpawnPatients = true;
            notFirst = true;
        }

        public void CheckLevel(int achievementsDone)
        {
            if (actualLevel > -1 && actualLevel < unlockNumbers.Length)
            {
                if (achievementsDone == unlockNumbers[actualLevel])
                {
                    SetLevel(actualLevel += 1);
                }
            }
        }

        public override void SetLevel(int lvl, bool showParticles = true)
        {
            lvl = Mathf.Clamp(lvl, -1, levelPrefabs.Count - 1);
            base.SetLevel(lvl);
            if (lvl < 0 || lvl > levelPrefabs.Count - 1)
                throw new IsoException("Reception doesn't have such a lvl");
            if (!initialized)
            {
                actualLevel = lvl;
                initialized = true;
            }
            actualLevel = lvl;
            canSpawnPatients = lvl > 0 && notFirst;
            if (lvl > 0)
            {
                IsClickable = true;
                if (showParticles)
                {
                    var fp = (GameObject)Instantiate(ResourcesHolder.GetHospital().ParticleReceptionOpen, new Vector3(30f, -1f, 45f), Quaternion.Euler(0, 0, 0));
                    fp.transform.localScale = Vector3.one;
                    fp.SetActive(true);
                    SoundsController.Instance.PlayMagicPoof();
                }
            }

            if (go != null)
                GameObject.Destroy(go);
            go = (GameObject)Instantiate(levelPrefabs[lvl].desk, new Vector3(position.x + 2, 0, position.y + 5), Quaternion.Euler(0, 0, 0));

            receptionist = go.transform.GetChild(0).gameObject;

            MakeChairs(lvl);
        }

        private void MakeChairs(int lvl)
        {
            if (waitingChairSpots == null)
                waitingChairSpots = new List<IsoObjectPrefabData.SpotData>();

            if (waitingQueueSpots == null)
                waitingQueueSpots = new List<IsoObjectPrefabData.SpotData>();

            if (patientsInReception == null)
                patientsInReception = new List<ClinicPatientAI>();

            if (lvl == 0)
                return;

            if (waitingChairSpots.Count > 0)
            {
                GameObject.Destroy(ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().RemoveObjectAndGetGameObject(chairPos.x + position.x, chairPos.y + position.y));
                waitingChairSpots.Clear();
            }

            if (null == ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().AddObject(chairPos.x + position.x - 2, chairPos.y + position.y, levelPrefabs[lvl].chairsID))
            {
                Debug.Log("Error: " + levelPrefabs[lvl].chairsID);
                //throw new IsoException("Error: " + levelPrefabs[lvl].chairsID);
            }
            else
                Debug.LogFormat("<color=green>Reception.MakeChairs() - added chairs to map</color> " + levelPrefabs[lvl].chairsID);
            var actualData = ReferenceHolder.Get().engine.objects[levelPrefabs[lvl].chairsID].GetComponent<IsoObjectPrefabController>().prefabData;

            foreach (var p in actualData.spotsData)
            {
                if (p.id == (int)SpotTypes.CorridorChair)
                {
                    waitingChairSpots.Add(p);
                }
            }

            for (int i = 0; i < 25; i++)
            {
                IsoObjectPrefabData.SpotData newSpotForWait = new IsoObjectPrefabData.SpotData();

                if (i < 8)
                {
                    newSpotForWait.x = (EntranceSpot.x + 6) - i;
                    newSpotForWait.y = EntranceSpot.y + 1;
                    newSpotForWait.direction = new Vector2(1, 0);
                }
                else
                {
                    newSpotForWait.x = (EntranceSpot.x + 6) - 8;
                    newSpotForWait.y = EntranceSpot.y + 8 - i;
                    newSpotForWait.direction = new Vector2(0, -1);
                }

                waitingQueueSpots.Add(newSpotForWait);
                // Debug.LogWarning(i);
            }

            if (spotsChairTaken.Count < waitingChairSpots.Count)
            {
                for (int i = spotsChairTaken.Count; i < waitingChairSpots.Count; i++)
                {
                    spotsChairTaken.Add(false);
                }
            }

            if (spotsChairTaken.Count > waitingChairSpots.Count)
            {
                for (int i = spotsChairTaken.Count; i < waitingChairSpots.Count; i++)
                {
                    spotsChairTaken.RemoveAt(spotsChairTaken.Count - 1);
                }
            }
            //spotsChairTaken = new bool[waitingChairSpots.Count];
            if (spotsQueueTaken.Count < waitingQueueSpots.Count)
            {
                for (int i = spotsQueueTaken.Count; i < waitingQueueSpots.Count; i++)
                {
                    spotsQueueTaken.Add(false);
                }
            }

            if (spotsQueueTaken.Count > waitingQueueSpots.Count)
            {
                for (int i = spotsQueueTaken.Count; i < waitingQueueSpots.Count; i++)
                {
                    spotsQueueTaken.RemoveAt(spotsQueueTaken.Count - 1);
                }
            }

            foreach (var p in patientsInReception)
                p.Notify((int)StateNotifications.ReceptionChanged, null);
        }

        public GameObject GetImpassable()
        {
            return impassable2x1;

        }
    }

    [Serializable]
    public class Level
    {
        public GameObject desk;
        public GameObject chairs;
        public int chairsID;
    }
}
