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

        // Si le courant n'a pas changé, ne rien faire
        if (_isOn == allCharged)
            return;

        // Mettre à jour le courant
        _isOn = allCharged;

        // Prévenir les autres systèmes
        OnPowerChanged?.Invoke(_isOn);
        UpdateLights(_isOn);
        UpdateGenerator(_isOn);
    }

    private void UpdateLights(bool powerOn)
    {
        _mainLights.SetActive(powerOn);
        _emergencyLights.SetActive(!powerOn);
    }

    private void UpdateGenerator(bool powerOn)
    {
        if (powerOn)
        {
            _generatorAudio.Play();
        }

        else
        {
            _generatorAudio.Stop();
        }         
    }


}