using UnityEngine;
using System.Collections;
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
        // Vérifie l'état du socket au démarrage (sans jouer de son)
        if (_socket.hasSelection)
        {
            if (_socket.interactablesSelected[0].transform.CompareTag("CellCharged"))
                TurnGreen();
            else
                TurnRed();
        }
        else
        {
            TurnYellow();
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
        }
    }

    public void TurnYellow()
    {
        _renderer.sharedMaterial = _yellowLight;
        if (_light != null)
        {
            _light.color = Color.yellow;
            _light.enabled = true;
        }
    }

    public void OnCellEnter(SelectEnterEventArgs args)
    {
        Transform cell = args.interactableObject.transform;

        // Couper le son de drop déclenché par le relâchement de la main
        foreach (var src in cell.GetComponentsInChildren<AudioSource>())
            src.Stop();

        if (cell.CompareTag("CellCharged"))
        {
            TurnGreen();
            if (_insertAudio != null) _insertAudio.Play();
        }
        else
        {
            TurnYellow();
            if (_refusedAudio != null) _refusedAudio.Play();
        }

        // La main a déjà relâché la cellule : vibrer (chargée ou non)
        if (_lastCell == cell)
            SendHaptic();
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

        // Le socket a déjà pris la cellule : vibrer tout de suite
        if (_socket.hasSelection &&
            _socket.interactablesSelected[0].transform == _lastCell)
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