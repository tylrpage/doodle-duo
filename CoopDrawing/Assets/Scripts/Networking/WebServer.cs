using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
using JamesFrowen.SimpleWeb;
using NetStack.Serialization;
using Networking;
using UnityEngine;

public class WebServer : MonoBehaviour, IServer
{
    public event IServer.PeerConnectedDelegate PeerConnected;
    public event IServer.PeerDisconnectedDelegate PeerDisconnected;
    public event Action<int, BitBuffer> DataReceived;

    private SimpleWebServer _webServer;
    private Messenger _messenger;
    private bool _listening;
    private NetworkManager _networkManager;
    private UIManager _uiManager;
    public List<int> ConnectedPeers { get; private set; } = new List<int>();
    private StateManager _stateManager;
    private ImageManager _imageManager;
    private DrawingManager _drawingManager;
    private Coroutine _heartbeatCoroutine;

    private void Awake()
    {
        Application.targetFrameRate = Constants.Tick;
        
        _uiManager = GameManager.Instance.GetService<UIManager>();
        _networkManager = GameManager.Instance.GetService<NetworkManager>();
        
        _webServer = Listen();
        
        _webServer.onConnect += WebServerOnonConnect;
        _webServer.onData += WebServerOnonData;
        _webServer.onError += WsOnonError;
        _webServer.onDisconnect += WebServerOnonDisconnect;
    }

    private void Start()
    {
        _stateManager = GameManager.Instance.GetService<StateManager>();
        _stateManager.ChangeServerState(StateManager.State.Waiting);
        
        _imageManager = GameManager.Instance.GetService<ImageManager>();
        _drawingManager = GameManager.Instance.GetService<DrawingManager>();
    }

    private void OnDestroy()
    {
        if (_listening)
        {
            _webServer.Stop();
        }
        
        _webServer.onConnect -= WebServerOnonConnect;
        _webServer.onData -= WebServerOnonData;
        _webServer.onError -= WsOnonError;
        _webServer.onDisconnect -= WebServerOnonDisconnect;
    }

    void Update()
    {
        _webServer.ProcessMessageQueue();
    }

    private SimpleWebServer Listen()
    {
        SimpleWebServer webServer;
        
        SslConfig sslConfig;
        TcpConfig tcpConfig = new TcpConfig(false, 5000, Constants.ReceiveTimeoutMS);
        if (Application.isBatchMode)
        {
            Debug.Log($"Setting up secure server");
            sslConfig = new SslConfig(true, "cert-legacy.pfx", "", SslProtocols.Tls12);
        }
        else
        {
            Debug.Log($"Setting up non secure server");
            sslConfig = new SslConfig(false, "", "", SslProtocols.Tls12);
        }

        webServer = new SimpleWebServer(5000, tcpConfig, 16 * 1024, 3000, sslConfig);
        webServer.Start(Constants.GamePort);

        Debug.Log($"Server started, port: {Constants.GamePort}");
        _listening = true;
        
        _uiManager.SetStatusText("Listening...");

        _heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());

        return webServer;
    }
    
    private void WebServerOnonConnect(int peerId)
    {
        Debug.Log($"Client connected, id: {peerId}");
        
        ConnectedPeers.Add(peerId);

        if (_stateManager.CurrentState == StateManager.State.Waiting && ConnectedPeers.Count >= 2)
        {
            // Begin game
            _stateManager.ChangeServerState(StateManager.State.CountIn);
        }
        else
        {
            // Send current state to connecting client
            _networkManager.SendToClient(peerId, new ServerStateChangeMessage()
            {
                StateId = (short)_stateManager.CurrentState,
            });

            if (_stateManager.CurrentState != StateManager.State.Waiting)
            {
                // Give more info about the game that they would need to know
                _networkManager.SendToClient(peerId, new ServerPlayingStateMessage()
                {
                    ImageIndex = _imageManager.CurrentImageIndex,
                    TimeLeft = _drawingManager.TimeLeft,
                });   
            }
        }
        
        // Give them the win count
        _networkManager.SendToClient(peerId, new ServerUpdateWinCountAndAttemptsMessage()
        {
            WinCount = _drawingManager.WinCount,
            Attempts = _drawingManager.Attempts,
        });
        
        PeerConnected?.Invoke(peerId);
    }

    private void WebServerOnonDisconnect(int peerId)
    {
        Debug.Log($"Client disconnected, id: {peerId}");
        
        ConnectedPeers.Remove(peerId);

        // Not enough players anymore, go back to waiting
        if (ConnectedPeers.Count < 2)
        {
            // Reset the image we are on
            _imageManager.Reset();
            _stateManager.ChangeServerState(StateManager.State.Waiting);
        }
        
        PeerDisconnected?.Invoke(peerId);
    }

    private void WebServerOnonData(int peerId, ArraySegment<byte> data)
    {
        BitBuffer bitBuffer = BufferPool.GetBitBuffer();
        bitBuffer.FromArray(data.Array, data.Count);
        
        DataReceived?.Invoke(peerId, bitBuffer);
    }
    
    private void WsOnonError(int connectionId, Exception exception)
    {
        Debug.LogError($"Web Server Error, Id: {connectionId}, {exception.Message}");
    }

    public void Send(int peerId, BitBuffer data)
    {
        byte[] byteBuffer = BufferPool.GetByteBuffer();
        data.ToArray(byteBuffer);
        ArraySegment<byte> bytes = new ArraySegment<byte>(byteBuffer, 0, data.Length);
        _webServer.SendOne(peerId, bytes);
    }

    private IEnumerator HeartbeatCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds((Constants.ReceiveTimeoutMS / 1000f) / 2);
            
            //SendAll(new HeartbeatMessage());
        }
    }
}