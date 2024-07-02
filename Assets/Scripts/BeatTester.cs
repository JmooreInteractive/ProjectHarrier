using UnityEngine;

// TODO: @Guille
// This class exists on every BouncyMushroom. When the player collides with the mushroom, we call Interact().
// This calls OnSuccess or OnFailure depending on if the collision happened on the beat or not.
//
// We now want to support different beat patterns on every BouncyMushroom. I suggest we add a
// Beat[] _beats = new Beat[] { new Beat(numBeats: 1, isActive: true), new Beat(numBeats: 2, isActive: false) }.
// Instead of Interact and Update, just make a new method bool TestBeat(). This beat returns true
// if a beat is currently active.
public class BeatTester : MonoBehaviour
{   
    [field: SerializeField]
    public float Leeway { get; set; }
  
    public event System.Action OnSuccess;
    public event System.Action OnFailure;

    private bool _isThisFrameABeat;
    private bool _didInteractHappen;
    private float _beatTime;
    private float _interactTime;

    private void Update()
    {
        float timeSinceBeat = Time.time - _beatTime; 
        float timeSinceInteract = Time.time - _interactTime;
        // Checks if time since last beat fits in window

        if (_didInteractHappen && timeSinceInteract > Leeway)
        {
            OnFailure?.Invoke();
            _didInteractHappen = false;
        } else 
        if (_didInteractHappen && timeSinceBeat <= Leeway)
        {
            OnSuccess?.Invoke();
            _didInteractHappen = false;
        }
    }

    public void Interact()
    {
       _interactTime = Time.time; 
       _didInteractHappen = true;
    }

    private void Start()
    {
        BeatMaker.Instance.OnBeat += OnBeat;
    }

    private void OnDestroy()
    {
        BeatMaker.Instance.OnBeat -= OnBeat;
    }

    private void OnBeat()
    {
        if (_didInteractHappen)
        {
            OnSuccess?.Invoke();
            _didInteractHappen = false;
        }

        _isThisFrameABeat = true;
        _beatTime = Time.time; 
    }

    private void LateUpdate()
    {
        _isThisFrameABeat = false;
    }
}
