using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils = ParametrizedUtilities;

public static class Deriver
{
    private static int nextId = 0;
    public static int GenerateId()
    {
        return nextId++;
    }
    /// <summary>
    /// Resetea el contador de ID, útil para recalcular IDs tras un corte.
    /// </summary>
    public static void ResetIdCounter()
    {
        nextId = 0;
    }
    public static List<string> Derive(GrammarTree grammar, string axiom, int amount)
    {
        ResetIdCounter();
        List<string> generations = new List<string>();
        string current = axiom;
        generations.Add(current);

        for (int i = 0; i < amount; i++)
        {
            string outputGen = "";
            for (int j = 0; j < current.Length; j++)
            {
                if (Utils.IsParameterized(current, j))
                {
                    string substring = current.Substring(j + 1);
                    var (parameters, endIndex) = Utils.ExtractFromParentheses(substring);
                    var exps = parameters.Split(';');
                    var rules = grammar.GetRules(current[j]);
                    rules = rules.Where(rule => rule.CheckCondition(exps, grammar.generalVariables)).ToList();

                    if (rules.Count <= 0)
                    {
                        outputGen += current.Substring(j, 1 + (endIndex + 1));
                    }
                    else
                    {
                        var rule = grammar.isStochastic ? rules.RandomRullete(r => r.weight) : rules[0];
                        string result = rule.CalcOutput(exps, grammar.generalVariables);
                        // Aquí se asume que CalcOutput ya añade el sufijo "#ID" al final
                        outputGen += result;
                    }
                    j += (endIndex + 1);
                }
                else
                {
                    var rules = grammar.GetRules(current[j]);
                    if (rules.Count <= 0)
                    {
                        outputGen += current[j];
                    }
                    else
                    {
                        outputGen += rules[0].output;
                    }
                }
            }
            generations.Add(outputGen);
            current = outputGen;
        }
        return generations;
    }
}
