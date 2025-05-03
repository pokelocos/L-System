using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class SubStruct : MonoBehaviour
{
    /*────────────────────────── Datos geométricos ──────────────────────────*/
    public Vector3 start;
    public Vector3 dir;
    public float size;
    public int uniqueId = -1;          // ID que viene de la cadena L‑Sys

    [HideInInspector] public TreeStruct parent;   // asignado por Generator

    /*────────────────────────── Lógica de poda ─────────────────────────────*/
    [Header("Pruning")]
    [Tooltip("Marcar si esta rama es objetivo de corte")]
    public bool targetToCut = false;

    bool isCut = false;                    // evita dobles cortes

    /*────────────────────────── Visualización ──────────────────────────────*/
    [Header("Highlight (segundo material)")]
    public bool usesHighlight = true;
    public Material highlightMaterial;      // arrastra Mat_Outliner aquí

    [HideInInspector] public Material[] defaultMaterials;
    [HideInInspector] public MeshRenderer[] renderers;

    /*────────────────────────── Unity life‑cycle ───────────────────────────*/
    void Awake()
    {
        renderers = GetComponentsInChildren<MeshRenderer>(true);
        if (renderers.Length > 0)
            defaultMaterials = renderers[0].sharedMaterials;

        UpdateVisual();    // aplica o quita highlight según targetToCut
    }

#if UNITY_EDITOR
    /* Para verlo en escena al marcar/desmarcar targetToCut */
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            renderers = GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length > 0)
                defaultMaterials = renderers[0].sharedMaterials;
            UpdateVisual();
        }
    }
#endif

    /*────────────────────────── Visual helper ──────────────────────────────*/
    public void UpdateVisual()
    {
        if (renderers == null || highlightMaterial == null) return;

        foreach (var mr in renderers)
        {
            if (targetToCut && usesHighlight)
            {
                // Añadir highlight si no existe (material extra en slot 1)
                if (mr.sharedMaterials.Length == 1)
                {
                    var list = mr.sharedMaterials.ToList();
                    list.Add(highlightMaterial);
                    mr.sharedMaterials = list.ToArray();
                }
            }
            else
            {

                if (mr.sharedMaterials.Length > 0)
                mr.sharedMaterials = defaultMaterials;
#if UNITY_EDITOR
                // Restaurar materiales SOLO en Editor (evita Destroy problemas)
                if (!Application.isPlaying && mr.sharedMaterials.Length > 1)
                    mr.sharedMaterials = defaultMaterials;
#endif
            }
        }
    }

    /*────────────────────────── Cortar rama ───────────────────────────────*/
    public void CutCompoundWithCapsules()
    {
        if (isCut) return;          // ya fue cortada
        isCut = true;

        /* 0)  registrar métrica */
        if (targetToCut) SessionManager.Instance.RegisterCorrectCut();
        else SessionManager.Instance.RegisterError();

        targetToCut = false;        // ya no es objetivo
        //UpdateVisual();             // quita highlight en el árbol
        RemoveHighlight(highlightMaterial);    // quita el borde del segmento original
            
#if UNITY_EDITOR
        Undo.RecordObject(parent, "Cut subStructs");
#endif

        /* 1)  Crear contenedor físico para la rama cortada */
        var cutRoot = new GameObject("CutBranchRoot");
        var rb = cutRoot.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.mass = 5f;
        cutRoot.transform.SetPositionAndRotation(transform.position, transform.rotation);

        /* 2)  Re‑parent de todos los subSegmentos descendientes */
        var subs = GetAllChildRecursive(transform);
        subs.Add(this);

        foreach (var sub in subs)
        {
            parent.subStructs.Remove(sub);
            sub.RemoveHighlight(highlightMaterial);
            var childRb = sub.GetComponent<Rigidbody>();
            if (childRb)
            {
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(childRb);
#else
                Destroy(childRb);
#endif
            }
            sub.transform.SetParent(cutRoot.transform, true);
        }

        /* 3)  Actualizar cadena L‑Sys */
        var ids = subs.Select(s => s.uniqueId).Where(id => id >= 0).OrderByDescending(i => i);
        foreach (var id in ids) parent.RemoveSymbolById(id);

        parent.CleanOrphanBrackets();
        parent.RecalculateUniqueIds();
        parent.UpdateSubStructIdsFromLSys();

        /* 4)  Hacer agarrable en VR */
        var grab = cutRoot.AddComponent<XRGrabInteractable>();
        grab.enabled = false;  // reinicia colliders internos
        grab.enabled = true;

        // Cambiar layer para que el raycast ya no lo considere
        foreach (var col in cutRoot.GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("CutBranch");

#if UNITY_EDITOR
        parent.subStructs.RemoveAll(x => x == null);
        EditorUtility.SetDirty(parent);
#endif
    }

    /*────────────────────────── Helper recursivo ───────────────────────────*/
    public List<SubStruct> GetAllChildRecursive(Transform tr)
    {
        var list = new List<SubStruct>();
        for (int i = 0; i < tr.childCount; i++)
        {
            var child = tr.GetChild(i);
            if (child.TryGetComponent(out SubStruct sub))
            {
                list.Add(sub);
                list.AddRange(GetAllChildRecursive(child));
            }
        }
        return list;
    }
    void RemoveHighlight(Material highlightMat)
    {
        // puede estar en varios renderers del SubStruct
        foreach (var mr in renderers)          // renderers = GetComponentsInChildren<MeshRenderer>()
        {
            var mats = mr.sharedMaterials;     // array actual

            // ①  ¿hay más de un material?
            if (mats.Length <= 1) continue;

            // ②  Buscar la posición del material de contorno
            int index = System.Array.IndexOf(mats, highlightMat);
            if (index < 0) continue;           // no lo encontró

            // ③  Crear un nuevo array sin ese elemento
            var list = mats.ToList();
            list.RemoveAt(index);              // o list.Remove(highlightMat);
            mr.sharedMaterials = list.ToArray();
        }
    }

}




/*────────────────────────── Botón CUT en Inspector ───────────────────────*/
#if UNITY_EDITOR
[CustomEditor(typeof(SubStruct))]
public class SubStructEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Cut")) ((SubStruct)target).CutCompoundWithCapsules();
    }
}
#endif
