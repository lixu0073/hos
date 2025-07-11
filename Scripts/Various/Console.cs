using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A console to display Unity's debug logs in-game.
/// </summary>
public class Console : MonoBehaviour
{
	struct Log
	{
		public string message;
		public string stackTrace;
		public LogType type;
	}

	/// <summary>
	/// The hotkey to show and hide the console window.
	/// </summary>
	public KeyCode toggleKey = KeyCode.BackQuote;
    public bool consoleOnTheStart;

	List<Log> logs = new List<Log>();
	Vector2 scrollPosition;
	bool show;
	bool collapse;

	// Visual elements:

	static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
	{
		{ LogType.Assert, Color.white },
		{ LogType.Error, Color.red },
		{ LogType.Exception, Color.red },
		{ LogType.Log, Color.white },
		{ LogType.Warning, Color.yellow },
	};

	const int margin = 100;

	Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
	//Rect titleBarRect = new Rect(0, 0, 10000, 20);
	GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
	GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

    void Start()
    {
        show |= consoleOnTheStart;
    }

    void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	public void toggleVisible()
	{
		show = !show;
	}

    void OnGUI()
	{
		if (!show)
			return;

		windowRect = GUILayout.Window(123456, windowRect, ConsoleWindow, "Console");
	}

	public Texture2D bg;

	/// <summary>
	/// A window that displayss the recorded logs.
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	void ConsoleWindow(int windowID)
	{
		GUI.skin.window.normal.background = bg;
		GUI.skin.window.active.background = bg;
		GUI.skin.window.focused.background = bg;
		GUI.skin.window.hover.background = bg;
		GUI.skin.window.onNormal.background = bg;
		GUI.skin.window.onActive.background = bg;
		GUI.skin.window.onFocused.background = bg;
		GUI.skin.window.onHover.background = bg;

		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		// Iterate through the recorded logs.
		for (int i = 0; i < logs.Count; ++i)
		{
			var log = logs[i];

			// Combine identical messages if collapse option is chosen.
			if (collapse)
			{
				var messageSameAsPrevious = i > 0 && log.message == logs[i - 1].message;

				if (messageSameAsPrevious)
					continue;
			}

			GUI.contentColor = logTypeColors[log.type];
			GUILayout.Label(log.message);
			GUILayout.Label(log.stackTrace);
		}

		GUILayout.EndScrollView();

		GUI.contentColor = Color.white;

		GUILayout.BeginHorizontal();

		if (GUILayout.Button(clearLabel))
			logs.Clear();

		collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));

		GUILayout.EndHorizontal();

		// Allow the window to be dragged by its title bar.
	    // GUI.DragWindow(titleBarRect);
	}

	/// <summary>
	/// Records a log from the log callback.
	/// </summary>
	/// <param name="message">Message.</param>
	/// <param name="stackTrace">Trace of where the message came from.</param>
	/// <param name="type">Type of message (error, exception, warning, assert).</param>
	void HandleLog(string message, string stackTrace, LogType type)
	{
        if (type != LogType.Error)
            return;

		logs.Add(new Log()
		{
			message = message,
			stackTrace = stackTrace,
			type = type,
		});
	}
}