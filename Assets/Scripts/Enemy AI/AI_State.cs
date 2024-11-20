using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AI_StateID
{
    Dormant,
    Expand,
    BuildUp,
    Attack,
    Defense,
    Death
}
public interface AI_State 
{
    AI_StateID GetId();
    void Enter(AI_Agent ai_agent);
    void Update(AI_Agent ai_agent);
    void Exit(AI_Agent ai_agent);
}
