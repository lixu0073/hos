using UnityEngine;
using System.Collections.Generic;
using Hospital;
using IsoEngine;
using MovementEffects;
using System;
using System.Collections;

/// <summary>
/// 医院病床控制器，负责管理所有医院病床的状态、病人分配、指示器更新等核心功能。
/// 这是整个医院病人管理系统的核心组件，处理病床的生命周期、病人的分配和治疗流程。
/// </summary>
public class HospitalBedController : MonoBehaviour
{
	/// <summary>
	/// 教程是否阻止病床操作的静态标志
	/// </summary>
	public static bool blockedByTutorial = true;
	/// <summary>
	/// 是否可以生成第一个病人的静态标志
	/// </summary>
	public static bool canSpawnFirstEverPatient = false;
	/// <summary>
	/// 是否有新的治疗方案可用的静态标志
	/// </summary>
	public static bool isNewCureAvailable;

	/// <summary>
	/// 医院病床列表，包含所有由此控制器管理的病床
	/// </summary>
	public List<HospitalBed> Beds;
	/// <summary>
	/// 更新病床指示器的协程
	/// </summary>
	private IEnumerator<float> updateCorountine;
	/// <summary>
	/// 全局计时器，用于跟踪游戏时间
	/// </summary>
	[HideInInspector]
	public float globalTimer;
	/// <summary>
	/// 正常情况下病人生成的时间间隔（秒）
	/// </summary>
	public int NormalTimeSpawn = 5;

	/// <summary>
	/// 标识此控制器是否为主控制器
	/// </summary>
	private bool isMainController = false;

	/// <summary>
	/// 长时间生成间隔属性，基于医院等级计算病人生成的长时间间隔
	/// </summary>
	private int LongTimeSpawn
	{
		get
		{
			if (Game.Instance.gameState().GetHospitalLevel() > 30)
				return 30 * 60;
			else return (6 + (Game.Instance.gameState().GetHospitalLevel() - 3)) * 60;
		}
	}

	/// <summary>
	/// 医院病床类，表示医院中的单个病床及其相关状态和数据
	/// </summary>
	[System.Serializable]
	public class HospitalBed
	{
		/// <summary>
		/// 本地病床ID，用于标识房间内的病床位置
		/// </summary>
		public int LocalBedId = 0;

		/// <summary>
		/// 病床游戏对象引用
		/// </summary>
		public GameObject Bed;
		/// <summary>
		/// 病床状态指示器组件
		/// </summary>
		public BedStatusIndicator Indicator;
		/// <summary>
		/// 当前在病床上的病人接口
		/// </summary>
		public IDiagnosePatient Patient;
		/// <summary>
		/// 病床所属的医院房间
		/// </summary>
		public HospitalRoom room;

		/// <summary>
		/// 下次生成病人的时间间隔（私有字段）
		/// </summary>
		float timeToNextSpawn = 0f;
		/// <summary>
		/// 下次生成病人的时间间隔属性（公开访问）
		/// </summary>
		//[HideInInspector]
		public float TimeToNextSpawn
		{
			get
			{
				return timeToNextSpawn;
			}
			set
			{
				timeToNextSpawn = value;
			}
		}
		/// <summary>
		/// 隐藏的检查器属性
		/// </summary>
		[HideInInspector]

		/// <summary>
		/// 生成时间范围（秒）
		/// </summary>
		public int spawnTimeRange = 6;
		/// <summary>
		/// 病床状态（私有字段）
		/// </summary>
		public BedStatus _bedStatus = BedStatus.WaitForPatient;
		/// <summary>
		/// 病床状态属性（公开访问）
		/// </summary>
		public BedStatus _BedStatus
		{
			get
			{
				return _bedStatus;
			}
			set
			{
				_bedStatus = value;
			}
		}
		/// <summary>
		/// 病床状态枚举，定义病床可能的各种状态
		/// </summary>
		public enum BedStatus
		{
			/// <summary>等待病人生成</summary>
			WaitForPatientSpawn,
			/// <summary>病床被占用</summary>
			OccupiedBed,
			/// <summary>等待诊断</summary>
			WaitForDiagnose,
			/// <summary>等待病人</summary>
			WaitForPatient,
			/// <summary>不存在</summary>
			NoExist,
		}
	}

	/// <summary>
	/// Unity生命周期方法，在对象被加载时调用，初始化全局计时器
	/// </summary>
	public void Awake()
	{
		this.globalTimer = 0;
	}

	/// <summary>
	/// Unity生命周期方法，在第一帧更新前调用，注册到增益效果管理器
	/// </summary>
	private void Start()
	{
		if (BoosterEffectManager.Instance.hospitalRoomBoosters != null) BoosterEffectManager.Instance.hospitalRoomBoosters.Add(this);
	}

	/// <summary>
	/// Unity生命周期方法，在对象销毁时调用，从增益效果管理器中移除
	/// </summary>
	private void OnDestroy()
	{
		if (BoosterEffectManager.Instance.hospitalRoomBoosters != null) BoosterEffectManager.Instance.hospitalRoomBoosters.Remove(this);
	}

