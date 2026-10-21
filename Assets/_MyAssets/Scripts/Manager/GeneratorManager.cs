using UnityEngine;

public class GeneratorManager : MonoBehaviour
{
    [SerializeField] private Transform _propFan;
    [SerializeField] private float _propFanSpeed = 180f;
    [SerializeField] private AudioSource _generatorAudio;
    [SerializeField] private AudioSource _generatorAudioStart;
    [SerializeField] private AudioSource _generatorAudioDown;
    [SerializeField] private PowerManager _powerManager;

    private bool _isStarted;

    private void OnEnable()
    {
        _powerManager.OnPowerChanged += _powerManager_OnPowerChanged;
    }
    private void OnDisable()
    {
        _powerManager.OnPowerChanged -= _powerManager_OnPowerChanged;
    }

    private void _powerManager_OnPowerChanged(bool obj)
    {
        if (obj)
        {
            StartGenerator();
        }
        else
        {
            StopGenerator();
        }
    }

    public void StartGenerator()
    {
        _isStarted = true;
        _generatorAudio.Play();
        _generatorAudioStart.Play();
        _generatorAudioDown.Stop();
    }

    public void StopGenerator()
    {
        _isStarted = false;
        _generatorAudio.Stop();
        _generatorAudioStart.Stop();
        _generatorAudioDown.Play();
    }

    private void Update()
    {
        if (_isStarted && _propFan != null)
        {
            _propFan.Rotate(
                0f,
                _propFanSpeed * Time.deltaTime,
                0f,
                Space.Self
            );
        }
    }
}
