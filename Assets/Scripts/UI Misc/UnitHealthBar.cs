using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UnitHealthBar : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    public Image backgroundImage;
    public Image foregroundImage; 

    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Camera.main.WorldToScreenPoint(target.position + offset);
    }

    public void SetHealthBarPercentage(float percentage)
    {
        transform.GetChild(0).gameObject.SetActive(true);
        float parentHeight = GetComponent<RectTransform>().rect.height;
        float height = parentHeight * percentage;
        foregroundImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}
