using UnityEngine;
using UnityEngine.UI;  

public class ImagenTitilante : MonoBehaviour
{
    public float scaleMin = 1f;       
    public float scaleMax = 1.3f;     
    public float speed = 2f;          

    private RectTransform rt;         
    private bool creciendo = true;

    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (creciendo)
        {
            rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one * scaleMax, Time.deltaTime * speed);
            if (rt.localScale.x >= scaleMax - 0.01f)
                creciendo = false;
        }
        else
        {
            rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one * scaleMin, Time.deltaTime * speed);
            if (rt.localScale.x <= scaleMin + 0.01f)
                creciendo = true;
        }
    }
}
