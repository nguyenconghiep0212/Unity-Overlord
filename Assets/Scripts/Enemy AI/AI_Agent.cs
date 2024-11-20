using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Agent : MonoBehaviour
{ 
    [Header("States")]
    public AI_StateMachine stateMachine;
    public AI_StateID initState;
    public AI_StateID currentState;


    // Start is called before the first frame update
    void Start()
    {
        stateMachine = new AI_StateMachine(this);


        // Register State
        stateMachine.RegisterState(new AI_State_Dormant());
        stateMachine.RegisterState(new AI_State_Expand());
        stateMachine.RegisterState(new AI_State_BuildUp());
        stateMachine.RegisterState(new AI_State_Attack());
        stateMachine.RegisterState(new AI_State_Defense());
        stateMachine.RegisterState(new AI_State_Death());
        stateMachine.ChangeState(initState);

    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
        currentState = stateMachine.currentState;

    }
}
