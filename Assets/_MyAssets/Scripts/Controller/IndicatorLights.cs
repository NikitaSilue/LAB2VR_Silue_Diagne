using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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




    Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        _socket.selectEntered.AddListener(OnCellEnter);
        _socket.selectExited.AddListener(OnCellRemoved);
    }
    private void OnDisable()
    {
        _socket.selectEntered.RemoveListener(OnCellEnter);
        _socket.selectExited.RemoveListener(OnCellRemoved);
    }
    private void Start()
    {
        // Vérifie l'état du socket au démarrage
        if (_socket.hasSelection)
        {
            if (_socket.interactablesSelected[0].transform.CompareTag("ChargedCell"))
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
        }
    }

    public void OnCellRemoved(SelectExitEventArgs args)
    {
        TurnRed();
    }


}