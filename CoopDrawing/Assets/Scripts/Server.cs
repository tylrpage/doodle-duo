using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using JamesFrowen.SimpleWeb;
using NetStack.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class Server : MonoBehaviour
{
    public event Action<BitSerializable> MessageReceived;
    
    private SimpleWebServer _webServer;
    private bool _listening;
    private UIManager _uiManager;
    private List<int> _connectedPeers = new List<int>();
    private StateManager _stateManager;
    private ImageManager _imageManager;

    private void Awake()
    {
        Application.targetFrameRate = Constants.Tick;
        
        _uiManager = GameManager.Instance.GetService<UIManager>();
        
        _webServer = Listen();
        
        _webServer.onConnect += WebServerOnonConnect;
        _webServer.onData += WebServerOnonData;
        _webServer.onError += WsOnonError;
        _webServer.onDisconnect += WebServerOnonDisconnect;
    }

    private void Start()
    {
        _stateManager = GameManager.Instance.GetService<StateManager>();
        _stateManager.ChangeState(StateManager.State.Waiting);
        
        _imageManager = GameManager.Instance.GetService<ImageManager>();
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
        TcpConfig tcpConfig = new TcpConfig(false, 5000, 20000);
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

        return webServer;
    }
    
    private void WebServerOnonConnect(int peerId)
    {
        Debug.Log($"Client connected, id: {peerId}");
        
        _connectedPeers.Add(peerId);

        if (_stateManager.CurrentState == StateManager.State.Waiting && _connectedPeers.Count >= 2)
        {
            // Begin game
            _stateManager.ChangeState(StateManager.State.Playing);
            
            // Tell all clients about the new state
            SendAll(new ServerStateChangeMessage()
            {
                StateId = (short)_stateManager.CurrentState,
            });
            
            // Get the first image
            _imageManager.GetNextImage();
            SendAll(new ServerChangeImageMessage()
            {
                ImageIndex = _imageManager.CurrentImageIndex,
            });
            
            // Assign roles randomly
            int horizontalPeerId = _connectedPeers[Random.Range(0, _connectedPeers.Count)];
            Send(horizontalPeerId, new ServerRoleAssignmentMessage()
            {
                CurrentRole = ServerRoleAssignmentMessage.Role.Horizontal,
            });
            
            List<int> availablePeers = _connectedPeers.Where(x => x != horizontalPeerId).ToList();
            int verticalPeerId = _connectedPeers[Random.Range(0, availablePeers.Count)];
            Send(verticalPeerId, new ServerRoleAssignmentMessage()
            {
                CurrentRole = ServerRoleAssignmentMessage.Role.Vertical,
            });
        }
        else
        {
            // Send current state to connecting client
            Send(peerId, new ServerStateChangeMessage()
            {
                StateId = (short)_stateManager.CurrentState,
            });
        }
    }

    private void WebServerOnonDisconnect(int peerId)
    {
        Debug.Log($"Client disconnected, id: {peerId}");
        
        _connectedPeers.Remove(peerId);
    }

    private void WebServerOnonData(int peerId, ArraySegment<byte> data)
    {
        BitBuffer bitBuffer = BufferPool.GetBitBuffer();
        bitBuffer.FromArray(data.Array, data.Count);
        ushort messageId = bitBuffer.PeekUShort();

        BitSerializable message = null;
        // todo: get rid of this boilerplate somehow
        switch (messageId)
        {
            case ClientInputMessage.Id:
                message = new ClientInputMessage();
                message.Deserialize(ref bitBuffer);
                break;
            default:
                Debug.LogError($"Received a message with an unknown id: {messageId}");
                break;
        }

        if (message != null)
        {
            MessageReceived?.Invoke(message);
        }
    }
    
    private void WsOnonError(int connectionId, Exception exception)
    {
        Debug.LogError($"Web Server Error, Id: {connectionId}, {exception.Message}");
    }
    
    public void SendAll(BitSerializable serializable)
    {
        ArraySegment<byte> bytes = Writer.SerializeToByteSegment(serializable);
        _webServer.SendAll(_connectedPeers, bytes);
    }
    
    public void Send(int peerId, BitSerializable serializable)
    {
        ArraySegment<byte> bytes = Writer.SerializeToByteSegment(serializable);
        _webServer.SendOne(peerId, bytes);
    }
}