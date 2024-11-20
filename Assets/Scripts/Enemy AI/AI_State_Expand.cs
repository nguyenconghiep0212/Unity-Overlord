using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AI_State_Expand : AI_State
{



    public AI_StateID GetId()
    {
        return AI_StateID.Expand;
    }
    public void Enter(AI_Agent ai_agent)
    { 
        EnemyOperationManager.Instance.CalculateDistanceToPlayerHQ();
        EnemyOperationManager.Instance.StartExpand();
    }

    public void Update(AI_Agent ai_agent)
    {
        
    }

    public void Exit(AI_Agent ai_agent)
    {
    }

   
}
