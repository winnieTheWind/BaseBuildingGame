using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private IState currentState;

    private Character Character;

    public StateMachine(Character character)
    {
        Character = character;
    }

    public void TransitionToState(IState newState)
    {
        currentState?.OnExit();  // Call OnExit on the current state if it's not null.

        currentState = newState;
        currentState.SetStateMachine(this);  // Ensure the new state has a reference to the state machine.
        currentState.SetCharacter(Character);
        currentState.OnEnter();
    }

    public void Update()
    {
        currentState?.OnUpdate();  // Safeguard against null, in case no initial state is set.
    }
}
