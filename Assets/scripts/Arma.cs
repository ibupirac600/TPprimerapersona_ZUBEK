using UnityEngine;
using UnityEngine.InputSystem;

public class Arma : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Rigidbody prefabProyectil;
    [SerializeField] private Transform puntoDeDisparo;

    [Header("Parametros")]
    [SerializeField] private float velocidadProyectil = 30f;
    [SerializeField] private float cadenciaDeDisparo = 0.25f;

    [Header("Input")]
    [SerializeField] private InputAction accionDisparar =
        new InputAction("Disparar", InputActionType.Button, "<Mouse>/leftButton");

    private float proximoDisparo;

    private void OnEnable() => accionDisparar.Enable();
    private void OnDisable() => accionDisparar.Disable();

    private void Update()
    {
        if (accionDisparar.IsPressed() && Time.time >= proximoDisparo)
        {
            Disparar();
            proximoDisparo = Time.time + cadenciaDeDisparo;
        }
    }

    private void Disparar()
    {
        Rigidbody bala = Instantiate(prefabProyectil, puntoDeDisparo.position, puntoDeDisparo.rotation);
        bala.linearVelocity = puntoDeDisparo.forward * velocidadProyectil;
    }
}
