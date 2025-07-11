using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using Hospital;
//matward trzeb zrobic maternity
public class HospitalDataHolder : MonoBehaviour
{
	public static HospitalDataHolder Instance;

	public List<DiagnosticRoom> LungTestingRoomList = new List<DiagnosticRoom>();
	public List<DiagnosticRoom> XRayRoomList = new List<DiagnosticRoom>();
	public List<DiagnosticRoom> MRIRoomList = new List<DiagnosticRoom>();
	public List<DiagnosticRoom> UltrasoundRoomList = new List<DiagnosticRoom>();
	public List<DiagnosticRoom> LaserRoomList = new List<DiagnosticRoom>();

	public LinkedList<IDiagnosePatient> LungTestingQueue = new LinkedList<IDiagnosePatient>();
	public LinkedList<IDiagnosePatient> XRayQueue = new LinkedList<IDiagnosePatient>();
	public LinkedList<IDiagnosePatient> MRIQueue = new LinkedList<IDiagnosePatient>();
	public LinkedList<IDiagnosePatient> UltrasoundQueue = new LinkedList<IDiagnosePatient>();
	public LinkedList<IDiagnosePatient> LaserQueue = new LinkedList<IDiagnosePatient>();

	public LinkedList<RefactoredHospitalPatientAI> LungTestingQueue2 = new LinkedList<RefactoredHospitalPatientAI>();
	public LinkedList<RefactoredHospitalPatientAI> XRayQueue2 = new LinkedList<RefactoredHospitalPatientAI>();
	public LinkedList<RefactoredHospitalPatientAI> MRIQueue2 = new LinkedList<RefactoredHospitalPatientAI>();
	public LinkedList<RefactoredHospitalPatientAI> UltrasoundQueue2 = new LinkedList<RefactoredHospitalPatientAI>();
	public LinkedList<RefactoredHospitalPatientAI> LaserQueue2 = new LinkedList<RefactoredHospitalPatientAI>();

    public int MaxQueueSize = 9;
    public int LungTestingQueueSize = 2;
    public int XRayQueueSize = 2;
    public int MRIQueueSize = 2;
    public int UltrasoundQueueSize = 2;
    public int LaserQueueSize = 2;

    public Emergency Emergency;
    public DiagnosticRoomInfo XRayRoom;
    public DiagnosticRoomInfo LungTestingRoom;
    public DiagnosticRoomInfo MRIRoom;
    public DiagnosticRoomInfo UltrasoundRoom;
    public DiagnosticRoomInfo LaserRoom;

    public List<IProductables> BuiltProductionMachines = new List<IProductables>();

    void Awake(){
		if (Instance == null) {
			Instance = this;
		}
	}

	public void Reset(){
		//listy roomów?
		LungTestingRoomList = new List<DiagnosticRoom>();
		XRayRoomList = new List<DiagnosticRoom>();
		MRIRoomList = new List<DiagnosticRoom>();
		UltrasoundRoomList = new List<DiagnosticRoom>();
		LaserRoomList = new List<DiagnosticRoom>();

		LungTestingQueue = new LinkedList<IDiagnosePatient>();
		XRayQueue = new LinkedList<IDiagnosePatient>();
		MRIQueue = new LinkedList<IDiagnosePatient>();
		UltrasoundQueue = new LinkedList<IDiagnosePatient>();
		LaserQueue = new LinkedList<IDiagnosePatient>();

		LungTestingQueue2 = new LinkedList<RefactoredHospitalPatientAI>();
		XRayQueue2 = new LinkedList<RefactoredHospitalPatientAI>();
		MRIQueue2 = new LinkedList<RefactoredHospitalPatientAI>();
		UltrasoundQueue2 = new LinkedList<RefactoredHospitalPatientAI>();
		LaserQueue2 = new LinkedList<RefactoredHospitalPatientAI>();

		MaxQueueSize = 9;
		LungTestingQueueSize = 2;
		XRayQueueSize = 2;
		MRIQueueSize = 2;
		UltrasoundQueueSize = 2;
		LaserQueueSize = 2;

        BuiltProductionMachines = new List<IProductables>();
    }

