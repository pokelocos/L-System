using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;
using Utils = ParametrizedUtilities;

public static class CollectionUtilities
{
    public static T RandomRullete<T>( this IEnumerable<T> collection, Func<T,float> aa)
    {
        var max = 0f;

        foreach (var item in collection)
            max += aa(item);

        var random = UnityEngine.Random.Range(0f, max);

        var acumulated = 0f;
        foreach (var item in collection)
        {
            acumulated += aa(item);
            if (random <= acumulated)
            {
                return item;
            }
        }

        return default(T);
    }
}

public static class Deriver
{
    public static List<string> Derive(GrammarTree grammar, string axiom, int amount)
    {
        Stack<tortoiseData> pushdown = new();
        List<string> generations = new();

        var current = axiom;
        generations.Add(current);

        // Iterate over the amount of generations
        for (int i = 0; i < amount; i++)
        {
            var outputGen = "";

            // Iterate over the char in the current generation
            for (int j = 0; j < current.Length; j++)
            {
                // Check if the current character is parameterized
                if (Utils.IsParameterized(current, j))
                {
                    // Extract the parameters from the current character
                    var sub = current.Substring(j + 1);
                    var (parm, end) = Utils.ExtractFromParentheses(sub);
                    var exps = parm.Split(';');

                    // Get all the rules for the current character
                    var rules = grammar.GetRules(current[j]);

                    // Check if the rules are valid
                    rules = rules.Where(x => x.CheckCondition(exps,grammar.generalVariables)).ToList();

                    if (rules.Count <= 0)
                    {
                        outputGen += current.Substring(j, 1 + (end + 1));
                    }
                    else
                    {

                         var rule = grammar.isStochastic? rules.RandomRullete(x => x.weight) : rules[0];

                        var _out = rule.CalcOutput(exps, grammar.generalVariables);
                        outputGen += _out;
                    }

                    j += (end + 1);
                }
                else
                {
                    // Get all the rules for the current character
                    var rules = grammar.GetRules(current[j]);
                    if (rules.Count <= 0)
                    {
                        outputGen += current[j];
                    }
                    else
                    {
                        outputGen += rules[0].output; // FIX: esto solo conidera el primer rule
                    }
                }
            }

            // Add the output generation to the list
            generations.Add(outputGen);
            current = outputGen;

        }

        return generations;
    }
}