using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeStruct : MonoBehaviour
{
    // Aquí guardas la cadena L-System correspondiente
    [TextArea]
    public string LSys = "";

    // Lista de substructs (segmentos) que se crearon al generar el árbol
    [HideInInspector]
    public List<SubStruct> subStructs = new();

    private void OnDrawGizmos()
    {
        // Dibuja los pequeños gismos de cada SubStruct (líneas)
        foreach (var s in subStructs)
        {
            //s.DrawGizmos();
        }
    }

    /// <summary>
    /// Método que elimina una porción de la cadena (p.ej. bracket) a partir de un índice,
    /// usando tu lógica de bracketCount.
    /// </summary>
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

            // bracketCount == -1 significa que encontramos un ']' que cierra antes de un '[',
            // interpretando que se completa el bloque a eliminar
            if (bracketCount == -1)
            {
                LSys = LSys.Remove(index, i - index + 1);
                return;
            }
        }

        // Si al final no se alcanzó bracketCount == -1, 
        // significa que no se encontró un cierre ']', y se elimina hasta el final
        LSys = LSys.Substring(0, index);
    }


    /// <summary>
    /// Elimina un símbolo paramétrico del tipo B(...) o F(...) en la cadena LSys,
    /// asumiendo que LSys[index] es la letra (p.ej. 'B').
    /// Si encuentra '(', buscará la ')' emparejada y quitará todo el bloque "B(...)". 
    /// Si no la encuentra, elimina hasta final.
    /// </summary>
    public void RemoveParamSymbol(int index)
    {
        // Validar
        if (index < 0 || index >= LSys.Length) return;

        // Letras típicas que tienen parámetros, e.g. 'B', 'F', 'A' si así lo usas
        char symbol = LSys[index];
        // (Opcional) Verificar que sea un símbolo paramétrico real:
        // if (symbol != 'B' && symbol != 'F') return;  // ajusta a tu gusto

        // Contar cuántos caracteres consumiremos
        int lengthToRemove = 1; // al menos la letra
        int next = index + 1;
        if (next < LSys.Length && LSys[next] == '(')
        {
            // Buscar la ')'
            int parenCount = 1;
            for (int i = next + 1; i < LSys.Length; i++)
            {
                if (LSys[i] == '(') parenCount++;
                else if (LSys[i] == ')') parenCount--;

                if (parenCount == 0)
                {
                    // 'i' apunta al ')'
                    lengthToRemove = i - index + 1;
                    break;
                }
            }

            // Si parenCount > 0, no se cerró => remove hasta final
            if (parenCount > 0)
            {
                lengthToRemove = LSys.Length - index;
            }
        }

        // Elimina [index..index + lengthToRemove)
        LSys = LSys.Remove(index, lengthToRemove);
    }


    /// <summary>
    /// (Opcional) Si quieres limpiar el árbol y substructs antes de volver a regenerarlo,
    /// podrías usar un método así.
    /// </summary>
    public void ClearTree()
    {
        // Destruir SubStructs de la escena (opcional).
        foreach (var s in subStructs)
        {
            if (s != null)
                Destroy(s.gameObject);
        }
        subStructs.Clear();

        // Podrías reiniciar la cadena, o no, según tu lógica:
        // LSys = "";
    }



    /// <summary>
    /// Elimina bloques de corchetes [ ... ] que 
    /// no contengan ninguno de los símbolos generadores (F, A, B, etc.).
    /// </summary>
    public void CleanOrphanBrackets()
    {
        bool changed = true;
        // Repetir mientras se vayan eliminando bloques
        while (changed)
        {
            changed = false;
            int i = 0;

            while (i < LSys.Length)
            {
                if (LSys[i] == '[')
                {
                    // Comenzamos un bloque
                    int startIndex = i;
                    int bracketCount = 1;
                    int j = i + 1;

                    // Buscar la ']' que empareja
                    for (; j < LSys.Length && bracketCount > 0; j++)
                    {
                        if (LSys[j] == '[') bracketCount++;
                        else if (LSys[j] == ']') bracketCount--;
                    }

                    // Si bracketCount == 0, se cerró el bloque
                    if (bracketCount == 0)
                    {
                        int endIndex = j - 1; // posición del ']'
                                              // Extraemos el contenido entre [ y ]
                        string inside = LSys.Substring(startIndex + 1, endIndex - (startIndex + 1));

                        // Revisar si tiene algún símbolo generador
                        bool hasGenerator = ContainsGeneratorSymbol(inside);

                        if (!hasGenerator)
                        {
                            // Borrar todo el bloque [startIndex..endIndex]
                            LSys = LSys.Remove(startIndex, endIndex - startIndex + 1);
                            changed = true;
                            break; // Reiniciar el proceso
                        }
                        else
                        {
                            // Si sí tiene generadores, no lo borramos
                            // Avanzar i para no re-analizar este bloque
                            i = startIndex + 1;
                        }
                    }
                    else
                    {
                        // No se cerró => no hacemos nada o quitas desde startIndex al final
                        // Para no complicar, salimos del while
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
    /// Verifica si en la subcadena aparece alguno de los símbolos generadores
    /// que consideras (F, A, B, etc.).
    /// Ajusta según tus necesidades.
    /// </summary>
    private bool ContainsGeneratorSymbol(string content)
    {
        // Ajusta el set/array de símbolos que consideras "generadores".
        char[] generators = { 'F', 'A', 'B' };
        // Si usas minúsculas o más letras, agrégalas aquí.

        foreach (char c in content)
        {
            if (generators.Contains(c))
                return true;
        }
        return false;
    }



}
