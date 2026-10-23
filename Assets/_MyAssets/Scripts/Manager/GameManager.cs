using System;
using System.Collections;
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

    [SerializeField] private AudioSource _clickAudio;
    [SerializeField] private float _clickDelay = 0.3f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Appelée par le bouton Quitter
    public void Quit()
    {
        StartCoroutine(PlayClickThen(DoQuit));
    }

    // Appelée par le bouton Recommencer
    public void ReStart()
    {
        StartCoroutine(PlayClickThen(DoRestart));
    }

    // Joue le clic, attend un peu, puis exécute l'action
    private IEnumerator PlayClickThen(Action action)
    {
        if (_clickAudio != null)
            _clickAudio.Play();

        yield return new WaitForSeconds(_clickDelay);
        action();
    }

    private void DoQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DoRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
