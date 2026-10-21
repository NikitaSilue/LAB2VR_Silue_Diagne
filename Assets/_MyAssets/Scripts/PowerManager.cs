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

        // Si le courant n'a pas changé, ne rien faire
        if (_isOn == allCharged)
            return;

        // Mettre à jour le courant
        _isOn = allCharged;

        // Prévenir les autres systèmes
        OnPowerChanged?.Invoke(_isOn);
        UpdateLights(_isOn);
    }

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
        }
        else
        {
            // Retour à l'état initial
            _mainLights.SetActive(false);
            _emergencyLights.SetActive(true);
        }
    }

  

    

}