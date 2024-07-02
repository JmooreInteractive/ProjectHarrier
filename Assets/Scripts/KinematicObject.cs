using UnityEngine;

/// <summary>
/// Implements game physics for some in game entity.
/// </summary>
public class KinematicObject : MonoBehaviour
{
    [field: SerializeField]
    public bool InvertGravity { get; set; } = false;
    public float Gravity { get; set; } = -1F;
    public Vector2 PrevVelocity;
    public Vector2 Velocity;

    public bool IsGrounded { get; private set; }

    [SerializeField]
    private float _terminalVelocity;

    #region  --- Commands ---

    public void BounceY(float value)
    {
        Velocity.y = value;
    }

    public void Bounce(Vector2 direction)
    {
        Velocity.y = direction.y;
        Velocity.x = direction.x;
    }

    public void Teleport(Vector2 position)
    {
        _rb2d.position = position;
        _rb2d.velocity = Vector2.zero;
        Velocity = Vector2.zero;
    }

    #endregion

    private void Awake()
    {
        Init();
    }

    private void OnValidate()
    {
        Init();
    }

    private void Init()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _rb2d.isKinematic = true;
    }

    private void Start()
    {
        _contactFilter.useTriggers = false;
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        _contactFilter.useLayerMask = true;
    }

    private void FixedUpdate()
    {
        PrevVelocity = Velocity;
        MoveInY();     
        MoveInX();
    }

    private void MoveInX()
    {
        var moveDirection = new Vector2(Mathf.Sign(Velocity.x), 0);
        var moveMagnitude = Mathf.Abs(Velocity.x) * Time.fixedDeltaTime;

        if (IsGrounded)
        {
            Debug.DrawLine(_hitBuffer[0].point, _hitBuffer[0].point + _groundNormal);
            float slopeSpeedModifier;
            var dot = Vector2.Dot(moveDirection, _groundNormal);
            if (dot >= 0)
            {
                slopeSpeedModifier = 1;
            }
            else
            {
                slopeSpeedModifier = 1 + dot;
            }
            slopeSpeedModifier = Mathf.Clamp(slopeSpeedModifier, 0.7F, 1);
            // var slopeSpeedModifier = Mathf.Clamp01(-Vector2.Dot(moveDirection, _groundNormal));

            moveDirection = Mathf.Sign(_groundTangent.x) == moveDirection.x
                ? _groundTangent
                : -_groundTangent;
            moveMagnitude *= slopeSpeedModifier;
        }
        
        if (moveMagnitude > 0.01F)
        {
            CastRb2dAlongDirection(moveDirection, moveMagnitude);

            var minDistance = moveMagnitude;   
            for (var i = 0; i < _numHits; i++)
            {
                var hitNormal = _hitBuffer[i].normal;

                // TODO: Add being blocked.
                // var projection = Vector2.Dot(moveDirection, hitNormal);
                // if (projection < 0)
                // {

                // }

                var hitDistance = _hitBuffer[i].distance - _shellRadius;
                if (hitDistance < minDistance)
                {
                    minDistance = hitDistance;
                }
            }
            _rb2d.position += moveDirection.normalized * minDistance;
        }
    }

    private void MoveInY()
    {
        AdjustVerticalVelocity();

        IsGrounded = false;

        var moveDirection = new Vector2(0, Mathf.Sign(Velocity.y));
        var moveMagnitude = Mathf.Abs(Velocity.y) * Time.fixedDeltaTime;

        CastRb2dAlongDirection(moveDirection, moveMagnitude);

        var minDistance = moveMagnitude;   
        for (var i = 0; i < _numHits; i++)
        {
            var hitNormal = _hitBuffer[i].normal;
            if (hitNormal.y > 0)
            {
                IsGrounded = true;
                _groundNormal = hitNormal;
                Velocity.y = 0;
            }
            
            var hitDistance = _hitBuffer[i].distance - _shellRadius;
            if (hitDistance < minDistance)
            {
                minDistance = hitDistance;
            }
        }

        if (_numHits == 0)
        {
            IsGrounded = false;
            _groundNormal = Vector2.up;
        }

        _rb2d.position += moveDirection.normalized * minDistance;
    }
    
    private void CastRb2dAlongDirection(Vector2 direction, float magnitude)
    {
        var oldQueriesHitTriggers = Physics2D.queriesHitTriggers;
        Physics2D.queriesHitTriggers = false;
        _numHits = _rb2d.Cast(direction, _contactFilter, _hitBuffer, magnitude + _shellRadius);
        Physics2D.queriesHitTriggers = oldQueriesHitTriggers;
    }
    
    private void AdjustVerticalVelocity()
    {
        Debug.Log(Gravity + " " + InvertGravity);
        var delta = Gravity * Time.fixedDeltaTime * (InvertGravity ? -1 : +1);
        Velocity.y += delta;

        if (InvertGravity)
        {
            if (Velocity.y > -_terminalVelocity)
            {
                Velocity.y = -_terminalVelocity;
            }
        }
        else
        {
            if (Velocity.y < _terminalVelocity)
            {
                Velocity.y = _terminalVelocity;
            }
        }
    }

    private Vector2 _groundNormal;
    private Vector2 _groundTangent => new Vector2(-_groundNormal.y, _groundNormal.x);

    private Rigidbody2D _rb2d;
    private ContactFilter2D _contactFilter;
    private int _numHits;
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    protected const float _shellRadius = 0.01f;
}