	/// <summary>
	/// 将新的病床添加到控制器中进行管理
	/// </summary>
	/// <param name="newBeds">要添加的新病床列表</param>
	/// <param name="room">病床所属的医院房间</param>
	public void AddBedsToController(List<HospitalBed> newBeds, HospitalRoom room)
	{
		isMainController = true;
		int localId = 0;

		KillBedControllerCorountine();

		if (Beds == null)
		{
			Beds = new List<HospitalBed>();
		}

		if (room != null)
		{
			int emptyIndex = GetTheSameOrFreeHospitalBedsValuesPositionInArray(newBeds[0].room);

			if (emptyIndex == -1)
			{
				foreach (HospitalBed bed in newBeds)
				{
					if (!Beds.Contains(bed))
					{
						if (bed.Bed != null)
						{
							bed.LocalBedId = localId;
							AddIndictatorForBed(room, bed);
							Beds.Add(bed);
						}
						localId++;
					}
				}
			}
			else
			{
				for (int i = 0; i < newBeds.Count; i++)
				{
					Beds[emptyIndex + i].Bed = newBeds[i].Bed;
					Beds[emptyIndex + i].Indicator = newBeds[i].Indicator;
				}
			}

			room.RegisterToOnIsoDestroy(KillBedControllerCorountine);
		}
		else // ADD VIP BED TO LIST
		{
			// Beds.Insert()
			foreach (HospitalBed bed in newBeds)
			{
				if (!Beds.Contains(bed))
				{
					if (bed.Bed != null)
					{
						//Debug.LogError("ADD VIP BED TO LIST");
						bed.LocalBedId = localId;
						AddIndictatorForBed(room, bed);
						Beds.Insert(0, bed);
					}
					localId++;
				}
			}
		}

		UpdateBedController();


		if (updateCorountine == null)
		{
			updateCorountine = Timing.RunCoroutine(CheckBedIndicatorsCoroutine());
		}
	}


	/// <summary>
	/// 更新病床控制器，移除无效的病床并更新指示器
	/// </summary>
	private void UpdateBedController()
	{
		foreach (HospitalBed bed in Beds.ToArray())
		{
			if (bed.Bed == null)
			{
				Beds.Remove(bed);
			}
			else AddIndictatorForBed(bed.room, bed);
		}
	}

	/// <summary>
	/// 清空所有病床控制器数据并重置相关UI
	/// </summary>
	public void ClearAllBedController()
	{
		KillBedControllerCorountine();

		if (Beds != null && Beds.Count > 0)
			Beds.Clear();

		if (UIController.getHospital.PatientCard != null)
		{
			UIController.getHospital.PatientCard.ResetPatientCard();
		}

	}

	/// <summary>
	/// 终止病床控制器的更新协程
	/// </summary>
	public void KillBedControllerCorountine()
	{
		if (updateCorountine != null)
		{
			Timing.KillCoroutine(updateCorountine);
			updateCorountine = null;
		}
	}

	/// <summary>
	/// 获取数组中相同或空闲的医院病床位置
	/// </summary>
	/// <param name="room">医院房间，可选</param>
	/// <returns>相同或空闲病床的索引，如果没有则返回-1</returns>
	private int GetTheSameOrFreeHospitalBedsValuesPositionInArray(HospitalRoom room = null)
	{
		if (Beds == null || Beds.Count == 0)
			return -1;

		int id = -1;

		foreach (HospitalBed bed in Beds.ToArray())
		{
			id++;

			if ((bed.Bed == null) || (bed.Indicator == null) || ((bed.room != null) && (bed.room == room)))
			{
				return id;
			}
		}

		return -1;
	}


	/// <summary>
	/// 从控制器中移除病床
	/// </summary>
	/// <param name="oldBeds">要移除的病床列表</param>
	public void RemoveBedsFromController(List<HospitalBed> oldBeds)
	{
		if (Beds == null || Beds.Count <= 0)
			return;

		foreach (HospitalBed bed in oldBeds)
		{
			if (Beds.Contains(bed))
			{
				Beds.Remove(bed);
			}
		}
	}

	void SpawnIndicators()
	{
		for (int i = 0; i < Beds.Count; i++)
		{
			GameObject go = Instantiate(ResourcesHolder.GetHospital().BedStatusIndicator);
			go.transform.SetParent(Beds[i].Bed.transform);

			Rotation rot = GetComponent<IsoObjectPrefabController>().prefabData.rotation;
			if (rot == Rotation.East || rot == Rotation.West)
			{
				go.transform.localPosition = new Vector3(0, 1, -1);
				go.transform.localScale = Vector3.one;
				go.GetComponent<BedStatusIndicator>().FlipXBadges(false);

			}
			else
			{
				go.transform.localPosition = new Vector3(0, 1, -1);
				go.transform.localScale = new Vector3(-1, 1, 1);
				go.GetComponent<BedStatusIndicator>().FlipXBadges(true);
				//go.transform.localRotation = Quaternion.Euler(0,180,0);
			}
			go.transform.localRotation = Quaternion.identity;
			Beds[i].Indicator = go.GetComponent<BedStatusIndicator>();
		}
	}

	public void FitIndicator(GameObject indicator, Rotation rot)
	{
		if (rot == Rotation.East || rot == Rotation.West)
		{
			indicator.GetComponent<BedStatusIndicator>().FlipXBadges(false);
			indicator.transform.localPosition = new Vector3(0, 1.15f, -1);
		}
		else
		{
			indicator.GetComponent<BedStatusIndicator>().FlipXBadges(true);
			indicator.transform.localPosition = new Vector3(0, 1.15f, -1);
		}
	}

	void AddIndictatorForBed(HospitalRoom room, HospitalBed bed)
	{
		if (bed.Indicator == null)
		{
			GameObject go;

			if (bed.Bed.gameObject.transform.childCount > 8)
			{
				go = bed.Bed.gameObject.transform.GetChild(8).gameObject;
			}
			else
			{
				go = Instantiate(ResourcesHolder.GetHospital().BedStatusIndicator, bed.Bed.transform) as GameObject;
				// go.transform.SetParent(bed.Bed.transform);

				if (room != null)
				{
					Rotation rot = room.actualRotation;
					FitIndicator(go, rot);
				}
			}

			if (room == null)
			{
				go.transform.localPosition = new Vector3(0.25f, 1.25f, -1);
			}


			go.transform.localRotation = Quaternion.identity;
			bed.Indicator = go.GetComponent<BedStatusIndicator>();
			if (!TutorialController.Instance.tutorialEnabled || Game.Instance.gameState().GetHospitalLevel() >= 3)
			{
				if (TutorialController.Instance.CurrentTutorialStepIndex >= TutorialController.Instance.GetStepId(StepTag.keep_curing_text_1) || bed.LocalBedId == 0 || !TutorialController.Instance.tutorialEnabled)
					SetIndicator(bed);
			}
		}
	}

