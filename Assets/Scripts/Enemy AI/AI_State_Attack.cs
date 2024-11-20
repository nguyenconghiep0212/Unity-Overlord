using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AI_State_Attack : AI_State
{



    public AI_StateID GetId()
    {
        return AI_StateID.Attack;
    }
    public void Enter(AI_Agent ai_agent)
    {
        EnemyOperationManager.Instance.Attack();
    }
    public void Update(AI_Agent ai_agent)
    {
        if (EnemyOperationManager.Instance.totalDeployUnit.Count < 1) return;


        if ((float)EnemyOperationManager.Instance.totalDeployUnit.Sum(u => u.unitScriptableObject.supplyCost) / EnemyOperationManager.Instance.maxSupply < 0.25)
        {
            ai_agent.stateMachine.ChangeState(AI_StateID.BuildUp);
        }

        //if (EnemyOperationManager.Instance.headQuarter.deployedTile.occupiedAllyUnit || EnemyOperationManager.Instance.headQuarter.deployedTile.neighborTiles.Any(t => t.occupiedAllyUnit))
        //{
        //    ai_agent.stateMachine.ChangeState(AI_StateID.Defense);
        //}
    }

    public void Exit(AI_Agent ai_agent)
    {
    }


}
