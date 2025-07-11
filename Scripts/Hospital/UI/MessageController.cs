using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageController : MonoBehaviour {

    public static MessageController instance;
    public GameObject messagePrefab;
    int poolAmount = 1;
    Queue<Message> messageQueue;
    public Message lastMessage;
    public List<string> TextValues;
    private bool isOverride = false;
    private int override_id = 0;
    private int stacked_id = 0;
    void Awake() {
        instance = this;
        PoolMessages();
    }

    void PoolMessages() {
        messageQueue = new Queue<Message>();
        for (int i = 0; i < poolAmount; i++) {
            Message m = Instantiate(messagePrefab).GetComponent<Message>();
            messageQueue.Enqueue(m);
            m.transform.SetParent(transform);
            m.transform.localScale = Vector3.one;
            m.gameObject.SetActive(false);
        }
    }
    
    public void OverrideAllMessagesTo(int id)
    {
        isOverride = true;
        this.override_id = id;
    }

    public void DisableOverrideMessages()
    {
        isOverride = false;
        this.override_id = -1;
    }

    public void ShowMessage(string text) {

        Debug.Log("ShowMessage");
        transform.SetAsLastSibling();
        lastMessage = messageQueue.Dequeue();
        messageQueue.Enqueue(lastMessage);

        lastMessage.gameObject.SetActive(true);
        lastMessage.Initialize(text);
    }

    public void ShowLockedMastershipMessage() {
        ShowMessage(I2.Loc.ScriptLocalization.Get("UNLOCK_MASTERSHIP").Replace("{0}", DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel.ToString()));
    }

    public void ShowMessage(int id, bool toUpperCase = false)
    {
        if (isOverride)
        {
            id = this.override_id;
        }

        transform.SetAsLastSibling();
        lastMessage = messageQueue.Dequeue();
        messageQueue.Enqueue(lastMessage);

        lastMessage.gameObject.SetActive(true);
		if (TextValues.Count > id) {
            string txt = I2.Loc.ScriptLocalization.Get(TextValues[id]);
            if (toUpperCase)
            {
                txt = txt.ToUpper();
            }
			lastMessage.Initialize (txt);
		}
        else lastMessage.Initialize("THERE IS NO TEXT");
    }


    public void ShowMessageWithoutStacking(int id)
    {
        if (isOverride)
        {
            id = this.override_id;
        }

        stacked_id = id;
    }

    public void DisableStackedMessage()
    {
        stacked_id = 0;
    }

    public void Update()
    {
        if (stacked_id>0)
        {
            if (Input.GetMouseButtonUp(0))
            {
                ShowMessage(stacked_id);
                stacked_id = 0;
            }
        }
    }
}
