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
public class DailyQuestNotificationCenter
{
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
		public event UpdateHandler DailyQuestUpdate;
		/// <summary>
		/// Method used to send Notification to system.
		/// </summary>
		/// <param name="sender">In most uses it will be "this". Represents object sending notification. Not sure if needed. Probably will be deleted.</param>
		/// <param name="eventArgs">Object representing information about event. </param>
		public void Invoke(EventArgs eventArgs)
		{
            if (Notification != null)
				Notification(eventArgs);
		}

		public void InvokeUpdate()
		{
			if (DailyQuestUpdate != null)
                DailyQuestUpdate();
		}
	}

	#region Notifiers
	public readonly Notifier<DailyQuestProgressEventArgs> dailyQuestTaskUpdate = new Notifier<DailyQuestProgressEventArgs>();
    #endregion

    #region constructors and initialization
    private DailyQuestNotificationCenter() { }

	private static DailyQuestNotificationCenter instance = null;

	public static DailyQuestNotificationCenter Instance
	{
		get
		{
			if (instance == null)
				instance = new DailyQuestNotificationCenter();

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
		instance = new DailyQuestNotificationCenter();

	}
		
	#endregion

}

#region DailyQuestNotificationCenterEventArgs classes

public class DailyQuestProgressEventArgs : BaseNotificationEventArgs
{
	public readonly int amount;
    public readonly DailyTask.DailyTaskType taskType;
    public readonly string roomTag;

    public DailyQuestProgressEventArgs(int amount,DailyTask.DailyTaskType taskType,string roomTag = null, string message = null) : base(message) 
	{
        this.amount = amount;
        this.taskType = taskType;
        this.roomTag = roomTag;
    }
}

#endregion