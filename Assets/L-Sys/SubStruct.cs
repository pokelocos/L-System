using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public void Cut()
    {
        parent.subStructs.Remove(this);
        var subParts = GetAllChildRecursive(this.transform);
        subParts.ForEach(x => parent.subStructs.Remove(x));

        parent.RemoveAt(index);
        DestroyImmediate(this.gameObject);
    }

    public void DrawGizmos()
    {
        Gizmos.DrawLine(start, start + (dir * size));
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
            sub.Cut();
        }
    }
}
#endif