using UnityEngine;
using System.Collections;
using Hospital;

/// <summary>
/// Class designed to take care about gathering information about what is happening in entire game. Contains several Notifiers that enables you to send notification of desired type to every listner, or be such a listener.
/// Example use:
/// NotificationCenter.Get().ResourceAmountChanged.Notifications+=MyListenerMethod;
/// or
/// NotificationCenter.Get().ResourceAmountChanged.Invoke(this,new ResourceAmountChangedEventArgs(...));
/// </summary>
/// 
public class AchievementNotificationCenter {
	/// <summary>
	/// Generic Class responsible for commanding event and sending it's notifications.
	/// If you want to add new NotificationType to NotificationCenter - use this class. Just write your own EventArgs basend on provided BaseNotificationEventArgs and voila.
	/// </summary>
	/// <typeparam name="Sender"></typeparam>
	/// <typeparam name="EventArgs"></typeparam>
	public class Notifier<EventArgs>
		where EventArgs : BaseNotificationEventArgs
	{
		public delegate void EventHandler(EventArgs eventArgs);
		public delegate void UpdateHandler ();

		public event EventHandler Notification;
		public event UpdateHandler AchieveUpdate;
		/// <summary>
		/// Method used to send Notification to system.
		/// </summary>
		/// <param name="sender">In most uses it will be "this". Represents object sending notification. Not sure if needed. Probably will be deleted.</param>
		/// <param name="eventArgs">Object representing information about event. </param>
		public void Invoke(EventArgs eventArgs)
		{
            if (AreaMapController.Map.VisitingMode)
            {
                return;
            }

            //Debug.LogWarning("Invoke notification");
            if (Notification != null)
			{
				//Debug.LogWarning("Notification not null!");
				Notification(eventArgs);
			}
		}

		public void InvokeUpdate()
		{
            if (HospitalAreasMapController.HospitalMap.VisitingMode) {
                return;
            }

			if (AchieveUpdate != null)
			{
				AchieveUpdate();
			}
		}
	}

	#region Notifiers
	public readonly Notifier<AchievementProgressEventArgs> CureProduced = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> PatientInClinicCured = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> CoinsMadeOutOfPharmacySales = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> CoinsMadeByCuringClinicPatients = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> PatientDiagnosed = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> KidCured = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementVIPInfoEventArgs> VIPArrived = new Notifier<AchievementVIPInfoEventArgs>();
	public readonly Notifier<AchievementVIPInfoEventArgs> VIPCured = new Notifier<AchievementVIPInfoEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> CureLabBuilt = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> TrailerWatched = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<TimedAchievementProgressEventArgs> ElixirCollected = new Notifier<TimedAchievementProgressEventArgs>();
	public readonly Notifier<TimedAchievementProgressEventArgs> TreatmentCenterPatientCured = new Notifier<TimedAchievementProgressEventArgs>(); 
	public readonly Notifier<AchievementProgressEventArgs> PanaceaCollectorUpgraded = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> AdPlaced = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> ClinicRoomBuilt = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> CoinsInvestedInDecorating = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> MedicalPlantsPicked = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> MedicalFungiPicked = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<AchievementProgressEventArgs> HospitalExpanded = new Notifier<AchievementProgressEventArgs>();
	public readonly Notifier<TimedAchievementProgressEventArgs> BubblePopped = new Notifier<TimedAchievementProgressEventArgs>();
    public readonly Notifier<AchievementProgressEventArgs> NoYouDont = new Notifier<AchievementProgressEventArgs>();

    #endregion

    #region constructors and initialization
    private AchievementNotificationCenter() { }

	private static AchievementNotificationCenter instance = null;

	public static AchievementNotificationCenter Instance
	{
		get
		{
			if (instance == null)
				instance = new AchievementNotificationCenter();

			return instance;
		}
	}

	//Doesnt it need a rework or is it a solid solution?
	public static void UnsuscribeAllNotification()
	{
		if (instance != null)
		{
			instance = null;
			// http://www.informit.com/articles/article.aspx?p=101722&seqNum=2
			System.GC.Collect();
		}
		instance = new AchievementNotificationCenter();

	}
		
	#endregion

}

#region AchievementNotificationEventArgs classes

public class FinishedBuildingEventArgs : BaseNotificationEventArgs
{
	public readonly RotatableObject Obj;
	public FinishedBuildingEventArgs(RotatableObject rObj, string message = null) : base(message)
	{
		this.Obj = rObj;

		/*if (this.Obj.GetType () == typeof(DoctorRoom)) {
			AchievementNotificationCenter.Instance.ClinicRoomBuilt.Invoke (new AchievementProgressEventArgs (1));
		} else if (this.Obj.GetType () == typeof(MedicineProductionMachine)) {
			AchievementNotificationCenter.Instance.CureLabBuilt.Invoke (new AchievementProgressEventArgs (1));
		}*/
	}
}

public class PlantPickedEventArgs : BaseNotificationEventArgs
{
	public PlantPickedEventArgs(string message = null) : base (message)
	{
		Debug.Log ("Plant Picked");
		AchievementNotificationCenter.Instance.MedicalPlantsPicked.Invoke (new AchievementProgressEventArgs (1));
	}
}



public class TimedAchievementProgressEventArgs : BaseNotificationEventArgs
{
	public readonly int amount;
	public readonly int occurred;
	public TimedAchievementProgressEventArgs(int amount, int occurred, string message = null) : base(message) { this.amount = amount; this.occurred = occurred;  }
}

public class AchievementProgressEventArgs : BaseNotificationEventArgs
{
	public readonly int amount;
	public AchievementProgressEventArgs(int amount, string message = null) : base(message) 
	{ this.amount = amount; }
}

public class AchievementVIPInfoEventArgs : BaseNotificationEventArgs
{
	public BaseCharacterInfo info;
	public readonly int occurred;
	public AchievementVIPInfoEventArgs(BaseCharacterInfo info, int occurred, string message = null) : base(message) { this.info = info; this.occurred = occurred; }
}

#endregion