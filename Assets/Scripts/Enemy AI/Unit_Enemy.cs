using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit_Enemy : MonoBehaviour
{
    public UnitScriptableObject unitScriptableObject;


    internal Guid id = Guid.NewGuid();
    public float health = 100;
    public MapTile deployedTile;
    public bool inCombat;
    public List<Unit_Enemy> supportToUnit = new List<Unit_Enemy>();
    public List<Unit_Enemy> supportedByUnit = new List<Unit_Enemy>();
    public List<Infrastruture_Enemy> supportedByGarrison = new List<Infrastruture_Enemy>();
    internal MapTile draggingTile;
    internal bool isDragging = false;
    internal Vector3 lastStationPosition;
    private bool isInvisible;
    public float totalDamage
    {
        get
        {
            return CalculateTotalDamage();
        }
    }
    private string hexColor = "#803D43";
    // Start is called before the first frame update
    void Start()
    {
        health = unitScriptableObject.maxHealth;
        unitScriptableObject.dragPlane = new Plane(Vector3.up, Vector3.zero); // Adjust plane as needed 
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


        #region ---- || Combat || ----
        if (deployedTile)
        {
            // COMBAT
            if (deployedTile.occupiedAllyUnit || deployedTile.garrison || deployedTile.HQ || deployedTile.scanner)
            {
                inCombat = true;
            }
            else
            {
                inCombat = false;
                if (OperationManager.Instance.ownTiles.Contains(deployedTile))
                {
                    OperationManager.Instance.RemoveTileFromOwn(deployedTile);
                }
            }

            // SUPPORT COMBAT
            foreach (MapTile tile in deployedTile.neighborTiles)
            {
                if (tile.occupiedEnemyUnit)
                {
                    FindUnitNeedSupport(tile.occupiedEnemyUnit);
                }
            }
        }
        #endregion

        #region ---- || Death || ----
        if (health <= 0)
        {
            EnemyOperationManager.Instance.currentSupply -= unitScriptableObject.supplyCost;

            deployedTile.occupiedEnemyUnit = null;

            foreach (Infrastruture_Enemy garrison in EnemyOperationManager.Instance.Garrisions)
            {
                garrison.supportToUnit.Remove(this);
            }
            foreach(Unit_Enemy unit in supportedByUnit)
            {
                unit.supportToUnit.Remove(this);
            }

            EnemyOperationManager.Instance.RemoveDeployedUnit(this);

            Destroy(gameObject);
            GameManagement.Instance.ResetSupportLine(gameObject);

        }
        #endregion
    }



    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.GetComponent<MapTile>())
        {
            draggingTile = other.gameObject.GetComponent<MapTile>();

        }

        if (other.CompareTag("Barrier"))
        {
            draggingTile = null;
        }
    }

    private float CalculateTotalDamage()
    {
        float totalDamage = unitScriptableObject.baseDamage;

        switch (unitScriptableObject.unitType)
        {
            case GameManagement.UnitType.soldier:
                totalDamage = unitScriptableObject.baseDamage + (unitScriptableObject.baseDamage * deployedTile.footSoldierModifier) + (unitScriptableObject.baseDamage * TechTreeManager.Instance.totalFootSoldierDamageBuff);
                break;
            case GameManagement.UnitType.mechanize:
                totalDamage = unitScriptableObject.baseDamage + (unitScriptableObject.baseDamage * deployedTile.mechanizeForceModifier) + (unitScriptableObject.baseDamage * TechTreeManager.Instance.totalMechanizeForceDamageBuff); ;
                break;
            case GameManagement.UnitType.air:
                totalDamage = unitScriptableObject.baseDamage + (unitScriptableObject.baseDamage * deployedTile.airForceModifier) + (unitScriptableObject.baseDamage * TechTreeManager.Instance.totalAirForceDamageBuff); ;
                break;
        }

        switch (Setting.Instance.difficulty)
        {
            case Setting.DifficultyEnum.easy:
                totalDamage -= totalDamage * 0.15f;
                break;
            case Setting.DifficultyEnum.normal:
                break;
            case Setting.DifficultyEnum.hard:
                totalDamage += totalDamage * 0.25f;
                break;
        }
        return totalDamage;
    }

    public void AttackPlayer()
    {
        if (inCombat)
        {
            // ATTACK UNIT
            Unit ally = deployedTile.occupiedAllyUnit;
            if (ally)
            {
                ally.health -= totalDamage - (totalDamage * ally.totalDamageResis);
                if (ally.health > 0) ally.GetComponentInChildren<UnitHealthBar>().SetHealthBarPercentage(ally.health / ally.unitScriptableObject.maxHealth);
            }

            // ATTACK INFRASTRUCTURE
            Infrastruture allyInfrastructure = deployedTile.garrison;
            if (!allyInfrastructure) allyInfrastructure = deployedTile.scanner;
            if (!allyInfrastructure) allyInfrastructure = deployedTile.HQ;
            if (allyInfrastructure)
            {
                allyInfrastructure.health -= totalDamage
                    + supportedByUnit.Sum(u => u.unitScriptableObject.supportDamage)
                    + supportedByGarrison.Sum(g => g.infrastrutureScriptable.supportDamage)
                    - (totalDamage * allyInfrastructure.infrastrutureScriptable.baseDamageResistant);
                if (allyInfrastructure.health > 0) allyInfrastructure.GetComponentInChildren<UnitHealthBar>().SetHealthBarPercentage(allyInfrastructure.health / allyInfrastructure.infrastrutureScriptable.maxHealth);
            }
        }
    }

    public void FindUnitNeedSupport(Unit_Enemy unit)
    {

        if (unit.inCombat)
        {
            if (!unit.supportedByUnit.Contains(this))
            {
                unit.supportedByUnit.Add(this);
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
            if (unit.supportedByUnit.Contains(this))
            {
                GameManagement.Instance.ResetSupportLine(gameObject);
                unit.supportedByUnit.Remove(this);
                supportToUnit.Remove(unit);
            }
        }

    }
}
