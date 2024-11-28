using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class TreeLSys : MonoBehaviour
{
    public GrammarTree grammarTree;
    public int generations = 5;
    
    [TextArea]
    public string startInput = "p";
    [TextArea]
    public string output = "";

    public Generator generator = new();

}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(TreeLSys))]
public class TreeLSysEditor : UnityEditor.Editor
{
    /*
    public override VisualElement CreateInspectorGUI()
    {
        // Init root element
        var root = new VisualElement();
        var sys = (TreeLSys)target;


        // Init base variables
        var startPos = sys.transform.position + Vector3.zero;
        var startDir = sys.transform.up + Vector3.zero;

        // Object fields for the grammar tree
        var objField = new ObjectField("Grammar Tree");
        objField.objectType = typeof(GrammarTree);
        objField.value = sys.grammarTree;
        objField.RegisterValueChangedCallback(evt => sys.grammarTree = (GrammarTree)evt.newValue);
        root.Add(objField);

        // Add GramamrTree editor
        var box = SimpleBox(new Color(0.15f, 0.15f, 0.15f), 4, 4);
        var soEditor = CreateEditor(sys.grammarTree);

        // LO DEJO HASTA AQUI POR AHORA DESUES LO RETOMARE SI ES NECESARIO

        root.Add(box);

        return root;
    }

    private VisualElement SimpleBox(Color color, float padding, int radius)
    {
        var box = new VisualElement();
        box.style.flexGrow = 1;
        box.style.flexShrink = 1;
        box.style.backgroundColor = color;
        box.style.paddingTop = padding;
        box.style.paddingBottom = padding;
        box.style.paddingLeft = padding;
        box.style.paddingRight = padding;
        box.style.borderTopLeftRadius = radius;
        box.style.borderTopRightRadius = radius;
        box.style.borderBottomLeftRadius = radius;
        box.style.borderBottomRightRadius = radius;
        return box;
    }
    */

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var sys = (TreeLSys)target;
        var startPos = sys.transform.position + Vector3.zero;
        var startDir = sys.transform.up + Vector3.zero;

        GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f));
        boxStyle.padding = new RectOffset(10, 10, 10, 10);

        /*
        // Draw the grammar tree editor in THIS inspector
        GUILayout.Label("ScriptableObject Inspector", EditorStyles.boldLabel);
        GUILayout.BeginVertical(boxStyle);
        CreateEditor(sys.grammarTree).OnInspectorGUI();
        GUILayout.EndVertical();
        */

        if (GUILayout.Button("Generate output (derive)"))
        {
            Debug.Log("Start derive");

            // Derive the grammar tree
            var generations = Deriver.Derive(sys.grammarTree, sys.startInput, sys.generations);
            var axiom = generations[generations.Count - 1];
            sys.output = axiom;

            var msg = "";
            generations.ForEach(g => msg += g + "\n");
            Debug.Log(msg);
            Debug.Log("Axiom: " + axiom);

            Debug.Log("Output generated");
        }

        if (GUILayout.Button("Generate output in 3D"))
        {
            Debug.Log("Start 3D generation");

            // Save initial position and direction
            var _pos = sys.transform.position + Vector3.zero;
            var _dir = sys.transform.rotation;

            // Init actions
            sys.generator.generationAction = sys.generator.InitGenerateAction();

            // Generate the tree in the scene
            var tree = sys.generator.GenerateTree(sys.output, sys.transform);

            // Reset position and direction
            sys.transform.position = _pos;
            sys.transform.rotation = _dir;

            Debug.Log("Output generated in 3D");
        }
    }

    // CHANGE: replace this method with uitoolkit UI elements
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