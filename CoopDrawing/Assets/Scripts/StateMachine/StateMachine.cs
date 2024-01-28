using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class StateMachine
{
    public enum State : short
    {
        Waiting,
        Playing,
    }
    
    public State CurrentState { get; private set; }

    public StateMachine(Client client)
    {
        client.MessageReceived += OnMessageReceived;
    }

    public StateMachine(Server server)
    {
        server.MessageReceived += OnMessageReceived;
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
                // todo: show image
                break;
        }
    }
}