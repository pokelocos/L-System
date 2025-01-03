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

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(TreeLSys))]
public class TreeLSysEditor : UnityEditor.Editor
{
    /// <summary>
    /// Personalización del Inspector para TreeLSys.
    /// Permite generar el sistema L-System y renderizarlo en 3D.
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var sys = (TreeLSys)target;

        // Guardar posición y dirección inicial
        var startPos = sys.transform.position + Vector3.zero;
        var startDir = sys.transform.up + Vector3.zero;

        // Estilo para cuadros de ayuda
        GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox)
        {
            normal = { background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f)) },
            padding = new RectOffset(10, 10, 10, 10)
        };

        // Botón para generar derivaciones
        if (GUILayout.Button("Generate output (derive)"))
        {
            Debug.Log("Start derive");

            // Derivar el árbol gramatical
            var generations = Deriver.Derive(sys.grammarTree, sys.startInput, sys.generations);
            var axiom = generations[generations.Count - 1];
            sys.output = axiom;

            var msg = string.Join("\n", generations);
            Debug.Log(msg);
            Debug.Log("Axiom: " + axiom);
            Debug.Log("Output generated");
        }

        // Botón para generar la representación 3D
        if (GUILayout.Button("Generate output in 3D"))
        {
            Debug.Log("Start 3D generation");

            // Guardar posición y rotación inicial
            var _pos = sys.transform.position + Vector3.zero;
            var _dir = sys.transform.rotation;

            // Inicializar acción de generación
            sys.generator.generationAction = sys.generator.InitGenerateAction();

            // Generar el árbol en 3D
            var tree = sys.generator.GenerateTree(sys.output, sys.transform);

            // Restaurar posición y rotación
            sys.transform.position = _pos;
            sys.transform.rotation = _dir;

            Debug.Log("Output generated in 3D");
        }
    }

    /// <summary>
    /// Crea una textura sólida para estilos de GUI.
    /// </summary>
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
#endif
