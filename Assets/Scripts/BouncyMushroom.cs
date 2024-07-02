using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;

// TODO: @Guille
// This class needs to be modified to use the new BeatTester, and the color needs to be active for 
// the whole beat, not just 0.07 seconds. Instead of calling BeatTester.Interact, call BeatTester.TestBeat.
public class BouncyMushroom : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _onSuccess;

    private BeatTester _beatTester;
    private SpriteShapeRenderer _spriteRenderer;

    private bool _isResponsive = true;

    private void Awake()
    {
        _beatTester = GetComponent<BeatTester>();
        _spriteRenderer = GetComponentInChildren<SpriteShapeRenderer>();
    }
    
    private void Start()
    {
        BeatMaker.Instance.OnBeat += OnBeat;
        _beatTester.OnSuccess += OnSuccess; 
        _beatTester.OnFailure += OnFailure; 

        GameManager.Instance.OnPlayerRespawned += Reset;
    }

    private void OnDestroy()
    {
        BeatMaker.Instance.OnBeat -= OnBeat;
        _beatTester.OnSuccess -= OnSuccess;
        _beatTester.OnFailure -= OnFailure;

        GameManager.Instance.OnPlayerRespawned -= Reset;
    }

    private void OnBeat()
    {
        if (!_isResponsive)
        {
            return;
        }
        StartCoroutine(IE_PlayBeatAnimation());
    }

    private void OnSuccess()
    {
        Debug.Log("success");
        _onSuccess?.Invoke();
        _isResponsive = false;
        _spriteRenderer.color = Color.blue;
    }

    private void OnFailure()
    {
        Debug.Log("failure");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isResponsive)
        {
            return;
        }
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        var normal = GetMushroomNormal(collision);
        if (Vector2.Dot(normal, Vector2.up) <= 0)
        {
            return;
        }

        var kinematicObject = collision.gameObject.GetComponent<KinematicObject>();
        if (kinematicObject != null)
        {
            Debug.Log("Bounce!");
            kinematicObject.Bounce(normal * Mathf.Abs(kinematicObject.PrevVelocity.y * 3));
        }

        Debug.DrawLine(collision.contacts[0].point, collision.contacts[0].normal, Color.red);
        _beatTester.Interact();
    }

    private IEnumerator IE_PlayBeatAnimation()
    {
        var oldColor = _spriteRenderer.color;
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.07F);
        _spriteRenderer.color = oldColor;
    }

    private void Reset()
    {
        _isResponsive = true;
        _spriteRenderer.color = Color.white;
    }

    private Vector2 GetMushroomNormal(Collision2D collision)
    {
        var point1 = collision.contacts[0].point;
        var normal1 = collision.contacts[0].normal;
        var tangent1 = new Vector2(-normal1.y, normal1.x);

        var point2 = collision.collider.ClosestPoint(
            point1 + tangent1 * 0.001F
          );
        var tangent2 = (point2 - point1).normalized;
        var normal2 = new Vector2(-tangent2.y, tangent2.x);
        return normal2;
    }
}
