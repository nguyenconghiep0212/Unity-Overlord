using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField]
    private List<MapTile> _neighborTiles;
    public List<MapTile> neighborTiles
    {
        get { return _neighborTiles; }
    }

    public bool isHighlighted;
    public GameManagement.TileType tileType;
    public string callsign;
    public int population;
    public float supportPercentage = 0;
    public float opposePercentage = 0; 
    public int tax;
    public int supply;

    public bool isEnemyOwn;


    [Header("Enemy Operation")]
    public Infrastruture_Enemy enemyHQ;
    public Infrastruture_Enemy enemyGarrison;


    [Header("Operation")]
    public Unit occupiedAllyUnit;
    public Unit_Enemy occupiedEnemyUnit;

    public Infrastruture HQ;
    public Infrastruture scanner;
    public Infrastruture garrison;

    public bool isConstructingGarrison = false;
    public bool isConstructingScanner = false;

    [Range(1f, 100f)]
    private float _scanProgression = 0;
    public float scanProgression
    {
        get => _scanProgression;
        set
        {
            _scanProgression = Mathf.Clamp(value, 1f, 100f); // Clamp the value

            // Check if the value reached 100
            if (_scanProgression >= 100f)
            {
                ScannedTile();
            }
        }
    }
    public bool isScanned = false;
    public bool isScanning = false;

    public float footSoldierModifier;
    public float mechanizeForceModifier;
    public float airForceModifier;


    public float enemyHQSpawnPercentage;
    public int distanceFromPlayerHQ;
    public int distanceFromEnemyHQ;
    internal Transform center;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private float highlightDuration = 0.1f;
    private GameObject scanUI;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        center = transform.GetChild(0);

        if (!isScanned)
        {
            GetComponent<Renderer>().material = GameManagement.Instance.foggedTileMat;
        }

        InitTileModifier();
        InitTilePopulation();
    }

    // Update is called once per frame
    void Update()
    {
        if (!TimeManagement.Instance.isPause)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Check if the ray hits the current object
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        isHighlighted = !isHighlighted;
                        if (isHighlighted)
                        {
                            GameManagement.Instance.highlightedTile = this;
                        }
                        else
                        {
                            GameManagement.Instance.highlightedTile = null;
                        }
                    }
                    else
                    {
                        isHighlighted = false;
                    }
                    if (isHighlighted)
                    {
                        StartCoroutine(HighlightTile());
                    }
                    else
                    {
                        StartCoroutine(UnhighlightTile());
                    }
                }
            }


        }
    }

    #region ---- || TILE INTERACTION || ----
    public IEnumerator HighlightTile()
    {
        if (isScanned)
        {
            if (isEnemyOwn)
            {
                GetComponent<Renderer>().material = EnemyOperationManager.Instance.highlightedScannedEnemyTileMat;
            }
            else
            {
                GetComponent<Renderer>().material = GameManagement.Instance.highlightedScannedTileMat;
            }
        }
        else
        {
            if (isEnemyOwn)
            {
                GetComponent<Renderer>().material = EnemyOperationManager.Instance.highlightedFoggedEnemyTileMat;
            }
            else
            {
                GetComponent<Renderer>().material = GameManagement.Instance.highlightedFoggedTileMat;
            }
        }

        float elapsedTime = 0f;
        Vector3 targetPosition = originalPosition + new Vector3(0, 0, 0.05f);
        Vector3 targetScale = originalScale * 1.01f;

        while (elapsedTime < highlightDuration)
        {
            transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / highlightDuration);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / highlightDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure we set to the final target position and scale
        transform.localPosition = targetPosition;
        transform.localScale = targetScale;
    }

    public IEnumerator UnhighlightTile()
    {

        //OperationManager.Instance.selectingTile = null;
        //foreach (GameObject unwantedUI in GameObject.FindGameObjectsWithTag("PlanInfrastructureUI"))
        //{
        //    Destroy(unwantedUI);
        //}

        float elapsedTime = 0f;

        while (elapsedTime < highlightDuration)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, elapsedTime / highlightDuration);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, elapsedTime / highlightDuration);
            elapsedTime += Time.deltaTime;

            yield return null; // Wait for the next frame
        }
        if (transform.localScale == originalScale)
        {
            if (isScanned)
            {
                if (isEnemyOwn)
                {
                    GetComponent<Renderer>().material = EnemyOperationManager.Instance.scannedEnemyTileMat;
                }
                else
                {
                    GetComponent<Renderer>().material = GameManagement.Instance.scannedTileMat;
                }
            }
            else
            {
                if (isEnemyOwn)
                {
                    GetComponent<Renderer>().material = EnemyOperationManager.Instance.foggedEnemyTileMat;
                }
                else
                {
                    GetComponent<Renderer>().material = GameManagement.Instance.foggedTileMat;
                }
            }

        }
        // Ensure we set to the original position and scale
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;

    }
    #endregion

    private void InitTileModifier()
    {
        switch (tileType)
        {
            case GameManagement.TileType.Urban:
                footSoldierModifier = 0.5f;
                mechanizeForceModifier = 0f;
                airForceModifier = 0f;
                break;
            case GameManagement.TileType.Farmland:
                footSoldierModifier = 0;
                mechanizeForceModifier = 0.25f;
                airForceModifier = 0.25f;
                break;
            case GameManagement.TileType.Forest:
                footSoldierModifier = 0.25f;
                mechanizeForceModifier = -0.25f;
                airForceModifier = 0f;
                break;
            case GameManagement.TileType.Mountain:
                footSoldierModifier = -0.5f;
                mechanizeForceModifier = -0.5f;
                airForceModifier = 0.25f;
                break;
        }
    }
    private void InitTilePopulation()
    {
        switch (tileType)
        {
            case GameManagement.TileType.Urban:
                population = UnityEngine.Random.Range(120, 200) * 1_000;
                tax = UnityEngine.Random.Range(8, 10) * 100;
                supply = 4;
                break;
            case GameManagement.TileType.Farmland:
                population = UnityEngine.Random.Range(20, 60) * 1_000;
                tax = UnityEngine.Random.Range(5, 6) * 100;
                supply = 2;
                break;
            case GameManagement.TileType.Forest:
                population = UnityEngine.Random.Range(5, 15) * 1_000;
                tax = (int)(UnityEngine.Random.Range(2, 3.5f) * 100);
                supply = 1;
                break;
            case GameManagement.TileType.Mountain:
                population = UnityEngine.Random.Range(1, 2) * 1_000;
                tax = (int)(UnityEngine.Random.Range(1, 2) * 100);
                supply = 0;
                break;
        }
    }

    public void Scanning()
    {
        isScanning = true;
        scanProgression += OperationManager.Instance.scanSpeed;

        if (!scanUI)
            scanUI = Instantiate(GameManagement.Instance.scanUI, transform);
        scanUI.transform.localPosition = new Vector3(0, 0, 0.3f);
    }

    public void ScannedTile()
    {
        isScanning = false;
        isScanned = true;
        if (isHighlighted)
        {
            if (isEnemyOwn)
            {
                GetComponent<Renderer>().material = EnemyOperationManager.Instance.highlightedScannedEnemyTileMat;
            }
            else
            {
                GetComponent<Renderer>().material = GameManagement.Instance.highlightedScannedTileMat;
            }
        }
        else
        {
            if (isEnemyOwn)
            {
                GetComponent<Renderer>().material = EnemyOperationManager.Instance.scannedEnemyTileMat;
            }
            else
            {
                GetComponent<Renderer>().material = GameManagement.Instance.scannedTileMat;
            }
        }
        Destroy(scanUI);
    }

    public void RandomizeUprising()
    {
        if (opposePercentage >= 10)
        {
            int randomIndex = UnityEngine.Random.Range(1, 10);
            if (randomIndex == 1)
            {
                GameObject newUnit = Instantiate(EnemyOperationManager.Instance.EnemyFootSoldierPrefab, GameObject.FindWithTag("Enemy").transform.GetChild(0));
                newUnit.transform.position = transform.GetChild(0).position;
                newUnit.GetComponent<Unit_Enemy>().deployedTile = this;
                occupiedEnemyUnit = newUnit.GetComponent<Unit_Enemy>();
                opposePercentage = 0.3f;
            }
        }

    } 
    public void RemoveOccupiedAllyUnit()
    {
        occupiedAllyUnit = null;
    }
    public void RemoveOccupiedEnemyUnit()
    {
        occupiedEnemyUnit = null;
    }
}
