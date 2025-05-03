// Assets/Scripts/SessionManager.cs
using System;                        // Guid
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    /* ───────── Datos persistentes (por arranque) ───────── */
    public string playerID;                  // GUID único por ejecución

    /* ───────── Datos por nivel ───────── */
    public int currentLevel;              // 1,2,3…
    public float startTime;                 // Time.time al entrar al nivel
    public int correctCuts;               // cortes “buenos”
    public int errors;                    // cortes “malos”
    public int AllCuts => correctCuts + errors;
    public float ElapsedTime => Time.time - startTime;

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

        SceneManager.LoadScene(levelBuildIndex);
    }

    /* ───────── Registro de cortes ───────── */
    public void RegisterCorrectCut() => correctCuts++;
    public void RegisterError() => errors++;

    /* ───────── Fin de nivel / Guardar CSV ───────── */
    public void EndSessionAndSave()
    {
        CsvWriter.AppendLine(playerID, currentLevel, ElapsedTime,
                             AllCuts, correctCuts);
    }
}

/* ================================================================= */
/*  CSV helper muy simple                                            */
/* ================================================================= */
static class CsvWriter
{
    static readonly string dir = Application.persistentDataPath;
    static readonly string path = Path.Combine(dir, "metrics.csv");

    public static void AppendLine(string id, int level, float time,
                                  int allCuts, int correctCuts)
    {
        float precision = allCuts == 0 ? 0f : (float)correctCuts / allCuts;

        if (!File.Exists(path))
            File.WriteAllText(path,
              "PlayerID,Level,Time_s,AllCuts,CorrectCuts,Precision\n");

        string line =
            $"{id},{level},{time:F1},{allCuts},{correctCuts},{precision:P1}\n";

        File.AppendAllText(path, line);

        Debug.Log($"[CsvWriter] Saved metrics to {path}");
    }
}
