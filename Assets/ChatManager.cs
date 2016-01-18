using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ChatManager : NetworkBehaviour
{
    public GameObject m_chatObjects;
    public InputField m_messageList;
    public InputField m_messageToBeSent;
    public int m_maxMessages = 10;

    public List<string> m_messages = new List<string>();

    public static bool sm_chatOpen = false;

    private CustomNetworkManager m_networkManager;

    public void Start()
    {
        m_networkManager = FindObjectOfType<CustomNetworkManager>();
        if(m_messages.Count > 0)
        {
            for(int i = 0; i < m_messages.Count; ++i)
            {
                m_messageList.text += "\n" + m_messages[i];
            }
        }
    }
    
    public void AddMessage(string msg)
    {
        var player = CharManager.GetLocalPlayer();
        if (m_messages.Count > m_maxMessages)
            m_messages.RemoveAt(0);
        m_messages.Add(msg);
        m_messageList.text += msg;
    }

    public void SendMessage()
    {
        var player = CharManager.GetLocalPlayer();
        if (m_messageToBeSent.text.Length == 0)
            return;
        string message = player.ID.ToString() + ": " + m_messageToBeSent.text + "\n";
        SyncManager.AddChatMessage(message, player.ID);
        m_messageToBeSent.text = "";
    }

    void Update()
    {
        if(Input.GetKeyDown("enter"))
        {
            if (!sm_chatOpen)
                ToggleChat();
            else
            {
                if (m_messageToBeSent.text == "")
                {
                    EventSystem.current.SetSelectedGameObject(m_messageToBeSent.gameObject, null);
                    m_messageToBeSent.OnPointerClick(new PointerEventData(EventSystem.current));
                }
                else
                {
                    SendMessage();
                    
                }
            }
        }
    }

    public void ToggleChat()
    {
        if (!sm_chatOpen)
        {
            m_chatObjects.SetActive(true);
            sm_chatOpen = true;
        }
        else
        {
            m_chatObjects.SetActive(false);
            sm_chatOpen = false;
        }
        
    }
}
