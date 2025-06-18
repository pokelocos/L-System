// Assets/Scripts/SessionManager.cs
using System;
using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    /* ───────── Config ───────── */
    [Tooltip("BuildIndex de la(s) escena(s) cuya métrica NO debe guardarse.")]
    [SerializeField] int tutorialSceneIndex = 0;      // ← aquí tu escena-tutorial
    // Si quieres excluir varias, usa un int[] o una List<int>.

    /* ───────── Datos persistentes ───────── */
    public string playerID;                  // GUID único por ejecución

    /* ───────── Datos por nivel ───────── */
    public int currentLevel;   // 1,2,3…
    public float startTime;
    public int correctCuts;
    public int errors;
    public int AllCuts => correctCuts + errors;
    public float ElapsedTime => Time.time - startTime;

    /* ───────── Interno ───────── */
    bool recordMetrics = true;   // ← se desactiva en tutorial

    /* ───────── Singleton ───────── */
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerID = Guid.NewGuid().ToString();
            Debug.Log($"[SessionManager] New player ID: {playerID}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /* ───────── Arrancar un nivel ───────── */
    public void StartNewLevel(int levelBuildIndex)
    {
        currentLevel = levelBuildIndex;
        startTime = Time.time;
        correctCuts = 0;
        errors = 0;

        /* ¿Se deben grabar métricas? */
        recordMetrics = (levelBuildIndex != tutorialSceneIndex);

        SceneManager.LoadScene(levelBuildIndex);
    }

    /* ───────── Registro de cortes ───────── */
    public void RegisterCorrectCut() { if (recordMetrics) correctCuts++; }
    public void RegisterError() { if (recordMetrics) errors++; }

    /* ───────── Fin de nivel / Guardar CSV ───────── */
    public void EndSessionAndSave()
    {
        if (!recordMetrics) return;   // ← nada que guardar

        CsvWriter.AppendLine(playerID, currentLevel,
                             ElapsedTime,
                             AllCuts,
                             correctCuts,
                             errors);
    }
}

/* ================================================================= */
/*  CSV writer (sin cambios)                                         */
/* ================================================================= */
static class CsvWriter
{
    static readonly string dir = Application.persistentDataPath;
    static readonly string path = Path.Combine(dir, "metrics.csv");
    static readonly CultureInfo CsvCulture = CultureInfo.InvariantCulture;
    static readonly UTF8Encoding Utf8Bom = new UTF8Encoding(true);

    public static void AppendLine(string id, int level, float time,
                                  int allCuts, int correctCuts, int errors)
    {
        float precision = allCuts == 0 ? 0f : (float)correctCuts / allCuts;

        string line = string.Format(CsvCulture,
            "{0},{1},{2:F1},{3},{4},{5},{6:P1}\n",
            id, level, time, allCuts, correctCuts, errors, precision);

        if (!File.Exists(path))
        {
            string header = "PlayerID,Level,Time_s,AllCuts,CorrectCuts,Errors,Precision\n";
            File.WriteAllText(path, header + line, Utf8Bom);
        }
        else
        {
            File.AppendAllText(path, line, Utf8Bom);
        }

        Debug.Log($"[CsvWriter] Saved metrics to {path}");
    }
}
