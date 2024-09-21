using UnityEngine.UI;
using TMPro;
using AgoraChat;
using AgoraChat.MessageBody;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

public class Messaging : MonoBehaviour, IChatManagerDelegate, IConnectionDelegate
{

    private TMP_Text messageList;
    [SerializeField] private string userId;
    [SerializeField] private string token; 
    [SerializeField] private string appKey = "611189905#1377617";
    private bool isJoined = false;
    SDKClient agoraChatClient;

    public void MessageReactionDidChange(List<MessageReactionChange> list)
    {
        
    }

    public void OnAppActiveNumberReachLimitation()
    {
        
    }

    public void OnAuthFailed()
    {
        
    }

    public void OnChangedIMPwd()
    {
        
    }

    public void OnCmdMessagesReceived(List<Message> messages)
    {
        
    }

    public void OnConnected()
    {
        
    }

    public void OnConversationRead(string from, string to)
    {
        
    }

    public void OnConversationsUpdate()
    {
        
    }

    public void OnDisconnected()
    {
        Debug.Log("Disconnected");
    }

    public void OnForbidByServer()
    {
        
    }

    public void OnGroupMessageRead(List<GroupReadAck> list)
    {
        
    }

    public void OnKickedByOtherDevice()
    {
        
    }

    public void OnLoggedOtherDevice(string deviceName)
    {
        
    }

    public void OnLoginTooManyDevice()
    {
        
    }

    public void OnMessageContentChanged(Message msg, string operatorId, long operationTime)
    {
        
    }

    public void OnMessagesDelivered(List<Message> messages)
    {
        
    }

    public void OnMessagesRead(List<Message> messages)
    {
        
    }

    public void OnMessagesRecalled(List<Message> messages)
    {
        
    }

    public void OnMessagesReceived(List<Message> messages)
    {
            foreach (Message msg in messages) 
        {
            if (msg.Body.Type == MessageBodyType.TXT)
            {
                TextBody txtBody = msg.Body as TextBody;
                string Msg = msg.From + ":" + txtBody.Text;
                displayMessage(Msg, false);
            }
        }
    }

    public void OnReadAckForGroupMessageUpdated()
    {
        
    }

    public void OnRemovedFromServer()
    {
        
    }

    public void OnTokenExpired()
    {
        
    }

    public void OnTokenWillExpire()
    {
        
    }

    void setupChatSDK()
    {
        if (appKey == "")
        {
            Debug.Log("You should set your appKey first!");
            return;
        }

        // Initialize the Agora Chat SDK
        Options options = new Options(appKey);
        options.UsingHttpsOnly = true;
        options.DebugMode = true;
        agoraChatClient = SDKClient.Instance;
        agoraChatClient.InitWithOptions(options);
        agoraChatClient.ChatManager.AddChatManagerDelegate(this);
    }


    public void joinLeave()
    {
        if (isJoined)
        {
            agoraChatClient.Logout(true, callback: new CallBack(
            onSuccess: () =>
            {
                Debug.Log("Logout succeed");
                isJoined = false;
                GameObject.Find("joinBtn").GetComponentInChildren<TextMeshProUGUI>().text = "Join";
            },
            onError: (code, desc) =>
            {
                Debug.Log($"Logout failed, code: {code}, desc: {desc}");
            }));
        }
        else
        {
            //Boolean based assignment of UserId and token
            

            agoraChatClient.Login(userId, token, callback: new CallBack(
            onSuccess: () =>
            {
                Debug.Log("Login succeed" + " Token: " + token + " User ID: " + userId);
                isJoined = true;
                GameObject.Find("joinBtn").GetComponentInChildren<TextMeshProUGUI>().text = "Leave";
            },
            onError: (code, desc) =>
            {
                Debug.Log($"Login failed, code: {code}, desc: {desc}");
                Debug.Log("Token: " + token.Length + " User ID: " + userId);
            }));
        }
    }

    public void sendMessage()
    {
        string recipient = GameObject.Find("userName").GetComponent<TMP_InputField>().text;
        
        string Msg = GameObject.Find("message").GetComponent<TMP_InputField>().text;
        Debug.Log("Recipient: " + recipient);
        if (Msg == "" || recipient == "")
        {
            Debug.Log("You did not type your message");
            return;
        }
        Message msg = Message.CreateTextSendMessage(recipient, Msg);
        displayMessage(Msg, true);
        agoraChatClient.ChatManager.SendMessage(ref msg, new CallBack(
            onSuccess: () =>
            {
                Debug.Log($"Send message succeed");
                GameObject.Find("message").GetComponent<TMP_InputField>().text = "";
            },
            onError: (code, desc) =>
            {
                Debug.Log($"Send message failed, code: {code}, desc: {desc}");
            }));
    }

    public void displayMessage(string messageText, bool isSentMessage)
    {
        if (isSentMessage)
        {
            messageList.text += "<align=\"right\"><color=black><mark=#dcf8c655 padding=\"10, 10, 0, 0\">" + messageText + "</color></mark>\n";
        }
        else
        {
            messageList.text += "<align=\"left\"><color=black><mark=#ffffff55 padding=\"10, 10, 0, 0\">" + messageText + "</color></mark>\n";
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("userName/Text Area/Placeholder").GetComponent<TMP_Text>().text = "Enter recipient name";
        GameObject.Find("message/Text Area/Placeholder").GetComponent<TMP_Text>().text = "Message";
        messageList = GameObject.Find("scrollView/Viewport/Content").GetComponent<TextMeshProUGUI>();
        messageList.fontSize = 14;
        messageList.text = "";
        GameObject button = GameObject.Find("joinBtn");
        button.GetComponent<Button>().onClick.AddListener(joinLeave);
        button = GameObject.Find("sendBtn");
        button.GetComponent<Button>().onClick.AddListener(sendMessage);
        setupChatSDK();
    }



    void OnApplicationQuit()
    {
        agoraChatClient.ChatManager.RemoveChatManagerDelegate(this);
        agoraChatClient.Logout(true, callback: new CallBack(
            onSuccess: () => 
            {
                Debug.Log("Logout succeed");
            },
            onError: (code, desc) => 
            {
                Debug.Log($"Logout failed, code: {code}, desc: {desc}");
            }));
    }
}
