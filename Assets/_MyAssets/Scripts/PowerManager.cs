using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PowerManager : MonoBehaviour
{
    [Header("Sockets")]
    [SerializeField] private XRSocketInteractor _socket1;
    [SerializeField] private XRSocketInteractor _socket2;
    [SerializeField] private XRSocketInteractor _socket3;
    [Header("Lights")]
    [SerializeField] private GameObject _mainLights;
    [SerializeField] private GameObject _emergencyLights;
    [Header("Generator")]
    [SerializeField] private AudioSource _generatorAudio;
    [SerializeField] private Transform _fan;

    private bool _isOn;
    public bool IsOn => _isOn;

    public event Action<bool> OnPowerChanged;

    private void OnEnable()
    {
        _socket1.selectEntered.AddListener(OnCellChanged);
        _socket2.selectEntered.AddListener(OnCellChanged);
        _socket3.selectEntered.AddListener(OnCellChanged);
    }

    private void OnDisable()
    {
        _socket1.selectEntered.RemoveListener(OnCellChanged);
        _socket2.selectEntered.RemoveListener(OnCellChanged);
        _socket3.selectEntered.RemoveListener(OnCellChanged);
    }

    private void OnCellChanged(SelectEnterEventArgs args)
    {
        // Ici, vérifier si les trois bornes sont alimentées.
        // Si l'état du courant change, déclencher OnPowerChanged.
    }
}