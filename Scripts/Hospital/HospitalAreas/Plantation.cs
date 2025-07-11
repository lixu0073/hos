using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using IsoEngine;


namespace Hospital
{
    public class Plantation : SuperObjectWithVisiting
    {
        #region fields
        [SerializeField] GameObject PlantationPatchPrefab = null;
        public GameObject gardenerPrefab;

        [SerializeField] Transform pipeStart = null;
        [SerializeField] Transform longPipeStart = null;
        [SerializeField] Transform longPipeEnd = null;
        [SerializeField] Transform longPipe = null;
        [SerializeField] int startRows = 2;
        [SerializeField] public List<int> unlockLevels = null;
        [SerializeField] private int rowWidth = 0;

        [SerializeField] private Transform fence = null;
        [SerializeField] private GameObject span = null;
        [SerializeField] private GameObject frontFence = null;
        [SerializeField] private Transform thicket = null;
        [SerializeField] private GameObject hearts = null;

        [HideInInspector]
        public PlantationPatch[,] patches;
        public static Animator gardenerAnimator;
        private int plantationLevel = 0;

        public int maxRegrowth = 3;
        public int harvestAmount = 3;
        #endregion

        public int PlantationLevel
        {
            get { return plantationLevel; }
            private set
            {
                plantationLevel = value;
                if (plantationLevel > 0)
                {
                    for (int i = 0; i < plantationLevel; ++i)
                    {
                        if (patches[1, i] == null)
                        {
                            GeneratePlantationRow(i);
                            GenerateDefault(i);
                            ActualizeFrontFence(i);
                        }

                        for (int j = 0; j < rowWidth; j++)
                        {
                            //Debug.Log ("Enabling");
                            if (patches[j, i].PatchState == EPatchState.disabled)
                                patches[j, i].PatchState = EPatchState.empty;
                        }
                    }
                    //longPipeStart.gameObject.SetActive(true);
                }
                else
                {
                    longPipeStart.gameObject.SetActive(false);
                    longPipeEnd.gameObject.SetActive(false);
                }

                for (int i = 0; i < plantationLevel; i++)
                {
                    longPipe.GetChild(i).gameObject.SetActive(true);
                    longPipe.GetChild(i).GetChild(1).gameObject.SetActive(true);
                }

                pipeStart.localPosition = new Vector3(-1.66f - 1.269f * plantationLevel, -1.50f - 0.897f * plantationLevel, -2.9f - 0.897f * plantationLevel);

                for (int i = plantationLevel; i < longPipe.childCount; i++)
                {
                    longPipe.GetChild(i).gameObject.SetActive(false);
                }

                if (plantationLevel > startRows)
                {
                    for (int i = 0; i < plantationLevel - startRows; i++)
                    {
                        thicket.GetChild(i).gameObject.SetActive(false);
                    }
                    for (int i = plantationLevel - startRows; i < thicket.childCount; i++)
                    {
                        thicket.GetChild(i).gameObject.SetActive(true);
                    }
                }
                else
                {
                    for (int i = 0; i < thicket.childCount; i++)
                    {
                        thicket.GetChild(i).gameObject.SetActive(true);
                    }
                }
            }
        }

        [HideInInspector]
        public Vector2 plantationMaxSize;
        [HideInInspector]
        public Vector2 actualPlantationSize;

        public static bool HaveHelpRequests;
        public delegate void OnHaveHelpRequestChanged();
        public static event OnHaveHelpRequestChanged UpdateHelpRequests;

        private void Awake()
        {
            plantationMaxSize = new Vector2(rowWidth, unlockLevels.Count);
            patches = new PlantationPatch[(int)plantationMaxSize.x, (int)plantationMaxSize.y];
        }

