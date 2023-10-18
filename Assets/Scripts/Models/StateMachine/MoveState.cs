using System;
using UnityEngine;

public class MoveState : IState
{
    private StateMachine stateMachine;
    int time = 0;
    private Character character;

    public void SetStateMachine(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void SetCharacter(Character character)
    {
        this.character = character;
    }

    public void OnEnter()
    {
        Debug.Log("OnEnter Move State");
        time = 0;
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {
        time++;
        if (time > 1000)
        {
            this.stateMachine.TransitionToState(new IdleState());
        }
    }
}
