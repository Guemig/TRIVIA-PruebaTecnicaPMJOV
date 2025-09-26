#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_ANDROID && !UNITY_IOS
using SFB; // Standalone (pc)
#endif

public class RegistrationController : MonoBehaviour
{
    [Header("Campos de UI (TextMeshPro)")]
    public TMP_InputField nameInput;
    public TMP_InputField emailInput;
    public RawImage photoPreview;
    public Button takePhotoButton;
    public Button choosePhotoButton;
    public Button submitButton;
    public TMP_Text feedbackText;

    [Header("Canvas a alternar")]
    public Canvas canvasRegistro;
    public Canvas canvasConfirmacion;

    [Header("Opciones")]
    public bool requirePhoto = true;
    public int fixedPhotoSize = 100;

    private Texture2D originalPhotoTexture = null;
    private WebCamTexture webCamTexture = null;
    private bool isTakingPhoto = false;

    void Start()
    {
        if (takePhotoButton != null) takePhotoButton.onClick.AddListener(OnTakePhotoClicked);
        if (choosePhotoButton != null) choosePhotoButton.onClick.AddListener(OnChoosePhotoClicked);
        if (submitButton != null) submitButton.onClick.AddListener(OnSubmitClicked);

        if (feedbackText != null) feedbackText.text = "";
        if (canvasConfirmacion != null) canvasConfirmacion.gameObject.SetActive(false);

       
        PlayerPrefs.DeleteKey("JugadorFotoPath");
    }

    void OnDisable()
    {
        if (webCamTexture != null)
        {
            if (webCamTexture.isPlaying) webCamTexture.Stop();
            webCamTexture = null;
        }
    }

    // ---------------- FOTO ----------------
    private void OnTakePhotoClicked()
    {
        if (!isTakingPhoto) StartCoroutine(CaptureFromWebcam());
    }

    private IEnumerator CaptureFromWebcam()
    {
        isTakingPhoto = true;

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            if (feedbackText != null) feedbackText.text = "No se encontró ninguna cámara.";
            isTakingPhoto = false;
            yield break;
        }

        string camName = devices[0].name;
        if (feedbackText != null) feedbackText.text = "Inicializando cámara: " + camName;

        webCamTexture = new WebCamTexture(camName, 1280, 720, 30);
        webCamTexture.Play();

        float timeout = 6f;
        while (timeout > 0f && (webCamTexture.width <= 100 || !webCamTexture.didUpdateThisFrame))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!webCamTexture.isPlaying || webCamTexture.width <= 100)
        {
            if (feedbackText != null) feedbackText.text = "No se pudo inicializar la cámara.";
            if (webCamTexture != null) { webCamTexture.Stop(); webCamTexture = null; }
            isTakingPhoto = false;
            yield break;
        }

        yield return new WaitForEndOfFrame();

        Texture2D snap = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
        snap.SetPixels32(webCamTexture.GetPixels32());
        snap.Apply();

        webCamTexture.Stop();
        webCamTexture = null;

        SetPhotoTexture(snap);
        if (feedbackText != null) feedbackText.text = "Foto tomada correctamente.";
        isTakingPhoto = false;
    }

    // ---------------- GALERIA ----------------
    private void OnChoosePhotoClicked()
    {
#if UNITY_EDITOR
        string pathEd = EditorUtility.OpenFilePanel("Selecciona imagen", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(pathEd)) LoadImage(pathEd);

#elif UNITY_ANDROID || UNITY_IOS
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (!string.IsNullOrEmpty(path))
                LoadImage(path);
            else if (feedbackText != null)
                feedbackText.text = "No se seleccionó ninguna imagen.";
        }, "Selecciona una imagen", "image/*");

#else
        var extensions = new[] { new ExtensionFilter("Imágenes", "png", "jpg", "jpeg") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Selecciona una imagen", "", extensions, false);
        if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            LoadImage(paths[0]);
        }
        else
        {
            if (feedbackText != null) feedbackText.text = "No se seleccionó ninguna imagen.";
        }
#endif
    }

    // ---------------- CARGAR Y PREVIEW ----------------
    private void LoadImage(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                if (feedbackText != null) feedbackText.text = "Archivo no encontrado: " + path;
                return;
            }

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            if (!tex.LoadImage(bytes))
            {
                if (feedbackText != null) feedbackText.text = "No se pudo cargar la imagen.";
                return;
            }

            SetPhotoTexture(tex);

            if (feedbackText != null) feedbackText.text = "Foto cargada correctamente.";
        }
        catch (System.Exception ex)
        {
            if (feedbackText != null) feedbackText.text = "Error cargando imagen: " + ex.Message;
        }
    }

    private void SetPhotoTexture(Texture2D tex)
    {
        if (originalPhotoTexture != null) Destroy(originalPhotoTexture);

        
        originalPhotoTexture = ScaleTextureFixed(tex, fixedPhotoSize, fixedPhotoSize);

        if (photoPreview != null)
        {
            photoPreview.texture = originalPhotoTexture;
            photoPreview.rectTransform.sizeDelta = new Vector2(fixedPhotoSize, fixedPhotoSize);

            
            var fitter = photoPreview.GetComponent<AspectRatioFitter>();
            if (fitter != null) fitter.aspectRatio = 1f;
        }
    }

    private Texture2D ScaleTextureFixed(Texture2D source, int newW, int newH)
    {
        if (source == null) return null;

        RenderTexture rt = RenderTexture.GetTemporary(newW, newH, 0, RenderTextureFormat.Default);
        Graphics.Blit(source, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(newW, newH, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, newW, newH), 0, 0);
        result.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    // ---------------- GUARDAR ----------------
    private void OnSubmitClicked()
    {
        string name = (nameInput != null) ? nameInput.text.Trim() : "";
        string email = (emailInput != null) ? emailInput.text.Trim() : "";

        if (string.IsNullOrEmpty(name))
        {
            if (feedbackText != null) feedbackText.text = "El nombre es obligatorio.";
            return;
        }

        if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
        {
            if (feedbackText != null) feedbackText.text = "Correo electrónico no válido.";
            return;
        }

        if (requirePhoto && originalPhotoTexture == null)
        {
            if (feedbackText != null) feedbackText.text = "Debes cargar o tomar una foto.";
            return;
        }

        PlayerPrefs.SetString("JugadorNombre", name);
        PlayerPrefs.SetString("JugadorEmail", email);

        
        if (originalPhotoTexture != null)
        {
            try
            {
                byte[] png = originalPhotoTexture.EncodeToPNG();
                string folder = Application.persistentDataPath;
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, "JugadorFoto.png");
                File.WriteAllBytes(filePath, png);

                
                PlayerPrefs.SetString("JugadorFotoPath", filePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("No se pudo guardar la foto: " + ex.Message);
                if (feedbackText != null) feedbackText.text = "Registro, pero no se pudo guardar la imagen.";
            }
        }

        PlayerPrefs.Save();

        if (feedbackText != null) feedbackText.text = "Registro exitoso.";

        if (canvasRegistro != null) canvasRegistro.gameObject.SetActive(false);
        if (canvasConfirmacion != null) canvasConfirmacion.gameObject.SetActive(true);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
}
