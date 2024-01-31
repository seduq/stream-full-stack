using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ObsController : MonoBehaviour
{
    private static OBSWebsocket obs = null;
    private static CancellationTokenSource keepAliveTokenSource;
    private static readonly int keepAliveInterval = 500;


    public GameObject canvas;
    public TMP_InputField ip;
    public TMP_InputField password;
    public string port;

    public string sceneNameStarting;
    public string sceneNameEnding;
    public string sceneNameBrb;

    public TextController textController;
    public ConstellationController constellationController;

    void Awake() {
        if (obs != null && obs.IsConnected)
            canvas.SetActive(false);
    }

    public void Connect() {
        if(obs == null)
            obs = new OBSWebsocket();
        obs.Connected += OnConnect;
        obs.Disconnected += OnDisconnect;
        obs.CurrentProgramSceneChanged += OnCurrentProgramSceneChanged;
        obs.ConnectAsync($"ws://{ip.text}:{port}", password.text);
    }

    private void Update() {
        if(obs != null && obs.IsConnected)
            canvas.SetActive(false);
        else
            canvas.SetActive(true);
    }

    private void OnConnect(object sender, EventArgs e) {
        var versionInfo = obs.GetVersion();
        var pluginVersion = versionInfo.PluginVersion;
        var obsVersion = versionInfo.OBSStudioVersion;

        Debug.Log($"Connected Version {versionInfo}, Plugin Version {pluginVersion}, OBS Version {obsVersion}");

        keepAliveTokenSource = new CancellationTokenSource();
        CancellationToken keepAliveToken = keepAliveTokenSource.Token;
        Task statPollKeepAlive = Task.Factory.StartNew(() => {
            while (true) {
                Thread.Sleep(keepAliveInterval);
                if (keepAliveToken.IsCancellationRequested) {
                    break;
                }
            }
        }, keepAliveToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private void OnDisconnect(object sender, OBSWebsocketDotNet.Communication.ObsDisconnectionInfo e) {
        var info = e.WebsocketDisconnectionInfo;
        Debug.Log($"Disconnected Reason {info.Type} Close Code {e.ObsCloseCode}");
    }

    private void OnCurrentProgramSceneChanged(object sender, ProgramSceneChangedEventArgs args) {
        var currentScene = args.SceneName;
        Debug.Log($"Program Scene Changed {currentScene}");

        if (currentScene == null) {
            textController.SetState(TextController.State.None);
            constellationController.SetState(false);
            return;
        } else {
            var state = currentScene switch {
                "Starting" => TextController.State.Starting,
                "Ending" => TextController.State.Ending,
                "Be Right Back" => TextController.State.BeRightBack,
                _ => TextController.State.None,
            };
            textController.SetState(state);
            constellationController.SetState(state != TextController.State.None);
        }
    }

    void OnApplicationQuit() {
        obs.Disconnect();
    }
}
