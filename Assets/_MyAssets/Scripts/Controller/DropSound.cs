
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class DropSound : MonoBehaviour
{
    [SerializeField] private AudioSource _dropAudio;
    [SerializeField] private float _minImpactSpeed = 0.5f;

    private XRGrabInteractable _interactable;

    private void Awake()
    {
        _interactable = GetComponent<XRGrabInteractable>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignorer si la cellule est en main ou dans un socket
        if (_interactable.isSelected)
            return;

        // Ignorer les petits contacts (cellule qui se pose dans le socket)
        if (collision.relativeVelocity.magnitude < _minImpactSpeed)
            return;

        _dropAudio.Play();
    }
}