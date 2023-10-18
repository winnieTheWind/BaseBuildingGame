using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState
{
    private StateMachine stateMachine;
    private Character character;
    int time = 0;

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
        time = 0;
        Debug.Log("OnEnter Idle State");
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {
        time++;
        if (time > 1000)
        {
            this.stateMachine.TransitionToState(new MoveState());
        }
    }
}
