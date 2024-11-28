using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using UnityEngine.Windows;
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

    private float alphaAngle = 20;
    private float betaAngle = 45;
    private float segmentSize = 1;

    public List<PrefPairs> prefPairs = new();

    [System.Serializable]
    public class GenerationAction
    {
        public char variable;
        public Action<int,Transform, Stack<tortoiseData>, 
            TreeStruct, SubStruct, List<float>> action;
    }

    public List<GenerationAction> generationAction = new();

    public void SimpleGeneration (int index, Transform tortoise, Stack<tortoiseData> pushdown,
            TreeStruct tree, SubStruct last, List<float> _params, string value)
    {
        var segmentSize = _params[0];

        var tPart = prefPairs.Where(t => t.name == value).ToList();

        var part = GameObject.Instantiate(tPart[0].prefs[0], tortoise.position + Vector3.zero, Quaternion.identity);
        
        var sub = part.AddComponent<SubStruct>();
        sub.size = this.segmentSize;
        sub.index = index;
        sub.start = tortoise.position + Vector3.zero;
        sub.dir = tortoise.up + Vector3.zero;
        sub.parent = tree;

        part.transform.parent = last.transform;
        part.transform.up = sub.dir;
        //part.transform.localScale = Vector3.one * (1 + (segmentSize * 0.1f));

        tree.subStructs.Add(sub);
        tortoise.position += tortoise.up * this.segmentSize;
    }

    public List<GenerationAction> InitGenerateAction()
    {
        var toR = new List<GenerationAction>();

        toR.Add(new GenerationAction()
        {
            variable = '+', // rotation
            action = (i, tortoise, pushdown, tree, last, _params) =>
            {
                tortoise.Rotate(tortoise.forward, _params[0]);
            }
        });

        toR.Add(new GenerationAction()
        {
            variable = '-', // rotation
            action = (i, tortoise, pushdown, tree, last, _params) =>
            {
                tortoise.Rotate(tortoise.forward, _params[0]);
            }
        });

        toR.Add(new GenerationAction()
        {
            variable = '&', // rotation
            action = (i, tortoise, pushdown, tree, last, _params) =>
            {
                tortoise.Rotate(tortoise.up, _params[0]);

            }
        });

        toR.Add(new GenerationAction()
        {
            variable = '^', // rotation
            action = (i, tortoise, pushdown, tree, last, _params) =>
            {
                tortoise.Rotate(tortoise.up, _params[0]);
            }
        });

        toR.Add(new GenerationAction()
        {
            variable = 't', // tallo
            action = (i,tortoise, pushdown, tree, last, _params) =>
            {
               SimpleGeneration(i, tortoise, pushdown, tree, last, _params, "t");
            }
        });

        toR.Add(new GenerationAction()
        {
            variable = 'g', // tallo cortado
            action = (i, tortoise, pushdown, tree, last, _params) =>
            {
                SimpleGeneration(i, tortoise, pushdown, tree, last, _params, "g");
            }
        });

        toR.Add(new GenerationAction()
        {
            variable = 'f', // flor
            action = (i, tortoise, pushdown, toR, last, _params) =>
            {
                SimpleGeneration(i, tortoise, pushdown, toR, last, _params, "f");
            }
        });

        toR.Add(new GenerationAction()
        {
            variable = 'r', // tallo cortado
            action = (i, tortoise, pushdown, tree, last, _params) =>
            {
                SimpleGeneration(i, tortoise, pushdown, tree, last, _params, "r");
            }
        });

        toR.Add(new GenerationAction() 
        {
            variable = 'h', // hoja
            action = (i, tortoise, pushdown, toR, last, _params) =>
            {
                SimpleGeneration(i, tortoise, pushdown, toR, last, _params, "h");
            }
        });

        toR.Add(new GenerationAction()
        {
            variable = 'p', // axioma o punta
            action = (i, tortoise, pushdown, toR, last, _params) =>
            {
                SimpleGeneration(i, tortoise, pushdown, toR, last, _params, "p");
            }
        });

        return toR;
    }

    public TreeStruct GenerateTree(string value, Transform tortoise)
    {
        var pushdown = new Stack<tortoiseData>();

        var root = new GameObject("Tree");
        var tree = root.AddComponent<TreeStruct>();
        tree.LSys = value;

        var last = root.AddComponent<SubStruct>(); // parche


        // Iterate over the axiom
        for (int i = 0; i < value.Length; i++)
        {
            var v = value[i];

            if (Utils.IsParameterized(value, i))
            {
                // Extract the parameters from the current character
                var sub = value.Substring(i + 1);
                var (param, end) = Utils.ExtractFromParentheses(sub);
                var exps = param.Split(';');

                // Get all the rules for the current character
                var rules = generationAction.Where(a => a.variable == v).ToList();

                // Get the rule selected to execute
                var rule = rules[0]; // FIX: esto solo conidera el primer rule
                
                if(rules.Count > 0)
                {
                    // Get the last amount of subStructs
                    var subAmount = tree.subStructs.Count();

                    // Execute the action of the rule
                    rule.action?.Invoke(i, tortoise, pushdown, tree, last,
                        exps.Select(e => float.Parse(e)).ToList());

                    // If the amount of subStructs has changed, get the last one
                    if (subAmount != tree.subStructs.Count())
                        last = tree.subStructs[subAmount];
                }

                // Jump to the end of the parameters
                i += (end + 1);
            }
            else
            {
                if(v == '[')
                {
                    pushdown.Push(new tortoiseData() { 
                        pos = tortoise.position + Vector3.zero,
                        dir = tortoise.rotation.eulerAngles,
                        last = last.transform
                    });
                }
                else if (v == ']')
                {
                    var t = pushdown.Pop();
                    tortoise.position = t.pos;
                    tortoise.rotation = Quaternion.Euler(t.dir);
                    last = t.last.GetComponent<SubStruct>();
                }

                if (v == '+')
                {
                    tortoise.Rotate(tortoise.forward, alphaAngle);
                }
                else if (v == '-')
                {
                    tortoise.Rotate(tortoise.forward, -alphaAngle);
                }

                if (v == '&')
                {
                    tortoise.Rotate(tortoise.up, betaAngle);
                }
                else if (v == '^')
                {
                    tortoise.Rotate(tortoise.up, -betaAngle);
                }
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

/*
public class TreeTurlte : MonoBehaviour
{

    public class GenerationAction
    {
        public char variable;
        public Func<Vector3, Vector3, List<object>, treeStruct> rule;
    }

    private static List<GenerationAction> generationRules = new();


    public static List<GenerationAction> InitGeneration()
    {
        var toR = new List<GenerationAction>();

        // Init generation rules
        generationRules.Add(new GenerationAction()
        {
            variable = 'f',
            rule = (pos, dir, _params) =>
            {
                var dist = 1;
                return new treeStruct() { start = pos, end = pos + (dir * dist) };
            }
        });
        generationRules.Add(new GenerationAction()
        {
            variable = '+',
            rule = (pos, dir, _params) =>
            {
                return null;
            }
        });
        generationRules.Add(new GenerationAction()
        {
            variable = '-',
            rule = (pos, dir, _params) =>
            {
                return null;
            }
        });
        generationRules.Add(new GenerationAction()
        {
            variable = '[',
            rule = (pos, dir, _params) =>
            {
                Generator.pushdown.Push(new tutleData() { pos = pos, dir = dir });
                return null;
            }
        });
        generationRules.Add(new GenerationAction()
        {
            variable = ']',
            rule = (pos, dir, _params) =>
            {
                return null;
            }
        });
        generationRules.Add(new GenerationAction()
        {
            variable = '&',
            rule = (pos, dir, _params) =>
            {
                return null;
            }
        });
        generationRules.Add(new GenerationAction()
        {
            variable = '^', // FIX?: cambiar por un simbolo mas intuitivo
            rule = (pos, dir, _params) =>
            {
                return null;
            }
        });
        generationRules.Add(new GenerationAction()
        {
            variable = '/',
            rule = (pos, dir, _params) =>
            {
                return null;
            }
        });
        generationRules.Add(new GenerationAction()
        {
            variable = '\',
            rule = (pos, dir, _params) =>
            {
                return null;
            }
        });


        return toR;
    }





    // Start is called before the first frame update
    void Start()
    {


        // init turtle
        pos = transform.position + Vector3.zero;
        dir = transform.forward + Vector3.zero;

        var strg = IterateString(startInput);
        structs = ReadString(strg);
    }

    private string IterateString(string input)
    {
        var output = input;

        // implementar

        return output;
    }

    private bool IsParameterized(string input, int index)
    {
        return input[index + 1] == '(';
    }

    private (List<object>,int) GetParams(string input, int index)
    {
        for (int j = index; j < input.Length; j++)
        {
            if (input[j] == ')')
            {
                var sub = input.Substring(index, j - index).Split(',');
                List<object> parameters = new();
                parameters.AddRange(sub); // FIX: esto se esta pasando como string, hay que convertirlo a su respectivo valor
                return (parameters, j);
            }
        }

        return (null, index);
    }

    private List<treeStruct> ReadString(string input)
    {
        var structs = new List<treeStruct>();

        var last = ' ';
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '[')
            {
                pushdown.Push(new tutleData() { pos = pos, dir = dir });
            }
            else if(input[i] == ']')
            {
                var p = pushdown.Pop();
                pos = p.pos;
                dir = p.dir;
            }
            else if (generationRules.Where( gr => gr.variable == input[i]) is List<GenerationRule> rules  
                && rules.Count() > 0)
            {
                if (IsParameterized(input,i)) //  check of the current rule is parameterized
                {
                    var _params = GetParams(input, i);
                    i = _params.Item2;
                    rules[0].rule?.Invoke(pos, dir, _params.Item1);
                }
                else
                {
                    rules[0].rule?.Invoke(pos, dir, null);
                }
            }
            last = input[i];
        }

        // implementar

        // +Turn left by angle δ, using rotation matrix RU(δ).
        // − Turn right by angle δ, using rotation matrix RU(−δ).
        // & Pitch down by angle δ, using rotation matrix RL(δ).
        // ∧ Pitch up by angle δ, using rotation matrix RL(−δ).
        // \ Roll left by angle δ, using rotation matrix RH(δ).
        // / Roll right by angle δ, using rotation matrix RH(−δ).
        // | Turn around, using rotation matrix RU(180).

        return structs;
    }

    private void OnDrawGizmos()
    {
        // Draw the turtle
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pos, 0.1f);

        // Draw the structs
        Gizmos.color = Color.green;
        foreach (var s in structs)
        {
            s.DrawGizmos();
        }
    }
}
*/
