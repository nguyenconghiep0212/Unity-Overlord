using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManagement;

public class Infrastruture_Enemy : MonoBehaviour
{
    public InfrastructureScriptableObject infrastrutureScriptable;

    internal Guid id = Guid.NewGuid();
    public MapTile deployedTile;
    public float health = 500;
    public int cost;
    private string hexColor = "#803D43"; 

    public List<Unit_Enemy> supportToUnit;

    private bool isInvisible;

    // Start is called before the first frame update
    void Start()
    {
        health = infrastrutureScriptable.maxHealth; 
    }

    // Update is called once per frame
    void Update()
    {
        if (!deployedTile.isScanned)
        {
            if (!isInvisible)
            {
                GameManagement.Instance.SetTransparency(transform, false);
                isInvisible = true;
            }
        }
        else
        {
            if (isInvisible)
            {
                GameManagement.Instance.SetTransparency(transform, true);
                isInvisible = false;
            }
        }


        if (deployedTile)
        {
            foreach (MapTile tile in deployedTile.neighborTiles)
            {
                if (tile.occupiedEnemyUnit)
                {
                    FindUnitNeedSupport(tile.occupiedEnemyUnit);
                }
            }
        }

        if (health <= 0)
        {
            if (infrastrutureScriptable.infrastructureType == InfrastructureType.Garrison)
            {
                GameManagement.Instance.ResetSupportLine(gameObject);
                deployedTile.enemyGarrison = null;
                Destroy(gameObject);
            }
            if (infrastrutureScriptable.infrastructureType == InfrastructureType.HQ)
            {
                if (health <= 0)
                {
                    EnemyOperationManager.Instance.ai_agent.stateMachine.ChangeState(AI_StateID.Death);
                }
            }
        }
    }

    public void FindUnitNeedSupport(Unit_Enemy unit)
    {

        if (unit.inCombat)
        {
            if (!unit.supportedByGarrison.Contains(this))
            {
                unit.supportedByGarrison.Add(this);
                supportToUnit.Add(unit);

                //GameObject supportLine = Instantiate(GameManagement.Instance.lineRenderer.gameObject, GameObject.FindWithTag("Enemy").transform.GetChild(0));
                //supportLine.GetComponent<LineControl>().points.Add(unit.transform);
                //supportLine.GetComponent<LineControl>().points.Add(transform);
                //supportLine.GetComponent<LineControl>().id = id + unit.id.ToString();
                //supportLine.GetComponent<LineControl>().setColor(hexColor);
                //GameManagement.Instance.supportLineList.Add(supportLine);
            }
        }
        else
        {
            if (unit.supportedByGarrison.Contains(this))
            {
                unit.supportedByGarrison.Remove(this);
                supportToUnit.Remove(unit);
            }
        } 
    }

    public void AttackPlayer()
    {
        if (deployedTile.occupiedAllyUnit)
        {
            Unit ally = deployedTile.occupiedAllyUnit;
            if (ally)
            {
                ally.health -= infrastrutureScriptable.supportDamage
                    - (infrastrutureScriptable.supportDamage * ally.totalDamageResis);
                if (ally.health > 0) ally.GetComponentInChildren<UnitHealthBar>().SetHealthBarPercentage(ally.health / ally.unitScriptableObject.maxHealth);
            }
        }
    }
}
