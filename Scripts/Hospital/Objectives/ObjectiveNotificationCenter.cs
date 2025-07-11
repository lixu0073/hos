using UnityEngine;
using System.Collections;
using Hospital;


public class ObjectiveNotificationCenter
{
    public class Notifier<EventArgs>
        where EventArgs : BaseNotificationEventArgs
    {
        public delegate void EventHandler(EventArgs eventArgs);
        public delegate void UpdateHandler();

        public event EventHandler Notification;
        public event UpdateHandler GoalsUpdate;

        public void Invoke(EventArgs eventArgs)
        {
            if (AreaMapController.Map.VisitingMode)
                return;

            if (Notification != null)
                Notification(eventArgs);
        }

        public void InvokeUpdate()
        {
            if (GoalsUpdate != null)
                GoalsUpdate();
        }
    }

    #region Notifiers
    public readonly Notifier<ObjectiveEventArgs> DefaultObjectiveUpdate = new Notifier<ObjectiveEventArgs>();
    public readonly Notifier<ObjectiveRotatableEventArgs> RotatableBuildObjectiveUpdate = new Notifier<ObjectiveRotatableEventArgs>();
    public readonly Notifier<ObjectiveRotatableEventArgs> RenovateSpecialObjectiveUpdate = new Notifier<ObjectiveRotatableEventArgs>();
    public readonly Notifier<ObjectiveExpandAreaEventArgs> ExpandAreaObjectiveUpdate = new Notifier<ObjectiveExpandAreaEventArgs>();
    public readonly Notifier<ObjectiveDoctorPatientEventArgs> DoctorPatientObjectiveUpdate = new Notifier<ObjectiveDoctorPatientEventArgs>();
    public readonly Notifier<ObjectiveHospitalPatientWithDiseaseEventArgs> HospitalPatientWithDiseaseObjectiveUpdate = new Notifier<ObjectiveHospitalPatientWithDiseaseEventArgs>();
    public readonly Notifier<ObjectiveHospitalPatientWithInfoEventArgs> HospitalPatientWithInfoObjectiveUpdate = new Notifier<ObjectiveHospitalPatientWithInfoEventArgs>();
    public readonly Notifier<ObjectiveEventArgs> DiagnosePatientObjectiveUpdate = new Notifier<ObjectiveEventArgs>();
    public readonly Notifier<ObjectiveDiagnosePatientWithDiseaseEventArgs> DiagnosePatientWithDiseaseObjectiveUpdate = new Notifier<ObjectiveDiagnosePatientWithDiseaseEventArgs>();
    public readonly Notifier<ObjectiveEventArgs> LevelUpObjectiveUpdate = new Notifier<ObjectiveEventArgs>();
    public readonly Notifier<ObjectiveTreatmentHelpRequestEventArgs> TreatmentHelpRequestObjectiveUpdate = new Notifier<ObjectiveTreatmentHelpRequestEventArgs>();

    #endregion

    #region constructors and initialization
    private ObjectiveNotificationCenter() { }

    private static ObjectiveNotificationCenter instance = null;

    public static ObjectiveNotificationCenter Instance
    {
        get
        {
            if (instance == null)
                instance = new ObjectiveNotificationCenter();

            return instance;
        }
    }

    #endregion

}

#region ObjectivesNotificationCenterEventArgs classes

public class ObjectiveEventArgs : BaseNotificationEventArgs
{
    public readonly int amount;

    public ObjectiveEventArgs(int amount, string message = null) : base(message)
    {
        this.amount = amount;
    }
}

public class ObjectiveRotatableEventArgs : BaseNotificationEventArgs
{
    public enum EventType 
    {
        StartBuilding,
        Unwrap
    };

    public readonly int amount;
    public readonly string rotatableTag;
    public EventType eventType = EventType.StartBuilding;

    public ObjectiveRotatableEventArgs(int amount, string rotatableTag, EventType eventType = EventType.StartBuilding, string message = null) : base(message)
    {
        this.amount = amount;
        this.rotatableTag = rotatableTag;
        this.eventType = eventType;
    }
}

public class ObjectiveExpandAreaEventArgs : BaseNotificationEventArgs
{
    public readonly int amount;
    public readonly HospitalArea area;

    public ObjectiveExpandAreaEventArgs(int amount, HospitalArea area, string message = null) : base(message)
    {
        this.amount = amount;
        this.area = area;
    }
}

public class ObjectiveDoctorPatientEventArgs : BaseNotificationEventArgs
{
    public readonly int amount;
    public readonly string rotatableTag;
    public readonly bool isKid;

    public ObjectiveDoctorPatientEventArgs(int amount, string rotatableTag, bool isKid, string message = null) : base(message)
    {
        this.amount = amount;
        this.rotatableTag = rotatableTag;
        this.isKid = isKid;
    }
}

public class ObjectiveHospitalPatientWithDiseaseEventArgs : BaseNotificationEventArgs
{
    public readonly int amount;
    public readonly DiseaseType diseaseType;
    public readonly bool needPlantToCure;

    public ObjectiveHospitalPatientWithDiseaseEventArgs(int amount, DiseaseType diseaseType, bool needPlantToCure, string message = null) : base(message)
    {
        this.amount = amount;
        this.diseaseType = diseaseType;
        this.needPlantToCure = needPlantToCure;
    }
}


public class ObjectiveHospitalPatientWithInfoEventArgs : BaseNotificationEventArgs
{
    public readonly int amount;
    public readonly BloodType bloodType;
    public readonly int sex;
    public readonly bool isVip;

    public ObjectiveHospitalPatientWithInfoEventArgs(int amount, BloodType bloodType, int sex, bool isVIP, string message = null) : base(message)
    {
        this.amount = amount;
        this.bloodType = bloodType;
        this.sex = sex;
        this.isVip = isVIP;
    }
}

public class ObjectiveDiagnosePatientWithDiseaseEventArgs : BaseNotificationEventArgs
{
    public readonly int amount;
    public readonly DiseaseType diseaseType;

    public ObjectiveDiagnosePatientWithDiseaseEventArgs(int amount, DiseaseType diseaseType,string message = null) : base(message)
    {
        this.amount = amount;
        this.diseaseType = diseaseType;
    }
}

public class ObjectiveTreatmentHelpRequestEventArgs : BaseNotificationEventArgs
{
    public readonly int amount;

    public ObjectiveTreatmentHelpRequestEventArgs(int amount, string message = null) : base(message)
    {
        this.amount = amount;
    }
}

#endregion