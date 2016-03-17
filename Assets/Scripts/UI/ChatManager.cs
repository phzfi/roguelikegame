using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ChatManager : NetworkBehaviour
{
    public GameObject m_chatObjects;
    public GameObject m_messageList;
    public InputField m_messageToBeSent;
    public int m_maxMessages = 10;
    public Text m_chatMessagePrefab;

    public static bool sm_chatOpen = false;

    private static List<Text> sm_messages = new List<Text>();
    private AudioSource m_audioSource;

    public void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_chatMessagePrefab.text = "";
        for(int i = 0; i<m_maxMessages; ++i)
        {
            var emptyMessagePlaceholder = Instantiate(m_chatMessagePrefab);
            sm_messages.Add(emptyMessagePlaceholder);
            emptyMessagePlaceholder.transform.SetParent(m_messageList.transform);
            emptyMessagePlaceholder.transform.localScale = Vector3.one;
        }
    }

    public void AddMessage(string msg)
    {
        var player = CharManager.GetLocalPlayer();
        m_chatMessagePrefab.text = msg;
        var messageText = Instantiate(m_chatMessagePrefab);
        sm_messages.Add(messageText);
        messageText.transform.SetParent(m_messageList.transform);
        messageText.transform.localScale = Vector3.one;
        m_audioSource.Play();
        if (sm_messages.Count > m_maxMessages)
        {
            sm_messages.RemoveAt(0);
            Destroy(m_messageList.transform.GetChild(0).gameObject);
        }
    }

    public void SendMessage()
    {
        var player = CharManager.GetLocalPlayer();
        if (m_messageToBeSent.text.Length == 0)
            return;
        string message = player.m_name + ": " + m_messageToBeSent.text;
        SyncManager.AddChatMessage(message, player.ID);
        m_messageToBeSent.text = "";
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
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