	public bool AddLastToDiagnosticQueue(IDiagnosePatient patient, bool addWhenFull = false)
	{
		HospitalCharacterInfo patientInfo = patient.GetAI().GetComponent<HospitalCharacterInfo>();
		for (int i = 0; i < patientInfo.requiredMedicines.Length; ++i)
		{
			switch (patientInfo.requiredMedicines[i].Key.Disease.DiseaseType)
			{
				case DiseaseType.Lungs:
                    if (LungTestingQueue.Count < LungTestingQueueSize)
                    {
                        LungTestingQueue.AddLast(patient);
					patient.SetQueueID (LungTestingQueue.Count - 1);
                        return true;
                    }
                    else if (addWhenFull)
                    {
                        LungTestingQueueSize++;
                        LungTestingQueue.AddLast(patient);
					    patient.SetQueueID (LungTestingQueue.Count - 1);
                        return true;
                    }
                    else return false;
				case DiseaseType.Bone:
			        if (XRayQueue.Count < XRayQueueSize)
                    {
					    XRayQueue.AddLast(patient);
					patient.SetQueueID (XRayQueue.Count - 1);
					    return true;
			        }
                    else if (addWhenFull)
                    {
                        XRayQueueSize++;
                        XRayQueue.AddLast(patient);
					patient.SetQueueID (XRayQueue.Count - 1);
                        return true;
                    }
                    else return false;
				case DiseaseType.Brain:
			        if (MRIQueue.Count < MRIQueueSize)
                    {
					
                        MRIQueue.AddLast(patient);
					patient.SetQueueID (MRIQueue.Count - 1);
                        return true;
                    }
                    else if (addWhenFull)
                    {
                        MRIQueueSize++;
                        MRIQueue.AddLast(patient);
					patient.SetQueueID (MRIQueue.Count - 1);
                        return true;
                    }
                    else return false;
				case DiseaseType.Ear:
			        if (UltrasoundQueue.Count < UltrasoundQueueSize)
                    {
                        UltrasoundQueue.AddLast(patient);
					patient.SetQueueID (UltrasoundQueue.Count - 1);
                        return true;
                    }
                    else if (addWhenFull)
                    {
                        UltrasoundQueueSize++;
                        UltrasoundQueue.AddLast(patient);
					patient.SetQueueID (UltrasoundQueue.Count - 1);
                        return true;
                    }
                    else return false;
				case DiseaseType.Kidneys:
			        if (LaserQueue.Count < LaserQueueSize)
                    {
                        LaserQueue.AddLast(patient);
					patient.SetQueueID (LaserQueue.Count - 1);
                        return true;
                    }
                    else if (addWhenFull)
                    {
                        LaserQueueSize++;
                        LaserQueue.AddLast(patient);
					patient.SetQueueID (LaserQueue.Count - 1);
                        return true;
                    }
                    else return false;
			}
		}
	    return false;
	}

	public bool AddLastToDiagnosticQueue(RefactoredHospitalPatientAI patient, bool addWhenFull = false)
	{
		HospitalCharacterInfo patientInfo = patient.GetComponent<HospitalCharacterInfo>();
		for (int i = 0; i < patientInfo.requiredMedicines.Length; ++i)
		{
			switch (patientInfo.requiredMedicines[i].Key.Disease.DiseaseType)
			{
			case DiseaseType.Lungs:
				if (LungTestingQueue2.Count < LungTestingQueueSize)
				{
					LungTestingQueue2.AddLast(patient);
					return true;
				}
				else if (addWhenFull)
				{
					LungTestingQueueSize++;
					LungTestingQueue2.AddLast(patient);
					return true;
				}
				else return false;
			case DiseaseType.Bone:
				if (XRayQueue2.Count < XRayQueueSize)
				{
					XRayQueue2.AddLast(patient);
					return true;
				}
				else if (addWhenFull)
				{
					XRayQueueSize++;
					XRayQueue2.AddLast(patient);
					return true;
				}
				else return false;
			case DiseaseType.Brain:
				if (MRIQueue2.Count < MRIQueueSize)
				{
					MRIQueue2.AddLast(patient);
					return true;
				}
				else if (addWhenFull)
				{
					MRIQueueSize++;
					MRIQueue2.AddLast(patient);
					return true;
				}
				else return false;
			case DiseaseType.Ear:
				if (UltrasoundQueue2.Count < UltrasoundQueueSize)
				{
					UltrasoundQueue2.AddLast(patient);
					return true;
				}
				else if (addWhenFull)
				{
					UltrasoundQueueSize++;
					UltrasoundQueue2.AddLast(patient);
					return true;
				}
				else return false;
			case DiseaseType.Kidneys:
				if (LaserQueue2.Count < LaserQueueSize)
				{
					LaserQueue2.AddLast(patient);
					return true;
				}
				else if (addWhenFull)
				{
					LaserQueueSize++;
					LaserQueue2.AddLast(patient);
					return true;
				}
				else return false;
			}
		}
		return false;
	}

