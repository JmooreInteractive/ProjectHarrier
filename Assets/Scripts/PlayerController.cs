using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed = 3.5F;

    [SerializeField]
    private float _jumpHeight = 3F;

    [SerializeField]
    private float _jumpTime = 0.5F;

    #region --- Derived Jump Speed & Gravity Params ---

    // TODO
    public float JumpSpeed { get; private set; }
    public float Gravity1 { get; private set; }
    public float Gravity2 { get; private set; }

    #endregion

    [SerializeField]
    private float _coyoteTime = 0.05F;

    [SerializeField]
    private float _jumpBuffer = 0.05F;

    [SerializeField, TextArea(1, 4)]
    private string _toString;

    private float _timeLastJumped;
    private float _timeLastGrounded;

    private bool _prevIsGrounded;
    private bool _isGrounded;
    private bool _isJumping;

    public KinematicObject Ko { get; private set; }

    public bool IsControlEnabled { get; set; } = true;
    
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
        Ko = GetComponent<KinematicObject>();
        CalculateJumpSpeedAndGravity();
    }

    private void Start()
    {
        Ko.Gravity = Gravity1;
    }

    private void CalculateJumpSpeedAndGravity()
    {
        JumpSpeed = 5;
        Gravity1 = -9.81F;
        Gravity2 = Gravity1 * 2.5F;
    }

    private void Update()
    {
        _prevIsGrounded = _isGrounded;
        _isGrounded = Ko.IsGrounded;
        if (!_prevIsGrounded && _isGrounded)
        {
            // We landed, reset gravity.
            _isJumping = false;
            Ko.Gravity = Gravity1;
        }

        if (_isGrounded)
        {
            _timeLastGrounded = Time.time;
        }

        if (IsControlEnabled && Input.GetButtonDown("Jump"))
        {
            _timeLastJumped = Time.time;
        }

        var delta1 = Time.time - _timeLastGrounded;
        var delta2 = Time.time - _timeLastJumped;

        if (delta1 <= _coyoteTime && delta2 <= _jumpBuffer)
        {
            Jump();
        }

        if (IsControlEnabled && _isJumping && Input.GetButtonUp("Jump"))
        {
            StopJump();
        }

        Ko.Velocity.x = Input.GetAxisRaw("Horizontal") * _moveSpeed;

        _toString = 
            $"Delta 1: {delta1}\n" +
            $"Delta 2: {delta2}\n" +
            $"Should jump: {delta1 <= _coyoteTime && delta2 <= _jumpBuffer}\n" +
            $"Is jumping: {_isJumping}";
    }

    public void Jump()
    {
        _isJumping = true;
        Ko.Velocity.y = JumpSpeed;
        Ko.Gravity = Gravity1;
    }

    public void StopJump()
    {
        Ko.Gravity = Gravity2;
    }
}