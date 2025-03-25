using UnityEngine;
using TMPro;

public class ControlOculus : MonoBehaviour
{
    [Header("Rotación")]
    public Transform objetoCentro;
    public float velocidadRotacion = 30f;

    [Header("UI Debug")]
    public TextMeshProUGUI textoDebug;
    public TextMeshProUGUI textoDebug2;
    public TextMeshProUGUI textoDebug3;

    private bool controladorDerechoConectado = false;

    void Start()
    {
        VerificarConexionControladores();
    }

    void Update()
    {
        VerificarConexionControladores();

        if (!controladorDerechoConectado) return;

        ProcesarRotacion();
        ProcesarBotones();
        DepurarBotonesPresionados();
    }

    void VerificarConexionControladores()
    {
        controladorDerechoConectado = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

        if (!controladorDerechoConectado)
        {
            Debug.LogWarning("Controlador derecho no conectado!");
            if (textoDebug3 != null) textoDebug3.text = "Conecta el control derecho";
        }
    }

    void ProcesarRotacion()
    {
        Vector2 stickIzquierdo = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);

        if (Mathf.Abs(stickIzquierdo.x) > 0.1f)
        {
            float rotacion = stickIzquierdo.x * -velocidadRotacion * Time.deltaTime;
            transform.RotateAround(objetoCentro.position, Vector3.up, rotacion);

            if (textoDebug != null)
                textoDebug.text = $"Rotando (Left Stick): {stickIzquierdo.x:F2}";
        }
    }

    void ProcesarBotones()
    {
        // Botón del stick derecho (CLICK)
        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstickDown, OVRInput.Controller.RTouch))
        {
            ActualizarTextoDebug3("¡Click en stick derecho!");
        }

        // Gatillo de agarre (GRIP)
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.RTouch))
        {
            ActualizarTextoDebug3("¡Gatillo de agarre presionado!");
        }

        // Gatillo índice (TRIGGER frontal)
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            ActualizarTextoDebug3("¡Gatillo índice presionado!");
        }
    }

    void DepurarBotonesPresionados()
    {
        foreach (OVRInput.Button button in System.Enum.GetValues(typeof(OVRInput.Button)))
        {
            if (button == OVRInput.Button.Any || button == OVRInput.Button.None) continue;

            if (OVRInput.GetDown(button))
            {
                Debug.Log($"Botón presionado: {button}");
                if (textoDebug2 != null)
                    textoDebug2.text = $"Último botón: {button}";
            }
        }
    }

    void ActualizarTextoDebug3(string mensaje)
    {
        if (textoDebug3 != null)
        {
            textoDebug3.text = mensaje;
            Debug.Log(mensaje);
        }
    }
}