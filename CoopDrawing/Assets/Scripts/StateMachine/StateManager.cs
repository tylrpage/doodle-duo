using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class StateManager : MonoBehaviour, IService
{
    public event Action<State> StateChanged;
    
    public enum State : short
    {
        Waiting,
        Playing,
    }
    
    public State CurrentState { get; private set; }

    private NetworkManager _networkManager;

    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }

    private void Start()
    {
        _networkManager = GameManager.Instance.GetService<NetworkManager>();
        _networkManager.Sided += OnSided;
        if (_networkManager.IsSided)
        {
            OnSided();
        }
    }

    private void OnSided()
    {
        if (_networkManager.IsServer)
        {
            _networkManager.Server.MessageReceived += OnMessageReceived;
        }
        else
        {
            _networkManager.Client.MessageReceived += OnMessageReceived;
        }
    }
    
    private void OnMessageReceived(BitSerializable message)
    {
        switch (message)
        {
            case StateChangeMessage stateChangeMessage:
                ChangeState((State)stateChangeMessage.StateId);
                break;
        }
    }
    
    public void ChangeState(State newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case State.Waiting:
                break;
            case State.Playing:
                break;
        }
        
        StateChanged?.Invoke(newState);
    }
}