using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

public class TreeStruct : MonoBehaviour
{
    [TextArea]
    public string LSys = "";

    [HideInInspector]
    public List<SubStruct> subStructs = new List<SubStruct>();

    private void OnDrawGizmos()
    {
        // Por ejemplo, para depuración: dibujar gizmos de cada SubStruct
        foreach (var s in subStructs)
        {
            // s.DrawGizmos();
        }
    }

    /// <summary>
    /// Elimina de LSys el bloque de corchetes que comienza en el índice dado.
    /// Se basa en un conteo de corchetes.
    /// </summary>
    public void RemoveAt(int index)
    {
        int bracketCount = 0;
        for (int i = index; i < LSys.Length; i++)
        {
            char c = LSys[i];
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
        // Si no se encontró un cierre, elimina hasta el final.
        LSys = LSys.Substring(0, index);
    }

    /// <summary>
    /// Elimina un símbolo paramétrico (por ejemplo, B(3) o F(0.5)) asumiendo que LSys[index] es la letra.
    /// </summary>
    public void RemoveParamSymbol(int index)
    {
        if (index < 0 || index >= LSys.Length) return;
        int lengthToRemove = 1;
        int next = index + 1;
        if (next < LSys.Length && LSys[next] == '(')
        {
            int parenCount = 1;
            for (int i = next + 1; i < LSys.Length; i++)
            {
                if (LSys[i] == '(') parenCount++;
                else if (LSys[i] == ')') parenCount--;
                if (parenCount == 0)
                {
                    lengthToRemove = i - index + 1;
                    break;
                }
            }
            if (parenCount > 0)
                lengthToRemove = LSys.Length - index;
        }
        LSys = LSys.Remove(index, lengthToRemove);
    }

    /// <summary>
    /// Elimina de LSys el símbolo paramétrico que tenga el sufijo "#<id>".
    /// Se busca el patrón "#<id>" y se retrocede hasta el inicio del símbolo para eliminarlo.
    /// </summary>
    /*public void RemoveSymbolById(int id)
    {
        string idTag = "#" + id;
        int pos = LSys.IndexOf(idTag);
        if (pos < 0)
        {
            Debug.LogWarning($"RemoveSymbolById: No se encontró el ID {id} en LSys.");
            return;
        }
        int start = pos - 1;
        while (start >= 0 && IsPartOfParamSymbol(LSys[start]))
        {
            start--;
        }
        start++; // Ahora 'start' es el inicio del símbolo
        int lengthToRemove = (pos + idTag.Length) - start;
        lengthToRemove = Mathf.Min(lengthToRemove, LSys.Length - start);
        LSys = LSys.Remove(start, lengthToRemove);
    }*/

    public void RemoveSymbolById(int id)
    {
        // Construimos un patrón que capture símbolos del tipo:
        // [FAB]\([^)]*\)#(id)\b
        // Esto buscará, por ejemplo, "B(3)#13" exactamente.
        string pattern = $@"[FAB]\([^)]*\)#({id})\b";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(LSys);
        if (match.Success)
        {
            Debug.Log($"RemoveSymbolById: Eliminando símbolo: {match.Value}");
            LSys = regex.Replace(LSys, "");
        }
        else
        {
            Debug.LogWarning($"RemoveSymbolById: No se encontró el ID {id} en LSys.");
        }
    }


    /// <summary>
    /// Determina si un carácter forma parte de un símbolo paramétrico (letras, dígitos, paréntesis, puntos, guiones).
    /// </summary>
    private bool IsPartOfParamSymbol(char c)
    {
        return char.IsLetterOrDigit(c) || c == '(' || c == ')' || c == '.' || c == '-';
    }

    /// <summary>
    /// Elimina bloques de corchetes [ ... ] que no contengan ningún símbolo generador (por ejemplo, F, A, B).
    /// </summary>
    public void CleanOrphanBrackets()
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            int i = 0;
            while (i < LSys.Length)
            {
                if (LSys[i] == '[')
                {
                    int startIndex = i;
                    int bracketCount = 1;
                    int j = i + 1;
                    for (; j < LSys.Length && bracketCount > 0; j++)
                    {
                        if (LSys[j] == '[') bracketCount++;
                        else if (LSys[j] == ']') bracketCount--;
                    }
                    if (bracketCount == 0)
                    {
                        int endIndex = j - 1;
                        string inside = LSys.Substring(startIndex + 1, endIndex - startIndex - 1);
                        if (!ContainsGeneratorSymbol(inside))
                        {
                            LSys = LSys.Remove(startIndex, endIndex - startIndex + 1);
                            changed = true;
                            break;
                        }
                        else
                        {
                            i = startIndex + 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    i++;
                }
            }
        }
    }

    /// <summary>
    /// Verifica si el contenido contiene alguno de los símbolos generadores (por ejemplo, F, A, B).
    /// </summary>
    private bool ContainsGeneratorSymbol(string content)
    {
        char[] generators = { 'F', 'A', 'B' };
        return content.Any(c => generators.Contains(c));
    }

    /// <summary>
    /// Recalcula los sufijos de ID para cada símbolo paramétrico en LSys.
    /// Se reinicia (opcionalmente) el contador y se asignan nuevos sufijos en orden de aparición.
    /// El patrón busca símbolos de la forma: [FAB](...)(#\d+)? y reemplaza el sufijo por "#".
    /// </summary>
    public void RecalculateUniqueIds()
    {
        // Opcional: Reiniciar el contador de IDs (si lo deseas)
        // Deriver.ResetIdCounter();

        // El patrón asume que los símbolos son del tipo: [FAB]\([^)]*\)(#\d+)?
        Regex regex = new Regex(@"([FAB])\([^)]*\)(#\d+)?");
        LSys = regex.Replace(LSys, match =>
        {
            // Extraer la parte base sin sufijo, si existe.
            string baseSymbol = match.Value;
            int hashIndex = baseSymbol.IndexOf("#");
            if (hashIndex >= 0)
            {
                baseSymbol = baseSymbol.Substring(0, hashIndex);
            }
            int newId = Deriver.GenerateId();
            return baseSymbol + "#" + newId;
        });
    }


    /// <summary>
    /// Actualiza los uniqueId de los SubStructs leyendo la cadena LSys.
    /// Se asume que el orden de aparición de los símbolos en LSys coincide con el de la lista subStructs.
    /// </summary>
    public void UpdateSubStructIdsFromLSys()
    {
        Regex regex = new Regex(@"[FAB]\([^)]*\)#(\d+)");
        MatchCollection matches = regex.Matches(LSys);
        if (matches.Count == 0)
        {
            Debug.LogWarning("UpdateSubStructIdsFromLSys: No se encontraron símbolos con ID en LSys.");
            return;
        }
        int count = Mathf.Min(matches.Count, subStructs.Count);
        for (int i = 0; i < count; i++)
        {
            int newId = int.Parse(matches[i].Groups[1].Value);
            subStructs[i].uniqueId = newId;
        }
    }

    /// <summary>
    /// Destruye todos los SubStructs y limpia la lista.
    /// </summary>
    public void ClearTree()
    {
        foreach (var s in subStructs)
        {
            if (s != null)
                Destroy(s.gameObject);
        }
        subStructs.Clear();
    }

}