	public bool AddToDiagnosticQueue(IDiagnosePatient patient){
		HospitalCharacterInfo patientInfo = patient.GetAI().GetComponent<HospitalCharacterInfo>();
		for (int i = 0; i < patientInfo.requiredMedicines.Length; ++i)
		{
			switch (patientInfo.requiredMedicines[i].Key.Disease.DiseaseType)
			{
			case DiseaseType.Lungs:
				if (LungTestingQueue.Count < LungTestingQueueSize)
				{
					/*if (LungTestingQueue.Count == 0) {
						LungTestingQueue.AddLast (patient);
					} else {
						for(var node = LungTestingQueue.First; node != null; node = node.Next ){
							if(node.Value.GetQueueID > patient.GetQueueID){
								LungTestingQueue.AddBefore (node, patient);
							}
						}

					}*/
					AddToSpecifiedQueue (LungTestingQueue, patient);
					return true;
				} else return false;
			case DiseaseType.Bone:
				if (XRayQueue.Count < XRayQueueSize)
				{
					AddToSpecifiedQueue (XRayQueue, patient);
					return true;
				} else return false;
			case DiseaseType.Brain:
				if (MRIQueue.Count < MRIQueueSize)
				{
					AddToSpecifiedQueue (MRIQueue, patient);
					return true;
				} else return false;
			case DiseaseType.Ear:
				if (UltrasoundQueue.Count < UltrasoundQueueSize)
				{
					AddToSpecifiedQueue (UltrasoundQueue, patient);
					return true;
				} else return false;
			case DiseaseType.Kidneys:
				if (LaserQueue.Count < LaserQueueSize)
				{
					AddToSpecifiedQueue (LaserQueue, patient);
					return true;
				} else return false;
			}
		}
		return false;
	}

	private void AddToSpecifiedQueue (LinkedList<IDiagnosePatient> Queue, IDiagnosePatient patient){
		if (Queue.Count == 0) {
			Queue.AddLast (patient);
		} else {
			for(var node = Queue.First; node != null; node = node.Next ){
				if(node.Value.GetQueueID() > patient.GetQueueID()){
					Queue.AddBefore (node, patient);
					return;
				}
			}
			//if ID is max in queue
			Queue.AddLast (patient);

		}
	}

	public void DecreaseQueueIDs(IDiagnosePatient healedPatient){
		DecreaseSpecificQueueIDs (ReturnPatientQueue (healedPatient), healedPatient.GetQueueID());
	}

	private void DecreaseSpecificQueueIDs(LinkedList<IDiagnosePatient> Queue, int min){
		for(var node = Queue.First; node != null; node = node.Next ){
			if(node.Value.GetQueueID () > min){
				node.Value.SetQueueID (node.Value.GetQueueID () - 1);
			}
		}
	}
		
	public bool QueueContainsPatient(IDiagnosePatient patient)
	{
		return ReturnPatientQueue(patient) != null;
	}

	public bool QueueContainsPatient(RefactoredHospitalPatientAI patient)
	{
		return ReturnPatientQueue2(patient) != null;
	}

	public void RemovePatientFromQueues(HospitalPatientAI patient)
	{
		LungTestingQueue.Remove(patient);
		XRayQueue.Remove(patient);
		MRIQueue.Remove(patient);
		UltrasoundQueue.Remove(patient);
		LaserQueue.Remove(patient);
	}

	public void RemovePatientFromQueues(RefactoredHospitalPatientAI patient)
	{
		LungTestingQueue2.Remove(patient);
		XRayQueue2.Remove(patient);
		MRIQueue2.Remove(patient);
		UltrasoundQueue2.Remove(patient);
		LaserQueue2.Remove(patient);
	}

	public LinkedList<IDiagnosePatient> ReturnPatientQueue(IDiagnosePatient patient)
	{
		if (LungTestingQueue.Contains(patient))
		{
			return LungTestingQueue;
		}
		if (XRayQueue.Contains(patient))
		{
			return XRayQueue;
		}
		if (MRIQueue.Contains(patient))
		{
			return MRIQueue;
		}
		if (UltrasoundQueue.Contains(patient))
		{
			return UltrasoundQueue;
		}
		if (LaserQueue.Contains(patient))
		{
			return LaserQueue;
		}

		return null;
	}

	public LinkedList<IDiagnosePatient> ReturnDiseaseQueue(int disease){
		switch (disease){
		case (int)DiseaseType.Lungs:
			return LungTestingQueue;
		case (int)DiseaseType.Bone:
			return XRayQueue;
		case (int)DiseaseType.Brain:
			return MRIQueue;
		case (int)DiseaseType.Ear:
                return UltrasoundQueue;
		case (int)DiseaseType.Kidneys:
			return LaserQueue;
		default:
			return null;
		}
	}

    public LinkedList<IDiagnosePatient> ReturnDiseaseQueue(DiagRoomType roomType)
    {
        switch (roomType)
        {
            case DiagRoomType.LungTesting:
                return LungTestingQueue;
            case DiagRoomType.XRay:
                return XRayQueue;
            case DiagRoomType.MRI:
                return MRIQueue;
            case DiagRoomType.UltraSound:
                return UltrasoundQueue;
            case DiagRoomType.Laser:
                return LaserQueue;
            default:
                return null;
        }
    }

