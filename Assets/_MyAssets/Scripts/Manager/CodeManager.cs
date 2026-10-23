
using System;
using System.Collections.Generic;
using UnityEngine;

public class CodeManager : MonoBehaviour
{
    [SerializeField] private PowerManager _powerManager;
    [SerializeField] private int _codeLength = 4;

    private int[] _code;
    private readonly List<int> _entered = new List<int>();
    private string _codeText = "";
    public string CodeText => _codeText;

    private string _enteredText = "";
    public string EnteredText => _enteredText;

    private bool _validated = false;

    public bool IsPowered => _powerManager != null
                             && _powerManager.IsOn;

    // Événements
    public event Action<string> OnInputChanged;
    public event Action OnCodeAccepted;
    public event Action OnCodeRefused;
    public event Action OnDigitAdded;

    // Nouvel événement pour actualiser le code affiché
    public event Action<string> OnCodeChanged;

    private void Awake()
    {
        // Aucun code au démarrage
        _code = new int[_codeLength];
        _codeText = "";
    }

    private void OnEnable()
    {
        if (_powerManager != null)
            _powerManager.OnPowerChanged += OnPowerChanged;
    }

    private void OnDisable()
    {
        if (_powerManager != null)
            _powerManager.OnPowerChanged -= OnPowerChanged;
    }

    private void Start()
    {
        // Synchroniser l'état initial de la console
        OnPowerChanged(IsPowered);
    }

    private void OnPowerChanged(bool isOn)
    {

        ClearInput();
        _validated = false;

        if (isOn)
        {
            GenerateCode();
        }
        else
        {
            _codeText = "";
            Array.Clear(_code, 0, _code.Length);
            OnCodeChanged?.Invoke(_codeText);
        }
    }

    private void GenerateCode()
    {
        _codeText = "";

        for (int i = 0; i < _codeLength; i++)
        {
            _code[i] = UnityEngine.Random.Range(1, 5);
            _codeText += _code[i];
        }

        OnCodeChanged?.Invoke(_codeText);
    }

    // Appelé par les boutons de la console
    public void PressDigit(int digit)
    {
        // Aucun bouton ne fonctionne sans courant
        if (!IsPowered)
            return;

        // Empêcher la saisie après réussite
        if (_validated)
            return;

        // Accepter uniquement les chiffres de 1 à 4
        if (digit < 1 || digit > 4)
            return;

        _entered.Add(digit);
        _enteredText += digit;

        OnDigitAdded?.Invoke();
        OnInputChanged?.Invoke(_enteredText);

        if (_entered.Count < _codeLength)
            return;

        // Vérification après le quatrième chiffre
        bool correct = _enteredText == _codeText;

        if (correct)
        {
            _validated = true;
            OnCodeAccepted?.Invoke();
        }
        else
        {
            OnCodeRefused?.Invoke();
            ClearInput();
        }
    }

    private void ClearInput()
    {
        _entered.Clear();
        _enteredText = "";
        OnInputChanged?.Invoke(_enteredText);
    }
}
