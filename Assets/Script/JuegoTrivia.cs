using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Para cambiar de escenas
using TMPro;
using System.Linq;
using System.Collections;

[System.Serializable]
public class Pregunta
{
    public string enunciado;
    public string[] opciones;
    public int indiceCorrecto;
}

public class JuegoTrivia : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text textoPregunta;
    public TMP_Text textoTiempo;
    public TMP_Text textoPuntos;
    public TMP_Text textoRanking;
    public Button[] botonesRespuesta;
    public Image feedbackImage;

    [Header("Config")]
    public float tiempoMax = 30f;

    [Header("Audio")]
    public AudioClip sonidoCorrecto;
    public AudioClip sonidoIncorrecto;

    [Header("Botones Extras")]
    public Button botonVolverInicio;
    public Button botonBorrarRanking;

    private Pregunta[] preguntas;
    private int indicePreguntaActual = 0;
    private float tiempoRestante;
    private int puntosTotales = 0;
    private AudioSource audioSource;
    private string nombreJugador;

    // 🔹 Para evitar que se guarde varias veces en el ranking
    private bool juegoTerminado = false;

    void Start()
    {
        // sacar nombre del jugador
        nombreJugador = PlayerPrefs.GetString("JugadorNombre", "Jugador");

        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Botón Volver al Menú
        if (botonVolverInicio != null)
        {
            botonVolverInicio.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Menu");
            });
            botonVolverInicio.gameObject.SetActive(false);
        }

        // Botón Borrar Ranking
        if (botonBorrarRanking != null)
        {
            botonBorrarRanking.onClick.AddListener(() =>
            {
                RankingManager.ClearRanking();
                MostrarRanking();
            });
            botonBorrarRanking.gameObject.SetActive(false);
        }

        // Lista de preguntas
        preguntas = new Pregunta[]
        {
            // --- FACILES ---
            new Pregunta { enunciado = "¿Cuánto es 5 + 3?", opciones = new string[] {"6", "7", "8", "9"}, indiceCorrecto = 2 },
            new Pregunta { enunciado = "¿Cuánto es 9 - 4?", opciones = new string[] {"3", "5", "6", "7"}, indiceCorrecto = 1 },
            new Pregunta { enunciado = "¿Cuánto es 6 × 2?", opciones = new string[] {"8", "10", "12", "14"}, indiceCorrecto = 2 },

            // --- MEDIAS ---
            new Pregunta { enunciado = "¿Cuánto es 12 ÷ 3?", opciones = new string[] {"2", "3", "4", "6"}, indiceCorrecto = 2 },
            new Pregunta { enunciado = "¿Cuánto es (5 × 2) + 8?", opciones = new string[] {"15", "18", "20", "25"}, indiceCorrecto = 1 },
            new Pregunta { enunciado = "¿Cuánto es 15 - (3 × 4)?", opciones = new string[] {"1", "2", "3", "4"}, indiceCorrecto = 2 },

            // --- DIFICILES ---
            new Pregunta { enunciado = "¿Cuál es la raíz cuadrada de 144?", opciones = new string[] {"10", "11", "12", "13"}, indiceCorrecto = 2 },
            new Pregunta { enunciado = "¿Cuánto es (8 × 5) ÷ 2?", opciones = new string[] {"15", "18", "20", "25"}, indiceCorrecto = 2 },
            new Pregunta { enunciado = "¿Cuánto es 3² + 4²?", opciones = new string[] {"12", "25", "18", "30"}, indiceCorrecto = 1 },

            // --- EXTREMAS ---
            new Pregunta { enunciado = "¿Cuánto es (7 × 7) - (8 × 6)?", opciones = new string[] {"1", "5", "7", "9"}, indiceCorrecto = 0 },
            new Pregunta { enunciado = "Si x = 3, ¿cuánto vale 2x² + 5?", opciones = new string[] {"17", "19", "20", "23"}, indiceCorrecto = 3 },
            new Pregunta { enunciado = "¿Cuál es la suma de los primeros 10 números naturales? (1+2+...+10)", opciones = new string[] {"45", "50", "55", "60"}, indiceCorrecto = 0 }
        };

        // Ordenar aleatoriamente
        preguntas = preguntas.OrderBy(x => Random.value).ToArray();

        MostrarPregunta();
    }

    void Update()
    {
        if (!juegoTerminado && tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            textoTiempo.text = "Tiempo: " + Mathf.Ceil(tiempoRestante).ToString();
        }
        else if (!juegoTerminado && tiempoRestante <= 0)
        {
            SiguientePregunta(false);
        }
    }

    void MostrarPregunta()
    {
        if (indicePreguntaActual >= preguntas.Length)
        {
            if (!juegoTerminado) // 👈 Evita duplicar en el ranking
            {
                juegoTerminado = true;

                // Juego terminado
                textoPregunta.text = "Juego terminado\nPuntos: " + puntosTotales;

                // Guardar en ranking
                RankingManager.AddScore(nombreJugador, puntosTotales);

                // Mostrar ranking
                MostrarRanking();

                // Desactivar tiempo y puntos
                if (textoTiempo != null) textoTiempo.gameObject.SetActive(false);
                if (textoPuntos != null) textoPuntos.gameObject.SetActive(false);

                foreach (var b in botonesRespuesta) b.gameObject.SetActive(false);

                // botones extra
                if (botonVolverInicio != null) botonVolverInicio.gameObject.SetActive(true);
                if (botonBorrarRanking != null) botonBorrarRanking.gameObject.SetActive(true);
            }
            return;
        }

        Pregunta p = preguntas[indicePreguntaActual];
        textoPregunta.text = p.enunciado;

        for (int i = 0; i < botonesRespuesta.Length; i++)
        {
            int index = i;
            botonesRespuesta[i].GetComponentInChildren<TMP_Text>().text = p.opciones[i];
            botonesRespuesta[i].onClick.RemoveAllListeners();
            botonesRespuesta[i].onClick.AddListener(() => ComprobarRespuesta(index));
        }

        tiempoRestante = tiempoMax;
    }

    void ComprobarRespuesta(int indexElegido)
    {
        Pregunta p = preguntas[indicePreguntaActual];

        if (indexElegido == p.indiceCorrecto)
        {
            puntosTotales += Mathf.CeilToInt(tiempoRestante);
            if (sonidoCorrecto) audioSource.PlayOneShot(sonidoCorrecto);
            StartCoroutine(MostrarFeedback(Color.green));
        }
        else
        {
            if (sonidoIncorrecto) audioSource.PlayOneShot(sonidoIncorrecto);
            StartCoroutine(MostrarFeedback(Color.red));
        }

        SiguientePregunta(true);
    }

    void SiguientePregunta(bool respondida)
    {
        indicePreguntaActual++;
        textoPuntos.text = "Puntos: " + puntosTotales;
        MostrarPregunta();
    }

    IEnumerator MostrarFeedback(Color color)
    {
        if (feedbackImage != null)
        {
            Color original = feedbackImage.color;
            color.a = 0.5f;

            feedbackImage.color = color;
            feedbackImage.gameObject.SetActive(true);

            yield return new WaitForSeconds(0.5f);

            feedbackImage.gameObject.SetActive(false);
            feedbackImage.color = original;
        }
    }

    void MostrarRanking()
    {
        if (textoRanking == null) return;

        ScoreList scores = RankingManager.LoadRanking();

        string tabla = "Ranking Top 3:\n";
        int pos = 1;
        foreach (var s in scores.lista)
        {
            tabla += pos + ". " + s.nombre + " - " + s.puntaje + "\n";
            pos++;
        }

        textoRanking.text = tabla;
    }
}