        void Start()
        {
            HaveHelpRequests = false;
            //GameState.OnLevelUp += enableOnLevelUp;
            //int x = unlockLevels.Count;
            actualPlantationSize = new Vector2(rowWidth, 0);
            for (int i = 0; i < startRows; i++)
            {
                GeneratePlantationRow(i);
                GenerateDefault(i);
            }

            ActualizeFrontFence((int)actualPlantationSize.y - 1);

            /*for (int i = 0; i < plantationSize.x / 2; i++)
				for (int j = 0; j < plantationSize.y; j++)
					InstantiatePatch(i, j);
			for (int i = (int)plantationSize.x / 2; i < (int)plantationSize.x; i++)
				for (int j = 0; j < plantationSize.y; j++)
					InstantiatePatch(i, j, true);
			PlantationLevel = 0;
			pipeStart.gameObject.SetActive(true);*/
        }

        private void InstantiatePatch(int i, int j, bool second = false)
        {
            var p = GameObject.Instantiate(PlantationPatchPrefab);
            p.transform.position = transform.position + new Vector3(13.85f - j * 1.75f, 0, i * 1.6f + (second ? 2.2f : 1f));
            p.transform.SetParent(transform);
            patches[i, j] = p.GetComponent<PlantationPatch>();
            patches[i, j].position = new Vector2i(i, j);
            patches[i, j].PatchState = EPatchState.disabled;
        }

        public override void IsoDestroy()
        {
            //not implemented
            PlantationManager.Instance.unbindAllCallbacks();
            PlantationLevel = 0;
        }

        public override void OnClick()
        {
            Debug.LogError("Plantation Clicked");
        }

        void OnDestroy()
        {
            GlobalEventNotificationCenter.Instance.OnEventStart.Notification -= OnEventStart_Notification;
            GlobalEventNotificationCenter.Instance.OnEventEnd.Notification -= OnEventEnd_Notification;
            PlantationManager.Instance.unbindAllCallbacks();
        }

        public void LoadFromStringList(List<string> saveList, TimePassedObject timeFromSave, bool visitingMode = false)
        {
            //preload cleaning
            for (int j = 0; j < actualPlantationSize.y; j++)
            {
                for (int i = 0; i < plantationMaxSize.x; i++)
                {
                    if (patches[i, j] != null)
                    {
                        
                        Destroy(patches[i, j].gameObject);
                        patches[i, j] = null;
                    }
                }
                Destroy(fence.GetChild(2 + 2 * j).gameObject);
                Destroy(fence.GetChild(2 + 2 * j + 1).gameObject);
            }

            actualPlantationSize.y = 2;
            ActualizeFrontFence((int)actualPlantationSize.y - 1);

            int plantLvl = 0;
            if (saveList != null)
            {
                int c = Math.Min(saveList.Count(), patches.GetLength(1));
                for (int j = 0; j < c; j++)
                {
                    if (saveList == null || string.IsNullOrEmpty(saveList[j]))
                    {
                        GeneratePlantationRow(j);
                        actualPlantationSize.y = 2;
                        GenerateDefault(j);
                        return;
                    }

                    if (patches[1, j] == null)
                    {
                        GeneratePlantationRow(j);
                        ActualizeFrontFence(j);
                        GenerateDefault(j);
                    }

                    var row = saveList[j].Split(';');
                    bool isRowEnabled = false;
                    int r = Math.Min(row.Length, patches.GetLength(0));
                    for (int i = 0; i < row.Length; i++)
                    {
                        var rowPatch = row[i].Split('/');//medicine, timefromseed, regrowthleft, harvestleft, state
                        if (rowPatch[0] != "null")
                            patches[i, j].GrowingPlant = MedicineRef.Parse(rowPatch[0]);
                        else                        
                            patches[i, j].GrowingPlant = null;

                        try
                        {
                            patches[i, j].TimeFromSeed = Mathf.Max(int.Parse(rowPatch[1], System.Globalization.CultureInfo.InvariantCulture) + timeFromSave.GetTimePassed(), 0);
                        }
                        catch (Exception)
                        {
                            Debug.LogError("There was an error with time in Plantation so i generated defaults");
                            patches[i, j].TimeFromSeed = 1000000;
                        }

                        patches[i, j].RegrowthLeft = (int)Mathf.Clamp(int.Parse(rowPatch[2], System.Globalization.CultureInfo.InvariantCulture), -1, maxRegrowth);
                        patches[i, j].HarvestLeft = int.Parse(rowPatch[3], System.Globalization.CultureInfo.InvariantCulture);

                        EPatchState loadedState = (EPatchState)Enum.Parse(typeof(EPatchState), rowPatch[4]);
                        if (loadedState == EPatchState.waitingForHelp)
                        {
                            if (patches[i, j].GrowingPlant == null)
                            {
                                Debug.LogError("PlantationPath has waitingForHelp state and GrowingPlant is null so i validate it");
                                loadedState = EPatchState.empty;
                            }
                        }
                        patches[i, j].PatchState = loadedState;
                        patches[i, j].InitHelp(visitingMode);
                        if (patches[i, j].PatchState != EPatchState.disabled)
                            isRowEnabled = true;
                    }
                    if (isRowEnabled)
                        plantLvl++;
                }
            }
            else
            {
                for (int i = 0; i < actualPlantationSize.y; i++)
                {
                    GeneratePlantationRow(i);
                    actualPlantationSize.y = 2;
                    GenerateDefault(i);
                }
            }

            if (!visitingMode)
                RefreshMyNewHelpRequests();
            else
                PlantationManager.Instance.GetUserRequests(SaveLoadController.SaveState.ID, OnForeignHelpGet);

            PlantationLevel = GetPlantationLevel();

            EnableGardener();
        }

