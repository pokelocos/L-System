using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TreeStruct : MonoBehaviour
{
    [TextArea]
    public string LSys = "";

    public List<SubStruct> subStructs = new();

    private void OnDrawGizmos()
    {
        foreach (var s in subStructs)
        {
            s.DrawGizmos();
        }
    }

    public void RemoveAt(int index)
    {
        int bracketCount = 0;

        for (int i = index; i < LSys.Length; i++)
        {
            var c = LSys[i];

            if (c == '[')
                bracketCount++;
            
            if (c == ']')
                bracketCount--;

            if (bracketCount == -1)
            { 
                LSys = LSys.Remove(index, i - index + 1);
                return;
            }
        }

        // If the bracket count is not -1,remove to end of string
        LSys = LSys.Substring(0, index);
    }
}

