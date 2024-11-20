using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegionManagement : MonoBehaviour
{
    public GameObject RegionTabUI;
    public GameObject RegionUIPrefab;

    public string[] regionNamePool = new string[30] { "Sebluff Bay", "Oubeach Du Nkportlat", "Fort Berkiah", "Annprai", "Southfelstown", "Mia", "Spri Springs", "Ceanphur", "Red Ntahydepom", "Las Ston",
                                                    "Worthsportauck", "Wardsfield", "St Hoeghworth", "Bridthemarsh-In-Northlu", "Ilhymskirk-On-River", "Hotgrave", "North Wryce", "Bilthbu", "New Bridgbigver", "Tawhampton",
                                                    "Ktukloeil", "Fort Socum Aux Smithska", "Vergrace", "Fort Coon", "North Stamins", "Rathtown", "Moose Rurnia", "Moose Be", "Saint Northsaintei", "Meei"};

    private string enemyHex = "#7E0000";
    private string allyHex = "#0F5A21";
    private string neutralHex = "#444444";

    public GameObject selectedRegionUI;
    public Image selectedRegionScanProgressUI;

    [Header("Selected Region")]
    public Image selectedTileBackground;
    public TextMeshProUGUI selectedTileName;
    public TextMeshProUGUI selectedTilePopulation;
    public GameObject selectedTileSupport;
    public GameObject selectedTileOwner;
    public GameObject selectedTileNeutral;

    public Sprite urbanSprite;
    public Sprite farmSprite;
    public Sprite forestSprite;
    public Sprite mountainSprite;
    // Start is called before the first frame update
    public static RegionManagement Instance { get; set; }

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

    private void Start()
    {
        AssertNameToRegion();
    }

    private void Update()
    {
        if (!OperationManager.Instance.selectingTile)
        {
            selectedRegionUI.SetActive(false);
        }
    }

    public async void UpdateSelectedRegionUI(MapTile tile)
    {
        selectedRegionUI.SetActive(false);

        if (!tile) return;

        string useColor = neutralHex;
        if (OperationManager.Instance.ownTiles.Contains(tile)) useColor = allyHex;
        if (EnemyOperationManager.Instance.ownTiles.Contains(tile)) useColor = enemyHex;

         Color color;
        if (ColorUtility.TryParseHtmlString(useColor, out color))
        {
            selectedRegionUI.GetComponent<UIGradient>().m_color1 = color;
        }

        selectedRegionUI.GetComponentInChildren<TextMeshProUGUI>().text = tile.callsign;
        selectedRegionUI.SetActive(true);

        selectedRegionScanProgressUI.fillAmount = 0;
        if (tile.isScanning)
        {
            selectedRegionScanProgressUI.fillAmount = 0;
            while (tile.scanProgression < 100 && OperationManager.Instance.selectingTile == tile)
            {
                await Task.Delay(20);
                selectedRegionScanProgressUI.fillAmount = tile.scanProgression / 100;
            }
        }

        if (tile.isScanned)
        {
            selectedRegionScanProgressUI.fillAmount = tile.scanProgression;
        }

    }

    private void AssertNameToRegion()
    {
        List<string> uniqueArray = GetRandomUniqueFruits(regionNamePool, GameManagement.Instance.totalTile.Count);
        for (int i = 0; i < uniqueArray.Count; i++)
        {
            GameManagement.Instance.totalTile[i].callsign = uniqueArray[i];
        }
        List<string> GetRandomUniqueFruits(string[] array, int numberOfElements)
        {
            // Create a list from the array to shuffle
            List<string> shuffledList = new List<string>(array);

            // Shuffle the list
            for (int i = 0; i < shuffledList.Count; i++)
            {
                // Pick a random index
                int randomIndex = Random.Range(i, shuffledList.Count);
                // Swap current element with the random element
                string temp = shuffledList[i];
                shuffledList[i] = shuffledList[randomIndex];
                shuffledList[randomIndex] = temp;
            }
            // Take the first 'numberOfElements' from the shuffled list
            return shuffledList.GetRange(0, Mathf.Min(numberOfElements, shuffledList.Count));
        }
    }


    public void InitializeVendorList()
    {
        foreach (Transform child in RegionTabUI.transform)
        {
            Destroy(child.gameObject);
        }


        foreach (MapTile tile in GameManagement.Instance.totalTile)
        {
            GameObject RegionUI = Instantiate(RegionUIPrefab, RegionTabUI.transform);
            RegionUI.GetComponent<RegionUI>().tile = tile;
            RegionUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tile.callsign;
            RegionUI.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = tile.population.ToString("N0");

            string useColor = neutralHex;
            if (OperationManager.Instance.ownTiles.Contains(tile)) useColor = allyHex;
            if (EnemyOperationManager.Instance.ownTiles.Contains(tile)) useColor = enemyHex;

            Color color;
            if (ColorUtility.TryParseHtmlString(useColor, out color))
            {
                RegionUI.GetComponent<UIGradient>().m_color2 = color;
            }
        }
    }

    public void SelectRegionDetail(MapTile region)
    {
        selectedTileName.text = region.callsign;
        selectedTilePopulation.text = region.population.ToString("N0");

        if (EnemyOperationManager.Instance.ownTiles.Contains(region))
        {
            selectedTileNeutral.SetActive(false);
            selectedTileOwner.SetActive(true);
            selectedTileOwner.transform.GetChild(0).gameObject.SetActive(true);
            selectedTileOwner.transform.GetChild(1).gameObject.SetActive(false);
        }
        else if (OperationManager.Instance.ownTiles.Contains(region))
        {
            selectedTileNeutral.SetActive(false);
            selectedTileOwner.SetActive(true);
            selectedTileOwner.transform.GetChild(0).gameObject.SetActive(false);
            selectedTileOwner.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            selectedTileNeutral.SetActive(true);
            selectedTileOwner.SetActive(false);
        }

        if (region.supportPercentage > region.opposePercentage)
        {
            selectedTileSupport.transform.GetChild(0).gameObject.SetActive(true);
            selectedTileSupport.transform.GetChild(1).gameObject.SetActive(false);
            selectedTileSupport.transform.GetChild(2).gameObject.SetActive(false);
        }
        else if (region.supportPercentage < region.opposePercentage)
        {
            selectedTileSupport.transform.GetChild(0).gameObject.SetActive(false);
            selectedTileSupport.transform.GetChild(1).gameObject.SetActive(true);
            selectedTileSupport.transform.GetChild(2).gameObject.SetActive(false);
        }
        else if (region.supportPercentage == region.opposePercentage)
        {
            selectedTileSupport.transform.GetChild(0).gameObject.SetActive(false);
            selectedTileSupport.transform.GetChild(1).gameObject.SetActive(false);
            selectedTileSupport.transform.GetChild(2).gameObject.SetActive(true);
        }


        switch (region.tileType)
        {
            case GameManagement.TileType.Urban:
                selectedTileBackground.sprite = urbanSprite;
                break;
            case GameManagement.TileType.Farmland:
                selectedTileBackground.sprite = farmSprite;
                break;
            case GameManagement.TileType.Forest:
                selectedTileBackground.sprite = forestSprite;
                break;
            case GameManagement.TileType.Mountain:
                selectedTileBackground.sprite = mountainSprite;
                break;
        }
    }

}
