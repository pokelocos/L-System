using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void OnFinishLevel()
    {
        SessionManager.Instance.EndSessionAndSave();   // escribe CSV
        SceneManager.LoadScene(0);                     // volver al menú (idx 0)
    }
}
