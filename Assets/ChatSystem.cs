using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class ChatSystem : NetworkBehaviour
{
    [Header("Objects")]
    public GameObject chatEntryCanvas;
    public TMP_InputField chatEntryInput;
    public TextMeshProUGUI chatBody;
    public GameObject chatDisplay;

    [HideInInspector] public static TextMeshProUGUI MyChatBody;

   // [Header("Networked")]
    [HideInInspector] [Networked(OnChanged = nameof(LastPublicChatChanged))] public NetworkString<_256> LastPublicChat { get; set; }
    [HideInInspector] [Networked(OnChanged = nameof(LastPrivateChatChanged))] public NetworkString<_256> LastPrivateChat { get; set; }

    private string thisPlayersName;
    private bool isChatActive = false;

    private void Start() {
        thisPlayersName = gameObject.name;
        chatEntryCanvas.SetActive(false);
        MyChatBody = chatBody;
        chatBody.gameObject.SetActive(true);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.C)) {
            StartChat();
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            SendChat();
        }
        chatDisplay.SetActive(true);
    }

    protected static void LastPublicChatChanged(Changed<ChatSystem> change)
    {
        MyChatBody.text += "\n" + change.Behaviour.thisPlayersName + ": " + change.Behaviour.LastPublicChat.ToString();
    }

    protected static void LastPrivateChatChanged(Changed<ChatSystem> change)
    {
        // Handle private chat changes here if necessary
    }

    private void SendChat()
    {
        if (isChatActive)
        {
            // Send chat message as a networked variable
            RPC_SendChat(chatEntryInput.text);
            chatEntryCanvas.SetActive(false);
            isChatActive = false;
        }
    }

    private void StartChat()
    {
        isChatActive = true;
        chatEntryCanvas.SetActive(true);
        chatEntryInput.ActivateInputField();  // This will focus the input field
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SendChat(string message)
    {
        LastPublicChat = message;
    }
}


