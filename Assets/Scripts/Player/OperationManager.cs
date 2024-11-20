using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static GameManagement;

public class OperationManager : MonoBehaviour
{
    public static OperationManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null & Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public UnitScriptableObject footSoldierScriptable;
    public UnitScriptableObject mechanizeForceScriptable;
    public UnitScriptableObject airForceScriptable;


    [Header("Operation")]
    public Infrastruture headQuarter;
    public List<Infrastruture> Garrisions;
    public List<MapTile> ownTiles = new List<MapTile>();
    public float scanSpeed = 10;
    public int totalFund;
    public int fundPerTurn = 5000;
    public int maxSupply = 0;
    public int currentSupply;
    public bool isDefeated = false;


    [Header("Prefabs")]
    public GameObject footSoldierPrefab;
    public GameObject mechanizeForcePrefab;
    public GameObject airForcePrefab;
    public GameObject headQuarterPrefab;
    public GameObject garrisionPrefab;
    public GameObject scannerPrefab;

    public GameObject hqSelectorUIPrefabs;
    public GameObject scannerSelectorUIPrefabs;
    public GameObject garisonSelectorUIPrefabs;
    public GameObject scannerGarisonSelectorUIPrefabs;

    [Header("Unit")]
    public int trainedFootSoldier;
    public int trainedMechanizeForce;
    public int trainedAirForce;
    public List<Unit> deployedFootSoldier = new List<Unit>();
    public List<Unit> deployedMechanizeForce = new List<Unit>();
    public List<Unit> deployedAirForce = new List<Unit>();

    public List<Unit> totalDeployUnit
    {
        get
        {
            return deployedFootSoldier.Concat(deployedMechanizeForce).Concat(deployedAirForce).ToList();
        }
    }

    public bool isFootSoldierUnlocked;
    public bool isMechanizeForceUnlocked;
    public bool isAirForceUnlocked;

    public bool isTrainingFootSoldier;
    public bool isTrainingMechanizeForce;
    public bool isTrainingAirForce;

    public int supplyFootSoldier = 1;
    public int supplyMechanizeForce = 2;
    public int supplyAirForce = 2;

    [Header("UI")]
    public TextMeshProUGUI fundUI;
    public TextMeshProUGUI currentSupplyUI;
    public TextMeshProUGUI maxSupplyUI;
    public TextMeshProUGUI trainedFootSoldierUI;
    public TextMeshProUGUI trainedMechanizeForceUI;
    public TextMeshProUGUI trainedAirForceUI;

    public GameObject trainingTabBackground;
    public GameObject footSoldierButton;
    public GameObject mechanizeForceButton;
    public GameObject airForceButton;


    internal GameObject hqSelectorUI;
    internal GameObject scannerSelectorUI;
    internal GameObject garisonSelectorUI;
    internal GameObject scannerGarisonSelectorUI;

    public UnitTrainingProgressBar soldierProgressBar;
    private int soldierCurrentProgress;
    private Coroutine soldierProgressCoroutine;
    public UnitTrainingProgressBar mechanizeProgressBar;
    private int mechanizeCurrentProgress;
    private Coroutine mechanizeProgressCoroutine;
    public UnitTrainingProgressBar airProgressBar;
    private int airCurrentProgress;
    private Coroutine airProgressCoroutine;

