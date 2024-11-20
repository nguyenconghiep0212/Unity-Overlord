using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_StateMachine
{
    public AI_State[] states;
    public AI_Agent ai_agent;
    public AI_StateID currentState;

    public AI_StateMachine(AI_Agent ai_agent)
    {
        this.ai_agent = ai_agent;
        int numState = System.Enum.GetNames(typeof(AI_StateID)).Length;
        states = new AI_State[numState];
    }

    public void RegisterState(AI_State state)
    {
        int index = (int)state.GetId();
        states[index] = state;
    }

    public AI_State GetState(AI_StateID stateID)
    {
        int index = (int)stateID;
        return states[index];
    }

    public void Update()
    {
        GetState(currentState)?.Update(ai_agent);
    }

    public void ChangeState(AI_StateID newState)
    {
        GetState(currentState)?.Exit(ai_agent);
        currentState = newState;
        GetState(currentState)?.Enter(ai_agent);
    }
}
