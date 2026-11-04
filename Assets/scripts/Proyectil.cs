using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Proyectil : MonoBehaviour
{
    [SerializeField] private int danio = 1;
    [SerializeField] private float tiempoDeVida = 3f;

    private void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    private void OnCollisionEnter(Collision colision)
    {
        IRecibeDanio objetivo = colision.collider.GetComponentInParent<IRecibeDanio>();
        if (objetivo != null)
        {
            objetivo.RecibirDanio(danio);
        }

        Destroy(gameObject);
    }
}
