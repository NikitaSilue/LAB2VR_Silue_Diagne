
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class UIManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private PowerManager _powerManager;
    [SerializeField] private CodeManager _codeManager;

    [Header("Canvas - Etat du courant")]
    [SerializeField] private TMP_Text _powerText;
    [SerializeField] private TMP_Text _borneText;

    [Header("Canvas - Code de securite")]
    [SerializeField] private TMP_Text _codeText;
    [SerializeField] private TMP_Text _inputText;
    [SerializeField] private TMP_Text _messageText;

    [Header("Boutons")]
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _reloadButton;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioCodeRefused;
    [SerializeField] private AudioSource _audioCodeAccepted;
    [SerializeField] private AudioSource _audioSignalDetresse;

    [Header("Panels")]
    [SerializeField] private GameObject _screenMountPanel;
    [SerializeField] private GameObject _endScreenPanel;
    [Header("Son de démarrage de l'écran")]
    [SerializeField] private AudioSource _screenBootAudio;


    private void OnEnable()
    {
        _quitButton.onClick.AddListener(OnQuitClicked);
        _reloadButton.onClick.AddListener(OnReloadClicked);

        // Événement du courant
        _powerManager.OnPowerChanged += OnPowerChanged;

        // Événements du code
        _codeManager.OnInputChanged += UpdateInput;
        _codeManager.OnCodeChanged += UpdateCode;
        _codeManager.OnCodeAccepted += ShowSuccess;
        _codeManager.OnCodeRefused += ShowRefused;
        _powerManager.OnChargedCellCountChanged += Updates;

    }

    private void Start()
    {
        // Affichage de l'état initial
        OnPowerChanged(_powerManager.IsOn);
        UpdateCode(_codeManager.CodeText);
        UpdateInput(_codeManager.EnteredText);
        Updates(_powerManager.ChargedCellCount);
        _screenMountPanel.SetActive(true);
        _endScreenPanel.SetActive(false);
    }

    private void OnDisable()
    {
        _quitButton.onClick.RemoveListener(OnQuitClicked);
        _reloadButton.onClick.RemoveListener(OnReloadClicked);

        _powerManager.OnPowerChanged -= OnPowerChanged;

        _codeManager.OnInputChanged -= UpdateInput;
        _codeManager.OnCodeChanged -= UpdateCode;
        _codeManager.OnCodeAccepted -= ShowSuccess;
        _codeManager.OnCodeRefused -= ShowRefused;
        _powerManager.OnChargedCellCountChanged -= Updates;
    }



    public void Updates(int count)
    {
        _borneText.text = "Nombre de bornes : " + count + "/3";
    }


    // État du courant
    private void OnPowerChanged(bool isOn)
    {
        if (isOn)
        {

            if (_screenBootAudio != null)
            {
                _screenBootAudio.Play();
            }
            _powerText.text = "COURANT RETABLI";
            _powerText.color = Color.green;

            _messageText.text = "SAISISSEZ LE CODE";
            _messageText.color = Color.white;
        }
        else
        {
            _powerText.text = "HORS SERVICE";
            _powerText.color = Color.red;

            _codeText.text = "CODE : ----";
            _inputText.text = "SAISIE : ----";

            _messageText.text = "CONSOLE INACTIVE";
            _messageText.color = Color.red;
        }
       
    }

    // Affichage du code généré
    private void UpdateCode(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            _codeText.text = "CODE : ----";
        }
        else
        {
            _codeText.text = "CODE : " + code;
        }
    }

    // Affichage des chiffres saisis
    private void UpdateInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            _inputText.text = "SAISIE : ----";
        }
        else
        {
            _inputText.text = "SAISIE : " + input;
        }
    }


    // Bon code
    private void ShowSuccess()
    {
        _messageText.text = "FIN DE LA PARTIE ";
        _messageText.color = Color.green;
        _audioSignalDetresse.Play();
        _screenMountPanel.SetActive(false);
        _endScreenPanel.SetActive(true);

    }

    // Mauvais code
    private void ShowRefused()
    {
        _messageText.text = "CODE INCORRECT";
        _messageText.color = Color.red;
        _audioCodeRefused.Play();
    }

    // Bouton Quitter
    void OnQuitClicked()
    {
        GameManager.Instance.Quit();
    }

    // Bouton Recommencer
    void OnReloadClicked()
    {
        GameManager.Instance.ReStart();
    }
}
