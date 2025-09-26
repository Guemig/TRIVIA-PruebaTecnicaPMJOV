using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Animación de Escala")]
    public float scaleMultiplier = 1.1f;
    public float animationSpeed = 5f;

    [Header("Paneles a activar/desactivar (opcional)")]
    public GameObject[] panelsToActivate;

    [Header("¿Este botón cierra el juego?")]
    public bool isExitButton = false;

    [Header("Sonidos")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    private Vector3 originalScale;
    private bool isHovered;

    void Start()
    {
        originalScale = transform.localScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // hover
        Vector3 targetScale = isHovered ? originalScale * scaleMultiplier : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        //sound hover
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
        {
        
            StartCoroutine(PlayClickAndExecute());
        }
        else
        {
            
            ExecuteAction();
        }
    }

    private IEnumerator PlayClickAndExecute()
    {
        audioSource.PlayOneShot(clickSound);

        //esperar sound
        yield return new WaitForSeconds(clickSound.length);

        ExecuteAction();
    }

    private void ExecuteAction()
    {
        if (isExitButton)
        {
            Debug.Log("Saliendo del juego...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else
        {
            foreach (GameObject panel in panelsToActivate)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }
    }
}
