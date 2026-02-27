using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;
using Unity.Mathematics;

/// <summary>
/// Handles player movement, jumping, ground detection, coyote time, jump buffering,
/// and ledge mechanics. Uses coroutines for smooth movement and gravity control.
/// </summary>
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float m_MoveSpeed = 8f;
    [SerializeField] private float m_JumpStrength = 9f;
    [SerializeField] private float m_Acceleration = 60f;
    [SerializeField] private float m_Deceleration = 80f;
    
    [Header("Speed Smoothing")]
    [SerializeField] private float accelSmoothTime = 0.08f;   // smaller = quicker ramp up
    [SerializeField] private float decelSmoothTime = 0.05f;   // smaller = quicker slow down
    [SerializeField] private float maxSpeedChange = 50f;      // clamps how fast X can change (units/sec)

    private float _xSmoothVel; // required by SmoothDamp 
    
    [Header("VFX/SFX")]
    [SerializeField] private float walkDustDistance = 0.9f;
    private Vector3 lastDustSpawnPos = Vector3.zero;
    
    [Header("Footsteps")]
    [SerializeField] private string[] footstepSfxIds = { "Footstep1"};
    [SerializeField] private float footstepInterval = 0.18f;
    [SerializeField] private float minLocalSpeedForFootsteps = 0.35f;

    private Coroutine _footstepCo;
    private WaitForSeconds _footstepWait;


    [Header("Apex / Gravity")]
    [SerializeField] private float m_AntiGravityThreshold = 0.15f;
    [SerializeField] private float m_NormalGravity = 1f;
    [SerializeField] private float m_ReducedGravity = 0.8f;

    [Header("Coyote & Buffer")]
    [SerializeField] private float m_CoyoteTimeThreshold = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask m_GroundLayer;
    private FloatingPlatform m_CurrentPlatform;
    private Vector2 m_PlatformVelocity;

    private Rigidbody2D m_RB;
    private PlayerControls m_ActionMap;

    [SerializeField] private bool m_IsGrounded;
    private bool m_JumpHeld;
    private float m_InMove;
    private float m_CoyoteTimeCounter;
    private float m_LastJumpPressedTime;

    public event Action OnJumpStarted;
    public event Action OnJumpEnded;
    public event Action OnLanded;
    public event Action OnMoveStarted;

    private Coroutine coyoteCoroutine;
    private Coroutine jumpBufferCoroutine;
    private Coroutine m_CGravity;

    private bool m_IsGravityActive;

    private Coroutine m_CMove;
    private bool m_IsMoveActive;

    [SerializeField] GroundCheck GroundCheck;
    private bool m_IsInitialized = false;

    [SerializeField] private Collider2D m_PlayerCollider;

    [Header("Hold Crouch to Stay on Ledge")]
    [SerializeField] private float ledgeCheckDistance = 0.2f;
    private bool m_IsHoldingCrouch;

    [Header("Sticky Feet")]
    [SerializeField] private bool enableStickyFeet = true;
    [SerializeField] private float stickyFeetLockTime = 0.08f;
    [SerializeField] private JumpStates m_JumpState = JumpStates.Grounded;
    private bool m_LandingLockActive = false;

    // Briefly locks horizontal movement after landing for tighter control
    private IEnumerator C_StickyFeetLock()
    {
        m_LandingLockActive = true;

        yield return new WaitForFixedUpdate();
        _xSmoothVel = 0f;

        // Landing Sfx
        if (!SfxManager.Instance.IsSfxPlaying("Landing1")) {
            SfxManager.Instance.PlaySfx("Landing1");
        }
        
        // Landing Dust Vfx
        Vector3 dustPos = transform.position + new Vector3(0, 0.5f, 0);
        float shockScale = Mathf.Clamp(Mathf.Abs(m_RB.linearVelocity.y) * 0.04f, 1f, 2f);
        VfxManager.Instance.PlayVFX("LandDust", dustPos, Quaternion.identity, shockScale);
        
        yield return new WaitForSeconds(stickyFeetLockTime);

        m_LandingLockActive = false;
    }

    public void CrouchPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            m_IsHoldingCrouch = true;

        }
        else if (ctx.canceled)
        {
            m_IsHoldingCrouch = false;

        }
    }

    // Returns -1 (left ledge), 0 (not on ledge), or 1 (right ledge)
    private int IsOnLedge()
    {
        if (!m_IsHoldingCrouch || !m_IsGrounded)
        {
            return 0;
        }

        Bounds bounds = m_PlayerCollider.bounds;
        float footOffset = 0.01f;

        // Check for ground under either foot
        Vector2 leftFoot = new Vector2(bounds.min.x + footOffset, bounds.min.y);
        Vector2 rightFoot = new Vector2(bounds.max.x - footOffset, bounds.min.y);

        bool leftGround = Physics2D.Raycast(leftFoot, Vector2.down, ledgeCheckDistance, m_GroundLayer);
        bool rightGround = Physics2D.Raycast(rightFoot, Vector2.down, ledgeCheckDistance, m_GroundLayer);

        bool isOnLedgeRight = leftGround && !rightGround;
        bool isOnLedgeLeft = rightGround && !leftGround;

#if UNITY_EDITOR
        Debug.DrawRay(leftFoot, Vector2.down * ledgeCheckDistance, leftGround ? Color.green : Color.red);
        Debug.DrawRay(rightFoot, Vector2.down * ledgeCheckDistance, rightGround ? Color.green : Color.red);
#endif

        if (isOnLedgeLeft)
        {
            return -1;
        }
        if (isOnLedgeRight)
        {
            return 1;
        }
        return 0;
    }

    private enum JumpStates
    {
        Grounded,
        Ascending,
        Apex,
        Falling
    };

    public void PlayerReady(GameObject player)
    {
        if (player == gameObject && !m_IsInitialized)
        {
            InitializeMovementSystem();
            m_IsInitialized = true;
        }
    }

    private void Awake()
    {
        m_ActionMap = new PlayerControls();
        m_RB = GetComponent<Rigidbody2D>();
        
        _footstepWait = new WaitForSeconds(footstepInterval); // coroutine wait instruction [web:268]

    }

    private void FixedUpdate()
    {
        RefreshPlatformVelocity();
        ApplyHorizontalMovement(); // always run, so we can decelerate smoothly 

        if (ShouldPlayFootsteps()) StartFootsteps();
        else StopFootsteps();

    }

    
    #region Bindings
    private void OnEnable()
    {
        m_ActionMap.Enable();
        m_IsGravityActive = true;

        PlayerInitializer.OnPlayerReady += PlayerReady;
    }

    public void InitializeMovementSystem()
    {
        m_RB = GetComponent<Rigidbody2D>();
        GroundCheck.OnGrounded += HandleGrounded;
        GroundCheck.OnLeftGround += HandleLeftGrounded;
        m_IsGravityActive = true;
    }

    private void OnDisable()
    {
        m_ActionMap.Disable();
        m_IsGravityActive = false;
        if (m_CGravity != null)
            StopCoroutine(m_CGravity);
        GroundCheck.OnGrounded -= HandleGrounded;
        GroundCheck.OnLeftGround -= HandleLeftGrounded;
        PlayerInitializer.OnPlayerReady -= PlayerReady;
    }

    #endregion

    #region InputFunctions
    public void SetInMove(float direction)
    {
        m_InMove = direction;

        if (m_InMove == 0)
        {
            m_IsMoveActive = false;
        }
        else
        {
            if (m_IsMoveActive) { return; }
            m_IsMoveActive = true;
            m_CMove = StartCoroutine(C_MoveUpdate());
            m_CGravity = StartCoroutine(C_JumpLoop());
            OnMoveStarted?.Invoke();
        }
    }

    private IEnumerator C_MoveUpdate()
    {
        while (m_IsMoveActive)
        {
            yield return new WaitForFixedUpdate();
            ApplyHorizontalMovement();
        }
    }

    // Continuously updates jump state and gravity
    private IEnumerator C_JumpLoop()
    {
        while (m_IsGravityActive)
        {
            yield return new WaitForFixedUpdate();
            UpdateJumpState();
            ApplyJumpGravity();
        }
    }

    // Determines current jump phase based on velocity
    private void UpdateJumpState()
    {
        float vY = m_RB.linearVelocity.y;

        if (m_IsGrounded && vY <= 0.01f)
        {
            m_JumpState = JumpStates.Grounded;
        }
        else if (vY > 0.1f)
        {
            m_JumpState = JumpStates.Ascending;
        }
        else if (Mathf.Abs(vY) <= m_AntiGravityThreshold)
        {
            m_JumpState = JumpStates.Apex;
        }
        else if (vY <= -0.1f)
        {
            m_JumpState = JumpStates.Falling;
        }
    }

    // Applies gravity modifiers based on jump state
    private void ApplyJumpGravity()
    {
        switch (m_JumpState)
        {
            case JumpStates.Ascending:
                m_RB.gravityScale = m_JumpHeld ? m_ReducedGravity : m_NormalGravity * 1.2f;
                break;
            case JumpStates.Grounded:
                m_RB.gravityScale = m_NormalGravity;
                break;
            case JumpStates.Falling:
                m_RB.gravityScale = m_NormalGravity * 1.8f;
                break;
            case JumpStates.Apex:
                m_RB.gravityScale = m_NormalGravity * 1.0f;
                break;
        }
    }

    public void JumpPerformed()
    {
        m_JumpHeld = true;
        m_LastJumpPressedTime = Time.time;

        // Start jump buffer to allow early jump input
        if (jumpBufferCoroutine != null)
        {
            StopCoroutine(jumpBufferCoroutine);
        }

        jumpBufferCoroutine = StartCoroutine(JumpBufferRoutine());
    }

    public void JumpCancelled()
    {
        m_JumpHeld = false;

        // Cut jump short for variable jump height
        if (m_RB.linearVelocity.y > 0)
        {
            m_RB.linearVelocity = new Vector2(m_RB.linearVelocity.x, m_RB.linearVelocity.y * 0.5f);
            OnJumpEnded?.Invoke();
        }
    }

    public void Jump()
    {
        StopFootsteps();

        m_RB.linearVelocityY = 0f;
        m_RB.AddForce(Vector2.up * m_JumpStrength, ForceMode2D.Impulse);
        m_CoyoteTimeCounter = 0;

        // Jump Dust Vfx
        Vector3 dustPos = transform.position + new Vector3(0, 0.5f, 0);
        VfxManager.Instance.PlayVFX("JumpDust", dustPos, Quaternion.identity, 1f);
                
        // 1st Jump Sfx
        if (!SfxManager.Instance.IsSfxPlaying("Jump1"))
        {
            SfxManager.Instance.PlaySfx("Jump1");
        }

        OnJumpStarted?.Invoke();
    }

    private void ApplyHorizontalMovement()
    {
        // // Prevent walking off ledge while crouching
        // int ledgeCheck = IsOnLedge();
        // if (m_IsGrounded && m_IsHoldingCrouch && ledgeCheck != 0)
        // {
        //     if ((ledgeCheck == 1 && m_InMove > 0) || (ledgeCheck == -1 && m_InMove < 0))
        //     {
        //         RefreshPlatformVelocity();
        //         m_RB.linearVelocityX = m_PlatformVelocity.x;
        //         return;
        //     }
        // }


        RefreshPlatformVelocity(); 
        // If landing lock is active, ignore input but still allow platform carry + smooth decel
        float input = m_LandingLockActive ? 0f : m_InMove;

        // Prevent walking off ledge while crouching (convert your early-return into "input = 0")
        int ledgeCheck = IsOnLedge();
        if (m_IsGrounded && m_IsHoldingCrouch && ledgeCheck != 0)
        {
            if ((ledgeCheck == 1 && input > 0) || (ledgeCheck == -1 && input < 0))
                input = 0f;
        }

        if (m_LandingLockActive)
            return; // Ignore horizontal input during sticky feet lock
        
        // Target speed (cap is m_MoveSpeed already)
        float targetLocalX = input * m_MoveSpeed;

        // Add platform carry so standing still keeps relative position
        float targetWorldX = targetLocalX + m_PlatformVelocity.x;

        // Choose accel vs decel smooth time
        float st = Mathf.Abs(targetLocalX) > 0.01f ? accelSmoothTime : decelSmoothTime;

        // Smoothly ease current X toward target X (no snapping) 
        float newX = Mathf.SmoothDamp(
            m_RB.linearVelocity.x,
            targetWorldX,
            ref _xSmoothVel,
            st,
            maxSpeedChange,
            Time.fixedDeltaTime
        );

        m_RB.linearVelocity = new Vector2(newX, m_RB.linearVelocity.y);

        // VFX dust spawning
        TrySpawnRunDust();

    }

    // Grace period after leaving ground for jump input
    private IEnumerator CoyoteTimeRoutine()
    {
        m_CoyoteTimeCounter = m_CoyoteTimeThreshold;
        while (m_CoyoteTimeCounter > 0)
        {
            m_CoyoteTimeCounter -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        coyoteCoroutine = null;
    }

    // Allows jump input slightly before landing
    private IEnumerator JumpBufferRoutine()
    {
        float endTime = Time.time + jumpBufferTime;
        while (Time.time < endTime)
        {
            if (GroundCheck.m_IsGrounded || m_CoyoteTimeCounter > 0)
            {
                Jump();
                yield break;
            }
            yield return null;
        }
        jumpBufferCoroutine = null;
    }
    #endregion

    private void HandleGrounded()
    {
        m_IsGrounded = true;

        RefreshCurrentPlatform();
        RefreshPlatformVelocity();

        if (enableStickyFeet)
            StartCoroutine(C_StickyFeetLock());

        if (coyoteCoroutine != null)
        {
            StopCoroutine(coyoteCoroutine);
            coyoteCoroutine = null;
        }
    }

    private void HandleLeftGrounded()
    {
        m_IsGrounded = false;

        m_CurrentPlatform = null;
        m_PlatformVelocity = Vector2.zero;

        if (coyoteCoroutine == null)
            coyoteCoroutine = StartCoroutine(CoyoteTimeRoutine());
    }

    
    private void RefreshCurrentPlatform()
    {
        m_CurrentPlatform = null;

        var groundCol = GroundCheck != null ? GroundCheck.GetCurrentSemiSolid() : null;
        if (!groundCol) return;

        // Works if the collider is on a child of the platform root.
        m_CurrentPlatform = groundCol.GetComponentInParent<FloatingPlatform>();
    }

    private void RefreshPlatformVelocity()
    {
        m_PlatformVelocity = (m_IsGrounded && m_CurrentPlatform != null)
            ? m_CurrentPlatform.PlatformVelocity
            : Vector2.zero;
    }

    
    // -- VFX & SFX Helpers --
    private void TrySpawnRunDust()
    {
        if (!m_IsGrounded || Mathf.Abs(m_InMove) == 0f) return;

        float curDistance = walkDustDistance;
        float dist = Vector3.Distance(transform.position, lastDustSpawnPos);

        if (dist >= curDistance)
        {
            Vector3 dustPos = transform.position + new Vector3(0, 0.55f, 0); // tweak offset
            VfxManager.Instance.PlayVFX("RunDust", dustPos, Quaternion.identity, 1f);
            lastDustSpawnPos = transform.position;
        }
    }
    
    private bool ShouldPlayFootsteps()
    {
        if (!m_IsGrounded) return false;
        if (m_LandingLockActive) return false;
        if (SfxManager.Instance == null) return false;
        if (footstepSfxIds == null || footstepSfxIds.Length == 0) return false;

        // LOCAL speed so platforms don't count as "walking"
        float localX = m_RB.linearVelocity.x - m_PlatformVelocity.x;
        return Mathf.Abs(localX) >= minLocalSpeedForFootsteps;
    }

    private void StartFootsteps()
    {
        if (_footstepCo != null) return;
        _footstepCo = StartCoroutine(C_Footsteps());
    }

    private void StopFootsteps()
    {
        if (_footstepCo == null) return;
        StopCoroutine(_footstepCo);
        _footstepCo = null;
    }

    private IEnumerator C_Footsteps()
    {
        while (true)
        {
            if (!ShouldPlayFootsteps())
            {
                _footstepCo = null;
                yield break;
            }

            // Random index: int overload is maxExclusive (good for arrays)
            string id = footstepSfxIds[UnityEngine.Random.Range(0, footstepSfxIds.Length)];
            SfxManager.Instance.PlaySfx(id);

            yield return _footstepWait; // interval between steps
        }
    }


}
