using UnityEngine;

namespace Hospital{
	public class ComicCloudMessagesDatabase : ScriptableObject {
		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/ComicCloudMessagesDatabase")]
		public static void CreateAsset()
		{
			ScriptableObjectUtility.CreateAsset<ComicCloudMessagesDatabase>();
		}
		#endif
		public MessageGroup[] messageGroups;

		[System.Serializable]
		public class MessageGroup{
			public CloudsManager.MessageType messageType;
			public Message[] messages;
		}
		[System.Serializable]
		public class Message{
			public string key;
			public CloudsManager.MessageGiftType giftType;
		}
	}
}
