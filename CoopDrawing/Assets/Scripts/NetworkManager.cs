using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class NetworkManager : MonoBehaviour, IService
{
    [SerializeField] private bool connectToRemote;
    
    public bool IsServer { get; private set; }
    
    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }
    
    void Start()
    {
#if UNITY_EDITOR
        string arg = ClonesManager.GetArgument();
        if (arg.Equals("server"))
        {
            IsServer = true;
            gameObject.AddComponent<Server>();
        }
        else
        {
            IsServer = false;
            Client client = gameObject.AddComponent<Client>();
            client.Connect(connectToRemote);
        }
#else
        if (Application.isBatchMode)
        {
            IsServer = true;
            gameObject.AddComponent<Server>();
        }
        else
        {
            IsServer = false;
            Client client = gameObject.AddComponent<Client>();
            client.Connect(true);
        }
#endif
    }
}
