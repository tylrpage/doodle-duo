using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class StateManager : MonoBehaviour, IService
{
    public event Action<State> StateChanged;
    
    public enum State : short
    {
        Waiting,
        Playing,
        Ending, // End of level sequence
        Fail,
    }
    
    public State CurrentState { get; private set; }

    [field: SerializeField] public float EndPictureDuration { get; private set; }
    private NetworkManager _networkManager;
    private ImageManager _imageManager;

    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }

    private void Start()
    {
        _networkManager = GameManager.Instance.GetService<NetworkManager>();
        _networkManager.MessageReceived += OnMessageReceived;

        _imageManager = GameManager.Instance.GetService<ImageManager>();
    }
    
    private void OnMessageReceived(BitSerializable message)
    {
        switch (message)
        {
            case ServerStateChangeMessage stateChangeMessage:
                ChangeState((State)stateChangeMessage.StateId);
                break;
        }
    }
    
    // Server only
    public void ChangeServerState(State newState)
    {
        ChangeState(newState);
        // Sync to clients
        _networkManager.Server.SendAll(new ServerStateChangeMessage()
        {
            StateId = (short)CurrentState,
        });
    }

    private IEnumerator ServerWaitAndChangeStateCoroutine(State newState, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ChangeServerState(newState);
    }

    private void ChangeState(State newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case State.Waiting:
                break;
            case State.Playing:
                if (_networkManager.IsServer)
                {
                    // Get the first image
                    _imageManager.GetNextImage();
                    _networkManager.Server.SendAll(new ServerChangeImageMessage()
                    {
                        ImageIndex = _imageManager.CurrentImageIndex,
                    });
            
                    // Assign roles randomly
                    List<int> connectedPeers = _networkManager.Server.ConnectedPeers;
                    int horizontalPeerId = connectedPeers[Random.Range(0, connectedPeers.Count)];
                    _networkManager.Server.Send(horizontalPeerId, new ServerRoleAssignmentMessage()
                    {
                        CurrentRole = ServerRoleAssignmentMessage.Role.Horizontal,
                    });
            
                    List<int> availablePeers = connectedPeers.Where(x => x != horizontalPeerId).ToList();
                    // If there is only one left in the list, go with the first person
                    int verticalPeerId = availablePeers[Random.Range(0, availablePeers.Count)];
                    _networkManager.Server.Send(verticalPeerId, new ServerRoleAssignmentMessage()
                    {
                        CurrentRole = ServerRoleAssignmentMessage.Role.Vertical,
                    });
                }
                break;
            case State.Ending:
                if (_networkManager.IsServer)
                {
                    // Give time to see the final image, then move onto the next image
                    StartCoroutine(ServerWaitAndChangeStateCoroutine(State.Playing, EndPictureDuration));
                }
                break;
        }
        
        StateChanged?.Invoke(newState);
    }
}