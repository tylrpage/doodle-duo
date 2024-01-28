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
        CountIn,
        Playing,
        Ending, // End of level sequence
        Restarting,
        Won,
    }
    
    public State CurrentState { get; private set; }

    [field: SerializeField] public float EndPictureDuration { get; private set; }
    [field: SerializeField] public float BaseTimeLimit { get; private set; } // determined by music duration
    [SerializeField] private float countInDuration;
    [SerializeField] private float restartingDuration;
    [SerializeField] private float endDurationBeforeWin;
    private NetworkManager _networkManager;
    private ImageManager _imageManager;
    private Coroutine _waitAndChangeCoroutine;

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

    private void ServerWaitAndChangeState(State newState, float waitTime)
    {
        if (_waitAndChangeCoroutine != null)
        {
            StopCoroutine(_waitAndChangeCoroutine);
        }
        _waitAndChangeCoroutine = StartCoroutine(ServerWaitAndChangeStateCoroutine(newState, waitTime));
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
                // This state is also used to cancel the game when a player leaves, so make sure we are not running
                // any coroutines to change state soon after
                if (_waitAndChangeCoroutine != null)
                {
                    StopCoroutine(_waitAndChangeCoroutine);
                }
                break;
            case State.CountIn:
                if (_networkManager.IsServer)
                {
                    // Get the next image
                    _imageManager.GetNextImage();
                    _networkManager.Server.SendAll(new ServerChangeImageMessage()
                    {
                        ImageIndex = _imageManager.CurrentImageIndex,
                    });

                    float scaledCountInDuration = countInDuration *
                                                  (_imageManager.CurrentLevel.timeLimit / BaseTimeLimit);
                    ServerWaitAndChangeState(State.Playing, scaledCountInDuration);
                }
                break;
            case State.Playing:
                if (_networkManager.IsServer)
                {
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
                    // Check for being at the last image
                    if (_imageManager.CurrentImageIndex == _imageManager.levels.Length - 1)
                    {
                        // They won!
                        ServerWaitAndChangeState(State.Won, endDurationBeforeWin);
                    }
                    else
                    {
                        // Give time to see the final image, then move onto the next image
                        ServerWaitAndChangeState(State.CountIn, EndPictureDuration);
                    }
                }
                break;
            case State.Restarting:
                if (_networkManager.IsServer)
                {
                    _waitAndChangeCoroutine = StartCoroutine(RestartCoroutine());
                }

                break;
            case State.Won:
                if (_networkManager.IsServer)
                {
                    ServerWaitAndChangeState(State.CountIn, EndPictureDuration);
                }

                break;
        }
        
        StateChanged?.Invoke(newState);
    }

    private IEnumerator RestartCoroutine()
    {
        yield return new WaitForSeconds(restartingDuration);
        
        // Go back to first image
        _imageManager.Reset();
        ChangeServerState(State.CountIn);
    }
}