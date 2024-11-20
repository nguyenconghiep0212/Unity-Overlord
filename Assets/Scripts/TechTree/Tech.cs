using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tech : MonoBehaviour, IPointerClickHandler
{
    public List<Tech> requiredTech;
    public List<Tech> leadToTech;

    public string techId;

    public string techName;
    [TextArea(3, 5)]
    public string techDescription;
    public Image techSprite;
    public Image techImg;
    public string purchasedColor;

    public int cost;
    public bool isUnlocked;
    public bool isPurchased;
    public bool isPurchasable;
    public bool isSelected;



    // Start is called before the first frame update
    void Start()
    {
        CheckUnlockTech();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CheckPurchasable()
    {
        if (OperationManager.Instance.totalFund >= cost)
        {
            isPurchasable = true;
        }
        else
        {
            isPurchasable = false;
        }
    }

    public void CheckUnlockTech()
    {
        isUnlocked = true;

        foreach (Tech tech in requiredTech)
        {
            if (!tech.isPurchased)
            {
                isUnlocked = false;
            }
        }
        gameObject.SetActive(isUnlocked);
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        CheckPurchasable();
        isSelected = true;
        TechTreeManager.Instance.SelectedTech(this);
    }
}
