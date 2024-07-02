using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip _level1BackgroundMusic;

    private AudioSource _audioSource;

    public static GameManager Instance { get; private set; }

    public event System.Action OnPlayerRespawned;

    private Health _playerHealth;
    private Transform _playerTransform;
    private Vector2 _startPosition;
    private Quaternion _startRotation;

    [SerializeField]
    private Animator _curtainAnimator;

    private void Awake()
    {
        Debug.Assert(
            Instance == null,
            $"Attempted to create multiple instances of {typeof(GameManager).Name}!"
          );
        Instance = this;

        var player = GameObject.FindGameObjectWithTag("Player");
        _playerHealth = player.GetComponent<Health>();
        _playerTransform = player.GetComponent<Transform>();
        _startPosition = _playerTransform.position;
        _startRotation = _playerTransform.rotation;
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _playerHealth.OnKilled += OnPlayerKilled;
        BeatMaker.Instance.OnBeat += OnBeat;
        BeatMaker.Instance.PlayBeats(60);

        _curtainAnimator.speed = 1 / _respawnFadeTime;

        _audioSource.clip = _level1BackgroundMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    private void OnDestroy()
    {
        BeatMaker.Instance.OnBeat -= OnBeat;
        _playerHealth.OnKilled -= OnPlayerKilled;
    }

    private void OnBeat()
    {
        Debug.Log("Beat!");
    }

    private void OnPlayerKilled()
    {
        StartCoroutine(IE_RespawnPlayer());
    }

    private float _respawnFadeTime = 1.0F;
    private float _respawnWaitTime = 0.5F;

    private IEnumerator IE_RespawnPlayer()
    {
        var controller = _playerTransform.GetComponent<PlayerController>();
        controller.IsControlEnabled = false;

        _curtainAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(_respawnFadeTime + 0.05F);

        ResetLevelState();
        _audioSource.Stop();
        BeatMaker.Instance.StopBeats();

        yield return new WaitForSeconds(_respawnWaitTime);

        _curtainAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(_respawnFadeTime + 0.05F);

        controller.IsControlEnabled = true;
        _audioSource.Play();
        BeatMaker.Instance.PlayBeats(60);
    }

    private void ResetLevelState()
    {
        var controller = _playerTransform.GetComponent<PlayerController>();
        controller.Ko.InvertGravity = false;
        _playerHealth.TopOff();
        _playerTransform.SetPositionAndRotation(_startPosition, _startRotation);
        OnPlayerRespawned?.Invoke();
    }
}
