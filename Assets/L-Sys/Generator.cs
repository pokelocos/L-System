using Antlr.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils = ParametrizedUtilities;

[System.Serializable]
public class Generator
{
    [System.Serializable]
    public class PrefPairs
    {
        public string name;
        public List<GameObject> prefs;
    }
    public float alphaAngle = 45f;
    public float betaAngle = 60f;
    public float segmentSize = 1f;
    public List<PrefPairs> prefPairs = new();
    /// <summary>
    /// Cada acción retorna el nuevo SubStruct creado (o el mismo 'last' si no se crea nada nuevo).
    /// </summary>
    [System.Serializable]
    public class GenerationAction
    {
        public char variable;
        public Func<int, Transform, Stack<tortoiseData>, TreeStruct, SubStruct, List<float>, SubStruct> action;
    }
    public List<GenerationAction> generationAction = new();
    /// <summary>
    /// Instancia y configura un segmento (prefab) para el símbolo dado, y devuelve el SubStruct creado.
    /// </summary>
    public SubStruct SimpleGeneration(
        int index,
        Transform tortoise,
        Stack<tortoiseData> pushdown,
        TreeStruct tree,
        SubStruct last,
        List<float> _params,
        string value)
    {
        // Valores predeterminados en caso de que no se provean parámetros
        float width = 1f;               // Valor por defecto para el grosor
        float length = segmentSize;     // Valor por defecto para la longitud

        // Si se proporcionan parámetros y son al menos 2, asume:
        // - _params[0]: ancho (w)
        // - _params[1]: longitud (L)
        if (_params != null && _params.Count >= 2)
        {
            width = _params[0];
            length = _params[1];
        }

        // Buscar el prefab asociado al símbolo (por ejemplo, "F", "A", etc.)
        var tPart = prefPairs.FirstOrDefault(t => t.name == value);
        if (tPart == null || tPart.prefs.Count == 0)
        {
            Debug.LogWarning($"No Prefab found for value: {value}");
            return last;
        }

        // Instanciar el prefab en la posición actual de la tortuga.
        var part = GameObject.Instantiate(tPart.prefs[0], tortoise.position, Quaternion.identity);
        part.transform.SetParent(last.transform, worldPositionStays: true);

        // Asegurarse de que el objeto padre tenga escala (1,1,1)
        part.transform.localScale = Vector3.one;
        // Acceder al primer hijo del prefab y asignarle la escala deseada:
        // Se asigna 'width' a X y Y, y 'length' a Z.
        Transform child = part.transform.GetChild(0);
        child.localScale = new Vector3(width, width, length);

        // Alinear el prefab de modo que su eje Z (forward) se oriente según la dirección de la tortuga.
        part.transform.up = tortoise.up;

        // Agregar el componente SubStruct y guardar los parámetros si se requiere.
        var sub = part.AddComponent<SubStruct>();
        sub.size = length;
        sub.parent = tree;
        tree.subStructs.Add(sub);

        // Calcular el desplazamiento real basado en la longitud del objeto hijo (eje Z):
        Renderer rend = child.GetComponent<Renderer>();
        if (rend != null)
        {
            float actualLength = rend.bounds.size.y;
            // Mover la tortuga en la dirección en la que se ha orientado el prefab.
            tortoise.position += tortoise.up * actualLength;
        }
        else
        {
            // Sino, usar la longitud definida.
            tortoise.position += tortoise.up * length;
        }

        return sub;
    }




