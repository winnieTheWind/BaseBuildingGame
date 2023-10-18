using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void OnEnter();
    void OnExit();
    void OnUpdate();
    void SetStateMachine(StateMachine stateMachine);
    void SetCharacter(Character character);

    // Optionally, if you want to have logic when transitioning out of a state.
    // void OnExit();
}
