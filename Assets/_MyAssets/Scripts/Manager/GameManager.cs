using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Déroulement de l'application. Pour l'instant : quitter.
/// L'exercice de la section 2.12 y ajoute la méthode Restart.
/// Singleton : les autres scripts l'appellent par GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Un seul GameManager : un doublon se détruit.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Appelée par l'UIManager quand on clique sur le bouton Quitter.
    public void Quit()
    {
#if UNITY_EDITOR
        // Dans l'éditeur, Application.Quit ne fait rien : on arrête plutôt le Play.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ReStart()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