        public void RefreshMyNewHelpRequests()
        {
            PlantationManager.Instance.GetMyRequests(OnPersonalHelpGet);
        }

        public void EmulateTime(TimePassedObject timePassed)
        {
            for (int j = 0; j < actualPlantationSize.y; j++)
            {
                for (int i = 0; i < plantationMaxSize.x; i++)
                {
                    if (patches[i, j] != null)
                        patches[i, j].EmulateTime(timePassed);
                }
            }
        }

        private void ShowInfoBadge(HelpRequest request, PlantationPatch patch)
        {
            //patch.ShowPlayerHelpBadge("Mariusz", 1, null);
            //return;
            if (request == null || request.user == null || patch == null)
            {
                Debug.LogError("Wrong Params: user or patch or request == null");
                return;
            }
            patch.ShowPlayerHelpBadge(request.user.Name, request.user.Level, request.user.FacebookID);
        }

        private void UpdateLostPatch(PlantationPatch patch)
        {
            patch.UpdateMissingData();
        }

        public void DeleteHelpRequest(string placeID)
        {
            // TO USE
            // placeID = request.PlaceID
            PlantationManager.Instance.DeleteHelpRequest(placeID, () =>
            {
                Debug.Log("Success HelpRequest Deletion");
            }, (ex) =>
            {
                Debug.LogError(ex.Message);
            });
        }

