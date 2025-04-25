using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
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

    public static string RecalculateUniqueIds(string lSys)
    {
        ResetIdCounter();
        // Este regex busca símbolos parametrizados que empiecen con F, A, B o G,
        // que tengan contenido entre paréntesis y, opcionalmente, un sufijo "#número".
        Regex regex = new Regex(@"([FABG])\([^)]*\)(#\d+)?");
        string result = regex.Replace(lSys, match =>
        {
            // Toma el símbolo sin el sufijo, si es que existe.
            string baseSymbol = match.Value;
            int hashIndex = baseSymbol.IndexOf("#");
            if (hashIndex >= 0)
            {
                baseSymbol = baseSymbol.Substring(0, hashIndex);
            }
            int newId = GenerateId();
            return baseSymbol + "#" + newId;
        });
        return result;
    }
    public static List<string> Derive(GrammarTree grammar, string axiom, int amount)
    {
        //ResetIdCounter();
        List<string> generations = new List<string>();
        string current = axiom;
        generations.Add(current);
        Debug.Log("Generación 0: " + current);

        for (int i = 0; i < amount; i++)
        {
            string outputGen = "";
            Debug.Log("----- Derivando generación " + (i + 1) + " -----");

            for (int j = 0; j < current.Length; j++)
            {
                char symbol = current[j];
                Debug.Log("Procesando símbolo: '" + symbol + "' en posición " + j);

                if (Utils.IsParameterized(current, j))
                {
                    string substring = current.Substring(j + 1);
                    var (parameters, endIndex) = Utils.ExtractFromParentheses(substring);
                    Debug.Log("Símbolo parametrizado detectado: '" + symbol + "' con parámetros: " + parameters);

                    var exps = parameters.Split(';');
                    var rules = grammar.GetRules(symbol);
                    Debug.Log("Reglas encontradas para '" + symbol + "': " + rules.Count);

                    // Evaluar condición de cada regla
                    var validRules = rules.Where(rule => {
                        bool conditionOk = rule.CheckCondition(exps, grammar.generalVariables);
                        Debug.Log("  Regla: '" + rule.input + "' -> '" + rule.output + "', Condición: '" + rule.condition + "' evaluada como: " + conditionOk);
                        return conditionOk;
                    }).ToList();

                    if (validRules.Count <= 0)
                    {
                        Debug.Log("No se encontró regla válida para el símbolo '" + symbol + "' en posición " + j + ". Se conserva el símbolo original.");
                        outputGen += current.Substring(j, 1 + (endIndex + 1));
                    }
                    else
                    {
                        // Selección de regla: estocástica o determinista.
                        var selectedRule = grammar.isStochastic ? validRules.RandomRullete(r => r.weight) : validRules[0];
                        Debug.Log("Regla seleccionada para '" + symbol + "': '" + selectedRule.input + "' -> '" + selectedRule.output + "'");

                        string result = selectedRule.CalcOutput(exps, grammar.generalVariables);
                        Debug.Log("Resultado de la evaluación: " + result);
                        outputGen += result;
                    }
                    // Avanza el índice para saltar la parte de los parámetros procesados.
                    j += (endIndex + 1);
                }
                else
                {
                    var rules = grammar.GetRules(symbol);
                    if (rules.Count <= 0)
                    {
                        Debug.Log("No hay reglas para el símbolo no parametrizado '" + symbol + "'. Se añade tal cual.");
                        outputGen += symbol;
                    }
                    else
                    {
                        Debug.Log("Símbolo no parametrizado '" + symbol + "' con regla aplicada: '" + rules[0].output + "'");
                        outputGen += rules[0].output;
                    }
                }
            }

            Debug.Log("Generación " + (i + 1) + ": " + outputGen);
            generations.Add(outputGen);
            current = outputGen;
        }
        return generations;
    }
}
