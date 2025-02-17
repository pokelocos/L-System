using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class SubStruct : MonoBehaviour
{
    public Vector3 start;
    public Vector3 dir;
    public float size;

    public int index;

    public TreeStruct parent;

    /*public void Cut()
    {
        parent.subStructs.Remove(this);
        var subParts = GetAllChildRecursive(this.transform);
        subParts.ForEach(x => parent.subStructs.Remove(x));

        parent.RemoveAt(index);
        DestroyImmediate(this.gameObject);
    }*/

    public void CutCompoundWithCapsules()
    {
#if UNITY_EDITOR
        // 1) Registrar cambios (Undo) en el parent
        Undo.RecordObject(parent, "Cut subStructs");
#endif

        // 2) Crear el root con Rigidbody
        var cutRoot = new GameObject("CutBranchRoot");
        var rb = cutRoot.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.mass = 5f;

        cutRoot.transform.position = transform.position;
        cutRoot.transform.rotation = transform.rotation;

        // 3) Re-parent la rama
        var subs = GetAllChildRecursive(this.transform);
        subs.Add(this);

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

        // 4) Recopilar los índices y ordenarlos desc
        var indices = subs.Select(s => s.index).OrderByDescending(x => x).ToList();

        // 5) Eliminar en la cadena LSys los símbolos paramétricos en orden descendente
        foreach (var idx in indices)
        {
            if (idx >= 0 && idx < parent.LSys.Length)
            {
                Debug.Log($"Before removing param symbol at idx={idx}: {parent.LSys}");
                Debug.Log($"Subcadena en idx={idx}: {GetPreview(parent.LSys, idx, 20)}");

                parent.RemoveParamSymbol(idx);

                Debug.Log($"After removing param symbol: {parent.LSys}");
            }
            else
            {
                Debug.LogWarning($"Skip removal: index {idx} out of range (LSys length={parent.LSys.Length})");
            }
        }

        // 6) Limpiar brackets huérfanos
        parent.CleanOrphanBrackets();

#if UNITY_EDITOR
        parent.subStructs.RemoveAll(x => x == null);
        EditorUtility.SetDirty(parent);
#endif
    }

    /// <summary>
    /// Pequeño helper para mostrar máximo 'length' caracteres de LSys a partir de 'index'.
    /// </summary>
    private string GetPreview(string text, int index, int length)
    {
        if (index < 0 || index >= text.Length) return "(out of range)";
        int realLength = Mathf.Min(length, text.Length - index);
        return text.Substring(index, realLength);
    }


    public List<SubStruct> GetAllChildRecursive(Transform parent)
    {
        var toR = new List<SubStruct>();

        for (int i = 0; i < this.transform.childCount; i++)
        {
            var child = this.transform.GetChild(i);
            var sub = child.GetComponent<SubStruct>();
            if (sub != null)
            {
                toR.Add(sub);
                var chidSubs = sub.GetAllChildRecursive(child);
                toR.AddRange(chidSubs);
            }
        }

        return toR;
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