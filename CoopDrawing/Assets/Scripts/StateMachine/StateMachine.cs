using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private readonly Dictionary<int, IState> States = new Dictionary<int, IState>()
    {
        { 0, new WaitingState() }
    };

    private IState _currentState;

    public void SetStateId(int id)
    {
        if (States.TryGetValue(id, out IState newState))
        {
            ChangeState(newState);
        }
        else
        {
            Debug.LogError($"Could not switch to state, unknown id: {id}");
        }
    }
    
    private void ChangeState(IState newState)
    {
        _currentState = newState;
        _currentState.Enter();
    }
}