    public DiagnosticRoom ReturnDiseaseRoom(int disease)
    {
        switch (disease)
        {
            case (int)DiseaseType.Lungs:
                if (LungTestingRoomList.Count > 0)
                {
                    return LungTestingRoomList[LungTestingRoomList.Count - 1];
                }
                else return null;
            case (int)DiseaseType.Bone:
                if (XRayRoomList.Count > 0)
                {
                    return XRayRoomList[XRayRoomList.Count - 1];
                }
                else return null;
            case (int)DiseaseType.Brain:
                if (MRIRoomList.Count > 0)
                {
                    return MRIRoomList[MRIRoomList.Count - 1];
                }
                else return null;
            case (int)DiseaseType.Ear:
                if (UltrasoundRoomList.Count > 0)
                {
                    return UltrasoundRoomList[UltrasoundRoomList.Count - 1];
                }
                else return null;
            case (int)DiseaseType.Kidneys:
                if (LaserRoomList.Count > 0)
                {
                    return LaserRoomList[LaserRoomList.Count - 1];
                }
                else return null;
            default:
                return null;
        }
    }

    public LinkedList<RefactoredHospitalPatientAI> ReturnPatientQueue2(RefactoredHospitalPatientAI patient)
	{
		if (LungTestingQueue2.Contains(patient))
		{
			return LungTestingQueue2;
		}
		if (XRayQueue2.Contains(patient))
		{
			return XRayQueue2;
		}
		if (MRIQueue2.Contains(patient))
		{
			return MRIQueue2;
		}
		if (UltrasoundQueue2.Contains(patient))
		{
			return UltrasoundQueue2;
		}
		if (LaserQueue2.Contains(patient))
		{
			return LaserQueue2;
		}

		return null;
	}

    public DiagnosticRoom ReturnPatientDiagnosisRoom(HospitalPatientAI patient)
    {
        if (LungTestingQueue.Contains(patient))
        {
            if (LungTestingRoomList.Count > 0)
            {
                return LungTestingRoomList[LungTestingRoomList.Count - 1];
            }
            else return null;
        }
        if (XRayQueue.Contains(patient))
        {
            if (XRayRoomList.Count > 0)
            {
                return XRayRoomList[XRayRoomList.Count - 1];
            }
            else return null;
        }
        if (MRIQueue.Contains(patient))
        {
            if (MRIRoomList.Count > 0)
            {
                return MRIRoomList[MRIRoomList.Count - 1];
            }
            else return null;
        }
        if (UltrasoundQueue.Contains(patient))
        {
            if (UltrasoundRoomList.Count > 0)
            {
                return UltrasoundRoomList[UltrasoundRoomList.Count - 1];
            }
            else return null;
        }
        if (LaserQueue.Contains(patient))
        {
            if (LaserRoomList.Count > 0)
            {
                return LaserRoomList[LaserRoomList.Count - 1];
            }
            else return null;
        }
        return null;
    }

    public DiagnosticRoom ReturnPatientDiagnosisRoom(IDiagnosePatient patient)
    {
        if (LungTestingQueue.Contains(patient))
        {
            if (LungTestingRoomList.Count > 0)
            {
                return LungTestingRoomList[LungTestingRoomList.Count - 1];
            }
            else return null;
        }
        if (XRayQueue.Contains(patient))
        {
            if (XRayRoomList.Count > 0)
            {
                return XRayRoomList[XRayRoomList.Count - 1];
            }
            else return null;
        }
        if (MRIQueue.Contains(patient))
        {
            if (MRIRoomList.Count > 0)
            {
                return MRIRoomList[MRIRoomList.Count - 1];
            }
            else return null;
        }
        if (UltrasoundQueue.Contains(patient))
        {
            if (UltrasoundRoomList.Count > 0)
            {
                return UltrasoundRoomList[UltrasoundRoomList.Count - 1];
            }
            else return null;
        }
        if (LaserQueue.Contains(patient))
        {
            if (LaserRoomList.Count > 0)
            {
                return LaserRoomList[LaserRoomList.Count - 1];
            }
            else return null;
        }
        return null;
    }
    /// <summary>
    /// Get the room for the diagnostics for the patient
    /// </summary>
    /// <param name="patient"></param>
    /// <returns>The room for the diagnostics for the patient</returns>
    public DiagnosticRoom ReturnPatientDiagnosisRoom(VIPPersonController patient)
	{
		if (LungTestingQueue.Contains(patient))
		{
			if (LungTestingRoomList.Count > 0)
			{
				return LungTestingRoomList[LungTestingRoomList.Count - 1];
			}
			else return null;
		}
		if (XRayQueue.Contains(patient))
		{
			if (XRayRoomList.Count > 0)
			{
				return XRayRoomList[XRayRoomList.Count - 1];
			}
			else return null;
		}
		if (MRIQueue.Contains(patient))
		{
			if (MRIRoomList.Count > 0)
			{
				return MRIRoomList[MRIRoomList.Count - 1];
			}
			else return null;
		}
		if (UltrasoundQueue.Contains(patient))
		{
			if (UltrasoundRoomList.Count > 0)
			{
				return UltrasoundRoomList[UltrasoundRoomList.Count - 1];
			}
			else return null;
		}
		if (LaserQueue.Contains(patient))
		{
			if (LaserRoomList.Count > 0)
			{
				return LaserRoomList[LaserRoomList.Count - 1];
			}
			else return null;
		}
		return null;
	}

