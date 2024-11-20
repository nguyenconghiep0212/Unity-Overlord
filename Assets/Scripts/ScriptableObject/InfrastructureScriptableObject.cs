using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "InfrastructureScriptableObject", menuName = "ScriptableObject/InfrastructureScriptableObject")]
public class InfrastructureScriptableObject : ScriptableObject
{
     
    public GameManagement.InfrastructureType infrastructureType;
    public float supportDamage;
    public float baseDamageResistant;
    public float maxHealth = 500;
    public int supply = 5;

    public float turnToBuilt;
}
