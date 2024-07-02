using UnityEngine;

// TODO: @Guille 
// This class triggers the beat every N milliseconds. I don't think you have to make any changes here.
public class BeatMaker : MonoBehaviour
{
    public static BeatMaker Instance { get; private set; }

    public event System.Action OnBeat;

    public bool IsPlaying { get; private set; }
    public int Bpm { get; private set; }

    public void PlayBeats(int bpm)
    {
        IsPlaying = true;
        _progress = 0;
        Bpm = bpm;
    }

    public void StopBeats()
    {
        IsPlaying = false;
        Bpm = 0;
    }
    
    private float _progress;

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        _progress += Time.deltaTime * (Bpm / 60.0F);
        while (_progress > 1)
        {
            OnBeat?.Invoke();
            _progress -= 1;
        }
    }

    private void Awake()
    {
        Debug.Assert(
            Instance == null,
            $"Attempted to create multiple instances of {typeof(BeatMaker).Name}!"
          );
        Instance = this;
    }
}
