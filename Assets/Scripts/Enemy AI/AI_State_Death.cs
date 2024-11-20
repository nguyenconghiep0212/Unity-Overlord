using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_State_Death: AI_State
{


 
    public AI_StateID GetId()
    {
        return AI_StateID.Death;
    }
    public void Enter(AI_Agent ai_agent)
    {
        GameManagement.Instance.TriggerVictory();
    }





    public void Update(AI_Agent ai_agent)
    {



    }

    public void Exit(AI_Agent ai_agent)
    {
    }
}
