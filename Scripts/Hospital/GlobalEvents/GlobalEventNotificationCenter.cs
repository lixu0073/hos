using UnityEngine;
using System.Collections;
using Hospital;


public class GlobalEventNotificationCenter
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
    public readonly Notifier<GlobalEventCurePatientProgressEventArgs> CurePatientGlobalEvent = new Notifier<GlobalEventCurePatientProgressEventArgs>();
    public readonly Notifier<GlobalEventCollectProgressEventArgs> CollectGlobalEvent = new Notifier<GlobalEventCollectProgressEventArgs>();

    public readonly Notifier<GlobalEventOnStateChangeEventArgs> OnEventStart = new Notifier<GlobalEventOnStateChangeEventArgs>();
    public readonly Notifier<GlobalEventOnStateChangeEventArgs> OnEventReload = new Notifier<GlobalEventOnStateChangeEventArgs>();
    public readonly Notifier<GlobalEventOnStateChangeEventArgs> OnEventEnd = new Notifier<GlobalEventOnStateChangeEventArgs>();
    #endregion

    #region constructors and initialization
    private GlobalEventNotificationCenter() { }

    private static GlobalEventNotificationCenter instance = null;

    public static GlobalEventNotificationCenter Instance
    {
        get
        {
            if (instance == null)
                instance = new GlobalEventNotificationCenter();

            return instance;
        }
    }

    #endregion

}

#region GlobalEventNotificationCenter classes

public class GlobalEventCurePatientProgressEventArgs : BaseNotificationEventArgs
{

    public readonly string rotatableTag;

    public GlobalEventCurePatientProgressEventArgs(string rotatableTag, string message = null) : base(message)
    {
        if (HospitalAreasMapController.HospitalMap.VisitingMode)
        {
            return;
        }
        this.rotatableTag = rotatableTag;
    }
}

public class GlobalEventCollectProgressEventArgs : BaseNotificationEventArgs
{
    public GlobalEventCollectProgressEventArgs(string message = null)// : base(message)
    {
    }
}

public class GlobalEventOnStateChangeEventArgs : BaseNotificationEventArgs
{
    public GlobalEvent.GlobalEventType globalEventType;
    public GlobalEvent.GlobalEventExtras globalEventExtras;

    public GlobalEventOnStateChangeEventArgs(GlobalEvent.GlobalEventType globalEventType, GlobalEvent.GlobalEventExtras globalEventExtras, string message = null)// : base(message)
    {
        this.globalEventType = globalEventType;
        this.globalEventExtras = globalEventExtras;
    }
}

#endregion