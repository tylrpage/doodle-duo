using System;
using System.Collections.Generic;
using NetStack.Serialization;
using Networking;
using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class NetworkManager : MonoBehaviour, IService
{
    public event Action ClientConnected;
    public event Action ClientDisconnected;
    
    [SerializeField] private bool nonCloneIsServer;
    [SerializeField] private bool editorConnectToRemote;
    
    public bool IsServer { get; private set; }
    
    private IClient _client;
    private IServer _server;
    // Have these message routers initialized, so that even if the client/server is not started, we can still subscribe to events
    public ClientMessageRouter ClientMessageRouter { get; private set; } = new ClientMessageRouter();
    public ServerMessageRouter ServerMessageRouter { get; private set; } = new ServerMessageRouter();
    
    // Server only
    public List<int> ConnectedPeers { get; private set; }
    
    private Messenger _messenger;
    
    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
        
        _messenger = new Messenger();
    }
    
    void Start()
    {
#if UNITY_EDITOR
        if (!ClonesManager.IsClone() && nonCloneIsServer)
        {
            StartServer();
        }
        else
        {
            // todo: call these when appropriate, not just immediately at start
            // string arg = ClonesManager.GetArgument();
            // if (arg.Equals("server"))
            // {
            //     StartServer();
            // }
            // else
            // {
            //     StartClient(editorConnectToRemote);
            // }
        }
#else
        if (Application.isBatchMode)
        {
            StartServer();
        }
        else
        {
            // non editor always connects to remote
            StartClient(true);
        }
#endif
    }

    private void StartClient(bool connectToRemote)
    {
        IsServer = false;
        WebClient webClient = gameObject.AddComponent<WebClient>();
        _client = webClient;
        _client.Connected += () => ClientConnected?.Invoke();
        _client.Disconnected += () => ClientDisconnected?.Invoke();
        _client.DataReceived += buffer =>
        {
            IBitSerializable message = _messenger.Receive(buffer);
            ClientMessageRouter.ProcessMessage(message);
        };
        webClient.Connect(connectToRemote);
    }

    private void StartServer()
    {
        IsServer = true;
        _server = gameObject.AddComponent<WebServer>();
        ConnectedPeers = new List<int>();
        _server.PeerConnected += peerId => ConnectedPeers.Add(peerId);
        _server.PeerDisconnected += peerId => ConnectedPeers.Remove(peerId);
        _server.DataReceived += (id, buffer) =>
        {
            IBitSerializable message = _messenger.Receive(buffer);
            ServerMessageRouter.ProcessMessage(id, message);
        };
    }
    
    public void SendToServer(IBitSerializable message)
    {
        if (!IsServer)
        {
            BitBuffer bitBuffer = _messenger.Serialize(message);
            _client.Send(bitBuffer);
        }
    }
    
    public void SendToAllClient(IBitSerializable message)
    {
        if (IsServer)
        {
            BitBuffer bitBuffer = _messenger.Serialize(message);
            foreach (int peerId in ConnectedPeers)
            {
                _server.Send(peerId, bitBuffer);   
            }
        }
    }
    
    public void SendToClient(int peerId, IBitSerializable message)
    {
        if (IsServer)
        {
            BitBuffer bitBuffer = _messenger.Serialize(message);
            _server.Send(peerId, bitBuffer);
        }
    }
}
