using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private VidaJugador vidaJugador;
    [SerializeField] private Arma arma;
    [SerializeField] private PlayerInput entradaJugador;

    [Header("Partida")]
    [SerializeField] private float tiempoLimite = 120f;

    [Header("Interfaz")]
    [SerializeField] private TMP_Text textoVida;
    [SerializeField] private TMP_Text textoTiempo;
    [SerializeField] private TMP_Text textoEnemigos;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    private float tiempoRestante;
    private int totalEnemigos;
    private int enemigosEliminados;
    private bool terminado;

    private void OnEnable()
    {
        Enemigo.EnemigoMuerto += EnemigoEliminado;
        if (vidaJugador != null)
        {
            vidaJugador.VidaCambio += ActualizarVida;
            vidaJugador.JugadorMuerto += Derrota;
        }
    }

    private void OnDisable()
    {
        Enemigo.EnemigoMuerto -= EnemigoEliminado;
        if (vidaJugador != null)
        {
            vidaJugador.VidaCambio -= ActualizarVida;
            vidaJugador.JugadorMuerto -= Derrota;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        tiempoRestante = tiempoLimite;
        totalEnemigos = FindObjectsByType<Enemigo>().Length;

        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        if (vidaJugador != null)
        {
            ActualizarVida(vidaJugador.VidaActual, vidaJugador.VidaMaxima);
        }
        ActualizarEnemigos();
        ActualizarTiempo();
    }

    private void Update()
    {
        if (terminado)
        {
            return;
        }

        tiempoRestante -= Time.deltaTime;
        ActualizarTiempo();

        if (tiempoRestante <= 0f)
        {
            Derrota();
        }
    }

    private void EnemigoEliminado()
    {
        enemigosEliminados++;
        ActualizarEnemigos();

        if (enemigosEliminados >= totalEnemigos)
        {
            Victoria();
        }
    }

    private void ActualizarVida(int actual, int maxima)
    {
        if (textoVida == null) return;
        textoVida.text = "Vida: " + actual + " / " + maxima;
    }

    private void ActualizarTiempo()
    {
        if (textoTiempo == null) return;
        float t = Mathf.Max(0f, tiempoRestante);
        int minutos = Mathf.FloorToInt(t / 60f);
        int segundos = Mathf.FloorToInt(t % 60f);
        textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");
    }

    private void ActualizarEnemigos()
    {
        if (textoEnemigos == null) return;
        textoEnemigos.text = "Enemigos: " + enemigosEliminados + " / " + totalEnemigos;
    }

    private void Victoria()
    {
        Terminar(panelVictoria);
    }

    private void Derrota()
    {
        Terminar(panelDerrota);
    }

    private void Terminar(GameObject panel)
    {
        if (terminado)
        {
            return;
        }

        terminado = true;
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;

        if (entradaJugador != null)
        {
            entradaJugador.enabled = false;
        }
        if (arma != null)
        {
            arma.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
