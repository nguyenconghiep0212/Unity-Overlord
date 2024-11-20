using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyOperationManager : MonoBehaviour
{
    public static EnemyOperationManager Instance { get; set; }
    internal AI_Agent ai_agent;
    private void Awake()
    {
        ai_agent = GetComponent<AI_Agent>();

        if (Instance != null & Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    public InfrastructureScriptableObject garrisonScriptable;
    public InfrastructureScriptableObject HQScriptable;


    [Header("Enemy MAT")]
    public Material foggedEnemyTileMat;
    public Material highlightedFoggedEnemyTileMat;
    public Material scannedEnemyTileMat;
    public Material highlightedScannedEnemyTileMat;


    [Header("Enemy Operation")]
    public Infrastruture_Enemy headQuarter;
    public List<Infrastruture_Enemy> Garrisions;
    public List<MapTile> ownTiles = new List<MapTile>();
    public int totalFund;
    public int fundPerTurn = 5000;
    public int maxSupply = 10;
    public int currentSupply;

    public List<Unit_Enemy> deployedFootSoldier = new List<Unit_Enemy>();
    public List<Unit_Enemy> deployedMechanizeForce = new List<Unit_Enemy>();
    public List<Unit_Enemy> deployedAirForce = new List<Unit_Enemy>();

    public float footSoldierPercentage = 0.5f;
    public float mechanizeForcePercentage = 0.3f;
    public float airForcePercentage = 0.2f;

    public int numOfGarrisonExpand = 3;
    public int garrisonBuildSpeedBuff = 0;

    public List<Unit_Enemy> totalDeployUnit
    {
        get
        {
            return deployedFootSoldier.Concat(deployedMechanizeForce).Concat(deployedAirForce).ToList();
        }
    }


    [Header("Prefabs")]
    public GameObject EnemyFootSoldierPrefab;
    public GameObject EnemyMechanizeForcePrefab;
    public GameObject EnemyAirForcePrefab;
    public GameObject EnemyHQPrefab;
    public GameObject EnemyGarrisonPrefab;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    #region ---- || Choose HQ || ----
    private int thresholdDistance = 2;
    private int highestDistance = -1;
    public void CalculateDistanceToPlayerHQ()
    {
        // Create a queue for BFS
        Queue<MapTile> queue = new Queue<MapTile>();
        // Dictionary to hold distances
        Dictionary<MapTile, int> distances = new Dictionary<MapTile, int>();

        // Initialize all distances to -1 (unvisited)
        foreach (MapTile tile in GameManagement.Instance.totalTile)
        {
            distances[tile] = -1; // -1 indicates unvisited
        }

        // Set the distance of the selected tile to 0 and enqueue it
        distances[OperationManager.Instance.headQuarter.deployedTile] = 0;
        queue.Enqueue(OperationManager.Instance.headQuarter.deployedTile);

        // Perform BFS
        while (queue.Count > 0)
        {
            MapTile currentTile = queue.Dequeue();
            int currentDistance = distances[currentTile];

            // Check each neighboring tile
            foreach (MapTile neighbor in currentTile.neighborTiles)
            {
                // If the neighbor hasn't been visited
                if (distances[neighbor] == -1)
                {
                    distances[neighbor] = currentDistance + 1; // Set distance
                    queue.Enqueue(neighbor); // Enqueue the neighbor
                }
            }
        }


        // Initialize to -1, indicating no distance found
        foreach (var kvp in distances)
        {
            kvp.Key.distanceFromPlayerHQ = kvp.Value;
            if (kvp.Value > highestDistance)
            {
                highestDistance = kvp.Value;
            }
        }

        FindHQPlacement();
    }

    private void FindHQPlacement()
    {
        foreach (MapTile tile in GameManagement.Instance.totalTile)
        {
            if (tile.distanceFromPlayerHQ < thresholdDistance)
            {
                tile.enemyHQSpawnPercentage = 0;
            }
            else
            {
                tile.enemyHQSpawnPercentage = (float)tile.distanceFromPlayerHQ / highestDistance;
            }
        }

        int randomIndex = Random.Range(0, GameManagement.Instance.totalTile.Where(tile => tile.enemyHQSpawnPercentage >= 0.7).ToList().Count);
        MapTile enemyHQTile = GameManagement.Instance.totalTile.Where(tile => tile.enemyHQSpawnPercentage >= 0.7).ToList()[randomIndex];

        GameObject newHQ = Instantiate(EnemyHQPrefab, GameObject.FindWithTag("Enemy").transform.GetChild(1));
        newHQ.transform.position = new Vector3(enemyHQTile.transform.GetChild(0).position.x, 0.03f, enemyHQTile.transform.GetChild(0).position.z);

        headQuarter = newHQ.GetComponent<Infrastruture_Enemy>();
        headQuarter.GetComponent<Infrastruture_Enemy>().deployedTile = enemyHQTile;
        enemyHQTile.enemyHQ = headQuarter;

        maxSupply = enemyHQTile.supply;

        UpdateOwnTile(enemyHQTile);
        CalculateDistanceToEnemyHQ();
    }
    public void CalculateDistanceToEnemyHQ()
    {
        // Create a queue for BFS
        Queue<MapTile> queue = new Queue<MapTile>();
        // Dictionary to hold distances
        Dictionary<MapTile, int> distances = new Dictionary<MapTile, int>();

        // Initialize all distances to -1 (unvisited)
        foreach (MapTile tile in GameManagement.Instance.totalTile)
        {
            distances[tile] = -1; // -1 indicates unvisited
        }

        // Set the distance of the selected tile to 0 and enqueue it
        distances[headQuarter.deployedTile] = 0;
        queue.Enqueue(headQuarter.deployedTile);

        // Perform BFS
        while (queue.Count > 0)
        {
            MapTile currentTile = queue.Dequeue();
            int currentDistance = distances[currentTile];

            // Check each neighboring tile
            foreach (MapTile neighbor in currentTile.neighborTiles)
            {
                // If the neighbor hasn't been visited
                if (distances[neighbor] == -1)
                {
                    distances[neighbor] = currentDistance + 1; // Set distance
                    queue.Enqueue(neighbor); // Enqueue the neighbor
                }
            }
        }
        // Initialize to -1, indicating no distance found
        foreach (var kvp in distances)
        {
            kvp.Key.distanceFromEnemyHQ = kvp.Value;
            if (kvp.Value > highestDistance)
            {
                highestDistance = kvp.Value;
            }
        }
    }
    #endregion

    #region ---- || GARRISON EXPAND || ----
    public void StartExpand()
    {
        StartCoroutine(SelectGarrisonPlacement());
    }
    public IEnumerator SelectGarrisonPlacement()
    {
        switch (Setting.Instance.difficulty)
        {
            case Setting.DifficultyEnum.easy:
                numOfGarrisonExpand = 2;
                garrisonBuildSpeedBuff = -1;
                break;
            case Setting.DifficultyEnum.normal:
                numOfGarrisonExpand = 3;
                garrisonBuildSpeedBuff = 0;
                break;
            case Setting.DifficultyEnum.hard:
                numOfGarrisonExpand = 5;
                garrisonBuildSpeedBuff = 4;
                break;
        }
        if (Garrisions.Count == numOfGarrisonExpand)
        {
            ai_agent.stateMachine.ChangeState(AI_StateID.BuildUp);
        }
        else
        {
            if (totalFund > EnemyGarrisonPrefab.GetComponent<Infrastruture_Enemy>().cost)
            {
                totalFund -= EnemyGarrisonPrefab.GetComponent<Infrastruture_Enemy>().cost;

                // Initialize variables to track max distance and matching tiles
                float maxDistance = float.MinValue;
                List<MapTile> matchingTiles = new List<MapTile>();

                foreach (MapTile tile in ownTiles.Where(t => !t.enemyGarrison && !t.enemyHQ && !t.scanner && !t.garrison && !t.HQ && !t.occupiedAllyUnit))
                {
                    if (tile.distanceFromPlayerHQ > maxDistance)
                    {
                        maxDistance = tile.distanceFromPlayerHQ; // Update max distance
                        matchingTiles.Clear(); // Clear previous matches
                        matchingTiles.Add(tile); // Add the new max tile
                    }
                    else if (tile.distanceFromPlayerHQ == maxDistance)
                    {
                        matchingTiles.Add(tile); // Add to matches if equal
                    }
                }

                int randomIndex = Random.Range(0, matchingTiles.Count);
                try
                {
                    MapTile newGarrisonTile = matchingTiles[randomIndex];

                    StartCoroutine(BuildingGarrison(newGarrisonTile));
                }
                catch (System.Exception error)
                {
                    Debug.Log("BuildingGarrison Error: " + error);
                }
            }
            else
            {
                yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval);
                StartExpand();
            }
        }

    }
    private IEnumerator BuildingGarrison(MapTile newGarrisonTile)
    {
        yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval * (EnemyGarrisonPrefab.GetComponent<Infrastruture_Enemy>().infrastrutureScriptable.turnToBuilt - garrisonBuildSpeedBuff));
        GameObject newGarrison = Instantiate(EnemyGarrisonPrefab, GameObject.FindWithTag("Enemy").transform.GetChild(1));
        newGarrison.transform.position = new Vector3(newGarrisonTile.transform.GetChild(0).position.x - 0.4f, 0.03f, newGarrisonTile.transform.GetChild(0).position.z);
        newGarrison.GetComponent<Infrastruture_Enemy>().deployedTile = newGarrisonTile;
        Garrisions.Add(newGarrison.GetComponent<Infrastruture_Enemy>());

        newGarrisonTile.enemyGarrison = newGarrison.GetComponent<Infrastruture_Enemy>();

        maxSupply += newGarrisonTile.supply;
        UpdateOwnTile(newGarrisonTile);
        StartExpand();

    }
    #endregion

    #region ---- || UNIT BUILD UP || ----
    public void TrainUnit()
    {
        List<MapTile> matchingTiles = ownTiles.Where(t => !t.occupiedEnemyUnit).ToList();
        if (matchingTiles.Count < 1)
        {
            ai_agent.stateMachine.ChangeState(AI_StateID.Attack);
            return;
        }
        int randomIndex = Random.Range(0, matchingTiles.Count);
        MapTile newUnitSpawnTile = matchingTiles[randomIndex];

        int totalSupplySoldier = deployedFootSoldier.Sum(e => e.unitScriptableObject.supplyCost);
        int totalSupplyMechanize = deployedMechanizeForce.Sum(e => e.unitScriptableObject.supplyCost);
        int totalSupplyAir = deployedAirForce.Sum(e => e.unitScriptableObject.supplyCost);

        if ((float)totalSupplySoldier / maxSupply <= footSoldierPercentage)
        {
            StartCoroutine(TrainingUnit(EnemyFootSoldierPrefab, newUnitSpawnTile));
        }
        else if ((float)totalSupplyMechanize / maxSupply <= mechanizeForcePercentage)
        {
            StartCoroutine(TrainingUnit(EnemyMechanizeForcePrefab, newUnitSpawnTile));
        }
        else if ((float)totalSupplyAir / maxSupply <= airForcePercentage)
        {
            StartCoroutine(TrainingUnit(EnemyAirForcePrefab, newUnitSpawnTile));
        }
        else
        {
            StartCoroutine(TrainingUnit(EnemyFootSoldierPrefab, newUnitSpawnTile));
        }
    }
    private IEnumerator TrainingUnit(GameObject newUnitPrefab, MapTile newUnitSpawnTile)
    {
        if (newUnitPrefab.GetComponent<Unit_Enemy>().unitScriptableObject.supplyCost + currentSupply <= maxSupply)
        {
            if (newUnitPrefab.GetComponent<Unit_Enemy>().unitScriptableObject.fundCost < totalFund)
            {

                yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval * newUnitPrefab.GetComponent<Unit_Enemy>().unitScriptableObject.trainTime);
                GameObject newUnit = Instantiate(newUnitPrefab, GameObject.FindWithTag("Enemy").transform.GetChild(0));
                newUnit.transform.position = newUnitSpawnTile.transform.GetChild(0).position;
                newUnit.GetComponent<Unit_Enemy>().deployedTile = newUnitSpawnTile;

                switch (newUnit.GetComponent<Unit_Enemy>().unitScriptableObject.unitType)
                {
                    case GameManagement.UnitType.soldier:
                        deployedFootSoldier.Add(newUnit.GetComponent<Unit_Enemy>());
                        break;
                    case GameManagement.UnitType.mechanize:
                        deployedMechanizeForce.Add(newUnit.GetComponent<Unit_Enemy>());
                        break;
                    case GameManagement.UnitType.air:
                        deployedAirForce.Add(newUnit.GetComponent<Unit_Enemy>());
                        break;
                }

                newUnitSpawnTile.occupiedEnemyUnit = newUnit.GetComponent<Unit_Enemy>();
                currentSupply += newUnitPrefab.GetComponent<Unit_Enemy>().unitScriptableObject.supplyCost;
                totalFund -= newUnitPrefab.GetComponent<Unit_Enemy>().unitScriptableObject.fundCost;

                TrainUnit();
            }
            else
            {
                yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval * newUnitPrefab.GetComponent<Unit_Enemy>().unitScriptableObject.trainTime);
                TrainUnit();
            }
        }
        else
        {
            yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval * newUnitPrefab.GetComponent<Unit_Enemy>().unitScriptableObject.trainTime);
            ai_agent.stateMachine.ChangeState(AI_StateID.Attack);
        }
    }
    #endregion

    #region ---- || UNIT AI (A lack of Intenlligent) || ----
    public void Attack()
    {
        try
        {
            StartCoroutine(Attacking());
        }
        catch (System.Exception error)
        {
            Debug.Log("Attack: " + error);
        }
    }
    private IEnumerator Attacking()
    {
        foreach (Unit_Enemy unit in totalDeployUnit)
        {
            if (unit.inCombat || unit.supportToUnit.Count > 0) continue;
            yield return StartCoroutine(FindTargetToAttack(unit));
        }
        //Attack();
        ai_agent.stateMachine.ChangeState(AI_StateID.BuildUp);
    }
    private IEnumerator FindTargetToAttack(Unit_Enemy unit)
    {
        MapTile moveToTile = null;
        foreach (MapTile tile in unit.deployedTile.neighborTiles)
        {
            if (!tile.occupiedEnemyUnit)
            {

                if (tile.occupiedAllyUnit || tile.garrison || tile.scanner || tile.HQ)
                {
                    moveToTile = tile;
                    break;
                }
            }
        }
        if (!moveToTile)
        {
            moveToTile = unit.deployedTile.neighborTiles.Where(t => !t.occupiedEnemyUnit).OrderBy(t => t.distanceFromPlayerHQ).FirstOrDefault();
        }

        if (moveToTile)
        {
            yield return StartCoroutine(MoveUnit(unit, moveToTile));
        }
    }


    //public void Defense()
    //{
    //    try
    //    {
    //        StartCoroutine(Defending());
    //    }
    //    catch (System.Exception error)
    //    {
    //        Debug.Log("Defense: " + error);
    //    }
    //}
    //private IEnumerator Defending()
    //{
    //    foreach (Unit_Enemy unit in totalDeployUnit)
    //    {
    //        if (unit.inCombat || unit.supportToUnit.Count > 0) continue;
    //        yield return StartCoroutine(FindTargetToDefense(unit));

    //    }
    //    Defense();
    //}
    //private IEnumerator FindTargetToDefense(Unit_Enemy unit)
    //{
    //    MapTile moveToTile = null;
    //    if (unit.supportToUnit.Count == 0)
    //    {
    //        try
    //        {
    //            foreach (MapTile tile in unit.deployedTile.neighborTiles)
    //            {
    //                if (tile.occupiedEnemyUnit)
    //                {
    //                    continue;
    //                }
    //                foreach (MapTile tile2 in tile.neighborTiles)
    //                {
    //                    if (tile2.neighborTiles.Any(t2 => t2.occupiedEnemyUnit.inCombat))
    //                    {
    //                        moveToTile = tile;
    //                        break;
    //                    }
    //                }
    //                if (moveToTile)
    //                {
    //                    break;
    //                }
    //            }

    //            if (!moveToTile)
    //            {
    //                moveToTile = unit.deployedTile.neighborTiles.Where(t => !t.occupiedEnemyUnit).OrderBy(t => t.distanceFromEnemyHQ).FirstOrDefault();
    //            }
    //        }
    //        catch (System.Exception error)
    //        {
    //            Debug.Log("FindTargetToDefense error: " + error);
    //        }
    //        if (moveToTile)
    //        {
    //            yield return StartCoroutine(MoveUnit(unit, moveToTile));
    //        }
    //    }
    //    else
    //    {
    //        yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval);
    //    }

    //}
    private IEnumerator MoveUnit(Unit_Enemy unit, MapTile moveToTile)
    {
        yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval * unit.unitScriptableObject.turnToMoveOneTile);
        if (!unit.inCombat)
        {
            try
            {
                unit.deployedTile.occupiedEnemyUnit = null;
                unit.transform.position = moveToTile.transform.GetChild(0).position;
                unit.GetComponent<Unit_Enemy>().deployedTile = moveToTile;
                moveToTile.occupiedEnemyUnit = unit.GetComponent<Unit_Enemy>();
                GameManagement.Instance.ResetSupportLine(unit.gameObject);
            }
            catch (System.Exception)
            {
                // UNIT GOT DESTROYED WHILE TRYING TO ACCESS
                print("Unit has been destroyed");
            }
        }

    }
    #endregion

    public void UpdateOwnTile(MapTile newTile, bool updateNeighbor = true)
    {
        if (!ownTiles.Contains(newTile))
        {
            ownTiles.Add(newTile);
            AddTileToOwn(newTile);
        }
        if (updateNeighbor)
        {
            foreach (MapTile tile in newTile.neighborTiles)
            {
                if (!ownTiles.Contains(tile))
                {
                    ownTiles.Add(tile);
                    AddTileToOwn(tile);
                }
            }
        }
    }
    public void AddTileToOwn(MapTile tile)
    {
        tile.isEnemyOwn = true;
        tile.GetComponent<Renderer>().material = tile.isScanned ? scannedEnemyTileMat : foggedEnemyTileMat;
    }

    public void RemoveDeployedUnit(Unit_Enemy unit)
    {
        switch (unit.unitScriptableObject.unitType)
        {
            case GameManagement.UnitType.soldier:
                deployedFootSoldier.Remove(unit);
                break;
            case GameManagement.UnitType.mechanize:
                deployedMechanizeForce.Remove(unit);
                break;
            case GameManagement.UnitType.air:
                deployedAirForce.Remove(unit);
                break;
        }
    }
    public void IncreaseFundPerTurn()
    {
        totalFund += fundPerTurn;
    }
}