	public void SetQueueSize(DiagRoomType roomType, int size){
		switch (roomType)
		{
		case DiagRoomType.LungTesting:
			LungTestingQueueSize = size;
			break;
		case DiagRoomType.XRay:
			XRayQueueSize = size;
			break;
		case DiagRoomType.Laser:
			LaserQueueSize = size;
			break;
		case DiagRoomType.MRI:
			MRIQueueSize = size;
			break;
		case DiagRoomType.UltraSound:
			UltrasoundQueueSize = size;
			break;
		}
	}

    public void EnlargeQueue(DiagnosticRoom room)
    {
        switch (room.RoomType)
        {
            case DiagRoomType.LungTesting:
                LungTestingQueueSize++;
                break;
            case DiagRoomType.XRay:
                XRayQueueSize++;
                break;
            case DiagRoomType.Laser:
                LaserQueueSize++;
                break;
            case DiagRoomType.MRI:
                MRIQueueSize++;
                break;
            case DiagRoomType.UltraSound:
                UltrasoundQueueSize++;
                break;
        }
    }

    public void EnlargeQueue(DiagRoomType RoomType)
    {
        switch (RoomType)
        {
            case DiagRoomType.LungTesting:
                LungTestingQueueSize++;
                break;
            case DiagRoomType.XRay:
                XRayQueueSize++;
                break;
            case DiagRoomType.Laser:
                LaserQueueSize++;
                break;
            case DiagRoomType.MRI:
                MRIQueueSize++;
                break;
            case DiagRoomType.UltraSound:
                UltrasoundQueueSize++;
                break;
        }
    }

    public void EnlargeQueue(int disease)
    {
        Debug.LogWarning("enlargeQueue" + (DiseaseType)disease);
        DiagnosticRoom[] allDiagnosisRoom = FindObjectsOfType<DiagnosticRoom>();
        foreach (DiagnosticRoom diag in allDiagnosisRoom)
        {
            switch (disease)
            {
                case (int)DiseaseType.Lungs:
                    if (diag.RoomType == DiagRoomType.LungTesting)
                    {
                        EnlargeQueue(diag.RoomType);
                        diag.EnlargeQueue();
                    }
                    break;
                case (int)DiseaseType.Bone:
                    if (diag.RoomType == DiagRoomType.XRay)
                    {
                        EnlargeQueue(diag.RoomType);
                        diag.EnlargeQueue();
                    }
                    break;
                case (int)DiseaseType.Kidneys:
                    if (diag.RoomType == DiagRoomType.Laser)
                    {
                        EnlargeQueue(diag.RoomType);
                        diag.EnlargeQueue();
                    }
                    break;
                case (int)DiseaseType.Brain:
                    if (diag.RoomType == DiagRoomType.MRI)
                    {
                        EnlargeQueue(diag.RoomType);
                        diag.EnlargeQueue();
                    }
                    break;
                case (int)DiseaseType.Ear:
                    if (diag.RoomType == DiagRoomType.UltraSound)
                    {
                        EnlargeQueue(diag.RoomType);
                        diag.EnlargeQueue();
                    }
                    break;
            }
        }
    }