    /// <summary>
    /// Inicializa las acciones para cada símbolo.
    /// </summary>
    public List<GenerationAction> InitGenerateAction()
    {
        var toR = new List<GenerationAction>();

        // Rotaciones: actualizan la tortuga y devuelven 'last' sin crear nuevos segmentos.
        toR.AddRange(new[]
        {
            new GenerationAction() {
                variable = '+',
                action = (i, tortoise, pushdown, tree, last, _params) => {
                    float angle = (_params.Count > 0) ? _params[0] : UnityEngine.Random.Range(15f, 45f);
                    tortoise.Rotate(tortoise.forward, angle);
                    return last;
                }
            },
            new GenerationAction() {
                variable = '-',
                action = (i, tortoise, pushdown, tree, last, _params) => {
                    float angle = (_params.Count > 0) ? _params[0] : UnityEngine.Random.Range(15f, 45f);
                    tortoise.Rotate(tortoise.forward, -angle);
                    return last;
                }
            },
            new GenerationAction() {
                variable = '&',
                action = (i, tortoise, pushdown, tree, last, _params) => {
                    float angle = (_params.Count > 0) ? _params[0] : UnityEngine.Random.Range(15f, 45f);
                    tortoise.Rotate(tortoise.right, angle);
                    return last;
                }
            },
            new GenerationAction() {
                variable = '^',
                action = (i, tortoise, pushdown, tree, last, _params) => {
                    float angle = (_params.Count > 0) ? _params[0] : UnityEngine.Random.Range(15f, 45f);
                    tortoise.Rotate(tortoise.right, -angle);
                    return last;
                }
            },
            new GenerationAction() {
                variable = '\\',
                action = (i, tortoise, pushdown, tree, last, _params) => {
                    float angle = (_params.Count > 0) ? _params[0] : UnityEngine.Random.Range(60f, 60f);
                    tortoise.Rotate(tortoise.forward, angle);
                    return last;
                }
            },
            new GenerationAction() {
                variable = '/',
                action = (i, tortoise, pushdown, tree, last, _params) => {
                    float angle = (_params.Count > 0) ? _params[0] : UnityEngine.Random.Range(60f, 60f);
                    tortoise.Rotate(tortoise.forward, -angle);
                    return last;
                }
            }
        });

        // Símbolos que generan un segmento: usamos SimpleGeneration.
        var symbols = new[] { 't', 'g', 'f', 'r', 'h', 'p' };
        foreach (var symbol in symbols)
        {
            toR.Add(new GenerationAction()
            {
                variable = symbol,
                action = (i, tortoise, pushdown, tree, last, exps) => {
                    return SimpleGeneration(i, tortoise, pushdown, tree, last, exps, symbol.ToString());
                }
            });
        }

        // Acciones explícitas para F, A, B y C (con C usando la lógica de B).
        toR.Add(new GenerationAction()
        {
            variable = 'F',
            action = (i, t, p, tr, l, exps) => {
                return SimpleGeneration(i, t, p, tr, l, exps, "F");
            }
        });
        toR.Add(new GenerationAction()
        {
            variable = 'A',
            action = (i, t, p, tr, l, exps) => {
                return SimpleGeneration(i, t, p, tr, l, exps, "A");
            }
        });
        toR.Add(new GenerationAction()
        {
            variable = 'B',
            action = (i, t, p, tr, l, exps) => {
                return SimpleGeneration(i, t, p, tr, l, exps, "B");
            }
        });
        toR.Add(new GenerationAction()
        {
            variable = 'C',
            action = (i, t, p, tr, l, exps) => {
                return SimpleGeneration(i, t, p, tr, l, exps, "B");
            }
        });

        // Corchetes: push y pop de la tortuga
        toR.AddRange(new[]
        {
            new GenerationAction() {
                variable = '[',
                action = (i, tortoise, pushdown, tree, last, exps) => {
                    pushdown.Push(new tortoiseData() {
                        pos = tortoise.position,
                        dir = tortoise.rotation.eulerAngles,
                        last = last.transform
                    });
                    return last;
                }
            },
            new GenerationAction() {
                variable = ']',
                action = (i, tortoise, pushdown, tree, last, exps) => {
                    if (pushdown.Count > 0)
                    {
                        var t = pushdown.Pop();
                        tortoise.position = t.pos;
                        tortoise.rotation = Quaternion.Euler(t.dir);
                        var sub = t.last.GetComponent<SubStruct>();
                        return sub;
                    }
                    return last;
                }
            }
        });

        return toR;
    }
    /// <summary>
    /// Genera el árbol 3D a partir de la cadena L-System.
    /// Procesa la cadena caracter a caracter. Si detecta un símbolo parametrizado,
    /// extrae sus parámetros y, si hay un sufijo que comience con '#' después de los paréntesis,
    /// lo interpreta como el uniqueId y lo asigna al SubStruct correspondiente.
    /// </summary>
    public TreeStruct GenerateTree(string value, Transform tortoise)
    {
        var pushdown = new Stack<tortoiseData>();

        // Crear el GameObject raíz para el árbol
        var root = new GameObject("Tree");
        var tree = root.AddComponent<TreeStruct>();
        tree.LSys = value;
        Debug.Log("LSys: " + value);

        // 'last' inicial es un SubStruct vacío en el root.
        var last = root.AddComponent<SubStruct>();
        last.parent = tree;
        last.uniqueId = Deriver.GenerateId(); // Asigna un ID al root
        int i = 0;
        while (i < value.Length)
        {
            char v = value[i];
            var actions = generationAction.Where(a => a.variable == v).ToList();

            if (Utils.IsParameterized(value, i))
            {
                // Extrae el contenido entre paréntesis
                string subStr = value.Substring(i + 1);
                var (param, endIndex) = Utils.ExtractFromParentheses(subStr);
                var exps = param.Split(';').Select(float.Parse).ToList();

                // Procesar el símbolo parametrizado
                if (actions.Count > 0 && actions[0].action != null)
                {
                    var newLast = actions[0].action(i, tortoise, pushdown, tree, last, exps);
                    if (newLast != null)
                        last = newLast;
                }

                // Avanzar el índice hasta justo después del cierre de paréntesis
                i += (endIndex + 2); // 1 para la letra, (endIndex + 1) para "(...)" 

                // Procesar sufijos de ID consecutivos (por ejemplo, "#0", "#1", etc.)
                while (i < value.Length && value[i] == '#')
                {
                    i++; // Salta el '#' 
                    int idStart = i;
                    while (i < value.Length && char.IsDigit(value[i]))
                    {
                        i++;
                    }
                    int idLength = i - idStart;
                    if (idLength > 0)
                    {
                        int uniqueId = int.Parse(value.Substring(idStart, idLength));
                        last.uniqueId = uniqueId;
                    }
                }
            }
            else
            {
                // Símbolo no parametrizado
                if (actions.Count > 0 && actions[0].action != null)
                {
                    var newLast = actions[0].action(i, tortoise, pushdown, tree, last, new List<float>());
                    if (newLast != null)
                        last = newLast;
                }
                i++;
            }
        }

        return tree;
    }

