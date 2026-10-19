using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class EnergyCell : MonoBehaviour
{

    [SerializeField] bool _isCharged = true;

    XRGrabInteractable _grab;
    Component _lastHand;

    public bool IsCharged => _isCharged;
    public bool IsHeld => _grab.isSelected;
    public Component LastHand => _lastHand;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        _grab.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDisable()
    {
        _grab.selectEntered.RemoveListener(OnSelectEntered);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        // On ignore les sockets : on veut seulement la main du joueur
        if (args.interactorObject is XRSocketInteractor) return;
        _lastHand = args.interactorObject as Component;
    }
}
