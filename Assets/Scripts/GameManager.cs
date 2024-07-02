using System.Collections;
using Platformer.Mechanics;
using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip _level1BackgroundMusic;
    [SerializeField]
    private AudioClip _onFail; // On fail clip
    
    private AudioSource _audioSource;
    [SerializeField]
    private AudioSource _audioSourceTwo; // Added audio source for onfail

    public static GameManager Instance { get; private set; }

    public event System.Action OnPlayerRespawned;
    public event System.Action OnGameOver;

    private Health _playerHealth;
    private Transform _playerTransform;
    private Vector2 _startPosition;
    private Quaternion _startRotation;

    [SerializeField]
    private Animator _curtainAnimator;
    [SerializeField]
    public Animator playerAnimator;

    [Header("Game HUD")]
    [SerializeField] 
    private TMP_Text _lifeCounter;
    [SerializeField] 
    private int _lives;
    
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
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {   _lives = 1;
        _lifeCounter.text = "x" + _lives.ToString();
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
        // Fail sfx when player is killed
        _audioSourceTwo.clip = _onFail;
        _audioSourceTwo.Play();
        playerAnimator.SetBool("Death", true);
        if(_lives > 1)
        {
            StartCoroutine(IE_RespawnPlayer());
        }
        else
        {   
            print("GAME OVER!");
            StartCoroutine(IE_GameOver());
        }
    }

    private float _respawnFadeTime = 1.0F;
    private float _respawnWaitTime = 0.5F;

    private IEnumerator IE_RespawnPlayer()
    {
        _lives--;
        _lifeCounter.text = _lives.ToString();
        var controller = _playerTransform.GetComponent<PlayerController>();
        controller.controlEnabled = false;

        _curtainAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(_respawnFadeTime + 0.05F);

        ResetLevelState();
        _audioSource.Stop();
        BeatMaker.Instance.StopBeats();

        yield return new WaitForSeconds(_respawnWaitTime);

        _curtainAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(_respawnFadeTime + 0.05F);

        controller.controlEnabled = true;
        _audioSource.Play();
        BeatMaker.Instance.PlayBeats(60);
    }
    private IEnumerator IE_GameOver()
    {   _lives--;
       var controller = _playerTransform.GetComponent<PlayerController>();
        controller.controlEnabled = false;

        _curtainAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1 + 0.05F);

        OnGameOver?.Invoke();
        _audioSource.Stop();
        BeatMaker.Instance.StopBeats();

        _curtainAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(1 + 0.05F);
    }

    private void ResetLevelState()
    {
        var controller = _playerTransform.GetComponent<PlayerController>();
        controller.InvertGravity = false;
        _playerHealth.TopOff();
        _playerTransform.SetPositionAndRotation(_startPosition, _startRotation);
        OnPlayerRespawned?.Invoke();
    }
}
