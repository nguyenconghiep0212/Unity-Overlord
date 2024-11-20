using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class TurnProgressBar : MonoBehaviour
{
 
    public Image backgroundImage;
    public Image foregroundImage; 

    // Start is called before the first frame update
    void Start()
    {
     }

    // Update is called once per frame
    void LateUpdate()
    {
    }

    public void SetTurnProgresssBarPercentage(float percentage)
    {
        if (percentage > 1) percentage = 1;
        transform.GetChild(0).gameObject.SetActive(true);
        float parentWidth = GetComponent<RectTransform>().rect.width;
        float width = parentWidth * percentage;
        foregroundImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}
