using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Métodos y herramientas para manejar parámetros en cadenas L-System y extraer expresiones.
/// </summary>
public static class ParametrizedUtilities
{
    /// <summary>
    /// Busca y devuelve el contenido que está dentro de los primeros
    /// paréntesis encontrados en el string, incluyendo soporte para paréntesis anidados.
    /// </summary>
    /// <param name="input">Cadena de texto que contiene (posiblemente) paréntesis.</param>
    /// <returns>
    /// Una tupla (string, int):
    /// - El primer elemento es el contenido interno del primer par de paréntesis.
    /// - El segundo es la posición en la cadena donde se cierra ese paréntesis.
    /// </returns>
    public static (string, int) ExtractFromParentheses(string input)
    {
        // Si la cadena no empieza con '(' o está vacía, retorna (string.Empty, 0).
        if (string.IsNullOrEmpty(input) || input[0] != '(')
            return (string.Empty, 0);

        int openParentheses = 0;

        // Recorre cada carácter de 'input'.
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '(')
                openParentheses++;
            else if (input[i] == ')')
                openParentheses--;

            // Cuando la cuenta de paréntesis abiertos vuelve a 0, 
            // significa que hemos encontrado el cierre del primer bloque.
            if (openParentheses == 0)
            {
                // Devuelve el contenido dentro de los paréntesis (excluyendo '(' y ')')
                // y la posición 'i' donde se cerró.
                return (input.Substring(1, i - 1), i);
            }
        }

        // Si no se encontró un paréntesis que cierre, retorna vacío.
        return (string.Empty, 0);
    }

    /// <summary>
    /// Determina si el carácter de la cadena en 'index' está seguido inmediatamente 
    /// de un paréntesis de apertura, indicando que es un símbolo "parametrizado".
    /// </summary>
    /// <param name="input">Cadena que se analiza.</param>
    /// <param name="index">Posición del carácter en la cadena.</param>
    /// <returns>
    /// True si el siguiente carácter es '('; False en caso contrario 
    /// (o si 'index+1' está fuera de la cadena).
    /// </returns>
    public static bool IsParameterized(string input, int index)
    {
        return index + 1 < input.Length && input[index + 1] == '(';
    }

    /// <summary>
    /// Busca todas las subcadenas que estén dentro de paréntesis y 
    /// las devuelve como una lista de tuplas (función, parámetros[]).
    /// </summary>
    /// <param name="input">Cadena que se analiza.</param>
    /// <returns>
    /// Cada elemento de la lista es (string, string[]):
    /// - El primer string es el contenido global, 
    /// - El segundo es un array con los parámetros separados por ','.
    /// </returns>
    public static List<(string, string[])> GetParams(string input)
    {
        var result = new List<(string, string[])>();

        // Expresión regular para buscar contenido entre paréntesis.
        string pattern = @"\((.*?)\)";
        var matches = Regex.Matches(input, pattern);

        foreach (Match match in matches)
        {
            // Se asume que 'functionName' es todo el contenido dentro de los paréntesis.
            // Realmente, "functionName" no es un nombre de función sino el contenido dentro de '()'.
            string functionName = match.Groups[1].Value;

            // Separar los parámetros por comas.
            string[] parameters = match.Groups[1].Value
                .Replace(" ", "")
                .Split(',');

            result.Add((functionName, parameters));
        }

        return result;
    }
}
