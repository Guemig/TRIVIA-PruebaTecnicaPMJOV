using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();

        
        rt.anchorMin = Vector2.zero;  
        rt.anchorMax = Vector2.one;   
        rt.offsetMin = Vector2.zero;   // eliminar margenes
        rt.offsetMax = Vector2.zero;   
    }
}
