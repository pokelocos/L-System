using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// Asegúrate de que este 'using UnityEngine.Windows;' sea necesario; 
// a menudo no se requiere si no usas nada de 'Windows'.
using Utils = ParametrizedUtilities;

/// <summary>
/// Clase estática que se encarga de derivar (aplicar iteraciones) 
/// de un sistema L-System, basándose en reglas y parámetros.
/// </summary>
public static class Deriver
{
    /// <summary>
    /// Deriva el axioma de un L-System una cantidad de generaciones, 
    /// devolviendo la cadena resultante en cada iteración.
    /// </summary>
    /// <param name="grammar">Objeto que contiene las reglas de producción del sistema.</param>
    /// <param name="axiom">Cadena inicial (axioma) desde la que se parte para derivar.</param>
    /// <param name="amount">Número de iteraciones (generaciones) que se van a aplicar.</param>
    /// <returns>Lista de strings que representan las cadenas resultantes 
    /// en cada generación, incluyendo la generación 0 (el axioma inicial).</returns>
    public static List<string> Derive(GrammarTree grammar, string axiom, int amount)
    {
        // Ejemplo de un stack que no estás usando actualmente. 
        // Si no lo usas, podrías eliminarlo, o en caso de planificar usarlo, coméntalo:
        // Stack<tortoiseData> pushdown = new();

        // Este listado almacenará la cadena de cada generación (iteración).
        List<string> generations = new();

        // 'current' representa la cadena actual que se derivará en cada iteración.
        string current = axiom;

        // Agrega la cadena inicial como la primera 'generación'.
        generations.Add(current);

        // Repite 'amount' veces el proceso de derivación.
        for (int i = 0; i < amount; i++)
        {
            // Acumula la nueva cadena generada en esta iteración.
            string outputGen = "";

            // Recorre carácter a carácter la cadena actual.
            for (int j = 0; j < current.Length; j++)
            {
                // Verifica si el carácter en la posición j está "parametrizado".
                // Dependemos de una clase/struct 'ParametrizedUtilities' (Utils) que desconozco,
                // pero suponemos que 'IsParameterized' indica si el carácter es algo como 'F(...)', 'G(...)', etc.
                if (Utils.IsParameterized(current, j))
                {
                    // Extrae la parte de la cadena donde están los paréntesis y sus parámetros.
                    string substring = current.Substring(j + 1);
                    var (parameters, endIndex) = Utils.ExtractFromParentheses(substring);

                    // Separa los distintos parámetros en 'exps' usando ';' como separador.
                    var exps = parameters.Split(';');

                    // Obtiene las reglas asociadas al símbolo actual.
                    var rules = grammar.GetRules(current[j]);

                    // Filtra solo las reglas que cumplan la condición (por ejemplo, 
                    // que coincidan con el número de parámetros esperado, etc.).
                    rules = rules.Where(rule => rule.CheckCondition(exps, grammar.generalVariables)).ToList();

                    // Si no hay reglas aplicables, copio directamente el símbolo con sus parámetros.
                    if (rules.Count <= 0)
                    {
                        // El '+ (endIndex + 1)' es para saltar el símbolo y el contenido de paréntesis.
                        outputGen += current.Substring(j, 1 + (endIndex + 1));
                    }
                    else
                    {
                        // Si la gramática es estocástica, elige una regla al azar con probabilidad 'weight'.
                        // De lo contrario, elige la primera regla.
                        var rule = grammar.isStochastic
                            ? rules.RandomRullete(r => r.weight)
                            : rules[0];

                        // Calcula la salida final (string) de esa regla.
                        string result = rule.CalcOutput(exps, grammar.generalVariables);
                        outputGen += result;
                    }

                    // Asegúrate de saltarte correctamente la porción ya procesada.
                    j += (endIndex + 1);
                }
                else
                {
                    // El carácter actual NO está parametrizado.
                    // Obtén las reglas aplicables.
                    var rules = grammar.GetRules(current[j]);

                    if (rules.Count <= 0)
                    {
                        // Si no hay reglas, pasa el mismo símbolo.
                        outputGen += current[j];
                    }
                    else
                    {
                        // Toma la primera regla. (Nota: podrías soportar varias reglas no-parametrizadas también).
                        outputGen += rules[0].output;
                    }
                }
            }

            // Al final de esta iteración, guarda la cadena resultante en la lista 'generations'.
            generations.Add(outputGen);

            // 'current' se actualiza para la siguiente iteración.
            current = outputGen;
        }

        // Devuelve todas las generaciones, incluida la 0.
        return generations;
    }
}