        private void OnPersonalHelpGet(List<HelpRequest> requests)
        {
            // Get all patchs with help request from save
            List<Vector2i> patchWithHelpIdsFromSave = new List<Vector2i>();

            for (int i = 0; i < plantationLevel; ++i)
            {
                for (int j = 0; j < rowWidth; j++)
                {
                    if (patches[j, i] != null && patches[j, i].PatchState == EPatchState.waitingForHelp)
                        patchWithHelpIdsFromSave.Add(patches[j, i].position);
                }
            }

            // Get all patchs with help request from aws
            bool haveHelpRequest = false;
            foreach (var request in requests)
            {
                Vector2i vec = Vector2i.Parse(request.PlaceID);
                //check if there are any plantation patches with helprequest on server but are not listed on cliend side
                if (!patchWithHelpIdsFromSave.Contains(vec))
                {
                    //Update plantation patches helprequest status according to data on server
                    UpdateLostPatch(patches[vec.x, vec.y]);
                    haveHelpRequest = true;
                    continue;
                }

                if (request.helped)
                {
                    if (patchWithHelpIdsFromSave.Contains(vec))
                        patchWithHelpIdsFromSave.Remove(vec);

                    ShowInfoBadge(request, patches[vec.x, vec.y]);
                    patches[vec.x, vec.y].SeedHelped();
                    DeleteHelpRequest(request.PlaceID);
                    try
                    {
                        LastHelpersProvider.Instance.AddLastHelper(request.ByWhom);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
                else
                {
                    if (patchWithHelpIdsFromSave.Contains(vec))
                        patchWithHelpIdsFromSave.Remove(vec);

                    haveHelpRequest = true;
                }
            }

            // Remove all patches with help request in save without help request on server
            if (patchWithHelpIdsFromSave != null && patchWithHelpIdsFromSave.Count > 0)
            {
                for (int i = 0; i < patchWithHelpIdsFromSave.Count; i++)
                    patches[patchWithHelpIdsFromSave[i].x, patchWithHelpIdsFromSave[i].y].CancelHelpRequestOnPlantationPatch();
            }

            patchWithHelpIdsFromSave.Clear();
            patchWithHelpIdsFromSave = null;

            // Do other things in normal way
            bool tempIsHelp = HaveHelpRequests;
            HaveHelpRequests = haveHelpRequest;
            if (haveHelpRequest != tempIsHelp)
            {
                UpdateHelpRequests?.Invoke();
            }

            if (haveHelpRequest && !PlantationManager.Instance.isRefreshing())
            {
                PlantationManager.Instance.bindCheckingForHelpCallback(CognitoEntry.SaveID, (x) =>
                {
                    OnPersonalHelpGet(x);
                }, (ex) =>
                {
                    Debug.LogWarning(ex.Message);
                }
                );
            }
            else if (!haveHelpRequest && PlantationManager.Instance.isRefreshing())
            {
                PlantationManager.Instance.unbindAllCallbacks();
            }
        }

        public void UpdateHelpRequestsStatus()
        {
            bool haveHelpRequest = false;
            for (int i = 0; i < plantationLevel; i++)
            {
                for (int j = 0; j < rowWidth; j++)
                {
                    if (patches[j, i].HasHelpRequest())
                    {
                        haveHelpRequest = true;
                        break;
                    }
                }
            }
            if (visitingMode)
                CacheManager.UpdatePlantationHelpRequest(SaveLoadController.SaveState.ID, haveHelpRequest);
        }

        private void OnForeignHelpGet(List<HelpRequest> requests)
        {
            bool haveHelpRequest = false;

            if (requests.Count == 0 || requests == null)
            {
                for (int i = 0; i < plantationLevel; i++)
                {
                    for (int j = 0; j < rowWidth; j++)
                    {
                        if (patches[j, i].PatchState == EPatchState.waitingForHelp)                        
                            patches[j, i].PatchState = EPatchState.empty;
                    }
                }
                if (visitingMode)
                    CacheManager.UpdatePlantationHelpRequest(SaveLoadController.SaveState.ID, haveHelpRequest);

                return;
            }

            CheckRequestsAmmountComapredToSaveStatus(requests);

            foreach (var request in requests)
            {
                Debug.Log(request.PlaceID + " " + request.helped);
                Vector2i vec = Vector2i.Parse(request.PlaceID);
                if (request.helped)
                    patches[vec.x, vec.y].PatchState = EPatchState.empty;
                else
                    haveHelpRequest = true;
            }

            if (visitingMode)            
                CacheManager.UpdatePlantationHelpRequest(SaveLoadController.SaveState.ID, haveHelpRequest);

            //if (haveHelpRequest && !PlantationManager.Instance.isRefreshing())
            //{
            //    PlantationManager.Instance.bindCheckingForHelpCallback(SaveLoadController.SaveState.ID, (x) =>
            //    {
            //        OnForeignHelpGet(x);
            //    }, (ex) =>
            //    {
            //        Debug.LogWarning(ex.Message);
            //    }
            //    );
            //}
            //else if (!haveHelpRequest && PlantationManager.Instance.isRefreshing())
            //{
            //    PlantationManager.Instance.unbindAllCallbacks();
            //}
        }

        private void CheckRequestsAmmountComapredToSaveStatus(List<HelpRequest> requests)
        {
            List<PlantationPatch> patchesWithHelpRequestFromSave = new List<PlantationPatch>();

            for (int i = 0; i < plantationLevel; i++)
            {
                for (int j = 0; j < rowWidth; j++)
                {
                    if (patches[j, i].PatchState == EPatchState.waitingForHelp)
                        patchesWithHelpRequestFromSave.Add(patches[j, i]);
                }
            }

            if (patchesWithHelpRequestFromSave.Count == requests.Count)
                return;
            else
            {
                for (int i = 0; i < requests.Count; i++)
                {
                    Vector2i vec = Vector2i.Parse(requests[i].PlaceID);
                    if (patchesWithHelpRequestFromSave.Contains(patches[vec.x, vec.y]))                    
                        patchesWithHelpRequestFromSave.Remove(patches[vec.x, vec.y]);
                    else
                        patches[vec.x, vec.y].PatchState = EPatchState.waitingForHelp;
                }

                for (int i = 0; i < patchesWithHelpRequestFromSave.Count; i++)
                {
                    patchesWithHelpRequestFromSave[i].PatchState = EPatchState.empty;
                }
            }

        }

        public void LoadFromString(string saveString, TimePassedObject timeFromSave)
        {
            //zrobić saveList
            if (string.IsNullOrEmpty(saveString))
            {
                LoadFromStringList(null, new NullableTimePassedObject(0, 0));
                return;
            }
            List<string> saveStringList = new List<string>();
            var saveStringArr = saveString.Split('!');
            for (int i = 0; i < saveStringArr.Length; i++)
            {
                saveStringList.Add(saveStringArr[i]);
            }
            LoadFromStringList(saveStringList, timeFromSave);
        }

        private void GenerateDefault(int row)
        {
            for (int i = 0; i < plantationMaxSize.x; i++)
            {
                patches[i, row].GrowingPlant = null;
                patches[i, row].TimeFromSeed = 0;
                patches[i, row].RegrowthLeft = 0;
                patches[i, row].HarvestLeft = 0;
                patches[i, row].PatchState = EPatchState.disabled;
            }
        }

        public List<string> SaveToStringList()
        {
            List<string> saveList = new List<String>();
            StringBuilder builder = new StringBuilder();
            for (int j = 0; j < actualPlantationSize.y; j++)
            {
                //here building list
                for (int i = 0; i < plantationMaxSize.x; i++)
                {
                    //here building list element
                    if (patches[i, j].GrowingPlant != null)                    
                        builder.Append(Checkers.CheckedMedicine(patches[i, j].GrowingPlant, patches[i, j].name).ToString());
                    else
                        builder.Append("null");

                    builder.Append('/');
                    builder.Append(Checkers.CheckedAmount(patches[i, j].TimeFromSeed, 0, float.MaxValue, patches[i, j].name + " TimeFromSeed").ToString("n0").Replace(",", ""));
                    builder.Append('/');
                    builder.Append(Checkers.CheckedAmount(patches[i, j].RegrowthLeft, -1, maxRegrowth, patches[i, j].name + " RegrowthLeft").ToString());
                    builder.Append('/');
                    builder.Append(Checkers.CheckedAmount(patches[i, j].HarvestLeft, -1, harvestAmount, patches[i, j].name + " HarvestLeft").ToString());
                    builder.Append('/');
                    builder.Append(Checkers.CheckedPatchState(patches[i, j].PatchState, patches[i, j].GrowingPlant).ToString());
                    if (i != plantationMaxSize.x - 1)
                        builder.Append(';');
                }
                saveList.Add(builder.ToString());
                builder.Length = 0;
                builder.Capacity = 0;
            }
            //string save = "";
            return saveList;
        }

        public string SaveToString()
        {
            List<string> saveList = SaveToStringList();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < saveList.Count; i++)
            {
                builder.Append(saveList[i]);
                if (i != saveList.Count - 1)
                    builder.Append('!');
            }
            return builder.ToString();
        }

        public void enableOnLevelUp()
        {
            if (plantationLevel >= unlockLevels.Count)            
                return;

            PlantationLevel = GetPlantationLevel();
            if (PlantationLevel > 0)            
                EnableGardener();
        }

        private void EnableGardener()
        {
            if (Game.Instance.gameState().GetHospitalLevel() >= 15)            
                gardenerPrefab.SetActive(true);
            else            
                gardenerPrefab.SetActive(false);

            if (ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive() && ReferenceHolder.GetHospital().globalEventController.GlobalEventExtras == GlobalEvent.GlobalEventExtras.ValentineHearts)
                SetHeartsActive();
            else
                SetHeartsDeactive();

            AddListeners();
        }

        private void AddListeners()
        {
            GlobalEventNotificationCenter.Instance.OnEventStart.Notification -= OnEventStart_Notification;
            GlobalEventNotificationCenter.Instance.OnEventStart.Notification += OnEventStart_Notification;

            GlobalEventNotificationCenter.Instance.OnEventEnd.Notification -= OnEventEnd_Notification;
            GlobalEventNotificationCenter.Instance.OnEventEnd.Notification += OnEventEnd_Notification;
        }

        private void OnEventEnd_Notification(GlobalEventOnStateChangeEventArgs eventArgs)
        {
            if (eventArgs.globalEventExtras == GlobalEvent.GlobalEventExtras.ValentineHearts)
                SetHeartsDeactive();
        }

        private void OnEventStart_Notification(GlobalEventOnStateChangeEventArgs eventArgs)
        {
            if (eventArgs.globalEventExtras == GlobalEvent.GlobalEventExtras.ValentineHearts)
                SetHeartsActive();
        }

        public static void AnimateGardener()
        {
            gardenerAnimator.SetTrigger("Go");
        }

        private void GeneratePlantationRow(int row)
        {
            actualPlantationSize.y = row + 1;
            GameObject temp = Instantiate(span, new Vector3(70.6f - 1.8f * row, 0.4f, 48.3f), Quaternion.Euler(45, 45, 0)) as GameObject;
            temp.transform.SetParent(fence);
            temp.GetComponent<SpriteRenderer>().sortingOrder = 1;
            temp = Instantiate(span, new Vector3(70.8f - 1.8f * row, 0.3f, 37.1f), Quaternion.Euler(45, 45, 0)) as GameObject;
            temp.transform.SetParent(fence);
            temp.GetComponent<SpriteRenderer>().sortingOrder = 4;
            //tu dodać kasowanie krzaków o ile są
            for (int i = 0; i < plantationMaxSize.x / 2; i++)
                //for (int j = 0; j < plantationSize.y; j++)
                InstantiatePatch(i, row);
            for (int i = (int)plantationMaxSize.x / 2; i < (int)plantationMaxSize.x; i++)
                //for (int j = 0; j < plantationSize.y; j++)
                InstantiatePatch(i, row, true);
        }

        private void ActualizeFrontFence(int row)
        {
            frontFence.transform.position = new Vector3(70.0f - 1.8f * (row), 0.5f, 47.05f);
        }

        private int GetPlantationLevel()
        {
            for (int i = 0; i < unlockLevels.Count; ++i)
            {
                if ((visitingMode ? SaveLoadController.SaveState.Level : Game.Instance.gameState().GetHospitalLevel()) < unlockLevels[i])
                    return i;
            }
            return unlockLevels.Count;
        }

        public int GetFieldOnlyToExcavateAmount()
        {
            int fieldsAmount = 0;
            for (int i = 0; i < plantationLevel; ++i)
            {
                for (int j = 0; j < rowWidth; ++j)
                {
                    if (patches[j, i] != null && patches[j, i].PatchState == EPatchState.fallow && patches[j, i].RegrowthLeft < 0)
                        ++fieldsAmount;
                }
            }
            return fieldsAmount;
        }

        public void SetHeartsActive()
        {
            if (hearts != null)            
                hearts.SetActive(true);
        }

        public void SetHeartsDeactive()
        {
            if (hearts != null)            
                hearts.SetActive(false);
        }
    }
}