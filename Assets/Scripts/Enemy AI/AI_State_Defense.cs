using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AI_State_Defense : AI_State
{


 
    public AI_StateID GetId()
    {
        return AI_StateID.Defense;
    }
    public void Enter(AI_Agent ai_agent)
    {
        //EnemyOperationManager.Instance.Defense();
    }
    public void Update(AI_Agent ai_agent)
    {
        if (!EnemyOperationManager.Instance.headQuarter.deployedTile.neighborTiles.Any(t => t.occupiedAllyUnit))
        {
            ai_agent.stateMachine.ChangeState(AI_StateID.BuildUp);
        }

    }

    public void Exit(AI_Agent ai_agent)
    {
    }
}
