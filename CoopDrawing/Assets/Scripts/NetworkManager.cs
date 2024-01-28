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
    
    [SerializeField] private bool nonCloneIsServer;
    [SerializeField] private bool connectToRemote;
    
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
            string arg = ClonesManager.GetArgument();
            if (arg.Equals("server"))
            {
                StartServer();
            }
            else
            {
                StartClient();
            }
        }
#else
        if (Application.isBatchMode)
        {
            StartServer();
        }
        else
        {
            StartClient();
        }
#endif
    }

    private void StartClient()
    {
        IsServer = false;
        Client = gameObject.AddComponent<Client>();
        Client.MessageReceived += OnClientOrServerMessageReceived;
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
