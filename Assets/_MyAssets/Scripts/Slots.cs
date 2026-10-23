/*using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(XROrigin))]

public class WallPreventer : MonoBehaviour
{
    CharacterController m_CharacterController;
    Transform m_LocalHeadTransform;

    void Awake()
    {
        if (!TryGetComponent(out m_CharacterController) || !TryGetComponent(out XROrigin xrOrigin))
        {
            Debug.LogWarning("Missing Components. Disabling Now.");
            this.enabled = false;
            return;
        }
        m_LocalHeadTransform = xrOrigin.Camera.transform;
    }

    void Update()
    {
        // Set the Character Controller's center to the local position of the head transform on X and Z.
        m_CharacterController.center = new Vector3(m_LocalHeadTransform.localPosition.x, m_CharacterController.center.y,
        m_LocalHeadTransform.localPosition.z);
        // This line is important, otherwise the physics on the Character Controller won't update per frame.
        m_CharacterController.SimpleMove(Vector3.zero);
    }
}
///
using UnityEngine;

public class AlienController : MonoBehaviour
{
    [Header("Joueur VR")]
    [SerializeField] private Transform _player;

    [Header("Détection")]
    [SerializeField] private float _detectionDistance = 4f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource _growlAudio;
    [SerializeField] private float _growlCooldown = 5f;

    // Utilisation de l'IA pour le grognement
    private Quaternion _initialRotation;
    private float _lastGrowlTime = Mathf.NegativeInfinity;
    private bool _playerWasNear;

    private void Start()
    {
        // Mémoriser la rotation initiale
        _initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (_player == null)
            return;

        // Direction horizontale vers le joueur
        Vector3 direction = _player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        bool playerNear = distance <= _detectionDistance;

        if (playerNear)
        {
            // Rotation vers le joueur, sur l'axe vertical
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }

            // Grognement à l'entrée de la zone
            if (!_playerWasNear &&
                Time.time - _lastGrowlTime >= _growlCooldown)
            {
                if (_growlAudio != null)
                {
                    _growlAudio.Play();
                    _lastGrowlTime = Time.time;
                }
            }
        }
        else
        {
            // Retour progressif à la position initiale
            transform.rotation = Quaternion.Slerp(transform.rotation, _initialRotation, _rotationSpeed * Time.deltaTime);
        }

        _playerWasNear = playerNear;
    }
}
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;

public class PowerManager : MonoBehaviour
{
    [Header("Sockets")]
    [SerializeField] private XRSocketInteractor _socket1;
    [SerializeField] private XRSocketInteractor _socket2;
    [SerializeField] private XRSocketInteractor _socket3;
    [Header("Lights")]
    [SerializeField] private GameObject _mainLights;
    [SerializeField] private GameObject _emergencyLights;
    [SerializeField] private AudioSource _neonBuzzAudio;
    [Header("Generator")]
    [SerializeField] private Transform _fan;





    private bool _isOn;
    public bool IsOn => _isOn;

    // Annonce l'état de changement du courant 
    public event Action<bool> OnPowerChanged;


    private void OnEnable()
    {
        _socket1.selectEntered.AddListener(OnCellChanged);
        _socket2.selectEntered.AddListener(OnCellChanged);
        _socket3.selectEntered.AddListener(OnCellChanged);

        _socket1.selectExited.AddListener(OnCellRemoved);
        _socket2.selectExited.AddListener(OnCellRemoved);
        _socket3.selectExited.AddListener(OnCellRemoved);


    }

    private void OnDisable()
    {
        _socket1.selectEntered.RemoveListener(OnCellChanged);
        _socket2.selectEntered.RemoveListener(OnCellChanged);
        _socket3.selectEntered.RemoveListener(OnCellChanged);

        _socket1.selectExited.RemoveListener(OnCellRemoved);
        _socket2.selectExited.RemoveListener(OnCellRemoved);
        _socket3.selectExited.RemoveListener(OnCellRemoved);
    }

    private void OnCellChanged(SelectEnterEventArgs args)
    {
        CheckPower();
    }

    private void OnCellRemoved(SelectExitEventArgs args)
    {
        CheckPower();
    }

    private void CheckPower()
    {
        // Vérifier si les 3 sockets sont occupés et verifie si les cellules ont un tag CellCharged
        bool socket1 = _socket1.hasSelection && _socket1.interactablesSelected[0].transform.CompareTag("CellCharged");
        bool socket2 = _socket2.hasSelection && _socket2.interactablesSelected[0].transform.CompareTag("CellCharged");
        bool socket3 = _socket3.hasSelection && _socket3.interactablesSelected[0].transform.CompareTag("CellCharged");
        bool allCharged = socket1 && socket2 && socket3;

        // Compter le nombre de cellules chargées
        int count = 0;

        if (socket1) count++;
        if (socket2) count++;
        if (socket3) count++;

        // Prévenir UIManager du nouveau nombre
        OnChargedCellCountChanged?.Invoke(count);

        // Si le courant n'a pas changé, ne rien faire
        if (_isOn == allCharged)
            return;

        // Mettre à jour le courant
        _isOn = allCharged;

        // Prévenir les autres systèmes
        OnPowerChanged?.Invoke(_isOn);
        UpdateLights(_isOn);

    }

    public event Action<int> OnChargedCellCountChanged;

    //UTILISATION DE L'IA POUR FAIRE LE CLIGNOTEMENT DE LA LUMIERE 
    // Fait clignoter les néons avant l'allumage définitif
    private IEnumerator StartNeonLights()
    {
        _mainLights.SetActive(false);

        for (int i = 0; i < 3; i++)
        {
            _mainLights.SetActive(true);
            yield return new WaitForSeconds(0.15f);

            _mainLights.SetActive(false);
            yield return new WaitForSeconds(0.25f);
        }

        _mainLights.SetActive(true);
        _neonBuzzAudio.Stop();
    }

    private Coroutine _neonCoroutine;

    private void UpdateLights(bool powerOn)
    {
        // Arrêter un éventuel clignotement en cours
        if (_neonCoroutine != null)
        {
            //UTILISATION DE L'IA POUR FAIRE LE CLIGNOTEMENT DE LA LUMIERE 
            StopCoroutine(_neonCoroutine);
            _neonCoroutine = null;
        }

        if (powerOn)
        {
            // Éteindre l'éclairage d'urgence
            _emergencyLights.SetActive(false);

            // Démarrer le clignotement
            _neonCoroutine = StartCoroutine(StartNeonLights());
            _neonBuzzAudio.Play();
        }
        else
        {
            // Retour à l'état initial
            _mainLights.SetActive(false);
            _emergencyLights.SetActive(true);
            _neonBuzzAudio.Stop();
        }
    }

    public int ChargedCellCount
    {
        get
        {
            int count = 0;

            if (IsCharged(_socket1)) count++;
            if (IsCharged(_socket2)) count++;
            if (IsCharged(_socket3)) count++;

            return count;
        }
    }

    private bool IsCharged(XRSocketInteractor socket)
    {
        return socket != null &&
               socket.hasSelection &&
               socket.interactablesSelected[0]
                   .transform.CompareTag("CellCharged");
    }

}using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;

public class PowerManager : MonoBehaviour
{
    [Header("Sockets")]
    [SerializeField] private XRSocketInteractor _socket1;
    [SerializeField] private XRSocketInteractor _socket2;
    [SerializeField] private XRSocketInteractor _socket3;
    [Header("Lights")]
    [SerializeField] private GameObject _mainLights;
    [SerializeField] private GameObject _emergencyLights;
    [SerializeField] private AudioSource _neonBuzzAudio;
    [Header("Generator")]
    [SerializeField] private Transform _fan;





    private bool _isOn;
    public bool IsOn => _isOn;

    // Annonce l'état de changement du courant 
    public event Action<bool> OnPowerChanged;


    private void OnEnable()
    {
        _socket1.selectEntered.AddListener(OnCellChanged);
        _socket2.selectEntered.AddListener(OnCellChanged);
        _socket3.selectEntered.AddListener(OnCellChanged);

        _socket1.selectExited.AddListener(OnCellRemoved);
        _socket2.selectExited.AddListener(OnCellRemoved);
        _socket3.selectExited.AddListener(OnCellRemoved);


    }

    private void OnDisable()
    {
        _socket1.selectEntered.RemoveListener(OnCellChanged);
        _socket2.selectEntered.RemoveListener(OnCellChanged);
        _socket3.selectEntered.RemoveListener(OnCellChanged);

        _socket1.selectExited.RemoveListener(OnCellRemoved);
        _socket2.selectExited.RemoveListener(OnCellRemoved);
        _socket3.selectExited.RemoveListener(OnCellRemoved);
    }

    private void OnCellChanged(SelectEnterEventArgs args)
    {
        CheckPower();
    }

    private void OnCellRemoved(SelectExitEventArgs args)
    {
        CheckPower();
    }

    private void CheckPower()
    {
        // Vérifier si les 3 sockets sont occupés et verifie si les cellules ont un tag CellCharged
        bool socket1 = _socket1.hasSelection && _socket1.interactablesSelected[0].transform.CompareTag("CellCharged");
        bool socket2 = _socket2.hasSelection && _socket2.interactablesSelected[0].transform.CompareTag("CellCharged");
        bool socket3 = _socket3.hasSelection && _socket3.interactablesSelected[0].transform.CompareTag("CellCharged");
        bool allCharged = socket1 && socket2 && socket3;

        // Compter le nombre de cellules chargées
        int count = 0;

        if (socket1) count++;
        if (socket2) count++;
        if (socket3) count++;

        // Prévenir UIManager du nouveau nombre
        OnChargedCellCountChanged?.Invoke(count);

        // Si le courant n'a pas changé, ne rien faire
        if (_isOn == allCharged)
            return;

        // Mettre à jour le courant
        _isOn = allCharged;

        // Prévenir les autres systèmes
        OnPowerChanged?.Invoke(_isOn);
        UpdateLights(_isOn);

    }

    public event Action<int> OnChargedCellCountChanged;

    //UTILISATION DE L'IA POUR FAIRE LE CLIGNOTEMENT DE LA LUMIERE 
    // Fait clignoter les néons avant l'allumage définitif
    private IEnumerator StartNeonLights()
    {
        _mainLights.SetActive(false);

        for (int i = 0; i < 3; i++)
        {
            _mainLights.SetActive(true);
            yield return new WaitForSeconds(0.15f);

            _mainLights.SetActive(false);
            yield return new WaitForSeconds(0.25f);
        }

        _mainLights.SetActive(true);
        _neonBuzzAudio.Stop();
    }

    private Coroutine _neonCoroutine;

    private void UpdateLights(bool powerOn)
    {
        // Arrêter un éventuel clignotement en cours
        if (_neonCoroutine != null)
        {
            //UTILISATION DE L'IA POUR FAIRE LE CLIGNOTEMENT DE LA LUMIERE 
            StopCoroutine(_neonCoroutine);
            _neonCoroutine = null;
        }

        if (powerOn)
        {
            // Éteindre l'éclairage d'urgence
            _emergencyLights.SetActive(false);

            // Démarrer le clignotement
            _neonCoroutine = StartCoroutine(StartNeonLights());
            _neonBuzzAudio.Play();
        }
        else
        {
            // Retour à l'état initial
            _mainLights.SetActive(false);
            _emergencyLights.SetActive(true);
            _neonBuzzAudio.Stop();
        }
    }

    public int ChargedCellCount
    {
        get
        {
            int count = 0;

            if (IsCharged(_socket1)) count++;
            if (IsCharged(_socket2)) count++;
            if (IsCharged(_socket3)) count++;

            return count;
        }
    }

    private bool IsCharged(XRSocketInteractor socket)
    {
        return socket != null &&
               socket.hasSelection &&
               socket.interactablesSelected[0]
                   .transform.CompareTag("CellCharged");
    }

}using UnityEngine;

public class GeneratorManager : MonoBehaviour
{
    [SerializeField] private Transform _propFan;
    [SerializeField] private float _propFanSpeed = 180f;
    [SerializeField] private AudioSource _generatorAudio;
    [SerializeField] private AudioSource _generatorAudioStart;
    [SerializeField] private AudioSource _generatorAudioDown;
    [SerializeField] private PowerManager _powerManager;

    private bool _isStarted;

    private void OnEnable()
    {
        _powerManager.OnPowerChanged += _powerManager_OnPowerChanged;
    }
    private void OnDisable()
    {
        _powerManager.OnPowerChanged -= _powerManager_OnPowerChanged;
    }

    private void _powerManager_OnPowerChanged(bool obj)
    {
        if (obj)
        {
            StartGenerator();
        }
        else
        {
            StopGenerator();
        }
    }

    public void StartGenerator()
    {
        _isStarted = true;
        _generatorAudio.Play();
        _generatorAudioStart.Play();
        _generatorAudioDown.Stop();
    }

    public void StopGenerator()
    {
        _isStarted = false;
        _generatorAudio.Stop();
        _generatorAudioStart.Stop();
        _generatorAudioDown.Play();
    }

    private void Update()
    {
        if (_isStarted && _propFan != null)
        {
            _propFan.Rotate(
                0f,
                _propFanSpeed * Time.deltaTime,
                0f,
                Space.Self
            );
        }
    }
}
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
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Affiche dans la console qui survole, saisit, relâche et active cet objet.
/// Sert à observer les événements d'un interactable avant de s'en servir.
/// </summary>
[RequireComponent(typeof(XRBaseInteractable))]
public class InteractionLogger : MonoBehaviour
{
    XRBaseInteractable _interactable;

    void Awake()
    {
        _interactable = GetComponent<XRBaseInteractable>();
    }

    // S'abonner dans OnEnable et se désabonner dans OnDisable : les deux vont
    // toujours par paire, sinon un objet désactivé puis réactivé s'abonne deux fois.
    void OnEnable()
    {
        _interactable.hoverEntered.AddListener(OnHoverEntered);
        _interactable.selectEntered.AddListener(OnSelectEntered);
        _interactable.selectExited.AddListener(OnSelectExited);
        _interactable.activated.AddListener(OnActivated);
    }

    void OnDisable()
    {
        _interactable.hoverEntered.RemoveListener(OnHoverEntered);
        _interactable.selectEntered.RemoveListener(OnSelectEntered);
        _interactable.selectExited.RemoveListener(OnSelectExited);
        _interactable.activated.RemoveListener(OnActivated);
    }

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log($"{name} : survolé par {HandName(args.interactorObject.transform)}");
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log($"{name} : saisi par {HandName(args.interactorObject.transform)}");
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log($"{name} : relâché par {HandName(args.interactorObject.transform)}");
    }

    void OnActivated(ActivateEventArgs args)
    {
        Debug.Log($"{name} : activé par {HandName(args.interactorObject.transform)}");
    }

    // L'interactor s'appelle « Near-Far Interactor » sur les deux mains.
    // C'est son parent, LeftController ou RightController, qui dit de quelle main
    // il s'agit.
    static string HandName(Transform interactor)
    {
        return interactor.parent != null ? interactor.parent.name : interactor.name;
    }
}
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[RequireComponent(typeof(Renderer))]
public class IndicatorLights : MonoBehaviour
{

    [Tooltip("Matériau du voyant allumé.")]
    [SerializeField] private Material _redLight;
    [SerializeField] private Material _greenLight;
    [SerializeField] private Material _yellowLight;
    [Tooltip("Référence au socket et au voyant physique")]
    [SerializeField] private XRSocketInteractor _socket;
    [SerializeField] private IndicatorLights _indicator;
    [SerializeField] private Light _light;
    [Tooltip("Référence au socket et au sons")]
    [SerializeField] private AudioSource _insertAudio;
    [SerializeField] private AudioSource _refusedAudio;



    [Header("Haptics")]
    [SerializeField] private float _hapticAmplitude = 0.7f;
    [SerializeField] private float _hapticDuration = 0.3f;

    // Cellules manipulables
    [SerializeField] private XRGrabInteractable[] _cells;

    // Manette ayant relâché la cellule
    private HapticImpulsePlayer _lastHand;
    private Transform _lastCell;



    Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        _socket.selectEntered.AddListener(OnCellEnter);
        _socket.selectExited.AddListener(OnCellRemoved);

        foreach (var cell in _cells)
        {
            if (cell != null)
                cell.selectExited.AddListener(OnCellReleased);
        }
    }
    private void OnDisable()
    {
        _socket.selectEntered.RemoveListener(OnCellEnter);
        _socket.selectExited.RemoveListener(OnCellRemoved);

        foreach (var cell in _cells)
        {
            if (cell != null)
                cell.selectExited.RemoveListener(OnCellReleased);
        }
    }
    private void Start()
    {
        // Vérifie l'état du socket au démarrage
        if (_socket.hasSelection)
        {
            if (_socket.interactablesSelected[0].transform.CompareTag("CellCharged"))
                TurnGreen();
            else
                TurnYellow();
        }
        else
        {
            TurnRed();
        }
    }

    public void TurnRed()
    {
        _renderer.sharedMaterial = _redLight;

        if (_light != null)
        {
            _light.color = Color.red;
            _light.enabled = true;
        }
    }

    public void TurnGreen()
    {
        _renderer.sharedMaterial = _greenLight;
        if (_light != null)
        {
            _light.color = Color.green;
            _light.enabled = true;
            _insertAudio.Play();
        }
    }

    public void TurnYellow()
    {
        _renderer.sharedMaterial = _yellowLight;
        if (_light != null)
        {
            _light.color = Color.yellow;
            _light.enabled = true;
            _refusedAudio.Play();
        }
    }

    public void OnCellEnter(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag("CellCharged"))
        {
            TurnGreen();
        }
        else
        {
            TurnYellow();

            // Vibration seulement si la cellule est déchargée
            if (_lastCell == args.interactableObject.transform)
            {
                SendHaptic();
            }
        }
    }

    public void OnCellRemoved(SelectExitEventArgs args)
    {
        TurnRed();
    }



    private void OnCellReleased(SelectExitEventArgs args)
    {
        // Ignorer les événements venant du socket
        if (args.interactorObject is XRSocketInteractor)
            return;

        _lastCell = args.interactableObject.transform;

        _lastHand = args.interactorObject.transform
            .GetComponentInParent<HapticImpulsePlayer>();

        // Si la cellule est déjà insérée, vibrer immédiatement
        if (_socket.hasSelection &&
            _socket.interactablesSelected[0].transform == _lastCell &&
            !_lastCell.CompareTag("CellCharged"))
        {
            SendHaptic();
        }


    }

    private void SendHaptic()
    {
        if (_lastHand != null)
        {
            _lastHand.SendHapticImpulse(
                _hapticAmplitude,
                _hapticDuration
            );
        }

        _lastHand = null;
        _lastCell = null;
    }

}*/