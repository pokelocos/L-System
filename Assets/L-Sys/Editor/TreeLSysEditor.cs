using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TreeLSys))]
public class TreeLSysEditor : Editor
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
            padding = new RectOffset(20, 20, 20, 20)
        };

        // Botón para generar derivaciones
        if (GUILayout.Button("Generate output (derive)"))
        {
            // 1) Muestra un botón en el Inspector. 
            //    Si se hace clic, se ejecuta el bloque dentro de la llave.

            Debug.Log("Start derive");
            // 2) Imprime en la consola "Start derive", para indicar que inicia el proceso de derivación del L-System.
            // Inicia el cronómetro
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            // Derivar el árbol gramatical
            var generations = Deriver.Derive(sys.grammarTree, sys.startInput, sys.generations);
            // 3) Llama al método estático "Derive" de la clase "Deriver".
            //    - sys.grammarTree: la estructura de reglas (gramática).
            //    - sys.startInput: el axioma o cadena inicial.
            //    - sys.generations: cuántas iteraciones se van a aplicar.
            //    Devuelve una lista de strings, donde cada elemento corresponde 
            //    al resultado en cada generación.

            
            var axiom = generations[generations.Count - 1];
            // 4) Toma el último string de la lista "generations", 
            //    que representa la cadena final tras todas las derivaciones.

            // Recalcula los IDs sobre el axioma final.
            axiom = Deriver.RecalculateUniqueIds(axiom);

            sys.output = axiom;
            // 5) Asigna ese último string al campo "output" del componente "TreeLSys"
            //    para que quede disponible (por ejemplo, en el Inspector).

            var msg = string.Join("\n", generations);
            // 6) Crea un string "msg" uniendo todos los elementos de la lista "generations"
            //    con saltos de línea (\n), para mostrarlos luego en la consola.

            Debug.Log(msg);
            // 7) Muestra en la consola la evolución de la cadena en cada generación, 
            //    cada una en una línea.

            Debug.Log("Axiom: " + axiom);
            // 8) Muestra específicamente el axioma final, para facilidad de lectura.
            // Detener el cronómetro y mostrar el tiempo transcurrido
            stopwatch.Stop();
            Debug.Log("Output generated in " + stopwatch.ElapsedMilliseconds + " ms");
            // 9) Imprime un mensaje final confirmando que el proceso de derivación ha terminado.
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