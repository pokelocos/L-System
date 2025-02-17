using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NCalc;  // Se usa para evaluar expresiones lógicas y matemáticas con parámetros.
/// <summary>
/// Representa un conjunto de reglas de un L-System, incluyendo variables globales
/// y la posibilidad de que sea un sistema estocástico.
/// </summary>
[CreateAssetMenu(fileName = "New Grammar Tree", menuName = "Create Grammar Tree")]
public class GrammarTree : ScriptableObject
{
    /// <summary>
    /// Representa una variable global en la gramática, con un nombre (variable) y un valor numérico.
    /// </summary>
    [Serializable]
    public class GeneralVariables
    {
        public string variable;
        public float value;
    }

    /// <summary>
    /// Una regla de producción (o reescritura) en la gramática.
    /// Incluye la cadena de entrada, la cadena de salida, una condición y un peso para estocasticidad.
    /// </summary>
    [Serializable]
    public class Rule
    {
        /// <summary> El símbolo (o secuencia) de entrada que activa la regla. </summary>
        public string input;
        /// <summary> La salida (string) que se producirá cuando se aplique la regla. </summary>
        public string output;
        /// <summary> Expresión lógica que, si está presente, debe cumplirse para aplicar la regla. </summary>
        public string condition;
        /// <summary> Peso usado en gramáticas estocásticas, para seleccionar reglas de forma aleatoria. </summary>
        public float weight = 1f;

        /// <summary>
        /// Constructor por defecto (útil si quieres crear reglas por código).
        /// </summary>
        /// <param name="input">Simbolo de entrada.</param>
        /// <param name="output">Reemplazo (string) de salida.</param>
        public Rule(string input, string output)
        {
            this.input = input;
            this.output = output;
        }

        /// <summary>
        /// Evalúa si la condición (si existe) se cumple con los parámetros dados.
        /// </summary>
        /// <param name="currentParams">Los valores actuales de los parámetros extraídos del símbolo en la cadena.</param>
        /// <param name="generalVariables">Lista de variables globales definidas en la gramática.</param>
        /// <returns>True si se cumple la condición o si no hay condición; False en caso contrario.</returns>
        public bool CheckCondition(string[] currentParams, List<GeneralVariables> generalVariables)
        {
            // Si la regla no tiene condición, la cumple automáticamente.
            if (string.IsNullOrEmpty(condition))
                return true;

            // Creamos una expresión de NCalc con la condición.
            Expression expression = new Expression(condition);

            // Extrae los parámetros definidos en 'input'. 
            // Ej: si input es 'F(x,y)' => iParms = { "x", "y" }
            var iSub = input.Substring(1);
            var (extractedParams, _) = ParametrizedUtilities.ExtractFromParentheses(iSub);
            var iParms = extractedParams.Split(';');

            // Asigna a la expresión los valores de cada parámetro local.
            for (int k = 0; k < iParms.Length; k++)
            {
                float value = float.Parse(currentParams[k]);
                expression.Parameters[iParms[k]] = value;
            }

            // También asigna los valores de las variables globales.
            foreach (var g in generalVariables)
            {
                expression.Parameters[g.variable] = g.value;
            }

            // Evalúa la expresión. Se asume que es booleana (por ej. "x>2 && y<5").
            bool result = (bool)expression.Evaluate();
            return result;
        }

