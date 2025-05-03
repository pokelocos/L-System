using UnityEngine;
using TMPro;

public class HudDebug : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI allCutsText;
    [SerializeField] TextMeshProUGUI errorCutsText;
    [SerializeField] TextMeshProUGUI correctCutsText;

    void Update()
    {
        var sm = SessionManager.Instance;
        timeText.text = $"{sm.ElapsedTime:F1} s";
        allCutsText.text = $"Cortes: {sm.AllCuts}";
        correctCutsText.text = $"Cortes Correctos: {sm.correctCuts}";
        errorCutsText.text = $"Errores: {sm.errors}";
    }
}
