using UnityEngine;

public class PincersRaycast : MonoBehaviour
{
    [Tooltip("Transform del punto de origen del raycast (ej. hijo 'RayOrigin').")]
    public Transform rayOrigin;
    [Tooltip("Distancia máxima del raycast.")]
    public float rayDistance = 5f;
    [Tooltip("Layer Mask para filtrar qué objetos pueden ser cortados.")]
    public LayerMask layerToCut;
    [Tooltip("Line Renderer para visualizar el raycast en VR.")]
    public LineRenderer lineRenderer;

    void Update()
    {
        // Actualiza la visualización del raycast cada frame.
        UpdateLineRenderer();

        // Ejemplo de input para pruebas en PC (en VR usar Activate/Select events).
        if (Input.GetKeyDown(KeyCode.C))
        {
            DoRaycast();
        }
    }

    /// <summary>
    /// Actualiza el Line Renderer para mostrar el raycast desde rayOrigin hasta el máximo (o hasta el hit, si ocurre).
    /// </summary>
    void UpdateLineRenderer()
    {
        if (lineRenderer != null && rayOrigin != null)
        {
            // Aseguramos que el Line Renderer tenga dos posiciones.
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, rayOrigin.position);
            // Por defecto, se establece el final del rayo al máximo.
            lineRenderer.SetPosition(1, rayOrigin.position + rayOrigin.forward * rayDistance);
        }
    }

    /// <summary>
    /// Lanza un raycast desde rayOrigin y, si impacta un objeto que tenga un SubStruct en sus padres,
    /// se llama al método de corte de ese SubStruct.
    /// </summary>
    public void DoRaycast()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * rayDistance, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, layerToCut))
        {
            // Actualiza la posición final del Line Renderer al punto de impacto.
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(1, hit.point);
            }

            // Buscar el SubStruct en el objeto impactado o en sus padres.
            var sub = hit.collider.GetComponentInParent<SubStruct>();
            if (sub != null)
            {
                sub.CutCompoundWithCapsules();
            }
            else
            {
                Debug.Log("Ray hit: " + hit.collider.name + " but no SubStruct found.");
            }
        }
        else
        {
            // Si no se impacta nada, restaurar el final del rayo al máximo.
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(1, rayOrigin.position + rayOrigin.forward * rayDistance);
            }
            Debug.Log("Raycast no hit.");
        }
    }


}
