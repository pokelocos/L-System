using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void OnFinishLevel(int levelIndex)
    {
        SessionManager.Instance.EndSessionAndSave();   // escribe CSV
        SceneManager.LoadScene(levelIndex);            // volver al menú (idx 0)
    }
}
