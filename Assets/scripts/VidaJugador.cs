using System;
using UnityEngine;

public class VidaJugador : MonoBehaviour, IRecibeDanio
{
    public event Action<int, int> VidaCambio;
    public event Action JugadorMuerto;

    [SerializeField] private int vidaMaxima = 5;

    public int VidaActual { get; private set; }
    public int VidaMaxima => vidaMaxima;

    private void Awake()
    {
        VidaActual = vidaMaxima;
    }

    public void RecibirDanio(int cantidad)
    {
        if (VidaActual <= 0)
        {
            return;
        }

        VidaActual = Mathf.Max(0, VidaActual - cantidad);
        VidaCambio?.Invoke(VidaActual, vidaMaxima);

        if (VidaActual == 0)
        {
            JugadorMuerto?.Invoke();
        }
    }
}
