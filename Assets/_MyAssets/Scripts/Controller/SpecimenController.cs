
using UnityEngine;

public class AlienController : MonoBehaviour
{
    [Header("Joueur VR")]
    [SerializeField] private Transform _player;

    [Header("Détection")]
    [SerializeField] private float _detectionDistance = 4f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource _growlAudio;
    [SerializeField] private float _growlCooldown = 5f;

    // Utilisation de l'IA pour le grognement
    private Quaternion _initialRotation;
    private float _lastGrowlTime = Mathf.NegativeInfinity;
    private bool _playerWasNear;

    private void Start()
    {
        // Mémoriser la rotation initiale
        _initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (_player == null)
            return;
        //UTILISATION DE L'IA POUR LE DEBUG
        // Direction horizontale vers le joueur
        Vector3 direction = _player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        bool playerNear =  distance <= _detectionDistance;

        if (playerNear)
        {
            // Rotation vers le joueur, sur l'axe vertical
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =   Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp( transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime  );
            }

            // Grognement à l'entrée de la zone
            if (!_playerWasNear &&
                Time.time - _lastGrowlTime >= _growlCooldown)
            {
                if (_growlAudio != null)
                {
                    _growlAudio.Play();
                    _lastGrowlTime = Time.time;
                }
            }
        }
        else
        {
            // Retour progressif à la position initiale
            transform.rotation = Quaternion.Slerp( transform.rotation,  _initialRotation, _rotationSpeed * Time.deltaTime );
        }

        _playerWasNear = playerNear;
    }
}
