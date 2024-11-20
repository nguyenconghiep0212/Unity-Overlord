using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagement : MonoBehaviour
{
    public static GameManagement Instance { get; set; }

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
    public Material cloudMat;

    public List<MapTile> totalTile;
    public GameObject victoryMenu;
    public GameObject defeatedMenu;

    public GameObject scanUI;
    public bool isStart;
    public GameObject upgradeTreeUI;
    public GameObject regionInfoUI;

    public LineRenderer lineRenderer;
    public List<GameObject> supportLineList;

    public GameObject garrisonConstructing;
    public GameObject scannerConstructing;

    [Header("Map")]
    public Material foggedTileMat;
    public Material highlightedFoggedTileMat;
    public Material scannedTileMat;
    public Material highlightedScannedTileMat;

    public MapTile highlightedTile;

    [Header("Unit parent")]
    public GameObject allyUnitParent;
    public GameObject allyInfrastructureParent;
    public GameObject enemyUnitParent;
    public GameObject enemyInfrastructureParent;

    public enum UnitType
    {
        soldier,
        mechanize,
        air
    }
    public enum InfrastructureType
    {
        HQ,
        Scanner,
        Garrison
    }

    public enum TileType
    {
        Urban,
        Farmland,
        Forest,
        Mountain
    }

    // Start is called before the first frame update
    void Start()
    {
        victoryMenu.SetActive(false);
        defeatedMenu.SetActive(false);

        regionInfoUI.SetActive(false);
        upgradeTreeUI.SetActive(false);
        TimeManagement.Instance.ResumeGame();
        CloudClearing();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BackToMainMenu()
    {
        Setting.Instance.LoadingScreen("MAIN MENU");
        //SceneManager.LoadScene("MAIN MENU");
    }

    public void StartOperation()
    {
        print("OPERATION START !!!");
        TimeManagement.Instance.StartFunding();
    }

    public void OpenOperationUpgradeTree()
    {
        upgradeTreeUI.SetActive(true);
        TimeManagement.Instance.PauseGame();
    }

    public void CloseOperationUpgradeTree()
    {
        upgradeTreeUI.SetActive(false);
        TimeManagement.Instance.ResumeGame();
        TechTreeManager.Instance.techInformationUI.SetActive(false);
    }
    public void OpenRegionInformationTab()
    {
        regionInfoUI.SetActive(true);
        RegionManagement.Instance.InitializeVendorList();
        TimeManagement.Instance.PauseGame();
    }

    public void CloseRegionInformationTab()
    {
        regionInfoUI.SetActive(false);
        TimeManagement.Instance.ResumeGame();
    }

    public void ResetSupportLine(GameObject unit)
    {
        try
        {
            if (unit.GetComponent<Unit>())
            {
                foreach (Unit supportSentBySelf in unit.GetComponent<Unit>().supportToUnit)
                {
                    supportSentBySelf.supportedByUnit.Remove(unit.GetComponent<Unit>());
                }
                foreach (Unit supportReceiveBySelf in unit.GetComponent<Unit>().supportedByUnit)
                {
                    supportReceiveBySelf.supportedByUnit.Remove(unit.GetComponent<Unit>());
                }
            }
            if (unit.GetComponent<Unit_Enemy>())
            {
                foreach (Unit_Enemy supportSentBySelf in unit.GetComponent<Unit_Enemy>().supportToUnit)
                {
                    supportSentBySelf.supportedByUnit.Remove(unit.GetComponent<Unit_Enemy>());
                }
                foreach (Unit_Enemy supportReceiveBySelf in unit.GetComponent<Unit_Enemy>().supportedByUnit)
                {
                    supportReceiveBySelf.supportedByUnit.Remove(unit.GetComponent<Unit_Enemy>());
                }
            }


            foreach (GameObject supportline in GameManagement.Instance.supportLineList)
            {
                string id = "";
                if (unit.GetComponent<Unit>()) id = unit.GetComponent<Unit>().id.ToString();
                if (unit.GetComponent<Unit_Enemy>()) id = unit.GetComponent<Unit_Enemy>().id.ToString();

                if (supportline.GetComponent<LineControl>().id.Contains(id))
                {
                    int index = supportline.GetComponent<LineControl>().points.IndexOf(unit.transform);
                    if (index < 0) continue;
                    supportline.GetComponent<LineControl>().points[index] = null;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("ResetSupportLine: " + ex);
        }
    }

    public void SetTransparency(Transform parent, bool toggle)
    {
        // Change the transparency of the parent
        Renderer parentRenderer = parent.GetComponent<Renderer>();
        if (parentRenderer != null)
        {
            parentRenderer.enabled = toggle;
        }

        // Recursively change transparency for each child
        foreach (Transform child in parent)
        {
            SetTransparency(child, toggle);
        }
    }

    internal void TriggerDefeat()
    {
        //TimeManagement.Instance.PauseGame();
        OperationManager.Instance.isDefeated = true;
        defeatedMenu.SetActive(true);
        SoundManager.Instance.PlayDefeatedMusic();
    }
    internal void TriggerVictory()
    {
        //TimeManagement.Instance.PauseGame();
        victoryMenu.SetActive(true);
        SoundManager.Instance.PlayVictoryMusic();
    }

    private async void CloudClearing()
    {
        cloudMat.SetFloat("_Cloud_Alpha", 15);
        float cloudAlpha = cloudMat.GetFloat("_Cloud_Alpha");
        float newCloudAlpha = cloudAlpha; 
        do
        {
            await Task.Delay(20);
            newCloudAlpha -= 0.2f;
            cloudMat.SetFloat("_Cloud_Alpha", newCloudAlpha);
        } while (newCloudAlpha > 0.6f);
    }
}