    public TreeStruct GenerateTreeOnExisting(TreeStruct existingTree, string value, Transform tortoise)
    {
        var pushdown = new Stack<tortoiseData>();

        // 1) Limpiar el TreeStruct existente (borrar hijos)
        existingTree.subStructs.Clear();
        for (int k = existingTree.transform.childCount - 1; k >= 0; k--)
        {
            Transform child = existingTree.transform.GetChild(k);
            UnityEngine.Object.Destroy(child.gameObject);
        }
        // Actualizar la cadena LSys
        existingTree.LSys = value;
        Debug.Log("LSys: " + value);

        // 2) Destruir el SubStruct del root, si existe.
        var oldLast = existingTree.GetComponent<SubStruct>();
        if (oldLast != null)
            UnityEngine.Object.Destroy(oldLast);

        // 3) Crear un nuevo SubStruct "last" en el mismo GameObject.
        var last = existingTree.gameObject.AddComponent<SubStruct>();
        last.parent = existingTree;
        // 4) Procesar la cadena LSys
        int pos = 0;
        while (pos < value.Length)
        {
            char v = value[pos];
            var actions = generationAction.Where(a => a.variable == v).ToList();

            if (Utils.IsParameterized(value, pos))
            {
                // Extrae el bloque de parámetros
                string subStr = value.Substring(pos + 1);
                var (param, end) = Utils.ExtractFromParentheses(subStr);
                var exps = param.Split(';').Select(float.Parse).ToList();

                if (actions.Count > 0 && actions[0].action != null)
                {
                    var newLast = actions[0].action(pos, tortoise, pushdown, existingTree, last, exps);
                    if (newLast != null)
                        last = newLast;
                }

                // Avanzar pos: 1 (letra) + (end+1) para el bloque entre paréntesis.
                pos += (end + 2);

                // Procesar sufijos de ID consecutivos, si existen.
                while (pos < value.Length && value[pos] == '#')
                {
                    pos++; // Saltar el '#'
                    int idStart = pos;
                    while (pos < value.Length && char.IsDigit(value[pos]))
                    {
                        pos++;
                    }
                    int idLength = pos - idStart;
                    if (idLength > 0)
                    {
                        int uniqueId = int.Parse(value.Substring(idStart, idLength));
                        last.uniqueId = uniqueId;
                    }
                }
            }
            else
            {
                if (actions.Count > 0 && actions[0].action != null)
                {
                    var newLast = actions[0].action(pos, tortoise, pushdown, existingTree, last, new List<float>());
                    if (newLast != null)
                        last = newLast;
                }
                pos++;
            }
        }
        return existingTree;
    }
}
/// <summary>
/// Clase para almacenar la posición, rotación y el "last" transform cuando se hace push/pop.
/// </summary>
public class tortoiseData
{
    public Vector3 pos;
    public Vector3 dir;
    public Transform last;
}
