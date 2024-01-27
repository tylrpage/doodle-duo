using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private readonly Type[] StateTypes = new[]
    {
        typeof(WaitingState),
        typeof(PlayingState),
    };
    
    public IState CurrentState { get; private set; }
    
    private Dictionary<short, Type> _idToState = new Dictionary<short, Type>();
    private Dictionary<Type, short> _stateToId = new Dictionary<Type, short>();

    public StateMachine()
    {
        // Cache state <-> id
        for (short i = 0; i < StateTypes.Length; i++)
        {
            _idToState[i] = StateTypes[i];
            _stateToId[StateTypes[i]] = i;
        }
    }
    
    public short GetStateId<T>() where T : IState
    {
        if (_stateToId.TryGetValue(typeof(T), out short id))
        {
            return id;
        }
        else
        {
            Debug.LogError($"Could not find state id for type {typeof(T)}");
            return -1;
        }
    }
    
    public short GetStateId(IState state)
    {
        if (_stateToId.TryGetValue(state.GetType(), out short id))
        {
            return id;
        }
        else
        {
            Debug.LogError($"Could not find state id for type {state.GetType()}");
            return -1;
        }
    }

    public void SetState<T>() where T : IState
    {
        IState state = (IState) Activator.CreateInstance(typeof(T));
        ChangeState(state);
    }

    public void SetStateId(short id)
    {
        if (_idToState.TryGetValue(id, out Type stateType))
        {
            var state = (IState) Activator.CreateInstance(stateType);
            ChangeState(state);
        }
        else
        {
            Debug.LogError($"Could not switch to state, unknown id: {id}");
        }
    }
    
    private void ChangeState(IState newState)
    {
        CurrentState = newState;
        CurrentState.Enter();
    }
}