using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSysUIController : MonoBehaviour
{
    // Referencia directa al ScriptableObject con las reglas
    public GrammarTree grammar;

    // En el Inspector, arrastra el GameObject que tiene "TreeStruct"
    public TreeStruct treeStruct;

    // Este generador podría estar aquí mismo, 
    // o podrías seguir usando uno en "TreeLSys".
    // Generador para la representación 3D
    public Generator generator = new();

    // Cadena inicial (axioma)
    [TextArea]
    public string startInput = "F";

    // Cuántas iteraciones de golpe
    public int generations = 5;

    /// <summary>
    /// Deriva la cadena actual (treeStruct.LSys) 
    /// UNA vez y regenera el árbol en 3D.
    /// </summary>
    public void NextIteration()
    {
        if (grammar == null)
        {
            Debug.LogWarning("GrammarTree is not assigned!");
            return;
        }
        if (generator == null)
        {
            Debug.LogWarning("Generator is not assigned!");
            return;
        }
        transform.position = Vector3.zero;
        var _pos = transform.position + Vector3.zero;
        var _dir = transform.rotation;
        // Deriva la cadena actual 1 iteración
        var gens = Deriver.Derive(grammar, treeStruct.LSys, 1);
        var newChain = gens[gens.Count - 1];

        // Actualizamos la cadena en el TreeStruct
        treeStruct.LSys = newChain;

        // Destruir el árbol anterior
        var oldTree = GameObject.Find("Tree1").GetComponent<TreeStruct>();

        generator.generationAction = generator.InitGenerateAction();
        // Generar el nuevo árbol
        generator.GenerateTreeOnExisting(oldTree, newChain, this.transform);
        transform.position = Vector3.zero;
    }

    /// <summary>
    /// Genera el árbol desde cero 
    /// derivando 'startInput' con 'generations' veces.
    /// </summary>
    public void GenerateFullTree()
    {
        if (grammar == null)
        {
            Debug.LogWarning("GrammarTree is not assigned!");
            return;
        }
        if (generator == null)
        {
            Debug.LogWarning("Generator is not assigned!");
            return;
        }

        var gens = Deriver.Derive(grammar, startInput, generations);
        var finalChain = gens[gens.Count - 1];

        // Guardamos en TreeStruct
        treeStruct.LSys = finalChain;

        var oldTree = GameObject.Find("Tree1");
        if (oldTree != null) Destroy(oldTree);

        // Genera el árbol
        generator.GenerateTree(finalChain, this.transform);
    }
}
