using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrainedUnit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameManagement.UnitType unitType;
    public GameObject newUnit;


    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            switch (unitType)
            {
                case GameManagement.UnitType.soldier:
                    if (OperationManager.Instance.trainedFootSoldier > 0)
                        newUnit = Instantiate(OperationManager.Instance.footSoldierPrefab, new Vector3(Input.mousePosition.x, 0.08f, Input.mousePosition.z), Quaternion.identity);
                    break;
                case GameManagement.UnitType.mechanize:
                    if (OperationManager.Instance.trainedMechanizeForce > 0)
                        newUnit = Instantiate(OperationManager.Instance.mechanizeForcePrefab, new Vector3(Input.mousePosition.x, 0.08f, Input.mousePosition.z), Quaternion.identity);
                    break;
                case GameManagement.UnitType.air:
                    if (OperationManager.Instance.trainedAirForce > 0)
                        newUnit = Instantiate(OperationManager.Instance.airForcePrefab, new Vector3(Input.mousePosition.x, 0.08f, Input.mousePosition.z), Quaternion.identity);
                    break;
            }
            if (newUnit)
            {
                newUnit.transform.SetParent(GameManagement.Instance.allyUnitParent.transform);
                newUnit.GetComponent<Unit>().isDragging = true;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (newUnit)
        {
            if (!newUnit.GetComponent<Unit>().draggingTile)
            {
            }
            else
            {
                switch (unitType)
                {
                    case GameManagement.UnitType.soldier:
                        OperationManager.Instance.trainedFootSoldier--;
                        OperationManager.Instance.trainedFootSoldierUI.text = OperationManager.Instance.trainedFootSoldier.ToString();
                        break;
                    case GameManagement.UnitType.mechanize:
                        OperationManager.Instance.trainedMechanizeForce--;
                        OperationManager.Instance.trainedMechanizeForceUI.text = OperationManager.Instance.trainedMechanizeForce.ToString();
                        break;
                    case GameManagement.UnitType.air:
                        OperationManager.Instance.trainedAirForce--;
                        OperationManager.Instance.trainedAirForceUI.text = OperationManager.Instance.trainedAirForce.ToString();
                        break;
                }
                newUnit = null;
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
