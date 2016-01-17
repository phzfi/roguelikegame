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
    
    public void SendChatMessage()
    {
        
        var player = CharManager.GetLocalPlayer();
        if (m_messages.Count > m_maxMessages)
            m_messages.RemoveAt(0);
        string message = player.ID.ToString() + ": " + m_messageToBeSent.text;
        m_messages.Add(message);
        m_messageList.text += message;
        if (m_messageList.text.Length != 0)
            m_messageList.text += "\n";
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
                    SendChatMessage();
                    
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
