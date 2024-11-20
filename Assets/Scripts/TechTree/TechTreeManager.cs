using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeManager : MonoBehaviour
{
    public static TechTreeManager Instance { get; set; }

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

    [Header("Tech Tree")]
    public GameObject civilianTree;
    public GameObject governmentTree;
    public GameObject militaryTree;

    [Header("Modifier")]
    public float totalFootSoldierDamageBuff = 0;
    public float totalFootSoldierArmorBuff = 0;
    public float totalMechanizeForceDamageBuff = 0;
    public float totalMechanizeForceArmorBuff = 0;
    public float totalAirForceDamageBuff = 0;
    public float totalAirForceArmorBuff = 0;
    public float totalOperationSupport = 0;

    [Header("TechDescription")]
    public GameObject techInformationUI;
    public TextMeshProUGUI techName;
    public TextMeshProUGUI techDescription;
    public TextMeshProUGUI techCost;
    public Image techImage;
    public Image techSprite;
    public GameObject purchaseValidation;

    [Header("Misc")]
    public Tech selectedTech;
    public List<string> purchasedUpgrades;

    public TextMeshProUGUI fundUI;

    // Start is called before the first frame update
    void Start()
    {
        techInformationUI.SetActive(false);
        civilianTree.SetActive(false);
        governmentTree.SetActive(false);
        militaryTree.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SelectedTech(Tech tech)
    {
        techInformationUI.SetActive(true);
        selectedTech = tech;

        techName.text = tech.techName;
        techDescription.text = tech.techDescription;
        techCost.text = tech.cost.ToString("N0");
        techImage.color = tech.techImg.color;
        techSprite.color = tech.techSprite.color;
        techSprite.sprite = tech.techSprite.sprite;

        if (tech.isPurchased)
        {
            purchaseValidation.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            purchaseValidation.transform.parent.gameObject.SetActive(true);
            if (tech.isPurchasable)
            {
                purchaseValidation.SetActive(false);
            }
            else
            {
                purchaseValidation.SetActive(true);
            }

        }
    }

    public void PurchaseTech()
    {
        if (selectedTech.isPurchasable)
        {
            SoundManager.Instance.mainGameSoundChannel.PlayOneShot(SoundManager.Instance.researchSound);

            OperationManager.Instance.totalFund -= selectedTech.cost;
            OperationManager.Instance.fundUI.text = OperationManager.Instance.totalFund.ToString("N0");
            fundUI.text = OperationManager.Instance.totalFund.ToString("N0");

            selectedTech.isPurchased = true;
            purchaseValidation.transform.parent.gameObject.SetActive(false);
            purchasedUpgrades.Add(selectedTech.techId);

            UnityEngine.Color color;
            if (UnityEngine.ColorUtility.TryParseHtmlString(selectedTech.purchasedColor, out color))
            {
                selectedTech.techImg.color = color;
                techImage.color = selectedTech.techImg.color;
            }

            foreach (Tech tech in selectedTech.leadToTech)
            {
                tech.CheckUnlockTech();
            }

            switch (selectedTech.techId)
            {
                case "SLDR_UNLK":
                    OperationManager.Instance.trainingTabBackground.SetActive(true);
                    OperationManager.Instance.footSoldierButton.SetActive(true);
                    break;
                case "MCFR_UNLK":
                    OperationManager.Instance.mechanizeForceButton.SetActive(true);
                    break;
                case "AIFR_UNLK":
                    OperationManager.Instance.airForceButton.SetActive(true);
                    break; 
                // UNIT SOLDIER 
                case "SLDR_DMG_1":
                    totalFootSoldierDamageBuff += 0.1f;
                    break;
                case "SLDR_DMG_2":
                    totalFootSoldierDamageBuff += 0.1f; 
                    break;
                case "SLDR_DMG_3":
                    totalFootSoldierDamageBuff += 0.2f;
                    break;
                case "SLDR_DEF_1":
                    totalFootSoldierArmorBuff += 0.05f;
                    break;
                case "SLDR_DEF_2":
                    totalFootSoldierArmorBuff += 0.1f;
                    break;
                case "SLDR_DEF_3":
                    totalFootSoldierArmorBuff += 0.15f;
                    break;
                // UNIT MECHANIZE
                case "MCFR_DMG_1":
                    totalMechanizeForceDamageBuff += 0.1f;
                    break;
                case "MCFR_DMG_2":
                    totalMechanizeForceDamageBuff += 0.1f;
                    break;
                case "MCFR_DMG_3":
                    totalMechanizeForceDamageBuff += 0.2f;
                    break;
                case "MCFR_DEF_1":
                    totalMechanizeForceArmorBuff += 0.05f;
                    break;
                case "MCFR_DEF_2":
                    totalMechanizeForceArmorBuff += 0.1f;
                    break;
                case "MCFR_DEF_3":
                    totalMechanizeForceArmorBuff += 0.15f;
                    break;

                // MAP TILE
                case "PROV_BASIC":
                    totalOperationSupport += 0.03f;
                    break;
                case "IMPR_POWER":
                    totalOperationSupport += 0.05f;
                    break;
                case "IMPR_TELE":
                    totalOperationSupport += 0.05f;
                    break;
                case "IMPR_ROAD":
                    totalOperationSupport += 0.05f;
                    break;
                case "UNIV_JUST":
                    totalOperationSupport += 0.1f;
                    break;
                case "INIT_TAX":
                    totalOperationSupport -= 0.1f;
                    break;
                case "INIT_DRAFT":
                    OperationManager.Instance.maxSupply += 4;
                    OperationManager.Instance.maxSupplyUI.text = OperationManager.Instance.maxSupply.ToString();
                    totalOperationSupport -= 0.1f;
                    break;
                case "LOCAL_MILI":
                    OperationManager.Instance.maxSupply += 4;
                    OperationManager.Instance.maxSupplyUI.text = OperationManager.Instance.maxSupply.ToString();
                    totalOperationSupport -= 0.25f;
                    break;
                case "INIT_EXPT":
                    totalOperationSupport += 0.05f;
                    break;
                case "PROV_SERV":
                    totalOperationSupport += 0.03f;
                    break;
                case "CORE_HEAL":
                    totalOperationSupport += 0.05f;
                    break;
                case "CORE_FOOD":
                    totalOperationSupport += 0.05f;
                    break;
                case "CORE_EDUC":
                    totalOperationSupport += 0.05f;
                    break;
            }
        }
    }
    #region ---- || SWITCH TECH TREE || ----
    public void SwitchCivilianTree()
    {
        civilianTree.SetActive(true);
        governmentTree.SetActive(false);
        militaryTree.SetActive(false);
    }
    public void SwitchGovernmentTree()
    {
        civilianTree.SetActive(false);
        governmentTree.SetActive(true);
        militaryTree.SetActive(false);
    }
    public void SwitchMilitaryTree()
    {
        civilianTree.SetActive(false);
        governmentTree.SetActive(false);
        militaryTree.SetActive(true);
    }
    #endregion
}
