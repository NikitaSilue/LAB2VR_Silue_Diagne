using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Slots : MonoBehaviour
{

    [SerializeField] XRSocketInteractor _socket;
    [SerializeField] AudioSource _audio;
    [SerializeField] AudioClip _insertClip;
    [SerializeField] AudioClip _refusedClip;
    [SerializeField] Renderer _indicator;
    [SerializeField] Color _emptyColor = Color.red;
    [SerializeField] Color _poweredColor = Color.green;
    [SerializeField] float _refusedCooldown = 1f;

    Material _material;
    float _lastRefusedTime = -10f;

    void Awake()
    {
        // Instance du matériau pour ne pas modifier celui des autres bornes
        _material = _indicator.material;
    }

    void OnEnable()
    {
        _socket.selectEntered.AddListener(OnCellInserted);
        _socket.selectExited.AddListener(OnCellRemoved);
    }

    void OnDisable()
    {
        _socket.selectEntered.RemoveListener(OnCellInserted);
        _socket.selectExited.RemoveListener(OnCellRemoved);
    }

    void Start()
    {
        SetColor(_emptyColor);
    }

    void OnCellInserted(SelectEnterEventArgs args)
    {
        SetColor(_poweredColor);
        _audio.PlayOneShot(_insertClip);

        EnergyCell cell = args.interactableObject.transform.GetComponent<EnergyCell>();
        //if (cell != null) HapticsUtility.Send(cell.LastHand, 0.5f, 0.12f);
    }

    void OnCellRemoved(SelectExitEventArgs args)
    {
        SetColor(_emptyColor);
    }

    // Une cellule épuisée tenue en main s'approche de la borne : refus
    void OnTriggerEnter(Collider other)
    {
        EnergyCell cell = other.GetComponentInParent<EnergyCell>();
        if (cell == null || cell.IsCharged || !cell.IsHeld) return;
        if (Time.time - _lastRefusedTime < _refusedCooldown) return;

        _lastRefusedTime = Time.time;
        _audio.PlayOneShot(_refusedClip);
        //HapticsUtility.Send(cell.LastHand, 0.8f, 0.3f); // plus long : "non"
        StartCoroutine(FlashRefused());
    }

    IEnumerator FlashRefused()
    {
        // Clignotement rapide de l'indicateur pour expliquer le refus
        for (int i = 0; i < 3; i++)
        {
            SetColor(Color.white);
            yield return new WaitForSeconds(0.07f);
            SetColor(_emptyColor);
            yield return new WaitForSeconds(0.07f);
        }
    }

    void SetColor(Color color)
    {
        _material.color = color;
        if (_material.HasProperty("_EmissionColor"))
        {
            _material.EnableKeyword("_EMISSION");
            _material.SetColor("_EmissionColor", color);
        }
    }
}
