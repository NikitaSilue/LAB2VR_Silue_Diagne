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

}