        /// <summary>
        /// Genera la cadena de salida para la regla, evaluando las expresiones 
        /// de los parámetros en 'output' con los valores proporcionados.
        /// </summary>
        /// <param name="currentParams">Los valores actuales de los parámetros para este símbolo.</param>
        /// <param name="generalVariables">Lista de variables globales definidas.</param>
        /// <returns>La cadena resultante tras evaluar cada parámetro.</returns>
        public string CalcOutput(string[] currentParams, List<GeneralVariables> generalVariables)
        {
            string resultString = "";

            // Ej: si input = "F(x,y)", iParms = { "x", "y" }
            var iSub = input.Substring(1);
            var (extractedParams, _) = ParametrizedUtilities.ExtractFromParentheses(iSub);
            var iParms = extractedParams.Split(';');

            // Define aquí qué símbolos paramétricos vas a etiquetar.
            // Por ejemplo, F, B, A, G...
            char[] paramSymbols = { 'F', 'B', 'A', 'G' };

            for (int i = 0; i < output.Length; i++)
            {
                // Chequea si es un símbolo parametrizado en 'output'.
                if (ParametrizedUtilities.IsParameterized(output, i))
                {
                    // Añadimos la letra, ej. 'G'
                    resultString += output[i] + "(";

                    // Extrae el contenido dentro de los paréntesis.
                    var oSub = output.Substring(i + 1);
                    var (parms, end) = ParametrizedUtilities.ExtractFromParentheses(oSub);
                    var oParms = parms.Split(';');

                    // Cada parámetro en 'oParms' es una expresión de NCalc que se evaluará.
                    for (int j = 0; j < oParms.Length; j++)
                    {
                        Expression expression = new Expression(oParms[j]);

                        // Asigna las variables locales.
                        for (int k = 0; k < iParms.Length; k++)
                        {
                            float value = float.Parse(currentParams[k]);
                            expression.Parameters[iParms[k]] = value;
                        }

                        // Asigna las variables globales.
                        foreach (var g in generalVariables)
                        {
                            expression.Parameters[g.variable] = g.value;
                        }

                        // Evalúa el resultado numérico.
                        var evalResult = expression.Evaluate();
                        float evaluatedNumber = Convert.ToSingle(evalResult);

                        // Añade el resultado + ';' para separar parámetros.
                        resultString += evaluatedNumber + ";";
                    }

                    // Elimina el último punto y coma sobrante.
                    resultString = resultString.Remove(resultString.Length - 1);

                    // Cierra paréntesis
                    resultString += ")";

                    // ---- COMENTARIO NUEVO ----
                    // Ahora agregamos "#ID" si el símbolo es uno de los paramSymbols (F, B, A, G...).
                    // output[i] es la letra que detectamos, ej 'B' o 'A'.
                    if (paramSymbols.Contains(output[i]))
                    {
                        int newId = Deriver.GenerateId();
                        // Insertamos el ID
                        resultString += "#" + newId;
                    }
                    // ---- FIN CAMBIO ----

                    // Avanza el índice 'i' para saltar la parte de paréntesis ya procesada.
                    i += (end + 1);
                }
                else
                {
                    // Si no está parametrizado, simplemente añade el carácter.
                    resultString += output[i];
                }
            }

            return resultString;
        }
    }

    /// <summary>
    /// Lista de reglas que componen la gramática.
    /// </summary>
    public List<Rule> rules;

    /// <summary>
    /// Lista de variables globales que pueden ser usadas en las reglas 
    /// (ej. "gravity", "angle", etc.).
    /// </summary>
    public List<GeneralVariables> generalVariables;

    /// <summary>
    /// Si es 'true', las reglas se escogerán de forma aleatoria según su 'weight'
    /// en caso de que haya múltiples reglas para el mismo símbolo.
    /// </summary>
    public bool isStochastic = false;

    /// <summary>
    /// Devuelve todas las reglas que tengan como 'input[0]' el carácter 'c'.
    /// (No contempla contexto en este ejemplo).
    /// </summary>
    /// <param name="c">Carácter a buscar.</param>
    /// <returns>Lista de reglas cuyo 'input[0]' coincida con 'c'.</returns>
    public List<Rule> GetRules(char c)
    {
        List<Rule> matchedRules = new();

        foreach (var rule in rules)
        {
            // Si la primera letra de 'rule.input' coincide con 'c', la añadimos.
            // (No considera reglas con contexto adicional. 
            // Ej. "A < B > C" - eso requeriría lógica adicional.)
            if (rule.input[0] == c)
            {
                matchedRules.Add(rule);
            }
        }

        return matchedRules;
    }
}