    internal MapTile selectingTile;
    // Start is called before the first frame update
    void Start()
    {
        totalFund = 0;
        fundUI.text = totalFund.ToString("N0");
        TechTreeManager.Instance.fundUI.text = totalFund.ToString("N0");

        maxSupplyUI.text = maxSupply.ToString();
        currentSupplyUI.text = currentSupply.ToString();

        trainingTabBackground.SetActive(false);
        footSoldierButton.SetActive(false);
        mechanizeForceButton.SetActive(false);
        airForceButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!TimeManagement.Instance.isPause)
        {
            if (!headQuarter)
            {
                ChooseHQ();
            }
            else
            {
                ChooseScannerAndGarrisonLocation();
            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits the current object
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Barrier"))
                {
                    if (selectingTile)
                    {
                        selectingTile = null;
                        RegionManagement.Instance.UpdateSelectedRegionUI(selectingTile);
                        foreach (GameObject unwantedUI in GameObject.FindGameObjectsWithTag("PlanInfrastructureUI"))
                        {
                            Destroy(unwantedUI);
                        }
                    }
                }
            } 
        }
    }


    #region ---- || Pre Operation Phase || ----
    public void ChooseHQ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits the current object
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.GetComponent<MapTile>())
                {
                    if (hit.collider.gameObject.GetComponent<MapTile>() != selectingTile)
                    {
                        foreach (GameObject unwantedUI in GameObject.FindGameObjectsWithTag("PlanInfrastructureUI"))
                        {
                            Destroy(unwantedUI);
                        }

                        selectingTile = hit.collider.gameObject.GetComponent<MapTile>();
                        RegionManagement.Instance.UpdateSelectedRegionUI(selectingTile);
                        hqSelectorUI = Instantiate(hqSelectorUIPrefabs, hit.collider.transform);
                        hqSelectorUI.GetComponent<ChooseHQUI>().target = hit.collider.gameObject.GetComponent<MapTile>().center;
                    }
                }
            }
        }
    }

    public void ConfirmHQ()
    {
        Destroy(hqSelectorUI);

        GameObject newHQ = Instantiate(headQuarterPrefab, GameObject.FindWithTag("Player").transform.GetChild(1));
        newHQ.transform.position = new Vector3(selectingTile.transform.GetChild(0).position.x, 0.03f, selectingTile.transform.GetChild(0).position.z);

        headQuarter = newHQ.GetComponent<Infrastruture>();
        headQuarter.GetComponent<Infrastruture>().deployedTile = selectingTile;
        selectingTile.scanProgression = 100;
        selectingTile.HQ = headQuarter;

        AddTileToOwn(selectingTile);
        foreach (MapTile tile in selectingTile.neighborTiles)
        {
            tile.scanProgression = 100;
            AddTileToOwn(tile);
        }
        selectingTile = null;

        maxSupply = headQuarter.deployedTile.GetComponent<MapTile>().supply + 1;
        maxSupplyUI.text = maxSupply.ToString();

        GameManagement.Instance.StartOperation();
    }
    #endregion

    #region ---- || Infrastructure Placement || ---- 
    public void ChooseScannerAndGarrisonLocation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits the current object
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.GetComponent<MapTile>())
                {
                    if (hit.collider.gameObject.GetComponent<MapTile>() != selectingTile)
                    {
                        foreach (GameObject unwantedUI in GameObject.FindGameObjectsWithTag("PlanInfrastructureUI"))
                        {
                            Destroy(unwantedUI);
                        }
                        selectingTile = hit.collider.gameObject.GetComponent<MapTile>();
                        RegionManagement.Instance.UpdateSelectedRegionUI(selectingTile);
                        if (selectingTile.isScanned && !selectingTile.HQ && !selectingTile.enemyGarrison && !selectingTile.enemyHQ && !selectingTile.occupiedEnemyUnit)
                        {
                            if ((!selectingTile.garrison && !selectingTile.isConstructingGarrison) && (!selectingTile.scanner && !selectingTile.isConstructingScanner))
                            {
                                if (TechTreeManager.Instance.purchasedUpgrades.Contains("GARS_UNLK") && TechTreeManager.Instance.purchasedUpgrades.Contains("SCAN_TOWR"))
                                {
                                    scannerGarisonSelectorUI = Instantiate(scannerGarisonSelectorUIPrefabs, hit.collider.transform);
                                    scannerGarisonSelectorUI.GetComponent<ChooseHQUI>().target = hit.collider.gameObject.GetComponent<MapTile>().center;
                                }
                                else if (TechTreeManager.Instance.purchasedUpgrades.Contains("SCAN_TOWR"))
                                {
                                    scannerSelectorUI = Instantiate(scannerSelectorUIPrefabs, hit.collider.transform);
                                    scannerSelectorUI.GetComponent<ChooseHQUI>().target = hit.collider.gameObject.GetComponent<MapTile>().center;
                                }
                                else if (TechTreeManager.Instance.purchasedUpgrades.Contains("GARS_UNLK"))
                                {
                                    garisonSelectorUI = Instantiate(garisonSelectorUIPrefabs, hit.collider.transform);
                                    garisonSelectorUI.GetComponent<ChooseHQUI>().target = hit.collider.gameObject.GetComponent<MapTile>().center;
                                }
                            }
                            else if ((!selectingTile.garrison || !selectingTile.isConstructingGarrison) && (selectingTile.scanner || selectingTile.isConstructingScanner))
                            {
                                if (TechTreeManager.Instance.purchasedUpgrades.Contains("GARS_UNLK"))
                                {
                                    garisonSelectorUI = Instantiate(garisonSelectorUIPrefabs, hit.collider.transform);
                                    garisonSelectorUI.GetComponent<ChooseHQUI>().target = hit.collider.gameObject.GetComponent<MapTile>().center;
                                }
                            }
                            else if ((!selectingTile.scanner || !selectingTile.isConstructingScanner) && (selectingTile.garrison || selectingTile.isConstructingGarrison))
                            {
                                if (TechTreeManager.Instance.purchasedUpgrades.Contains("SCAN_TOWR"))
                                {
                                    scannerSelectorUI = Instantiate(scannerSelectorUIPrefabs, hit.collider.transform);
                                    scannerSelectorUI.GetComponent<ChooseHQUI>().target = hit.collider.gameObject.GetComponent<MapTile>().center;
                                }
                            }
                        }

                    }

                }
            }
        }
    }
    public void ConfirmGarrison()
    {
        if (garrisionPrefab.GetComponent<Infrastruture>().cost <= totalFund)
        {
            totalFund -= garrisionPrefab.GetComponent<Infrastruture>().cost;
            fundUI.text = totalFund.ToString("N0");
            TechTreeManager.Instance.fundUI.text = totalFund.ToString("N0");

            Destroy(garisonSelectorUI);
            Destroy(scannerGarisonSelectorUI);
            if (selectingTile)
            {
                selectingTile.isConstructingGarrison = true;

                GameObject garrisonConstructUI = Instantiate(GameManagement.Instance.garrisonConstructing, GameObject.FindWithTag("Player").transform.GetChild(1));
                garrisonConstructUI.transform.position = new Vector3(selectingTile.transform.GetChild(0).position.x - 0.4f, 0.03f, selectingTile.transform.GetChild(0).position.z);

                StartCoroutine(BuildingInfrastructure(selectingTile, garrisionPrefab, true, garrisonConstructUI));
            }

            SoundManager.Instance.mainGameSoundChannel.PlayOneShot(SoundManager.Instance.hammerSound);
        }
    }
    public void ConfirmScanner()
    {
        if (scannerPrefab.GetComponent<Infrastruture>().cost <= totalFund)
        {
            totalFund -= scannerPrefab.GetComponent<Infrastruture>().cost;
            fundUI.text = totalFund.ToString("N0");
            TechTreeManager.Instance.fundUI.text = totalFund.ToString("N0");

            Destroy(scannerSelectorUI);
            Destroy(scannerGarisonSelectorUI);
            if (selectingTile)
            {
                selectingTile.isConstructingScanner = true;

                GameObject scannerConstructUI = Instantiate(GameManagement.Instance.scannerConstructing, GameObject.FindWithTag("Player").transform.GetChild(1));
                scannerConstructUI.transform.position = new Vector3(selectingTile.transform.GetChild(0).position.x + 0.4f, 0.03f, selectingTile.transform.GetChild(0).position.z);

                StartCoroutine(BuildingInfrastructure(selectingTile, scannerPrefab, false, scannerConstructUI));
            }

            SoundManager.Instance.mainGameSoundChannel.PlayOneShot(SoundManager.Instance.hammerSound);
        }
    }
    private IEnumerator BuildingInfrastructure(MapTile newTile, GameObject prefab, bool isGarrison, GameObject removeObject)
    {
        yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval * prefab.GetComponent<Infrastruture>().infrastrutureScriptable.turnToBuilt);
        GameObject newInfrastructure = Instantiate(prefab, GameObject.FindWithTag("Player").transform.GetChild(1));
        newInfrastructure.transform.position = new Vector3(isGarrison ? newTile.transform.GetChild(0).position.x - 0.4f : newTile.transform.GetChild(0).position.x + 0.4f, 0.03f, newTile.transform.GetChild(0).position.z);
        newInfrastructure.GetComponent<Infrastruture>().deployedTile = newTile;

        AddTileToOwn(newTile);
        selectingTile = null;

        if (isGarrison)
        {
            newTile.isConstructingGarrison = false;
            newTile.garrison = newInfrastructure.GetComponent<Infrastruture>();
            maxSupply += newInfrastructure.GetComponent<Infrastruture>().deployedTile.GetComponent<MapTile>().supply;
            maxSupplyUI.text = maxSupply.ToString();
        }
        else
        {
            newTile.isConstructingScanner = false;
            newTile.scanner = newInfrastructure.GetComponent<Infrastruture>();
        }

        Destroy(removeObject);

    }
    #endregion


    #region ---- || UNIT || ----
    public void TrainFootSoldier()
    {
        if (!isTrainingFootSoldier)
        {
            if (currentSupply + supplyFootSoldier <= maxSupply && totalFund >= footSoldierScriptable.fundCost)
            {
                totalFund -= footSoldierScriptable.fundCost;
                fundUI.text = totalFund.ToString("N0");
                TechTreeManager.Instance.fundUI.text = totalFund.ToString("N0");

                currentSupply += supplyFootSoldier;
                currentSupplyUI.text = currentSupply.ToString();
                isTrainingFootSoldier = true;

                StartCoroutine(TrainedFootSoldier());
                soldierProgressCoroutine = StartCoroutine(ProgressBarIncrement(isTrainingFootSoldier, soldierCurrentProgress, UnitType.soldier));
            }
        }
    }
    public void TrainMechanizeForce()
    {
        if (!isTrainingMechanizeForce)
        {
            if (currentSupply + supplyMechanizeForce <= maxSupply && totalFund >= mechanizeForceScriptable.fundCost)
            {
                totalFund -= mechanizeForceScriptable.fundCost;
                fundUI.text = totalFund.ToString("N0");
                TechTreeManager.Instance.fundUI.text = totalFund.ToString("N0");

                currentSupply += supplyMechanizeForce;
                currentSupplyUI.text = currentSupply.ToString();
                isTrainingMechanizeForce = true;

                mechanizeProgressCoroutine = StartCoroutine(ProgressBarIncrement(isTrainingMechanizeForce, mechanizeCurrentProgress, UnitType.mechanize));
                StartCoroutine(TrainedMechanizeForce());
            }
        }
    }
    public void TrainAirForce()
    {
        if (!isTrainingAirForce)
        {
            if (currentSupply + supplyAirForce <= maxSupply && totalFund >= airForceScriptable.fundCost)
            {
                totalFund -= airForceScriptable.fundCost;
                fundUI.text = totalFund.ToString("N0");
                TechTreeManager.Instance.fundUI.text = totalFund.ToString("N0");

                currentSupply += supplyAirForce;
                currentSupplyUI.text = currentSupply.ToString();
                isTrainingAirForce = true;

                airProgressCoroutine = StartCoroutine(ProgressBarIncrement(isTrainingAirForce, airCurrentProgress, UnitType.air));
                StartCoroutine(TrainedAirForce());
            }
        }
    }
    private IEnumerator TrainedFootSoldier()
    {
        yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval * footSoldierScriptable.trainTime);
        trainedFootSoldier++;
        trainedFootSoldierUI.text = trainedFootSoldier.ToString();
        isTrainingFootSoldier = false;
        StopProgressBar(soldierCurrentProgress, soldierProgressCoroutine, UnitType.soldier);

        SoundManager.Instance.mainGameSoundChannel.PlayOneShot(SoundManager.Instance.infantryReadySound);
    }
    private IEnumerator TrainedMechanizeForce()
    {
        yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval * mechanizeForceScriptable.trainTime);
        trainedMechanizeForce++;
        trainedMechanizeForceUI.text = trainedMechanizeForce.ToString();
        isTrainingMechanizeForce = false;
        StopProgressBar(mechanizeCurrentProgress, mechanizeProgressCoroutine, UnitType.mechanize);

        SoundManager.Instance.mainGameSoundChannel.PlayOneShot(SoundManager.Instance.mechanizeReadySound);
    }
    private IEnumerator TrainedAirForce()
    {
        yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval * airForceScriptable.trainTime);
        trainedAirForce++;
        trainedAirForceUI.text = trainedAirForce.ToString();
        isTrainingAirForce = false;
        StopProgressBar(airCurrentProgress, airProgressCoroutine, UnitType.air);

        SoundManager.Instance.mainGameSoundChannel.PlayOneShot(SoundManager.Instance.airReadySound);
    }

    public void AddDeployedUnit(Unit unit)
    {
        switch (unit.unitScriptableObject.unitType)
        {
            case UnitType.soldier:
                if (!deployedFootSoldier.Contains(unit))
                {
                    deployedFootSoldier.Add(unit);
                }
                break;
            case UnitType.mechanize:
                if (!deployedMechanizeForce.Contains(unit))
                {
                    deployedMechanizeForce.Add(unit);
                }
                break;
            case UnitType.air:
                if (!deployedAirForce.Contains(unit))
                {
                    deployedAirForce.Add(unit);
                }
                break;
        }
    }

    public void RemoveDeployedUnit(Unit unit)
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


    private IEnumerator ProgressBarIncrement(bool isUnitTraining, int unitProgress, UnitType unitBeingTrain)
    {
        while (isUnitTraining)
        {
            yield return new WaitForSeconds(TimeManagement.Instance.incrementInterval);
            unitProgress++;

            switch (unitBeingTrain)
            {
                case GameManagement.UnitType.soldier:
                    soldierProgressBar.SetTurnProgresssBarPercentage((float)unitProgress / footSoldierScriptable.trainTime);
                    if (unitProgress == footSoldierScriptable.trainTime)
                    {
                        unitProgress = 0;
                    }
                    break;
                case GameManagement.UnitType.mechanize:
                    mechanizeProgressBar.SetTurnProgresssBarPercentage((float)unitProgress / mechanizeForceScriptable.trainTime);
                    if (unitProgress == mechanizeForceScriptable.trainTime)
                    {
                        unitProgress = 0;
                    }
                    break;
                case GameManagement.UnitType.air:
                    airProgressBar.SetTurnProgresssBarPercentage((float)unitProgress / airForceScriptable.trainTime);
                    if (unitProgress == airForceScriptable.trainTime)
                    {
                        unitProgress = 0;
                    }
                    break;
            }
        }
    }
    private void StopProgressBar(int unitProgress, Coroutine unitCoroutine, UnitType unitBeingTrain)
    {
        StopCoroutine(unitCoroutine);
        unitCoroutine = null;
        unitProgress = 0;
        switch (unitBeingTrain)
        {
            case GameManagement.UnitType.soldier:
                soldierProgressBar.SetTurnProgresssBarPercentage((float)unitProgress / footSoldierScriptable.trainTime);

                break;
            case GameManagement.UnitType.mechanize:
                mechanizeProgressBar.SetTurnProgresssBarPercentage((float)unitProgress / mechanizeForceScriptable.trainTime);

                break;
            case GameManagement.UnitType.air:
                airProgressBar.SetTurnProgresssBarPercentage((float)unitProgress / airForceScriptable.trainTime);
                break;
        }
    }
    #endregion

    public void IncreaseFundByTurn()
    {
        totalFund += fundPerTurn + (TechTreeManager.Instance.purchasedUpgrades.Contains("INIT_TAX") ? ownTiles.Sum(t => t.tax) : 0);
        if (TechTreeManager.Instance.purchasedUpgrades.Contains("INIT_EXPT")) totalFund += (int)(totalFund * 0.15f);
        fundUI.text = totalFund.ToString("N0");
        TechTreeManager.Instance.fundUI.text = totalFund.ToString("N0");
    }

    private void AddTileToOwn(MapTile newTile)
    {
        if (!ownTiles.Contains(newTile))
        {
            ownTiles.Add(newTile);
        }
    }

    internal void RemoveTileFromOwn(MapTile lostTile)
    {
        if (ownTiles.Contains(lostTile))
        {
            ownTiles.Remove(lostTile);
        }
    }
}
