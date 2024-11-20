using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AI_State_BuildUp : AI_State
{



    public AI_StateID GetId()
    {
        return AI_StateID.BuildUp;
    }
    public void Enter(AI_Agent ai_agent)
    {
        EnemyOperationManager.Instance.TrainUnit();
    }

    public void Update(AI_Agent ai_agent)
    {
    }

    public void Exit(AI_Agent ai_agent)
    {
    }


}
