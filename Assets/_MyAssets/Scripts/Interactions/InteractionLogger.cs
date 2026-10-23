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
