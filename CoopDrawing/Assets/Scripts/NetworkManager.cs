using System.Collections;
using System.Collections.Generic;
using ParrelSync;
using UnityEngine;

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
        Client client = gameObject.AddComponent<Client>();
        client.Connect(connectToRemote);
#endif
    }
}
