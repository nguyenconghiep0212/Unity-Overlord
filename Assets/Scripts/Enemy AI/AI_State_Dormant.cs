using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_State_Dormant : AI_State
{



    public AI_StateID GetId()
    {
        return AI_StateID.Dormant;
    }
    public void Enter(AI_Agent ai_agent)
    {

    }

    public void Update(AI_Agent ai_agent)
    {

        if (OperationManager.Instance.headQuarter && TimeManagement.Instance.turn == 2)
        {
            ai_agent.stateMachine.ChangeState(AI_StateID.Expand);
        }

    }

    public void Exit(AI_Agent ai_agent)
    {
    }
}
