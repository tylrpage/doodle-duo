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
    public event Action<BitSerializable> MessageReceived;
    public event Action ClientConnected;
    public event Action ClientDisconnected;
    
    [SerializeField] private bool nonCloneIsServer;
    [SerializeField] private bool editorConnectToRemote;
    
    public bool IsServer { get; private set; }
    public Client Client { get; private set; }
    public Server Server { get; private set; }
    
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
        Client = gameObject.AddComponent<Client>();
        Client.MessageReceived += OnClientOrServerMessageReceived;
        Client.Connected += () => ClientConnected?.Invoke();
        Client.Disconnected += () => ClientDisconnected?.Invoke();
        Client.Connect(connectToRemote);
        
        Sided?.Invoke();
        IsSided = true;
    }

    private void OnClientOrServerMessageReceived(BitSerializable message)
    {
        MessageReceived?.Invoke(message);
    }

    private void StartServer()
    {
        IsServer = true;
        Server = gameObject.AddComponent<Server>();
        Server.MessageReceived += OnClientOrServerMessageReceived;
        
        Sided?.Invoke();
        IsSided = true;
    }
}
