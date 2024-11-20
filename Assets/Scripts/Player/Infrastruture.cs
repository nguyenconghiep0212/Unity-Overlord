using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManagement;

public class Infrastruture : MonoBehaviour
{
    public InfrastructureScriptableObject infrastrutureScriptable;

    internal Guid id = Guid.NewGuid();
    internal MapTile deployedTile;
    public float health = 500;
    public int cost;
    private string hexColor = "#3D803D";

    //public GameManagement.InfrastructureType infrastructureType;
    //public float supportDamage;
    //public float maxHealth = 500;

    public List<Unit> supportToUnit;

    // Start is called before the first frame update
    void Start()
    {
        health = infrastrutureScriptable.maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (infrastrutureScriptable.infrastructureType == GameManagement.InfrastructureType.Garrison || infrastrutureScriptable.infrastructureType == GameManagement.InfrastructureType.HQ)
        {
            foreach (MapTile tile in deployedTile.neighborTiles)
            {
                if (tile.occupiedAllyUnit)
                {
                    FindUnitNeedSupport(tile.occupiedAllyUnit);
                }
            }
        }

        if (health <= 0)
        {
            switch (infrastrutureScriptable.infrastructureType)
            {
                case InfrastructureType.HQ:
                    if(!OperationManager.Instance.isDefeated) GameManagement.Instance.TriggerDefeat(); 
                    break;
                case InfrastructureType.Garrison:
                    GameManagement.Instance.ResetSupportLine(gameObject);
                    OperationManager.Instance.ownTiles.Remove(deployedTile);
                    deployedTile.garrison = null;
                    Destroy(gameObject);
                    break;
                case InfrastructureType.Scanner:
                    OperationManager.Instance.ownTiles.Remove(deployedTile);
                    deployedTile.scanner = null;
                    Destroy(gameObject);
                    break;
            }

        }
    }

    public void FindUnitNeedSupport(Unit unit)
    {

        if (unit.inCombat)
        {
            if (!unit.supportedByGarrison.Contains(this))
            {
                unit.supportedByGarrison.Add(this);
                supportToUnit.Add(unit);

                GameObject supportLine = Instantiate(GameManagement.Instance.lineRenderer.gameObject, GameObject.FindWithTag("Player").transform.GetChild(0));
                supportLine.GetComponent<LineControl>().points.Add(unit.transform);
                supportLine.GetComponent<LineControl>().points.Add(transform);
                supportLine.GetComponent<LineControl>().id = id + unit.id.ToString();
                supportLine.GetComponent<LineControl>().setColor(hexColor);
                GameManagement.Instance.supportLineList.Add(supportLine);
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

    public void AttackEnemy()
    {
        if (deployedTile.occupiedEnemyUnit)
        {
            Unit_Enemy enemy = deployedTile.occupiedEnemyUnit;
            if (enemy)
            {
                enemy.health -= infrastrutureScriptable.supportDamage
                    + ((TechTreeManager.Instance.purchasedUpgrades.Contains("GARS_DMG_1") ? 0.1f : 0) * infrastrutureScriptable.supportDamage)
                    + ((TechTreeManager.Instance.purchasedUpgrades.Contains("GARS_DMG_2") ? 0.15f : 0) * infrastrutureScriptable.supportDamage)
                    - (infrastrutureScriptable.supportDamage * enemy.unitScriptableObject.baseDamageResistant);
                if (enemy.health > 0) enemy.GetComponentInChildren<UnitHealthBar>().SetHealthBarPercentage(enemy.health / enemy.unitScriptableObject.maxHealth);
            }
        }
    }
}
