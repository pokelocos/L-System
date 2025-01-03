using System;
using System.Collections;
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

    private float alphaAngle = 45f; // Ángulo de rotación para '+'
    private float betaAngle = 45f;  // Ángulo de rotación para '&'
    private float segmentSize = 1f; // Tamaño de un segmento

    public List<PrefPairs> prefPairs = new();

    [System.Serializable]
    public class GenerationAction
    {
        public char variable;
        public Action<int, Transform, Stack<tortoiseData>, TreeStruct, SubStruct, List<float>> action;
    }

    public List<GenerationAction> generationAction = new();

    /// <summary>
    /// Genera un segmento basado en un prefab.
    /// </summary>
    public void SimpleGeneration(int index, Transform tortoise, Stack<tortoiseData> pushdown,
                                 TreeStruct tree, SubStruct last, List<float> _params, string value)
    {
        var segmentSize = _params.Count > 0 ? _params[0] : this.segmentSize;

        var tPart = prefPairs.FirstOrDefault(t => t.name == value);
        if (tPart == null || tPart.prefs.Count == 0)
        {
            Debug.LogWarning($"No Prefab found for value: {value}");
            return;
        }

        var part = GameObject.Instantiate(tPart.prefs[0], tortoise.position, Quaternion.identity);

        var sub = part.AddComponent<SubStruct>();
        sub.size = segmentSize;
        sub.index = index;
        sub.start = tortoise.position;
        sub.dir = tortoise.up;
        sub.parent = tree;

        part.transform.parent = last.transform;
        part.transform.up = sub.dir;

        tree.subStructs.Add(sub);
        tortoise.position += tortoise.up * segmentSize;
    }

    /// <summary>
    /// Inicializa las acciones para los símbolos del L-System.
    /// </summary>
    public List<GenerationAction> InitGenerateAction()
    {
        var toR = new List<GenerationAction>();

        // Rotaciones
        toR.AddRange(new[]
        {
            new GenerationAction()
            {
                variable = '+',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                    tortoise.Rotate(tortoise.forward, _params.Count > 0 ? _params[0] : alphaAngle)
            },
            new GenerationAction()
            {
                variable = '-',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                    tortoise.Rotate(tortoise.forward, _params.Count > 0 ? -_params[0] : -alphaAngle)
            },
            new GenerationAction()
            {
                variable = '&',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                    tortoise.Rotate(tortoise.right, _params.Count > 0 ? _params[0] : betaAngle)
            },
            new GenerationAction()
            {
                variable = '^',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                    tortoise.Rotate(tortoise.right, _params.Count > 0 ? -_params[0] : -betaAngle)
            }
        });

        // Segmentos y Objetos
        var symbols = new[] { 't', 'g', 'f', 'r', 'h', 'p' };
        foreach (var symbol in symbols)
        {
            toR.Add(new GenerationAction()
            {
                variable = symbol,
                action = (i, t, p, tr, l, _p) => SimpleGeneration(i, t, p, tr, l, _p, symbol.ToString())
            });
        }

        // Movimiento sin generar objetos
        toR.Add(new GenerationAction()
        {
            variable = 'F', // Genera un segmento y avanza
            action = (i, tortoise, pushdown, tree, last, _params) =>
            {
                SimpleGeneration(i, tortoise, pushdown, tree, last, _params, "F");
            }
        });
        toR.Add(new GenerationAction()
        {
            variable = 'A', // Rama principal
            action = (i, tortoise, pushdown, tree, last, _params) =>
            {
                SimpleGeneration(i, tortoise, pushdown, tree, last, _params, "A");
            }
        });

        toR.Add(new GenerationAction()
        {
            variable = 'B', // Rama secundaria
            action = (i, tortoise, pushdown, tree, last, _params) =>
            {
                SimpleGeneration(i, tortoise, pushdown, tree, last, _params, "B");
            }
        });

        // Guardar y Restaurar Estado
        toR.AddRange(new[]
        {
            new GenerationAction()
            {
                variable = '[',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                    pushdown.Push(new tortoiseData()
                    {
                        pos = tortoise.position,
                        dir = tortoise.rotation.eulerAngles,
                        last = last.transform
                    })
            },
            new GenerationAction()
            {
                variable = ']',
                action = (i, tortoise, pushdown, tree, last, _params) =>
                {
                    if (pushdown.Count > 0)
                    {
                        var t = pushdown.Pop();
                        tortoise.position = t.pos;
                        tortoise.rotation = Quaternion.Euler(t.dir);
                        last = t.last.GetComponent<SubStruct>();
                    }
                }
            }
        });

        return toR;
    }

    /// <summary>
    /// Genera un árbol basado en una cadena derivada del sistema L-System.
    /// </summary>
    public TreeStruct GenerateTree(string value, Transform tortoise)
    {
        var pushdown = new Stack<tortoiseData>();

        var root = new GameObject("Tree");
        var tree = root.AddComponent<TreeStruct>();
        tree.LSys = value;

        var last = root.AddComponent<SubStruct>();

        for (int i = 0; i < value.Length; i++)
        {
            var v = value[i];
            var rules = generationAction.Where(a => a.variable == v).ToList();

            if (Utils.IsParameterized(value, i))
            {
                var sub = value.Substring(i + 1);
                var (param, end) = Utils.ExtractFromParentheses(sub);
                var exps = param.Split(';').Select(e => float.Parse(e)).ToList();

                if (rules.Count > 0)
                    rules[0].action?.Invoke(i, tortoise, pushdown, tree, last, exps);

                i += (end + 1);
            }
            else if (rules.Count > 0)
            {
                rules[0].action?.Invoke(i, tortoise, pushdown, tree, last, new List<float>());
            }
        }

        return tree;
    }
}

public class tortoiseData
{
    public Vector3 pos;
    public Vector3 dir;
    public Transform last;
}
