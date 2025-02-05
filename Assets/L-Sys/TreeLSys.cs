using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

/// <summary>
/// Clase principal para el sistema de árbol L-System.
/// Se encarga de inicializar los parámetros y generar el árbol.
/// </summary>
public class TreeLSys : MonoBehaviour
{
    // Árbol gramatical para generar el sistema L-System
    public GrammarTree grammarTree;

    // Número de generaciones para derivar el árbol
    public int generations = 5;

    [TextArea]
    public string startInput = "p"; // Entrada inicial para la gramática

    [TextArea]
    public string output = ""; // Salida después de aplicar las reglas

    // Generador para la representación 3D
    public Generator generator = new();
}