    public bool isRoomExistForPatient(HospitalPatientAI patient)
    {
        HospitalCharacterInfo patientInfo = patient.GetComponent<HospitalCharacterInfo>();
        for (int i = 0; i < patientInfo.requiredMedicines.Length; ++i)
        {
            switch (patientInfo.requiredMedicines[i].Key.Disease.DiseaseType)
            {
                case DiseaseType.Lungs:
                    if (LungTestingRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
                case DiseaseType.Bone:
                    if (XRayRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
                case DiseaseType.Brain:
                    if (MRIRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
                case DiseaseType.Ear:
                    if (UltrasoundRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
                case DiseaseType.Kidneys:
                    if (LaserRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
            }
        }
        return false;
    }

    public bool isRoomExistForPatient(VIPPersonController patient)
    {
        HospitalCharacterInfo patientInfo = patient.GetComponent<HospitalCharacterInfo>();
        for (int i = 0; i < patientInfo.requiredMedicines.Length; ++i)
        {
            switch (patientInfo.requiredMedicines[i].Key.Disease.DiseaseType)
            {
                case DiseaseType.Lungs:
                    if (LungTestingRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
                case DiseaseType.Bone:
                    if (XRayRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
                case DiseaseType.Brain:
                    if (MRIRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
                case DiseaseType.Ear:
                    if (UltrasoundRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
                case DiseaseType.Kidneys:
                    if (LaserRoomList.Count > 0)
                    {
                        return true;
                    }
                    else return false;
            }
        }
        return false;
    }

    public void ShowMessageRoomNeededForPatient(HospitalPatientAI patient)
    {
        HospitalCharacterInfo patientInfo = patient.GetComponent<HospitalCharacterInfo>();
        for (int i = 0; i < patientInfo.requiredMedicines.Length; ++i)
        {
            switch (patientInfo.GetComponent<HospitalCharacterInfo>().requiredMedicines[i].Key.Disease.DiseaseType)
            {
                case DiseaseType.Lungs:
                    MessageController.instance.ShowMessage(29);
                    return;
                case DiseaseType.Bone:
                    MessageController.instance.ShowMessage(28);
                    return;
                case DiseaseType.Brain:
                    MessageController.instance.ShowMessage(30);
                    return;
                case DiseaseType.Ear:
                    MessageController.instance.ShowMessage(31);
                    return;
                case DiseaseType.Kidneys:
                    MessageController.instance.ShowMessage(32);
                    return;
                default:
                    break;
            }
        }
    }

    public void ShowMessageRoomNeededForPatient(VIPPersonController patient)
    {
        HospitalCharacterInfo patientInfo = patient.GetComponent<HospitalCharacterInfo>();
        for (int i = 0; i < patientInfo.requiredMedicines.Length; ++i)
        {
            switch (patientInfo.GetComponent<HospitalCharacterInfo>().requiredMedicines[i].Key.Disease.DiseaseType)
            {
                case DiseaseType.Lungs:
                    MessageController.instance.ShowMessage(29);
                    return;
                case DiseaseType.Bone:
                    MessageController.instance.ShowMessage(28);
                    return;
                case DiseaseType.Brain:
                    MessageController.instance.ShowMessage(30);
                    return;
                case DiseaseType.Ear:
                    MessageController.instance.ShowMessage(31);
                    return;
                case DiseaseType.Kidneys:
                    MessageController.instance.ShowMessage(32);
                    return;
                default:
                    break;
            }
        }
    }


    public int NeededPositiveEnergy(int disease)
    {
            switch (disease)
            {
                case (int)DiseaseType.Lungs:
                    return LungTestingRoom.GetPositiveEnergyCost();
                case (int)DiseaseType.Bone:
                    return XRayRoom.GetPositiveEnergyCost();
                case (int)DiseaseType.Kidneys:
                    return LaserRoom.GetPositiveEnergyCost();
                case (int)DiseaseType.Brain:
                    return MRIRoom.GetPositiveEnergyCost();
                case (int)DiseaseType.Ear:
                    return UltrasoundRoom.GetPositiveEnergyCost();
                default:
                    return 1;
            }
    }

    public int NeededPositiveEnergy(DiagRoomType roomType)
    {
        switch (roomType)
        {
            case DiagRoomType.LungTesting:
                return LungTestingRoom.GetPositiveEnergyCost();
            case DiagRoomType.XRay:
                return XRayRoom.GetPositiveEnergyCost();
            case DiagRoomType.Laser:
                return LaserRoom.GetPositiveEnergyCost();
            case DiagRoomType.MRI:
                return MRIRoom.GetPositiveEnergyCost();
            case DiagRoomType.UltraSound:
                return UltrasoundRoom.GetPositiveEnergyCost();
            default:
                return 1;
        }
    }

    /*public int NeededPositiveEnergy(IDiagnosePatient patient){
		HospitalCharacterInfo patientInfo = patient.GetComponent<HospitalCharacterInfo>();
		for (int i = 0; i < patientInfo.RequiredCures.Count; ++i)
		{
			switch (patientInfo.RequiredCures[i].Disease.DiseaseType)
			{
			case DiseaseType.Lungs:
				return LungTestingRoom.PositiveEnergyCost;
			case DiseaseType.Bone:
				return XRayRoom.PositiveEnergyCost;
			case DiseaseType.Brain:
				return LaserRoom.PositiveEnergyCost;
			case DiseaseType.Ear:
				return MRIRoom.PositiveEnergyCost;
			case DiseaseType.Kidneys:
				return UltrasoundRoom.PositiveEnergyCost;
			}
		}
		return 1;
	}*/

    public int NeededCureTime(int disease)
    {
        switch (disease)
        {
            case (int)DiseaseType.Lungs:
                if (LungTestingRoomList.Count > 0)
                {
                    return LungTestingRoomList[LungTestingRoomList.Count - 1].DiagnosisTimeMastered;
                } else {
                    return LungTestingRoom.CureTime;
                }
            case (int)DiseaseType.Bone:
                if (XRayRoomList.Count > 0)
                {
                    return XRayRoomList[XRayRoomList.Count - 1].DiagnosisTimeMastered;
                }
                else {
                    return XRayRoom.CureTime;
                }
            case (int)DiseaseType.Kidneys:
                if (LaserRoomList.Count > 0)
                {
                    return LaserRoomList[LaserRoomList.Count - 1].DiagnosisTimeMastered;
                }
                else {
                    return LaserRoom.CureTime;
                }
            case (int)DiseaseType.Brain:
                if (MRIRoomList.Count > 0)
                {
                    return MRIRoomList[MRIRoomList.Count - 1].DiagnosisTimeMastered;
                }
                else {
                    return MRIRoom.CureTime;
                }
            case (int)DiseaseType.Ear:
                if (UltrasoundRoomList.Count > 0)
                {
                    return UltrasoundRoomList[UltrasoundRoomList.Count - 1].DiagnosisTimeMastered;
                }
                else {
                    return UltrasoundRoom.CureTime;
                }
            default:
                return 1;
        }
    }

    public DiagRoomType ReturnDiseaseTypeRoomType(int disease)
    {
        switch (disease)
        {
            case (int)DiseaseType.Lungs:
                return DiagRoomType.LungTesting;
            case (int)DiseaseType.Bone:
                return DiagRoomType.XRay;
            case (int)DiseaseType.Kidneys:
                return DiagRoomType.Laser;
            case (int)DiseaseType.Brain:
                return DiagRoomType.MRI;
            case (int)DiseaseType.Ear:
                return DiagRoomType.UltraSound;
        }
        return DiagRoomType.XRay;
    }

    public int GetQueueSize(DiagRoomType RoomType)
    {
        switch (RoomType)
        {
            case DiagRoomType.LungTesting:
                return LungTestingQueueSize;
            case DiagRoomType.XRay:
                return XRayQueueSize;
            case DiagRoomType.Laser:
                return LaserQueueSize;
            case DiagRoomType.MRI:
                return MRIQueueSize;
            case DiagRoomType.UltraSound:
                return UltrasoundQueueSize;
            default:
                return 2;
        }
    }

    public int GetQueueSize(int disease)
    {
        switch (disease)
        {
            case (int)DiseaseType.Lungs:
                return LungTestingQueueSize;
            case (int)DiseaseType.Bone:
                return XRayQueueSize;
            case (int)DiseaseType.Kidneys:
                return LaserQueueSize;
            case (int)DiseaseType.Brain:
                return MRIQueueSize;
            case (int)DiseaseType.Ear:
                return UltrasoundQueueSize;
            default:
                return 2;
       }
    }

    public void CheckAllDiagRooms() {
        if (LungTestingRoomList.Count > 0) {
            LungTestingRoomList[LungTestingRoomList.Count - 1].CheckToShowIndicator();
        }
		
		if (XRayRoomList.Count > 0) {
			XRayRoomList[XRayRoomList.Count - 1].CheckToShowIndicator();
		}
		
		if(MRIRoomList.Count > 0) {
			MRIRoomList[MRIRoomList.Count - 1].CheckToShowIndicator();
		}

		if (UltrasoundRoomList.Count > 0) {
			UltrasoundRoomList[UltrasoundRoomList.Count - 1].CheckToShowIndicator();
		}

		if (LaserRoomList.Count > 0) { 
			LaserRoomList[LaserRoomList.Count - 1].CheckToShowIndicator();
		}
			
    }

    public bool DiagnosisRoomIsBuilt(DiagRoomType RoomType) {
        bool roomBuilt = false;
        switch (RoomType)
        {
            case DiagRoomType.LungTesting:
                if (LungTestingRoomList.Count > 0) {
                    roomBuilt = true;
                }
                break;
            case DiagRoomType.XRay:
                if (LungTestingRoomList.Count > 0)
                {
                    roomBuilt = true;
                }
                break;
            case DiagRoomType.Laser:
                if (LungTestingRoomList.Count > 0)
                {
                    roomBuilt = true;
                }
                break;
            case DiagRoomType.MRI:
                if (LungTestingRoomList.Count > 0)
                {
                    roomBuilt = true;
                }
                break;
            case DiagRoomType.UltraSound:
                if (LungTestingRoomList.Count > 0)
                {
                    roomBuilt = true;
                }
                break;
            default:
                roomBuilt = false;
                break;
        }

        return roomBuilt;
    }

    public int GetPatientCounterForDiagnoseRoom(DiagRoomType RoomType)
    {
        int counter = 0;

        for (int i = 0; i < HospitalPatientAI.Patients.Count; i++)
        {
            if (HospitalPatientAI.Patients[i].RequiresDiagnosis && !HospitalDataHolder.Instance.QueueContainsPatient(HospitalPatientAI.Patients[i].GetComponent<HospitalPatientAI>()) && DiseaseMatchesRoom(HospitalPatientAI.Patients[i].DisaseDiagnoseType, RoomType) && HospitalPatientAI.Patients[i].GetComponent<HospitalPatientAI>().Person.State is HospitalPatientAI.InBed)
                counter++;
        }

        if (Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip != null)
        {
            if (Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<HospitalCharacterInfo>() != null)
            {
                if (Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis && !HospitalDataHolder.Instance.QueueContainsPatient(Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<VIPPersonController>()) && DiseaseMatchesRoom(Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<HospitalCharacterInfo>().DisaseDiagnoseType, RoomType) && Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<VIPPersonController>().Person.State is VIPPersonController.InBed)
                    counter++;
            }
        }
        return counter;
    }

    public string GetDiagnosisMachineName(HospitalCharacterInfo patient)
    {
        string diagnoseMachineName = "";

        switch ((int)patient.DisaseDiagnoseType)
        {
            case (int)DiseaseType.Brain:
                diagnoseMachineName = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/MRI_SCANNER").ToUpper();
                break;
            case (int)DiseaseType.Bone:
                diagnoseMachineName = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/X_RAY").ToUpper();
                break;
            case (int)DiseaseType.Ear:
                diagnoseMachineName = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/ULTRASOUND_STATION").ToUpper();
                break;
            case (int)DiseaseType.Lungs:
                diagnoseMachineName = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/LUNG_TESTING_STATION").ToUpper();
                break;
            case (int)DiseaseType.Kidneys:
                diagnoseMachineName = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/LASER_SCANNER").ToUpper();
                break;
            default:
                diagnoseMachineName = "?";
                Debug.Log("Incorrect diagnosis type: " + (int)patient.DisaseDiagnoseType);
                break;
        }

        return diagnoseMachineName;
    }

    public bool DiagnosisPatientExists(DiagRoomType RoomType) {
        for (int i = 0; i < HospitalPatientAI.Patients.Count; i++)
        {
            if (HospitalPatientAI.Patients[i].RequiresDiagnosis && !HospitalDataHolder.Instance.QueueContainsPatient(HospitalPatientAI.Patients[i].GetComponent<HospitalPatientAI>()) && DiseaseMatchesRoom(HospitalPatientAI.Patients[i].DisaseDiagnoseType, RoomType) && HospitalPatientAI.Patients[i].GetComponent<HospitalPatientAI>().Person.State is HospitalPatientAI.InBed)
            {
                return true;
            }
        }

        if (Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip != null)
        {
            if (Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<HospitalCharacterInfo>() != null)
            {
                if (Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<HospitalCharacterInfo>().RequiresDiagnosis && !HospitalDataHolder.Instance.QueueContainsPatient(Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<VIPPersonController>())  && DiseaseMatchesRoom(Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<HospitalCharacterInfo>().DisaseDiagnoseType, RoomType) && Hospital.HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<VIPPersonController>().Person.State is VIPPersonController.InBed)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool DiseaseMatchesRoom(DiseaseType disease, HospitalDataHolder.DiagRoomType roomType) {
        if (disease == DiseaseType.Bone && roomType == HospitalDataHolder.DiagRoomType.XRay)
            return true;
        if (disease == DiseaseType.Brain && roomType == HospitalDataHolder.DiagRoomType.MRI)
            return true;
        if (disease == DiseaseType.Ear && roomType == HospitalDataHolder.DiagRoomType.UltraSound)
            return true;
        if (disease == DiseaseType.Lungs && roomType == HospitalDataHolder.DiagRoomType.LungTesting)
            return true;
        if (disease == DiseaseType.Kidneys && roomType == HospitalDataHolder.DiagRoomType.Laser)
            return true;

        return false;
    }

    public int GetDiagnosedPatients()
    {
        int count = 0;

        if (LungTestingRoomList.Count > 0)
            count += LungTestingRoomList[LungTestingRoomList.Count - 1].DiagnosedPatients;
        if (XRayRoomList.Count > 0)
            count += XRayRoomList[XRayRoomList.Count - 1].DiagnosedPatients;
        if (MRIRoomList.Count > 0)
            count += MRIRoomList[MRIRoomList.Count - 1].DiagnosedPatients;
        if (UltrasoundRoomList.Count > 0)
            count += UltrasoundRoomList[UltrasoundRoomList.Count - 1].DiagnosedPatients;
        if (LaserRoomList.Count > 0)
            count += LaserRoomList[LaserRoomList.Count - 1].DiagnosedPatients;

        return count;
    }

    public enum DiagRoomType
	{
        LungTesting,
		XRay,
		MRI,
		UltraSound,
		Laser
	}

}
