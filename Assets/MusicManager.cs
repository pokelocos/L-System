using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Lista de canciones")]
    public AudioClip[] canciones; // [0] = para escena 0 y 4, [1] = para escenas 1, 2, 3

    private AudioSource audioSource;
    private int indiceActual = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        // Escuchar cuando se carga una nueva escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int buildIndex = scene.buildIndex;

        // Escenas 0 o 4 → canción[0]
        if (buildIndex == 0 || buildIndex == 4)
        {
            if (indiceActual != 0)
            {
                CambiarCancion(0);
            }
        }
        // Escenas 1, 2, 3 → canción[1]
        else if (buildIndex >= 1 && buildIndex <= 3)
        {
            if (indiceActual != 1)
            {
                CambiarCancion(1);
            }
        }
    }

    public void CambiarCancion(int index)
    {
        if (index < 0 || index >= canciones.Length) return;

        indiceActual = index;
        audioSource.clip = canciones[indiceActual];
        audioSource.loop = true;
        audioSource.Play();
    }

    public void DetenerMusica()
    {
        audioSource.Stop();
        indiceActual = -1;
    }

    public void AjustarVolumen(float volumen)
    {
        audioSource.volume = volumen;
    }
}