	/// <summary>
	/// 在指定病床上设置病人
	/// </summary>
	/// <param name="room">病床所在的房间</param>
	/// <param name="spotID">病床位置ID</param>
	/// <param name="patient">要设置的病人</param>
	public void SetPatientInBed(HospitalRoom room, int spotID, IDiagnosePatient patient)
	{
		// Debug.LogWarning("SPAWN IN" + room.name + " ON SPOT" + spotID + " PATIENT: " + patient.name);

		foreach (HospitalBed bed in Beds)
		{
			if (bed.room == room)
			{
				if (bed.LocalBedId == spotID)
				{
					bed.Patient = patient;
					if (bed.Bed != null)
					{
						if ((BasePatientAI)bed.Patient != null)
							if (!((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>().IsVIP)
							{
								bed.Bed.transform.GetChild(3).gameObject.SetActive(true);
								bed.Bed.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = ResourcesHolder.GetHospital().Beds[2];
								bed.Bed.transform.GetChild(6).gameObject.SetActive(true);
							}
							else
							{
								bed.Bed.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = ResourcesHolder.GetHospital().VIPQuilt[0];
							}
					}


					bed.TimeToNextSpawn = 0;
					bed._BedStatus = HospitalBed.BedStatus.OccupiedBed;
					if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_open)
					{
						NotificationCenter.Instance.TutorialArrowSet.Invoke(new TutorialArrowSetEventArgs(TutorialController.Instance.FindObjectForStep()));
					}
					//SetIndicator(bed);
					UpdateAllBedsIndicators(false);
					return;
				}
			}
		}
	}

	/// <summary>
	/// 检查指定病床是否已被病人占用
	/// </summary>
	/// <param name="room">病床所在的房间</param>
	/// <param name="spotID">病床位置ID</param>
	/// <param name="patient">要检查的病人</param>
	/// <returns>如果病床被占用则返回true</returns>
	public bool isPatientInBed(HospitalRoom room, int spotID, IDiagnosePatient patient)
	{
		foreach (HospitalBed bed in Beds)
		{
			if (bed.room == room)
			{
				if (bed.LocalBedId == spotID)
				{
					if (bed.Patient != null && (bed.Patient != patient))
						return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 从指定病床释放病人，并更新病床状态
	/// </summary>
	/// <param name="room">病床所在的房间</param>
	/// <param name="spotID">病床位置ID</param>
	public void FreePatientFromBed(HospitalRoom room, int spotID)
	{
		// Debug.LogError("FREE FROM ROOM " + room.name + " IN SPOT" + spotID);

		foreach (HospitalBed bed in Beds)
		{
			if (bed.room == room)
			{
				if (bed.LocalBedId == spotID)
				{
					if (bed.Bed != null)
					{
						if ((BasePatientAI)bed.Patient != null)
							if (!((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>().IsVIP)
							{
								MakeABed(bed, false);
							}
							else
							{
								MakeABed(bed, true);
							}
					}
					if ((BasePatientAI)bed.Patient != null)
						if (!((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>().IsVIP)
						{
							bed.TimeToNextSpawn = 0;
							bed._BedStatus = HospitalBed.BedStatus.WaitForPatient;
						}
						else
						{
							bed.TimeToNextSpawn = HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().arriveIntervalSeconds;
							bed._BedStatus = HospitalBed.BedStatus.WaitForPatientSpawn;

							HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().SetRequiredMedicines();

							HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().StartCountingExtraSeconds();
							HospitalAreasMapController.HospitalMap.vipRoom.gameObject.GetComponent<VIPSystemManager>().LastVIPHealed = true;
							HospitalAreasMapController.HospitalMap.vipRoom.gameObject.GetComponent<VIPSystemManager>().CureVip(1);
						}
					//bed.Patient = null;

					SetIndicator(bed);
					/*
                    if (bed.Patient != null && ((BasePatientAI)bed.Patient).gameObject.GetComponent<HospitalCharacterInfo>() != null)
                    {

                        if (((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>().IsVIP)
                        {
                            HintsController.Get().RemoveHint(new PatientStatusHint(((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>(), PatientStatusHint.PatientStatus.Cured, "VIP"));
                            HintsController.Get().RemoveHint(new PatientMedicineHint(((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>()), 1, true);
                        }
                        else
                        {
                            HintsController.Get().RemoveHint(new PatientStatusHint(((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>(), PatientStatusHint.PatientStatus.Cured, bed.room.Tag));
                            HintsController.Get().RemoveHint(new PatientMedicineHint(((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>()), 1, true);
                        }

                        HintsController.Get().UpdateAllHintsWithMedicineCount();
                        HintsController.Get().UpdateAllHintsWithPatientsStatus();
                    }
                    */

					bed.Patient = null; //MY CHANGES
					return;
				}
			}
		}
	}

	/// <summary>
	/// 整理病床，根据是否为VIP病床设置不同的外观
	/// </summary>
	/// <param name="bed">要整理的病床</param>
	/// <param name="isVIPBed">是否为VIP病床</param>
	public void MakeABed(HospitalBed bed, bool isVIPBed)
	{
		if (!isVIPBed)
		{
			bed.Bed.transform.GetChild(3).gameObject.SetActive(false);
			bed.Bed.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = ResourcesHolder.GetHospital().Beds[1];
			bed.Bed.transform.GetChild(6).gameObject.SetActive(true);
		}
		else
		{
			bed.Bed.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = ResourcesHolder.GetHospital().VIPQuilt[1];
		}

	}

	Dictionary<HospitalBed, Coroutine> bedTimers = new Dictionary<HospitalBed, Coroutine>();
	Dictionary<HospitalBed, IEnumerator> queuedTimerCoroutines = new Dictionary<HospitalBed, IEnumerator>();

	public void StartTimerCoroutine(HospitalBed bed)
	{
		if (bedTimers != null && bedTimers.ContainsKey(bed) && bedTimers[bed] != null)
			KillTimerCoroutine(bed);

		if (this.gameObject.activeSelf)
		{
			bedTimers.Add(bed, StartCoroutine(TimerCoroutine(bed)));
		}
		else
		{
			bedTimers.Add(bed, null);
			queuedTimerCoroutines.Add(bed, TimerCoroutine(bed));
		}
	}

	public void KillTimerCoroutine(HospitalBed bed)
	{
		try
		{
			if (bedTimers[bed] != null)
			{
				StopCoroutine(bedTimers[bed]);
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
		}
		bedTimers.Remove(bed);
	}

	IEnumerator TimerCoroutine(HospitalBed bed)
	{
		Debug.LogError("I HAVE STARTED ON BED AT " + bed.room.position);
		while (bed.TimeToNextSpawn > 0)
		{
			Debug.LogError("I AM RUNNING ON BED AT " + bed.room.position);
			bed.TimeToNextSpawn--;
			yield return new WaitForSeconds(1f);
		}
		bed.TimeToNextSpawn = 0;
		UpdateAllBedsIndicators(false);
		bedTimers.Remove(bed);
	}

	/// <summary>
	/// 从病床出院病人，处理出院流程和后续操作
	/// </summary>
	/// <param name="room">病床所在的房间</param>
	/// <param name="spotID">病床位置ID</param>
	public void DischargePatientFromBed(HospitalRoom room, int spotID)
	{
		foreach (HospitalBed bed in Beds)
		{
			if (bed.room == room)
			{
				if (bed.LocalBedId == spotID)
				{
					if (bed.Bed != null)
					{
						if ((BasePatientAI)bed.Patient != null)
							if (!((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>().IsVIP)
							{
								bed.Bed.transform.GetChild(3).gameObject.SetActive(false);
								bed.Bed.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = ResourcesHolder.GetHospital().Beds[0];
								bed.Bed.transform.GetChild(6).gameObject.SetActive(true);
							}
							else
							{
								bed.Bed.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = ResourcesHolder.GetHospital().VIPQuilt[1];
							}
					}


					if ((BasePatientAI)bed.Patient != null)
					{
						var characterInfo = ((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>();
						if (characterInfo != null)
						{
							if (!characterInfo.IsVIP)
							{
								bed.TimeToNextSpawn = LongTimeSpawn;
								StartTimerCoroutine(bed);
							}
							else
							{
								HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().SetRequiredMedicines(); //TODO - check if this is needed. Why would we set vip patient to need medicine if they are leaving?

								HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().StartCountingExtraSeconds();
								HospitalAreasMapController.HospitalMap.vipRoom.gameObject.GetComponent<VIPSystemManager>().LastVIPHealed = false;
								NotificationCenter.Instance.VipNotCured.Invoke(new BaseNotificationEventArgs());

								if (characterInfo.RequiresDiagnosis)
								{
									if (HospitalDataHolder.Instance.ReturnDiseaseRoom((int)(characterInfo.DisaseDiagnoseType)) != null)
										HospitalDataHolder.Instance.ReturnDiseaseRoom((int)(characterInfo.DisaseDiagnoseType)).HideIndicator();
								}
							}
						}
					}

					//Debug.LogError("BedStatus.WaitForPatientSpawn");
					bed._BedStatus = HospitalBed.BedStatus.WaitForPatientSpawn;
					SetIndicator(bed);

					bed.Patient = null; //Why NOT?
					return;
				}
			}
		}
	}

	public void WaitForDiagnose(HospitalRoom room, int spotID)
	{
		foreach (HospitalBed bed in Beds)
		{
			if (bed.room == room)
			{
				if (bed.LocalBedId == spotID)
				{
					Beds[spotID]._BedStatus = HospitalBed.BedStatus.WaitForDiagnose;
					SetIndicator(bed);
					return;
				}
			}
		}
	}

	public void UnCoverBed(HospitalRoom room, int spotID, HospitalPatientAI patient)
	{
		foreach (HospitalBed bed in Beds)
		{
			if (bed.room == room)
			{
				if (bed.LocalBedId == spotID)
				{
					bed.Patient = patient;
					bed._BedStatus = HospitalBed.BedStatus.OccupiedBed;
					SetIndicator(bed);
					if (bed.Bed != null)
					{
						bed.Bed.transform.GetChild(3).gameObject.SetActive(false);
						bed.Bed.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = ResourcesHolder.GetHospital().Beds[3];
						bed.Bed.transform.GetChild(6).gameObject.SetActive(true);
					}
					return;
				}
			}
		}
	}

	void OnEnable()
	{
		//Debug.LogError("OnEnable: " + this.gameObject.name);

		RotatableObject.OnMedicineCollected += UpdateAllBedsIndicators;

		if (queuedTimerCoroutines != null && queuedTimerCoroutines.Count > 0)
		{
			var keys = new List<HospitalBed>(queuedTimerCoroutines.Keys);
			for (int i = 0; i < keys.Count; i++)
			{
				try
				{
					StartTimerCoroutine(keys[i]);
					queuedTimerCoroutines.Remove(keys[i]);
				}
				catch
				{
					Debug.Log("Bed timer coroutine already running");
				}
			}
		}
	}

	void OnDisable()
	{
		//Debug.LogError("OnDisable: " + this.gameObject.name);

		RotatableObject.OnMedicineCollected -= UpdateAllBedsIndicators;
		StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
	}

	IEnumerator<float> CheckBedIndicatorsCoroutine()
	{
		while (true)
		{
			UpdateAllBedsIndicators(false);
			yield return Timing.WaitForSeconds(1);
		}
	}

	private HelpStatus GetHelpStatusForPatient(HospitalCharacterInfo bedPatient)
	{
		if (bedPatient != null)
		{
			if (Hospital.HospitalAreasMapController.HospitalMap.VisitingMode)
			{
				if (ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.CheckIsPatientHelpRequested(bedPatient.ID))
				{
					if (ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.CheckIsPatientFullfiled(bedPatient.ID))
						return HelpStatus.HelpRequestFulfilled;
					else
						return HelpStatus.HelpRequested;
				}
			}
			else
			{
				if (ReferenceHolder.GetHospital().treatmentRoomHelpController.IsHelpRequestForPatient(bedPatient))
					return HelpStatus.HelpRequested;
			}
		}

		return HelpStatus.None;
	}

	/// <summary>
	/// 更新所有病床的指示器状态，处理病人状态、治疗进度等
	/// </summary>
	/// <param name="medicineCollected">是否收集了药物</param>
	public void UpdateAllBedsIndicators(bool medicineCollected)
	{
		//if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.follow_ambulance))
		//    ReferenceHolder.GetHospital().Ambulance.IsBlockedByTutorial = true;
		//else
		//    ReferenceHolder.GetHospital().Ambulance.IsBlockedByTutorial = false;
		int id = 0;

		if (Beds != null && Beds.Count > 0)
		{
			foreach (HospitalBed bed in Beds)
			{
				bool is_Error = false;

				if ((id == 0 && canSpawnFirstEverPatient) || !blockedByTutorial)
				{
					try
					{
						if (bed != null)
						{
							if (bed.room != null)
							{
								if (bed.Patient != null)
								{
									HospitalCharacterInfo bedPatient = ((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>();
									HelpStatus helpStatus = GetHelpStatusForPatient(bedPatient);

									// UPDATE PLAGUE STATUS IN ROOM

									if (bedPatient.CanGetInfection())
									{
										// CHECK TIME IS TEMPORARY SOLUTION CUZ HERE WILL BE TIME FROM AWS CONFIG
										HospitalCharacterInfo infectedBy = null;

										if (bedPatient.GetTimeTillInfection(out infectedBy) < 0)
										{
											bed.Patient.AddBacteriaFromOtherPatient();
											bed.Indicator.SetPlagueIndicator(false);

											UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);
										}
										else
										{
											bed.Indicator.SetPlagueIndicator(true);
										}
									}
									else if (bedPatient.HasBacteria)
									{
										bed.Indicator.SetPlagueIndicator(false);
									}
									else bed.Indicator.HidePlagueIndicator();

									// DO OTHER STUFF NORMALLY

									if (bed._BedStatus == HospitalBed.BedStatus.OccupiedBed)
									{
										bool cureWithHelp;

										if (bedPatient.CheckCurePosible(out cureWithHelp) && !bed.Patient.goHomeGet())
										{
											if (!cureWithHelp)
											{
												if (bed.Indicator.GetIndicatorStatus() != BedStatus.Cure)
												{
													//HintsController.Get().AddHint(new PatientStatusHint(bed.Patient.GetHospitalCharacterInfo(), PatientStatusHint.PatientStatus.Cured, bed.room.Tag));

													SetIndicator(bed, BedStatus.Cure, helpStatus, medicineCollected);

													UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

													if (bed.Patient != null)
													{
														if (!bed.Patient.GetHospitalCharacterInfo().IsVIP)
															((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
														else ((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
													}
												}
											}
											else
											{
												if (bed.Indicator.GetIndicatorStatus() != BedStatus.CureHelped)
												{
													//HintsController.Get().AddHint(new PatientStatusHint(bed.Patient.GetHospitalCharacterInfo(), PatientStatusHint.PatientStatus.Cured, bed.room.Tag));

													SetIndicator(bed, BedStatus.CureHelped, helpStatus, medicineCollected);

													UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

													if (bed.Patient != null)
													{
														if (!bed.Patient.GetHospitalCharacterInfo().IsVIP)
															((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
														else ((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
													}
												}
											}
										}
										else if (!bedPatient.WasPatientCardSeen)
										{
											//Debug.LogWarning("BedStatus.New");
											if (bed.Indicator.GetIndicatorStatus() != BedStatus.New)
											{
												SetIndicator(bed, BedStatus.New, helpStatus);
												UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

												if (bed.Patient != null)
												{
													if (!bed.Patient.GetHospitalCharacterInfo().IsVIP)
														((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
													else ((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
												}
											}
										}
										else if (bedPatient.WasPatientCardSeen)
										{
											/*if (bedPatient.RequiresDiagnosis) {
                                                SetIndicator(bed, BedStatus.DiagnosisRequired);

                                                if (UIController.get.PatientCard.gameObject.activeInHierarchy)
                                                {
                                                    UIController.get.PatientCard.RefreshViewOnBed(UIController.get.PatientCard.selectedBedId);//MEMEME
                                                    UIController.get.PatientCard.UpdateOtherPatients();//MEMEME
                                                }
                                                if (bed.Patient != null)
                                                {
                                                    if (!bed.Patient.GetHospitalCharacterInfo().IsVIP)
                                                        ((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                                                    else ((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                                                }

                                            }*/ /* // do not delete
                                            else if (bedPatient.CheckIfLastCureNeeded() && !bed.Patient.goHomeGet())
                                            {
                                                if (bed.Indicator.GetIndicatorStatus() != BedStatus.LastMedicine)
                                                {

                                                    SetIndicator(bed, BedStatus.LastMedicine, medicineCollected, bedPatient.lastMedicine);

                                                    if (UIController.get.PatientCard.gameObject.activeInHierarchy)
                                                    {
                                                        UIController.get.PatientCard.RefreshViewOnBed(UIController.get.PatientCard.selectedBedId);//MEMEME
                                                        UIController.get.PatientCard.UpdateOtherPatients();//MEMEME
                                                    }
                                                    if (bed.Patient != null)
                                                    {
                                                        if (!bed.Patient.GetHospitalCharacterInfo().IsVIP)
                                                            ((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                                                        else ((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                                                    }
                                                }
                                            }*/
											//else

											if (!SetHelpIndictorForNotVisitingMode(bed, helpStatus))
											{
												if (bed.Indicator.GetIndicatorStatus() != BedStatus.None)
												{
													SetIndicator(bed, BedStatus.None, helpStatus);

													if (bed.Patient != null)
													{
														if (!bed.Patient.GetHospitalCharacterInfo().IsVIP)
														{
															((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
															((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.inBed);
														}
														else
														{
															((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
															((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.inBed);
														}
													}
												}
												else if (Hospital.HospitalAreasMapController.HospitalMap.VisitingMode)
												{
													SetIndicator(bed, BedStatus.None, helpStatus);
												}
											}
										}
									}
								}

								// SPAWNING CODE
								if (bed.room.CanSpawnpatientInRoom() && bed.room.isAmbulanceWaiting() && TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.follow_ambulance, true))
								{
									if (bed._BedStatus == HospitalBed.BedStatus.WaitForPatient)
									{
										if (bed.Patient == null)
											SpawnPersonForRoomAndBed(bed.room, bed.LocalBedId, 0.1f);

										SetIndicatorWhenRoomExist(bed, BedStatus.Follow);

									}
									else if (bed._BedStatus == HospitalBed.BedStatus.WaitForPatientSpawn)
									{
										//bed.TimeToNextSpawn--;

										if (bed.Indicator.GetIndicatorStatus() != BedStatus.Wait)
											bed.Indicator.SetIndicator(bed, BedStatus.Wait);

										if (bed.TimeToNextSpawn <= 0)
										{
											bed.TimeToNextSpawn = 0;
											bed._BedStatus = HospitalBed.BedStatus.WaitForPatient;
											//  bed.currentTime = globalTimer;

											SetIndicatorWhenRoomExist(bed, BedStatus.Wait);
										}
									}
								}
								else
								{
									//Debug.LogError("ELSE");
									//bed.timeToNextSpawn = 0;
								}
							}
							else  // VIP PATIENT
							{
								switch ((HospitalBed.BedStatus)bed._BedStatus)
								{
									case HospitalBed.BedStatus.OccupiedBed:
										if (bed.Patient != null)
										{
											HospitalCharacterInfo bedPatient = ((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>();

											bool cureWithHelp;

											if (bedPatient.CheckCurePosible(out cureWithHelp) && !bed.Patient.goHomeGet())
											{
												//	HintsController.Get().AddHint(new HintsController.Hint(HintsController.Hint.HintType.patientWaitingForHeal, bed.room.Tag,"",0, ((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>()));

												if (!cureWithHelp)
												{
													if (bed.Indicator.GetIndicatorStatus() != BedStatus.Cure)
													{
														//HintsController.Get().AddHint(new PatientStatusHint(bed.Patient.GetHospitalCharacterInfo(), PatientStatusHint.PatientStatus.Cured, "VIP"));
														SetIndicator(bed, BedStatus.Cure, HelpStatus.None, medicineCollected);
													}
												}
												else
												{
													if (bed.Indicator.GetIndicatorStatus() != BedStatus.CureHelped)
													{
														//HintsController.Get().AddHint(new PatientStatusHint(bed.Patient.GetHospitalCharacterInfo(), PatientStatusHint.PatientStatus.Cured, "VIP"));
														SetIndicator(bed, BedStatus.CureHelped, HelpStatus.None, medicineCollected);
													}
												}
											}
											else if (!bedPatient.WasPatientCardSeen)
											{
												SetIndicator(bed, BedStatus.New, HelpStatus.None);
											}
											else if (bedPatient.WasPatientCardSeen)
											{
												/* if (bedPatient.RequiresDiagnosis)
                                                 {
                                                     SetIndicator(bed, BedStatus.DiagnosisRequired);
                                                 }*/
												/* else if (bedPatient.CheckIfLastCureNeeded() && !bed.Patient.goHomeGet())
                                                 {
                                                     if (bed.Indicator.GetIndicatorStatus() != BedStatus.LastMedicine)
                                                     {
                                                         HintsController.Get().AddPatientToHintSystem((BasePatientAI)bed.Patient, "VIP");
                                                         SetIndicator(bed, BedStatus.LastMedicine, medicineCollected, bedPatient.lastMedicine);
                                                     }
                                                 }*/
												// else {
												SetIndicator(bed, BedStatus.None, HelpStatus.None);
												//  }

											}
										}
										break;
									case HospitalBed.BedStatus.WaitForPatient:
										SetIndicatorWhenRoomExist(bed, BedStatus.Follow);
										break;
									case HospitalBed.BedStatus.WaitForPatientSpawn:
										bed.TimeToNextSpawn--;

										if (bed.TimeToNextSpawn <= 0)
										{
											bed.TimeToNextSpawn = 0;
											bed._BedStatus = HospitalBed.BedStatus.WaitForPatient;
											SetIndicatorWhenRoomExist(bed, BedStatus.Wait);

										}
										break;
									default:
										break;

								}
							}
						}
					}
					catch (System.Exception)
					{
						is_Error = true;
					}
				}
				id++;
				if (is_Error) continue;
			}
		}
	}


	/// <summary>
	/// 为指定房间和病床生成病人
	/// </summary>
	/// <param name="room">要生成病人的房间</param>
	/// <param name="localBedId">病床的本地ID</param>
	/// <param name="time">生成延迟时间</param>
	public void SpawnPersonForRoomAndBed(HospitalRoom room, int localBedId, float time)
	{
		room.SpawnPerson(localBedId, time);
	}

	/// <summary>
	/// 设置病床指示器的状态
	/// </summary>
	/// <param name="bed">要设置指示器的病床</param>
	public void SetIndicator(HospitalBed bed)
	{

		switch (bed._BedStatus)
		{
			case HospitalBed.BedStatus.WaitForPatient:
				if (bed.Indicator.GetIndicatorStatus() != BedStatus.Follow)
				{

					SetIndicatorWhenRoomExist(bed, BedStatus.Follow);

					UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

				}
				break;
			case HospitalBed.BedStatus.WaitForPatientSpawn:
				if (bed.Indicator.GetIndicatorStatus() != BedStatus.Wait)
				{
					SetIndicatorWhenRoomExist(bed, BedStatus.Wait);

					UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

				}
				break;
			case HospitalBed.BedStatus.OccupiedBed:
				if (bed.Patient != null && ((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>() != null)
				{

					HospitalCharacterInfo bedPatient = ((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>();
					HelpStatus helpStatus = GetHelpStatusForPatient(bedPatient);

					if (Hospital.HospitalAreasMapController.HospitalMap.VisitingMode)
					{
						HospitalCharacterInfo info = bed.Patient.GetHospitalCharacterInfo();
						if (info != null)
						{
							if (ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.CheckIsPatientHelpRequested(info.ID))
							{
								if (ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.CheckIsPatientFullfiled(info.ID))
									helpStatus = HelpStatus.HelpRequestFulfilled;
								else
									helpStatus = HelpStatus.HelpRequested;
							}
						}
					}

					if (((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>().WasPatientCardSeen)
					{
						if (bed.Indicator.GetIndicatorStatus() != BedStatus.None)
						{
							if (((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>().RequiresDiagnosis)
							{
								SetIndicator(bed, BedStatus.DiagnosisRequired, HelpStatus.None);

								if (bed.Patient != null)
								{
									HospitalCharacterInfo bedPatientInfo = bed.Patient.GetHospitalCharacterInfo();

									if (!bedPatientInfo.IsVIP)
									{
										((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
									}
									else
									{
										((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
									}
								}
							}
							else
							{

								SetIndicator(bed, BedStatus.None, helpStatus);

								UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

								if (bed.Patient != null)
								{
									HospitalCharacterInfo bedPatientInfo = bed.Patient.GetHospitalCharacterInfo();

									if (!bedPatientInfo.IsVIP)
									{
										((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
										((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.inBed);
									}
									else
									{
										((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
										((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.inBed);
									}
								}
							}

						}
					}
					else
					{
						if (bed.Indicator.GetIndicatorStatus() != BedStatus.New)
						{
							SetIndicator(bed, BedStatus.New, helpStatus);
							UIController.getHospital.PatientCard.RefreshViewOnBed(UIController.getHospital.PatientCard.selectedBedId);

							if (bed.Patient != null)
							{
								HospitalCharacterInfo bedPatientInfo = bed.Patient.GetHospitalCharacterInfo();

								if (!bedPatientInfo.IsVIP)
									((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
								else ((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
							}
							//Debug.LogWarning("BedStatus.New");
						}
					}
				}
				break;
			default:
				break;
		}
	}

	public bool SetHelpIndictorForNotVisitingMode(HospitalBed bed, HelpStatus helpStatus)
	{
		if (!Hospital.HospitalAreasMapController.HospitalMap.VisitingMode)
		{
			//if (helpStatus == HelpStatus.HelpRequested && bed.Indicator.GetIndicatorStatus() != BedStatus.Cure && bed.Indicator.GetIndicatorStatus() != BedStatus.CureHelped)

			if (bed.Indicator.GetIndicatorStatus() != BedStatus.Cure && bed.Indicator.GetIndicatorStatus() != BedStatus.CureHelped)
			{
				//bed.Indicator.SetIndicator(bed, BedStatus.HelpRequested);
				bed.Indicator.SetIndicator(bed, BedStatus.None);

				if (bed.Patient != null)
				{
					if (!bed.Patient.GetHospitalCharacterInfo().IsVIP)
					{
						((HospitalPatientAI)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
					}
					else
					{
						((VIPPersonController)bed.Patient).gameObject.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
					}
				}
				return true;
			}
		}

		return false;
	}

	public void SetIndicator(HospitalBed bed, BedStatus status, HelpStatus helpStatus, bool medicineCollected = false, Sprite image = null)
	{
		BedStatus previousStatus = bed.Indicator.GetIndicatorStatus();

		if (Hospital.HospitalAreasMapController.HospitalMap.VisitingMode)
		{
			if (helpStatus == HelpStatus.HelpRequestFulfilled)
				bed.Indicator.SetIndicator(bed, BedStatus.HelpRequestFulfilled);
			else if (helpStatus == HelpStatus.HelpRequested)
				bed.Indicator.SetIndicator(bed, BedStatus.HelpRequested);
			else
				bed.Indicator.SetIndicator(bed, BedStatus.None);

			return;
		}

		bed.Indicator.SetIndicator(bed, status, image);

		if (medicineCollected && previousStatus != BedStatus.Cure && bed.Indicator.GetIndicatorStatus() == BedStatus.Cure)
		{
			isNewCureAvailable = true;
		}
		if (previousStatus == BedStatus.Cure && bed.Indicator.GetIndicatorStatus() != BedStatus.Cure)
			isNewCureAvailable = false;

		//Debug.LogError("isNewCureAvailable = " + isNewCureAvailable);
	}

	public void SetIndicatorWhenRoomExist(HospitalBed bed, BedStatus status)
	{
		if (bed.room == null)
			bed.Indicator.SetIndicator(bed, BedStatus.None);
		else bed.Indicator.SetIndicator(bed, status);
	}

	/// <summary>
	/// 获取任意一个有病人的病床
	/// </summary>
	/// <returns>返回第一个找到的有病人的病床，如果没有则返回null</returns>
	public HospitalBed GetAnyBedWithPatient()
	{
		if (Beds != null && Beds.Count > 0)
		{
			for (int i = 0; i < Beds.Count; i++)
			{
				if (Beds[i].Patient != null)
					return Beds[i];
			}
		}
		return null;
	}

	public HospitalBed GetAnyBedWithPatientFromRoom(HospitalRoom room)
	{
		if (Beds != null && Beds.Count > 0)
		{
			for (int i = 0; i < Beds.Count; i++)
			{
				if (Beds[i].room == room && Beds[i].Patient != null)
					return Beds[i];
			}
		}
		return null;
	}

	public HospitalBed GetBedWithIDFromRoom(HospitalRoom room, int spotID)
	{
		if (Beds != null && Beds.Count > 0)
		{
			for (int i = 0; i < Beds.Count; i++)
			{
				if (Beds[i].room == room && Beds[i].LocalBedId == spotID)
					return Beds[i];
			}
		}
		return null;
	}

	public int GetFreeyBedIDWithPatientFromRoom(HospitalRoom room)
	{
		if (Beds != null && Beds.Count > 0)
		{
			for (int i = 0; i < Beds.Count; i++)
			{
				if (Beds[i].room == room && Beds[i].Patient != null)
					return Beds[i].LocalBedId;
			}
		}
		return -1;
	}


	public int GetBedStatusForID(int id)
	{
		if (id >= Beds.Count || Beds == null || id < 0 | Beds.Count == 0)
			return (int)HospitalBed.BedStatus.NoExist;

		return (int)Beds[id]._BedStatus;
	}


	public int GetBedID(HospitalBed bed)
	{
		if (bed == null)
			return -1;

		if (Beds != null && Beds.Count > 0)
		{
			for (int i = 0; i < Beds.Count; i++)
			{
				if (bed == Beds[i])
					return i;
			}
		}
		return -1;
	}

	public HospitalBed GetClosestBed(HospitalRoom room, Vector3 pointClick, out int id, bool withHelpRequest = false)
	{
		float minDist = Mathf.Infinity;
		HospitalBed closestBed = null;

		int locID = 0;
		int neares_id = -1;

		foreach (HospitalBed bed in Beds)
		{
			if (!withHelpRequest)
			{
				if (locID > 0 && TutorialController.Instance.tutorialEnabled && TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.keep_curing_text_1))
				{
					neares_id = 0;
					Debug.LogWarning("neares_id = " + neares_id);
					break;
				}

				float distance = Vector3.Distance(pointClick, bed.Bed.transform.localPosition);
				if (distance < minDist && (bed.room == room || room == null))
				{
					minDist = distance;
					closestBed = bed;
					neares_id = locID;
					Debug.LogWarning("neares_id = " + neares_id);
				}
			}
			else
			{
				float distance = Vector3.Distance(pointClick, bed.Bed.transform.localPosition);
				if (distance < minDist && (bed.room == room || room == null))
				{
					if (bed.Patient != null)
					{
						HospitalCharacterInfo bedPatient = ((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>();

						if (ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.CheckIsPatientHelpRequested(bedPatient.ID))
						{
							minDist = distance;
							closestBed = bed;
							neares_id = locID;
							Debug.LogWarning("neares_id = " + neares_id);
						}
					}
				}
			}

			locID++;
		}

		// CHECK THAT OTHER PATIENT IS READY TO CURE ~ disabled 'cuz can enter via indicators

		/*
        if (closestBed != null && closestBed.room != null)
        {
            locID = 0;

            foreach (HospitalBed bed in Beds)
            {
                if (bed.room == closestBed.room)
                {
                    if (bed.Indicator.ReadyToCure())
                    {
                        closestBed = bed;
                        neares_id = locID;
                        Debug.LogWarning("neares_id with cure is = " + neares_id);
                        break;
                    }
                }
                locID++;
            }
        }
        */

		id = neares_id;

		Debug.LogWarning("neares_id = " + neares_id);
		return closestBed;
	}

	public void GetVIPBedID(out int id)
	{
		int locID = 0;

		if (Beds != null && Beds.Count > 0)
		{
			for (int i = 0; i < Beds.Count; i++)
			{
				if (Beds[i].room == null)
					break;
				locID++;
			}
		}

		id = locID;
	}

	public bool SpeedBedWaitingForID(int id)
	{
		if (id >= Beds.Count || Beds == null)
			return false;

		if (id > 0 && TutorialController.Instance.tutorialEnabled && TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.keep_curing_text_1))
		{
			return false;
		}

		if (Beds[id]._BedStatus == HospitalBed.BedStatus.WaitForPatientSpawn)
		{
			Beds[id].TimeToNextSpawn = 0;
			// Beds[id].currentTime = globalTimer;
			Beds[id]._BedStatus = HospitalBed.BedStatus.WaitForPatient;
			if (Beds[id].room == null)
			{
				HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().StartCounting(0);
			}

			return true;
		}
		return false;
	}


	public int GetWaitTimerForBed(int id)
	{
		if (id >= Beds.Count || Beds == null)
			return 0;

		return (int)Beds[id].TimeToNextSpawn;
	}

	/// <summary>
	/// Unity生命周期方法，每帧更新全局计时器
	/// </summary>
	public void Update()
	{
		if (isMainController)
			this.globalTimer += Time.deltaTime;
	}
}