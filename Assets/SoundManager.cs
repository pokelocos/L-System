using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sonidos disponibles")]
    public AudioClip[] sonidos;
    private AudioSource audioSource;

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
    }

    public void ReproducirSonidoAleatorioCortar()
    {
        if (sonidos.Length == 0) return;

        int index = Random.Range(0, sonidos.Length);
        audioSource.PlayOneShot(sonidos[index]);
    }
}
