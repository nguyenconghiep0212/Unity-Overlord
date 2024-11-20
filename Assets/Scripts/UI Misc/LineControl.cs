using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineControl : MonoBehaviour
{
    private LineRenderer lr;
    public string id;
    public List<Transform> points;

    // Start is called before the first frame update

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void SetUpLine(List<Transform> points)
    {
        lr.positionCount = points.Count;
        this.points = points;
    }


    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < points.Count; i++)
        {
            try
            {
                lr.SetPosition(i, points[i].position);

            }
            catch (System.Exception)
            {
                // AUTO DESTROY WHEN 1 OF THE CONNECTION IS LOST
                GameManagement.Instance.supportLineList.RemoveAll(s => s.GetComponent<LineControl>().id == id);
                Destroy(gameObject);
            }
        }
    }

    public void setColor(string hexColor)
    {
        UnityEngine.Color color;
        if (UnityEngine.ColorUtility.TryParseHtmlString(hexColor, out color))
        {
            GetComponent<LineRenderer>().startColor = color;
            GetComponent<LineRenderer>().endColor = color;
        }
    }
}