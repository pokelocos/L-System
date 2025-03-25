using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SubStruct : MonoBehaviour
{
    public Vector3 start;
    public Vector3 dir;
    public float size;

    // Usamos uniqueId para identificar el símbolo en LSys.
    public int uniqueId = -1;

    public TreeStruct parent;

    /// <summary>
    /// Corta la rama completa (compuesta) y elimina de LSys solo el símbolo correspondiente
    /// al SubStruct que invoca el corte. Los SubStructs internos se mantienen.
    /// Luego se recalculan los sufijos de ID y se actualizan los uniqueId en la jerarquía.
    /// </summary>
    public void CutCompoundWithCapsules()
    {
#if UNITY_EDITOR
        Undo.RecordObject(parent, "Cut subStructs");
#endif

        // 1) Crear el contenedor "CutBranchRoot" con Rigidbody.
        var cutRoot = new GameObject("CutBranchRoot");
        var rb = cutRoot.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.mass = 5f;
        cutRoot.transform.position = transform.position;
        cutRoot.transform.rotation = transform.rotation;

        // Agregar XR Grab Interactable para manipulación en VR.
        // Asegúrate de tener "using UnityEngine.XR.Interaction.Toolkit;" al inicio.


        // 2) Re-parent la rama cortada: obtener todos los SubStruct de esta rama.
        var subs = GetAllChildRecursive(this.transform);
        subs.Add(this); // Incluir el propio SubStruct que invoca el corte.

        foreach (var sub in subs)
        {
            parent.subStructs.Remove(sub);
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

        // 3) Recalcular y actualizar la cadena LSys:
        //    a) Eliminar en LSys los símbolos correspondientes a los uniqueId de los SubStructs involucrados.
        var ids = subs.Select(s => s.uniqueId)
                      .Where(id => id >= 0)
                      .OrderByDescending(x => x)
                      .ToList();
        foreach (var id in ids)
        {
            parent.RemoveSymbolById(id);
        }

        //    b) Limpiar los bloques de corchetes huérfanos.
        parent.CleanOrphanBrackets();

        //    c) Recalcular los sufijos de ID: se deja el sufijo "#" para luego actualizar.
        parent.RecalculateUniqueIds();

        // 4) Actualizar los uniqueId de cada SubStruct según la nueva cadena LSys.
        parent.UpdateSubStructIdsFromLSys();

        cutRoot.AddComponent<XRGrabInteractable>();
        var grabInteractable = cutRoot.GetComponent<XRGrabInteractable>();
        grabInteractable.enabled = false;
        grabInteractable.enabled = true;
        // Tras reparentar, obtén todos los colliders del cutRoot
        Collider[] childColliders = cutRoot.GetComponentsInChildren<Collider>();

        // Si necesitas usarlos para alguna lógica custom o debug, puedes hacerlo aquí:
        foreach (var col in childColliders)
        {
            Debug.Log("Collider encontrado: " + col.name);
        }
#if UNITY_EDITOR
        parent.subStructs.RemoveAll(x => x == null);
        EditorUtility.SetDirty(parent);
#endif
    }


    /// <summary>
    /// Recorre recursivamente los hijos de este objeto y devuelve todos los SubStruct encontrados.
    /// </summary>
    public List<SubStruct> GetAllChildRecursive(Transform unused)
    {
        var list = new List<SubStruct>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var sub = child.GetComponent<SubStruct>();
            if (sub != null)
            {
                list.Add(sub);
                list.AddRange(sub.GetAllChildRecursive(child));
            }
        }
        return list;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SubStruct))]
public class SubStructEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var sub = (SubStruct)target;
        if (GUILayout.Button("Cut"))
        {
            sub.CutCompoundWithCapsules();
        }
    }
}
#endif
