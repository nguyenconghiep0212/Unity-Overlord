using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChooseHQUI : MonoBehaviour
{
    public Transform target;
    public Button confirmHQButton;
    public Button confirmGarrisonButton;
    public Button confirmScannerButton;


    private void Awake()
    {
       if(confirmHQButton) confirmHQButton.onClick.AddListener(OperationManager.Instance.ConfirmHQ);
       if(confirmGarrisonButton) confirmGarrisonButton.onClick.AddListener(OperationManager.Instance.ConfirmGarrison);
       if(confirmScannerButton) confirmScannerButton.onClick.AddListener(OperationManager.Instance.ConfirmScanner);
    }
    // Start is called before the first frame update
    void LateUpdate()
    {
        if (target)
        { 
            transform.GetChild(0).position = Camera.main.WorldToScreenPoint(target.position);
            transform.GetChild(0).position = new Vector3(transform.GetChild(0).position.x, transform.GetChild(0).position.y - 10, transform.GetChild(0).position.z);
        }
    }
        
}
