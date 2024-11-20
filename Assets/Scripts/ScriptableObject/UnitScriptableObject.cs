using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitScriptableObject", menuName = "ScriptableObject/UnitScriptableObject")]
public class UnitScriptableObject : ScriptableObject
{

    [Header("Properties")]
    public float maxHealth = 100;
    public float baseDamage;
    public int trainTime = 5;
    public int turnToMoveOneTile = 3;

    public float baseDamageResistant = 0;
    public float supportDamage;
    public GameManagement.UnitType unitType;

    public int fundCost;
    public int supplyCost;

    internal Plane dragPlane;
    internal float smoothSpeed = 20f; // Speed of the smooth transition
}
