// Assets/Scripts/MenuController.cs
using UnityEngine;
using TMPro;                    // si usas TextMeshPro

public class MenuController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI idText;   // arrástralo en el Inspector

    void Start()
    {
        idText.text = $"ESTA ES SU ID: {SessionManager.Instance.playerID}";
    }

    // Hook a este método desde el botón “Nivel 1”
    public void OnStartLevel1()
    {
        // Level1 debe estar añadido a Build Settings con índice 1
        SessionManager.Instance.StartNewLevel(1);
    }
}
