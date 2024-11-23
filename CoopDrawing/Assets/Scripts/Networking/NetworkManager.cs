using System;
using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class NetworkManager : MonoBehaviour, IService
{
    // Don't forget to check if it already IsSided when subbing
    public event Action Sided;
    public bool IsSided { get; private set; }
    public event Action<IBitSerializable> MessageReceived;
    public event Action ClientConnected;
    public event Action ClientDisconnected;
    
    [SerializeField] private bool nonCloneIsServer;
    [SerializeField] private bool editorConnectToRemote;
    
    public bool IsServer { get; private set; }
    public WebClient Client { get; private set; }
    public WebServer Server { get; private set; }
    
    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
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
        Client = gameObject.AddComponent<WebClient>();
        Client.MessageReceived += OnClientOrServerMessageReceived;
        Client.Connected += () => ClientConnected?.Invoke();
        Client.Disconnected += () => ClientDisconnected?.Invoke();
        Client.Connect(connectToRemote);
        
        Sided?.Invoke();
        IsSided = true;
    }

    private void OnClientOrServerMessageReceived(IBitSerializable message)
    {
        MessageReceived?.Invoke(message);
    }

    private void StartServer()
    {
        IsServer = true;
        Server = gameObject.AddComponent<WebServer>();
        Server.MessageReceived += OnClientOrServerMessageReceived;
        
        Sided?.Invoke();
        IsSided = true;
    }
}
