using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManagement;

public class Unit : MonoBehaviour
{
    public UnitScriptableObject unitScriptableObject;

    private string hexColor = "#3D803D";

    internal Guid id = Guid.NewGuid();
    public float health = 100;
    public MapTile deployedTile;
    public bool inCombat;
    public List<Unit> supportToUnit = new List<Unit>();
    public List<Unit> supportedByUnit = new List<Unit>();
    public List<Infrastruture> supportedByGarrison = new List<Infrastruture>();
    internal MapTile draggingTile;
    internal bool isDragging = false;
    internal Vector3 lastStationPosition;
    public float totalDamage
    {
        get
        {
            return CalculateTotalDamage();
        }
    }
    public float totalDamageResis
    {
        get
        {
            return CalculateTotalDamageResis();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        health = unitScriptableObject.maxHealth;
        unitScriptableObject.dragPlane = new Plane(Vector3.up, Vector3.zero); // Adjust plane as needed 
    }

    // Update is called once per frame
    void Update()
    {
        #region ---- || Draging Unit || ----
        if (!TimeManagement.Instance.isPause)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Check if the ray hits the current object
                if (GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
                {
                    isDragging = true;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                if (draggingTile)
                {
                    if (!draggingTile.occupiedAllyUnit || draggingTile.isScanned)
                    {
                        if (deployedTile)
                        {
                            deployedTile.RemoveOccupiedAllyUnit();
                        }
                        deployedTile = draggingTile;
                        deployedTile.occupiedAllyUnit = this;
                        lastStationPosition = deployedTile.center.position + new Vector3(UnityEngine.Random.Range(0.2f, 0.7f), 0, UnityEngine.Random.Range(0.2f, 0.7f));

                        OperationManager.Instance.AddDeployedUnit(this);
                        GameManagement.Instance.ResetSupportLine(gameObject);

                    }
                    else
                    {
                        transform.position = lastStationPosition;
                    }
                }
                else
                {
                    if (deployedTile)
                    {
                        transform.position = lastStationPosition;
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }

            // Handle dragging
            if (isDragging)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float entry;

                if (unitScriptableObject.dragPlane.Raycast(ray, out entry))
                {
                    Vector3 hitPoint = ray.GetPoint(entry);
                    // Smoothly move the GameObject towards the hit point plus the offset
                    transform.position = Vector3.Lerp(transform.position, new Vector3(hitPoint.x, transform.position.y, hitPoint.z), Time.deltaTime * unitScriptableObject.smoothSpeed);

                }
            }
        }
        #endregion

        #region ---- || Combat || ----
        if (deployedTile)
        {
            // COMBAT
            if (deployedTile.occupiedEnemyUnit || deployedTile.enemyGarrison || deployedTile.enemyHQ)
            {
                inCombat = true; 
            }
            else
            {
                inCombat = false;
                if (EnemyOperationManager.Instance.ownTiles.Contains(deployedTile))
                {
                    EnemyOperationManager.Instance.ownTiles.Remove(deployedTile);
                    deployedTile.isEnemyOwn = false;
                    deployedTile.GetComponent<Renderer>().material = GameManagement.Instance.scannedTileMat;
                }
            }

            // SUPPORT COMBAT
            foreach (MapTile tile in deployedTile.neighborTiles)
            {
                if (tile.occupiedAllyUnit)
                {
                    FindUnitNeedSupport(tile.occupiedAllyUnit);
                }
            }
        }
        #endregion

        #region ---- || Death || ----
        if (health <= 0)
        {
            OperationManager.Instance.currentSupply -= unitScriptableObject.supplyCost;
            OperationManager.Instance.currentSupplyUI.text = OperationManager.Instance.currentSupply.ToString();

            deployedTile.occupiedAllyUnit = null;

            foreach (Infrastruture garrison in OperationManager.Instance.Garrisions)
            {
                garrison.supportToUnit.Remove(this);
            }
            OperationManager.Instance.RemoveDeployedUnit(this);

            Destroy(gameObject);
            GameManagement.Instance.ResetSupportLine(gameObject);

        }
        #endregion
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.GetComponent<MapTile>())
        {
            if (!other.gameObject.GetComponent<MapTile>().isScanned || other.gameObject.GetComponent<MapTile>().occupiedAllyUnit)
            {
                draggingTile = null;
            }
            else
            {
                if (draggingTile)
                {
                    draggingTile.StartCoroutine(draggingTile.UnhighlightTile());
                }
                draggingTile = other.gameObject.GetComponent<MapTile>();
                draggingTile.StartCoroutine(draggingTile.HighlightTile());
            }
        }

        if (other.CompareTag("Barrier"))
        {
            draggingTile = null;
        }
    }
    private float CalculateTotalDamageResis()
    {
        float newDamageResis = 0;

        switch (unitScriptableObject.unitType)
        {
            case GameManagement.UnitType.soldier:
                newDamageResis = unitScriptableObject.baseDamageResistant + TechTreeManager.Instance.totalFootSoldierArmorBuff;
                break;
            case GameManagement.UnitType.mechanize:
                newDamageResis = unitScriptableObject.baseDamageResistant + TechTreeManager.Instance.totalMechanizeForceArmorBuff;
                break;
            case GameManagement.UnitType.air:
                newDamageResis = unitScriptableObject.baseDamageResistant + TechTreeManager.Instance.totalAirForceArmorBuff;
                break;
        }
        return newDamageResis;
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
        return totalDamage;
    }

    public void AttackEnemy()
    {
        if (inCombat)
        {
            // ATTACK UNIT
            Unit_Enemy enemy = deployedTile.occupiedEnemyUnit;
            if (enemy)
            {
                enemy.health -= totalDamage - (totalDamage * enemy.unitScriptableObject.baseDamageResistant);
                if (enemy.health > 0) enemy.GetComponentInChildren<UnitHealthBar>().SetHealthBarPercentage(enemy.health / enemy.unitScriptableObject.maxHealth);
            }

            // ATTACK INFRASTRUCTURE
            Infrastruture_Enemy enemyInfrastructure = deployedTile.enemyGarrison;
            if (!enemyInfrastructure) enemyInfrastructure = deployedTile.enemyHQ;
            if (enemyInfrastructure)
            {
                enemyInfrastructure.health -= totalDamage
                    + supportedByUnit.Sum(u => u.unitScriptableObject.supportDamage)
                    + supportedByGarrison.Sum(g => g.infrastrutureScriptable.supportDamage)
                    - (totalDamage * enemyInfrastructure.infrastrutureScriptable.baseDamageResistant);
                if (enemyInfrastructure.health > 0) enemyInfrastructure.GetComponentInChildren<UnitHealthBar>().SetHealthBarPercentage(enemyInfrastructure.health / enemyInfrastructure.infrastrutureScriptable.maxHealth);

            }
        }
    }

    public void FindUnitNeedSupport(Unit unit)
    {

        if (unit.inCombat)
        {
            if (!unit.supportedByUnit.Contains(this))
            {
                unit.supportedByUnit.Add(this);
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
            if (unit.supportedByUnit.Contains(this))
            {
                GameManagement.Instance.ResetSupportLine(gameObject);
                unit.supportedByUnit.Remove(this);
                supportToUnit.Remove(unit);
            }
        }

    }
}
