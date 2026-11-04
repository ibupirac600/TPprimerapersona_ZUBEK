using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemigo : MonoBehaviour, IRecibeDanio
{
    public static event Action EnemigoMuerto;

    [SerializeField] private int vida = 3;
    [SerializeField] private float tiempoAntesDeDesaparecer = 3f;
    [SerializeField] private int danioAlJugador = 1;
    [SerializeField] private float distanciaAtaque = 1.5f;
    [SerializeField] private float tiempoEntreAtaques = 1f;

    private NavMeshAgent agente;
    private Animator animator;
    private Transform jugador;
    private IRecibeDanio vidaJugador;
    private float proximoAtaque;
    private bool muerto;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null)
        {
            jugador = objetoJugador.transform;
            vidaJugador = objetoJugador.GetComponent<IRecibeDanio>();
        }
    }

    private void Update()
    {
        if (muerto || jugador == null)
        {
            return;
        }

        agente.SetDestination(jugador.position);
        animator.SetFloat("Velocidad", agente.velocity.magnitude);

        if (vidaJugador != null
            && Vector3.Distance(transform.position, jugador.position) <= distanciaAtaque
            && Time.time >= proximoAtaque)
        {
            vidaJugador.RecibirDanio(danioAlJugador);
            proximoAtaque = Time.time + tiempoEntreAtaques;
        }
    }

    public void RecibirDanio(int cantidad)
    {
        if (muerto)
        {
            return;
        }

        vida -= cantidad;

        if (vida <= 0)
        {
            Morir();
        }
        else if (TieneParametro("Danio"))
        {
            animator.SetTrigger("Danio");
        }
    }

    private void Morir()
    {
        muerto = true;
        agente.isStopped = true;
        agente.enabled = false;

        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        animator.SetTrigger("Muerte");
        EnemigoMuerto?.Invoke();
        Destroy(gameObject, tiempoAntesDeDesaparecer);
    }

    private bool TieneParametro(string nombre)
    {
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == nombre)
            {
                return true;
            }
        }
        return false;
    }
}
