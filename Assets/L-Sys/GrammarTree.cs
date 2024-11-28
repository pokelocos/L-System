using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Utils = ParametrizedUtilities;
using NCalc;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ParametrizedUtilities
{
    /// <summary>
    /// Regresa el contenido dentrov de parentesis, permite que 
    /// existan parentesis anidados.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static (string, int) ExtractFromParentheses(string input)
    {
        if (string.IsNullOrEmpty(input) || input[0] != '(')
            return (string.Empty, 0);

        int openParentheses = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '(')
                openParentheses++;
            else if (input[i] == ')')
                openParentheses--;

            if (openParentheses == 0)
            {
                // Excluye los paréntesis externos
                return (input.Substring(1, i - 1), i);
            }
        }

        return (string.Empty, 0);
    }

    /// <summary>
    /// Pregunta al char en un string si esta parametrizada.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool IsParameterized(string input, int index)
    {
        if (index + 1 >= input.Length)
            return false;

        return input[index + 1] == '(';
    }

    /// <summary>
    /// Recive un string que contine multiples funciones,
    /// retorna una lista de tuplas, donde cada tupla contiene
    /// el nombre de la funcion y una array de parametros.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static List<(string, string[])> GetParams(string input)
    {
        var toR = new List<(string, string[])>();

        string pattern = @"\((.*?)\)";
        var matches = Regex.Matches(input, pattern);

        foreach (Match match in matches)
        {
            string functionName = match.Groups[1].Value;
            string[] parameters = match.Groups[2].Value.Replace(" ","").Split(',');

            toR.Add((functionName, parameters));
        }

        return toR;
    }

    public static (List<string>, int) GetParams(string input, int index)
    {
        for (int j = index; j < input.Length; j++)
        {
            if (input[j] == ')')
            {
                var sub = input.Substring(index, j - index).Split(',');
                return (sub.ToList(), j);
            }
        }

        return (null, index);
    }
}

[CreateAssetMenu(fileName = "New Grammar Tree", menuName = "Create Grammar Tree")]
public class GrammarTree : ScriptableObject
{
    [System.Serializable]
    public class GeneralVariables
    {
        public string variable;
        public float value;
    }

    [System.Serializable]
    public class Rule
    {
        public string input;
        public string output;
        public string condition;
        public float weight = 1f;

        public Rule(string input, string output)
        {
            this.input = input;
            this.output = output;
        }

        public bool CheckCondition(string[] currentParams, List<GeneralVariables> generalVariables)
        {
            if (string.IsNullOrEmpty(condition) || condition == "")
                return true;

            // Init expression
            Expression expression = new Expression(condition);

            // Add all paremeters from condition
            var iSub = input.Substring(1);
            var (a, b) = Utils.ExtractFromParentheses(iSub);
            var iParms = a.Split(';');
            for (int k = 0; k < iParms.Length; k++)
            {
                expression.Parameters[iParms[k]] = float.Parse(currentParams[k]);
            }

            // Add all parameters from general variables
            foreach (var g in generalVariables)
            {
                expression.Parameters.Add(g.variable, g.value);
            }

            // Evaluate the expression
            var result = (bool)expression.Evaluate();
            return result;
        }

        public string CalcOutput(string[] currentParams, List<GeneralVariables> generalVariables)
        {
            var toR = "";

            // Get all the parameters from RULE INPUT ecuation
            var iSub = input.Substring(1);
            var (a, b) = Utils.ExtractFromParentheses(iSub);
            var iParms = a.Split(';'); 

            for (int i = 0; i < output.Length; i++)
            {
                if (Utils.IsParameterized(output, i))
                {
                    toR += output[i] + "(";

                    // Get all the ecuations from RULE OUTPUT ecuation
                    var oSub = output.Substring(i + 1);
                    var (parms, end) = Utils.ExtractFromParentheses(oSub);
                    var oParms = parms.Split(';');

                    for (int j = 0; j < oParms.Length; j++)
                    {
                        // Generate experssion for each output parameter
                        Expression expression = new Expression(oParms[j]);

                        // Add all paremeters from current ecuation
                        for (int k = 0; k < iParms.Length; k++)
                        {
                            expression.Parameters[iParms[k]] = float.Parse(currentParams[k]);
                        }

                        // Add all parameters from general variables
                        foreach (var g in generalVariables)
                        {
                            expression.Parameters.Add(g.variable, g.value);
                        }

                        // Evaluate the expression
                        var result = expression.Evaluate();

                        var res = Convert.ToSingle(result);
                        toR += res + ";";
                    }

                    toR = toR.Remove(toR.Length - 1);
                    toR += ")";

                    i += (end + 1);
                }
                else
                {
                    toR += output[i];
                }

            }

            return toR;
        }

    }

    public List<Rule> rules;
    public List<GeneralVariables> generalVariables;
    public bool isStochastic = false;

    public List<Rule> GetRules(char c) //, (string,int) contex)
    {
        List<Rule> output = new();

        foreach (var rule in rules)
        {
            // FIX: Esto no concidera ni rules con contexto
            if (rule.input[0] == c) 
            {
                output.Add(rule);
            }
        }

        return output;
    }
}

/*
#if UNITY_EDITOR
[CustomEditor(typeof(GrammarTree))]
public class GrammarTreeEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var tree = (GrammarTree)target;

        // Add rules
        var ruleList = new ListView(tree.rules, 20, () => new VisualElement());


        return root;
    }
}
#endif
*/