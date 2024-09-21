using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using Agora.Rtm;
using io.agora.rtm.demo;
using TMPro;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

public class joinChannelVideo : MonoBehaviour
{
    [SerializeField] private string _appID = "e97337da3d464089b7d9fc90f990ea97";
    [SerializeField] private string _channelName = "TuteDude";
    [SerializeField] private string _token = "007eJxTYGjhc8y2aWOfln/zm6fwzbeTi2WtfjsvPHV+VpJYPOe616sUGFItzY2NzVMSjVNMzEwMLCyTzFMs05ItDdIsLQ1SEy3NX+zamtYQyMjg+/wTEyMDBIL4HAwhpSWpLqUpqQwMAOLpIjw=";



    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;

    internal VideoSurface ScreenShareView;
    internal IRtcEngine RtcEngine;
    
    public bool isScreenSharing = false;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
#endif

    private void Start()
    {
        SetupVideoSDKEngine();
        InitEventHandler();
        SetupUI();
    }

    private void Update()
    {
        CheckPermissions();
    }

    void OnApplicationQuit()
    {
        if (RtcEngine != null)
        {
            Leave();
            RtcEngine.Dispose();
            RtcEngine = null;
        }
    }

    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach (string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
            }
        }
#endif
    }

    private void SetupUI()
    {
        GameObject go = GameObject.Find("LocalView");
        LocalView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, -180.0f);

        go = GameObject.Find("RemoteView");
        RemoteView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, -180.0f);

        go = GameObject.Find("ScreenShareView");
        ScreenShareView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, -180.0f);

        go = GameObject.Find("Leave");
        go.GetComponent<Button>().onClick.AddListener(Leave);

        go = GameObject.Find("Join");
        go.GetComponent<Button>().onClick.AddListener(Join);

        go = GameObject.Find("StartScreenShare");
        go.GetComponent<Button>().onClick.AddListener(StartScreenShare);

        go = GameObject.Find("StopScreenShare");
        go.GetComponent<Button>().onClick.AddListener(StopScreenShare);
    }

    private void SetupVideoSDKEngine()
    {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        
        RtcEngineContext context = new RtcEngineContext
        {
            appId = _appID,
            channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION,
            audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT,
            areaCode = AREA_CODE.AREA_CODE_GLOB,
        };

        RtcEngine.Initialize(context);
    }

    private void InitEventHandler()
    {
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngine.InitEventHandler(handler);
    }

    public void Join()
    {
        RtcEngine.EnableVideo();
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.autoSubscribeAudio.SetValue(true);
        options.autoSubscribeVideo.SetValue(true);
        RtcEngine.JoinChannel(_token, _channelName, 0, options);
        // Login to chat when joining the channel
        
    }

    public void Leave()
    {
        if (isScreenSharing)
        {
            StopScreenShare();
        }
        RtcEngine.LeaveChannel();
        RtcEngine.DisableVideo();

        // Logout from chat when leaving the channel

    }

    public void StartScreenShare()
    {
        SIZE thumbSize = new SIZE(360, 240);
        SIZE iconSize = new SIZE(360, 240);
        ScreenCaptureSourceInfo[] screenCaptureSourceInfos = RtcEngine.GetScreenCaptureSources(thumbSize, iconSize, true);

        ScreenCaptureSourceInfo info = screenCaptureSourceInfos[0];

        ScreenCaptureParameters screenCaptureParams = new ScreenCaptureParameters();
        screenCaptureParams.frameRate = 15;
        screenCaptureParams.bitrate = 0;
        screenCaptureParams.dimensions = new VideoDimensions(1920, 1080);

        if (info.type == ScreenCaptureSourceType.ScreenCaptureSourceType_Screen)
        {
            ulong displayId = info.sourceId;
            RtcEngine.StartScreenCaptureByDisplayId((uint)displayId, default(Rectangle), screenCaptureParams);
        }
        else if (info.type == ScreenCaptureSourceType.ScreenCaptureSourceType_Window)
        {
            ulong windowId = info.sourceId;
            RtcEngine.StartScreenCaptureByWindowId(windowId, default(Rectangle), screenCaptureParams);
        }

        isScreenSharing = true;

        // Publish screen track
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.publishCameraTrack.SetValue(false);
        options.publishScreenTrack.SetValue(true);
        RtcEngine.UpdateChannelMediaOptions(options);

        // Set up VideoSurface for screen share locally
        ScreenShareView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN_PRIMARY);
        ScreenShareView.SetEnable(true);

        // Notify remote users about screen sharing
        SendScreenShareState(true);
    }

    public void StopScreenShare()
    {
        if (isScreenSharing)
        {
            // Stop the screen capture
            RtcEngine.StopScreenCapture();
            isScreenSharing = false;

            // Re-enable the camera track
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(true);
            options.publishScreenTrack.SetValue(false);
            RtcEngine.UpdateChannelMediaOptions(options);

            // Disable the screen share VideoSurface
            if (ScreenShareView != null)
            {
                ScreenShareView.SetEnable(false);
            }

            // Reset the RemoteView to show the camera feed
            RemoteView.SetForUser(0, _channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
            RemoteView.SetEnable(true);

            // Reset the LocalView to show the local camera feed
            LocalView.SetForUser(0, _channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
            LocalView.SetEnable(true);

            // Notify remote users about stopping screen sharing
            SendScreenShareState(false);

            Debug.Log("Screen sharing stopped, switched back to camera.");
        }
    }

    private void SendScreenShareState(bool isScreenSharing)
    {
        string message = isScreenSharing ? "START_SCREEN_SHARE" : "STOP_SCREEN_SHARE";
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        RtcEngine.SendStreamMessage(1, messageBytes, (uint)messageBytes.Length);
        Debug.Log(""+message);
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly joinChannelVideo _videoSample;

        internal UserEventHandler(joinChannelVideo videoSample)
        {
            _videoSample = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            Debug.LogError($"Error: {err}, {msg}");
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            if (_videoSample == null || _videoSample.RtcEngine == null)
            {
                Debug.LogError("VideoSample or RtcEngine is null in OnJoinChannelSuccess");
                return;
            }

        
            // Set LocalView for local user's camera feed
            //Check OnStreamMessage

            _videoSample.LocalView.SetForUser(0, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
            _videoSample.LocalView.SetEnable(true);

            Debug.Log("Local user joined");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            if (_videoSample == null)
            {
                Debug.Log("Videosample not found");
                return;
            }
            
            // By default, show the camera feed of the remote user
            OnStreamMessage(connection, uid, 1, System.Text.Encoding.UTF8.GetBytes("STOP_SCREEN_SHARE"), 0, 0);
            _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            _videoSample.RemoteView.SetEnable(true);

            Debug.Log("Remote user joined");
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.RemoteView.SetEnable(false);
        }

        public override void OnStreamMessage(RtcConnection connection, uint remoteUid, int streamId, byte[] data, ulong length, ulong sentTs)
        {
            string message = System.Text.Encoding.UTF8.GetString(data);
            //string message = System.Text.Encoding.UTF8.GetString(data, 0, (int)length);

            if (message == "START_SCREEN_SHARE")
            {
                _videoSample.RemoteView.SetEnable(false);
                _videoSample.ScreenShareView.SetForUser(remoteUid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                _videoSample.ScreenShareView.SetEnable(true);
                Debug.Log("Remote user started screen sharing");
            }
            else if (message == "STOP_SCREEN_SHARE")
            {
                _videoSample.ScreenShareView.SetEnable(false);
                _videoSample.RemoteView.SetForUser(remoteUid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
                _videoSample.RemoteView.SetEnable(true);
                Debug.Log("Remote user stopped screen sharing");
            }
            else{
                Debug.Log("Error OnStream Message");
            }
        }
    }
}

