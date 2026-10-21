using UnityEngine;

public class AlarmLight : MonoBehaviour
{

    [SerializeField] private Light _alarmPointLight;
    [SerializeField] private AudioSource _alarmAudio;
    [SerializeField] private float _interval = 0.5f;
    [SerializeField] private PowerManager _powerManager;


    private bool alarmActive;
    private float timer;

    private void Start()
    {
        SetAlarmActive(true);
    }

    private void Awake()
    {
            _alarmPointLight = GetComponentInChildren<Light>();
            _alarmAudio = GetComponent<AudioSource>();
    }

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
       SetAlarmActive(!obj);
    }

    private void Update()
    {
        if (!alarmActive || _alarmPointLight == null)
            return;

        timer += Time.deltaTime;

        if (timer >= _interval)
        {
            timer = 0f;
            _alarmPointLight.enabled = !_alarmPointLight.enabled;
        }
    }

    public void SetAlarmActive(bool active)
    {
        alarmActive = active;
        timer = 5f;

        if (_alarmPointLight != null)
        {
            _alarmPointLight.enabled = active;
        }
            

        if (_alarmAudio == null)
        {
            return;
        }
           

        if (active && !_alarmAudio.isPlaying)
        {
            _alarmAudio.Play();
        }
            
        else if (!active)
        {
            _alarmAudio.Stop();
        }
            
    }
}

