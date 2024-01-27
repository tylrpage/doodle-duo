using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private bool connectToRemote;
    
    void Start()
    {
#if UNITY_EDITOR
        string arg = ClonesManager.GetArgument();
        if (arg.Equals("server"))
        {
            gameObject.AddComponent<Server>();
        }
        else
        {
            Client client = gameObject.AddComponent<Client>();
            client.Connect(connectToRemote);
        }
#else
        if (Application.isBatchMode)
        {
            gameObject.AddComponent<Server>();
        }
        else
        {
            Client client = gameObject.AddComponent<Client>();
            client.Connect(true);
        }
#endif
    }
}
