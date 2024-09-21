using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;
using System.Threading.Tasks;
using Agora.Rtc;

public class ChatManager : MonoBehaviour
{
    [SerializeField] public string appId = "e97337da3d464089b7d9fc90f990ea97";
    [SerializeField] public string userId = "User1";
    [SerializeField] public string channelId = "TuteDude";

    public Text chatDisplayText;

    private RtmClient rtmClient;
    private bool isLoggedIn = false;

    private void Start()
    {
        InitializeChat();
    }

    private void UpdateChatDisplay(string message)
    {
        if (chatDisplayText != null)
        {
            chatDisplayText.text += message + "\n";
        }
        else
        {
            Debug.LogError("assdasd");
        }
    }

    private void InitializeChat()
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(appId))
        {
            ShowMessage("You need a userId and appId to initialize!");
            return;
        }

        LogConfig logConfig = new LogConfig
        {
            filePath = "E:\\Worksapce\\Game Dev\\Interviews\\Tutedude_Assignment\\2Multiplayer_Game_2D\\Assets\\LogFiles",
            fileSizeInKB = 512,
            level = (Agora.Rtc.LOG_LEVEL)Agora.Rtm.LOG_LEVEL.INFO
        };

        // RtmConfig config = new RtmConfig
        // {
        //     appId = appId,
        //     userId = userId,
        //     logConfig = logConfig
        // };

        RtmConfig config = new RtmConfig();
        config.appId = appId;
        config.userId = userId;
        //config.logConfig = logConfig;

        try
        {
            rtmClient = (RtmClient)RtmClient.CreateAgoraRtmClient(config);
            ShowMessage("Chat RTM Client Initialized Successfully");
            Debug.LogError("Chat RTM Client Initialized Successfully");
            AddEventListeners();
            //LoginToChat();
        }
        catch (RTMException e)
        {
            ShowMessage($"{e.Status.Operation} is failed");
            ShowMessage($"The error code is {e.Status.ErrorCode}, due to: {e.Status.Reason}");
        }
    }

    private void AddEventListeners()
    {
        rtmClient.OnConnectionStateChanged += OnConnectionStateChanged;
        rtmClient.OnMessageEvent += OnMessageEvent;
    }

    public async void LoginToChat()
    {
        try
        {
            RtmResult<LoginResult> result = await rtmClient.LoginAsync(appId);
            if (result == null)
            {
                Debug.LogError("Login result is null.");
                ShowMessage("Login result is null.");
                return;
            }

            if (result.Status == null)
            {
                Debug.LogError("Login result status is null.");
                ShowMessage("Login result status is null.");
                return;
            }

            if (result.Status.Error)
            {
                ShowMessage($"{result.Status.Operation} failed");
                ShowMessage($"The error code is {result.Status.ErrorCode}, due to: {result.Status.Reason}");
                Debug.LogError("Chat Login Failed");
            }
            else
            {
                isLoggedIn = true;
                ShowMessage("Chat Login Successful");
                await SubscribeToChatChannel();
                Debug.Log( "Chat Login Successful");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"An error occurred during login: {ex.Message}");
            ShowMessage($"An error occurred during login: {ex.Message}");
        }
    }

    private async Task SubscribeToChatChannel()
    {
        try
        {
            SubscribeOptions options = new SubscribeOptions
            {
                withMessage = true,
                withPresence = false
            };
            RtmResult<SubscribeResult> result = await rtmClient.SubscribeAsync(channelId, options);
            if (result.Status.Error)
            {
                ShowMessage($"{result.Status.Operation} failed");
                ShowMessage($"The error code is {result.Status.ErrorCode}, due to: {result.Status.Reason}");
                Debug.LogError("Channel Subscription Failed");
            }
            else
            {
                ShowMessage($"Subscribed to chat channel: {channelId}");
                Debug.LogError("Channel Subscription Successful");
            }
        }
        catch (System.Exception ex)
        {
            ShowMessage($"An error occurred during channel subscription: {ex.Message}");
            Debug.LogError($"An error occurred during channel subscription: {ex.Message}");
        }
    }

    public async void SendMessage(string message)
    {
        if (isLoggedIn && rtmClient != null)
        {
            try
            {
                PublishOptions options = new PublishOptions
                {
                    channelType = RTM_CHANNEL_TYPE.MESSAGE,
                    customType = "PlainText"
                };
                RtmResult<PublishResult> result = await rtmClient.PublishAsync(channelId, message, options);
                if (result.Status.Error)
                {
                    ShowMessage($"{result.Status.Operation} failed, The error code is {result.Status.ErrorCode}");
                }
                else
                {
                    ShowMessage("Chat Message Sent Successfully");
                }
            }
            catch (System.Exception ex)
            {
                ShowMessage($"An error occurred while sending the message: {ex.Message}");
            }
        }
        else
        {
            ShowMessage("You need to be logged in to send messages");
        }
    }

    private void OnMessageEvent(MessageEvent eve)
    {
        var message = eve.message.GetData<string>();
        string formattedMessage = $"Received message: {message} from {eve.publisher} in channel: {eve.channelName}";
        ShowMessage(formattedMessage);
        UpdateChatDisplay(formattedMessage);
        
    }

    private void OnConnectionStateChanged(string channelName, RTM_CONNECTION_STATE state, RTM_CONNECTION_CHANGE_REASON reason)
    {
        ShowMessage($"Channel: {channelName} connection state changed to: {state} because of {reason}");
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);
        // Optionally, display the message in the UI
    }

    public async void LogoutChat()
    {
                if (rtmClient != null)
        {
            try
            {
                // Unsubscribe from the channel
                RtmResult<UnsubscribeResult> unsubscribeResult = await rtmClient.UnsubscribeAsync(channelId);
                if (unsubscribeResult.Status.Error)
                {
                    ShowMessage($"{unsubscribeResult.Status.Operation} failed");
                    ShowMessage($"The error code is {unsubscribeResult.Status.ErrorCode}, because of: {unsubscribeResult.Status.Reason}");
                }
                else
                {
                    ShowMessage("Unsubscribed from chat channel successfully");
                }

                // Logout from the RTM service
                RtmResult<LogoutResult> logoutResult = await rtmClient.LogoutAsync();
                if (logoutResult.Status.Error)
                {
                    ShowMessage($"Logout failed: {logoutResult.Status.ErrorCode}, because of: {logoutResult.Status.Reason}");
                }
                else
                {
                    ShowMessage("Logged out successfully");
                }
            }
            catch (System.Exception ex)
            {
                ShowMessage($"An error occurred during logout: {ex.Message}");
            }
            finally
            {
                // Clean up resources
                rtmClient.Dispose();
                rtmClient = null;
                isLoggedIn = false;
            }
        }
    }

}

