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

    // Ángulos y segmentSize expuestos en el Inspector
    public float alphaAngle = 45f;
    public float betaAngle = 60f;
    public float segmentSize = 1f;

    public List<PrefPairs> prefPairs = new();

    /// <summary>
    /// Generación de acciones. Cambiamos para que devuelvan SubStruct 
    /// (en lugar de 'void'), así podemos reasignar 'last'.
    /// </summary>
    [System.Serializable]
    public class GenerationAction
    {
        public char variable;

        // OJO: en vez de Action<...>, lo cambiamos a Func<...> que retorna SubStruct.
        public Func<int, Transform, Stack<tortoiseData>, TreeStruct, SubStruct, List<float>, SubStruct> action;
    }

    public List<GenerationAction> generationAction = new();

    /// <summary>
    /// Cambiamos la firma: en vez de 'void SimpleGeneration(...)',
    /// la hacemos 'SubStruct SimpleGeneration(...)' y retornamos el nuevo SubStruct.
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
        float segSize = this.segmentSize;

        // Busca el Prefab correspondiente (F, A, B, etc.)
        var tPart = prefPairs.FirstOrDefault(t => t.name == value);
        if (tPart == null || tPart.prefs.Count == 0)
        {
            Debug.LogWarning($"No Prefab found for value: {value}");
            return last;  // Retornamos el 'last' original si no hallamos prefab
        }

        // Instanciamos el prefab en la posición actual de la tortuga
        var part = GameObject.Instantiate(
            tPart.prefs[0],
            tortoise.position,
            Quaternion.identity
        );

        // Lo hacemos hijo del transform de 'last'
        part.transform.SetParent(last.transform, worldPositionStays: true);

        // Ajuste de escala local
        part.transform.localScale = Vector3.one;
        var newScale = part.transform.localScale;
        newScale.y = segSize;
        part.transform.localScale = newScale;

        // Orientación: up = dirección de la tortuga
        part.transform.up = tortoise.up;

        // Creamos el SubStruct
        var sub = part.AddComponent<SubStruct>();
        sub.size = segSize;
        sub.index = index;
        sub.start = tortoise.position;
        sub.dir = tortoise.up;
        sub.parent = tree;

        // Lo añadimos al árbol
        tree.subStructs.Add(sub);

        // Movemos la tortuga hacia arriba segSize
        tortoise.position += tortoise.up * segSize;

        // Retornamos el nuevo SubStruct para reasignar 'last'
        return sub;
    }

    /// <summary>
    /// Inicializa las acciones de cada símbolo. 
    /// Ahora usamos Func<...> que retorna SubStruct, 
    /// en lugar de Action<...>.
    /// </summary>
    public List<GenerationAction> InitGenerateAction()
    {
        var toR = new List<GenerationAction>();

        // Aquí definimos las rotaciones (+, -, &, ^, \, /) 
        // que simplemente devuelven el 'last' sin cambiarlo 
        // (porque no crean un segmento).
        toR.AddRange(new[]
        {
            new GenerationAction()
            {
                variable = '+',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                {
                    float angle = (_params.Count > 0)
                        ? _params[0]
                        : UnityEngine.Random.Range(15f, 45f);
                    tortoise.Rotate(tortoise.forward, angle);

                    // No se crea un nuevo segmento, devolvemos el 'last' tal cual
                    return last;
                }
            },
            new GenerationAction()
            {
                variable = '-',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                {
                    float angle = (_params.Count > 0)
                        ? _params[0]
                        : UnityEngine.Random.Range(15f, 45f);
                    tortoise.Rotate(tortoise.forward, -angle);
                    return last;
                }
            },
            new GenerationAction()
            {
                variable = '&',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                {
                    float angle = (_params.Count > 0)
                        ? _params[0]
                        : UnityEngine.Random.Range(15f, 45f);
                    tortoise.Rotate(tortoise.right, angle);
                    return last;
                }
            },
            new GenerationAction()
            {
                variable = '^',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                {
                    float angle = (_params.Count > 0)
                        ? _params[0]
                        : UnityEngine.Random.Range(15f, 45f);
                    tortoise.Rotate(tortoise.right, -angle);
                    return last;
                }
            },
            new GenerationAction()
            {
                variable = '\\',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                {
                    float angle = (_params.Count > 0)
                        ? _params[0]
                        : UnityEngine.Random.Range(60f, 60f);

                    tortoise.Rotate(tortoise.forward, angle);
                    return last;
                }
            },
            new GenerationAction()
            {
                variable = '/',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                {
                    float angle = (_params.Count > 0)
                        ? _params[0]
                        : UnityEngine.Random.Range(60f, 60f);

                    tortoise.Rotate(tortoise.forward, -angle);
                    return last;
                }
            }
        });

        // Para símbolos que generan un Segmento (F, A, B, etc.), 
        // invocamos SimpleGeneration y reasignamos 'last' a lo retornado.
        // Pero aquí, en la Func, devolvemos directamente lo que retorne SimpleGeneration.
        var symbols = new[] { 't', 'g', 'f', 'r', 'h', 'p' };
        foreach (var symbol in symbols)
        {
            toR.Add(new GenerationAction()
            {
                variable = symbol,
                action = (i, tortoise, pushdown, tree, last, exps) =>
                {
                    // Retornamos el nuevo substruct
                    return SimpleGeneration(i, tortoise, pushdown, tree, last, exps, symbol.ToString());
                }
            });
        }

        toR.Add(new GenerationAction()
        {
            variable = 'F',
            action = (i, t, p, tr, l, exps) =>
            {
                return SimpleGeneration(i, t, p, tr, l, exps, "F");
            }
        });
        toR.Add(new GenerationAction()
        {
            variable = 'A',
            action = (i, t, p, tr, l, exps) =>
            {
                return SimpleGeneration(i, t, p, tr, l, exps, "A");
            }
        });
        toR.Add(new GenerationAction()
        {
            variable = 'B',
            action = (i, t, p, tr, l, exps) =>
            {
                return SimpleGeneration(i, t, p, tr, l, exps, "B");
            }
        });
        toR.Add(new GenerationAction()
        {
            variable = 'C',
            action = (i, t, p, tr, l, exps) =>
            {
                return SimpleGeneration(i, t, p, tr, l, exps, "B");
            }
        });

        // Corchetes para push/pop del estado
        // Al abrir corchete, guardamos pos/rot/last en la pila
        toR.AddRange(new[]
        {
            new GenerationAction()
            {
                variable = '[',
                action = (i, tortoise, pushdown, tree, last, exps) =>
                {
                    pushdown.Push(new tortoiseData()
                    {
                        pos = tortoise.position,
                        dir = tortoise.rotation.eulerAngles,
                        last = last.transform
                    });

                    // '[' no crea un nuevo substruct, así que devolvemos 'last' sin cambios
                    return last;
                }
            },
            new GenerationAction()
            {
                variable = ']',
                action = (i, tortoise, pushdown, tree, last, exps) =>
                {
                    if (pushdown.Count > 0)
                    {
                        var t = pushdown.Pop();
                        tortoise.position = t.pos;
                        tortoise.rotation = Quaternion.Euler(t.dir);

                        // Restauramos 'last' al substruct que había en la pila
                        var sub = t.last.GetComponent<SubStruct>();
                        return sub;
                    }
                    return last;  // Por seguridad, si la pila está vacía
                }
            }
        });

        return toR;
    }

    /// <summary>
    /// Genera el árbol 3D a partir de la cadena L-System.
    /// Ahora recogemos lo que retorne la acción y reasignamos 'last'.
    /// </summary>
    public TreeStruct GenerateTree(string value, Transform tortoise)
    {
        var pushdown = new Stack<tortoiseData>();

        // Creamos el objeto raíz
        var root = new GameObject("Tree");
        var tree = root.AddComponent<TreeStruct>();
        tree.LSys = value;
        Debug.Log(value);
        // 'last' inicial es un SubStruct del root (vacío)
        var last = root.AddComponent<SubStruct>();

        // Recorremos la cadena
        for (int i = 0; i < value.Length; i++)
        {
            char v = value[i];
            var actions = generationAction.Where(a => a.variable == v).ToList();

            if (Utils.IsParameterized(value, i))
            {
                var subStr = value.Substring(i + 1);
                var (param, end) = Utils.ExtractFromParentheses(subStr);

                var exps = param.Split(';').Select(float.Parse).ToList();

                if (actions.Count > 0 && actions[0].action != null)
                {
                    // Guardamos lo que retorna la acción
                    var newLast = actions[0].action(i, tortoise, pushdown, tree, last, exps);
                    if (newLast != null)
                        last = newLast;
                }

                i += (end + 1);
            }
            else
            {
                // No parametrizado => sin exps
                if (actions.Count > 0 && actions[0].action != null)
                {
                    var newLast = actions[0].action(i, tortoise, pushdown, tree, last, new List<float>());
                    if (newLast != null)
                        last = newLast;
                }
            }
        }

        return tree;
    }

    public TreeStruct GenerateTreeOnExisting(TreeStruct existingTree, string value, Transform tortoise)
    {
        var pushdown = new Stack<tortoiseData>();

        // 1) Limpiamos la lista de subStructs anterior
        existingTree.subStructs.Clear();
        for (int i = existingTree.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = existingTree.transform.GetChild(i);
            // Dentro de un script de editor, normalmente se usa DestroyImmediate.
            // En runtime usaría Destroy(child.gameObject).
            UnityEngine.Object.Destroy(child.gameObject);
        }
        // Guardamos la nueva cadena
        existingTree.LSys = value;
        Debug.Log(value);

        // 2) Borramos cualquier SubStruct del root
        // (opcional) si ya existía un "last" colgando
        var oldLast = existingTree.GetComponent<SubStruct>();
        if (oldLast != null) UnityEngine.Object.Destroy(oldLast);

        // 3) Creamos un substruct “last” en el *mismo GameObject* 
        var last = existingTree.gameObject.AddComponent<SubStruct>();

        // 4) Recorremos la cadena
        for (int i = 0; i < value.Length; i++)
        {
            char v = value[i];
            var actions = generationAction.Where(a => a.variable == v).ToList();

            if (Utils.IsParameterized(value, i))
            {
                var subStr = value.Substring(i + 1);
                var (param, end) = Utils.ExtractFromParentheses(subStr);
                var exps = param.Split(';').Select(float.Parse).ToList();

                if (actions.Count > 0 && actions[0].action != null)
                {
                    var newLast = actions[0].action(i, tortoise, pushdown, existingTree, last, exps);
                    if (newLast != null)
                        last = newLast;
                }
                i += (end + 1);
            }
            else
            {
                if (actions.Count > 0 && actions[0].action != null)
                {
                    var newLast = actions[0].action(i, tortoise, pushdown, existingTree, last, new List<float>());
                    if (newLast != null)
                        last = newLast;
                }
            }
        }

        return existingTree;
    }
}

/// <summary>
/// Clase para almacenar la posición y rotación
/// y el 'last' transform cuando abrimos corchetes.
/// </summary>
public class tortoiseData
{
    public Vector3 pos;
    public Vector3 dir;
    public Transform last;
}
