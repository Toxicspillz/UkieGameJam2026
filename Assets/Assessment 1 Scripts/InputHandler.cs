using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Central input handler that binds player input actions to movement, jumping, and interaction
/// Listens to Unity Input System and forwards events to relevant components
/// </summary>
public class InputHandler : MonoBehaviour
{
    private PlayerControls m_ActionMap;
    [SerializeField] private CharacterMovement m_CharacterMovement;
    [SerializeField] private LayerMask m_InteractLayer;

    public event Action OnDropThrough;
    public event Action OnPausePressed;

    [SerializeField] private bool m_IsInitialized = false;

    private void Awake()
    {
        m_ActionMap = new PlayerControls();
        PlayerReady(gameObject);
    }

    public void PlayerReady(GameObject player)
    {
        if (player == gameObject && !m_IsInitialized)
        {
            InitializeInputSystem();
            m_IsInitialized = true;
        }
    }

    #region Bindings
    #region Initialization
    public void InitializeInputSystem()
    {
        if (m_IsInitialized)
            return;
        m_ActionMap.Enable();

        // Bind actions once on initialization
        m_ActionMap.Default.MoveHoriz.performed += Handle_MovePerformed;
        m_ActionMap.Default.MoveHoriz.canceled += Handle_MoveCancelled;
        m_ActionMap.Default.Jump.performed += Handle_JumpPerformed;
        m_ActionMap.Default.Jump.canceled += Handle_JumpCancelled;
        // m_ActionMap.Player.Interact.performed += Handle_InteractPerformed;
        m_ActionMap.Default.MoveVertical.performed += Handle_VerticalPerformed;
        // m_ActionMap.Player.Crouch.performed += Handle_Crouch;
        // m_ActionMap.Player.Crouch.canceled += Handle_Crouch;
        // m_ActionMap.Player.Pause.performed += Handle_PausePerformed;

        m_IsInitialized = true;
        Debug.unityLogger.Log("Initializing input system");
    }
    #endregion

    private void OnDisable()
    {
        // Prevent errors when scene reloads or player despawns
        if (m_IsInitialized)
        {
            m_ActionMap.Disable();

            m_ActionMap.Default.MoveHoriz.performed -= Handle_MovePerformed;
            m_ActionMap.Default.MoveHoriz.canceled -= Handle_MoveCancelled;
            m_ActionMap.Default.Jump.performed -= Handle_JumpPerformed;
            m_ActionMap.Default.Jump.canceled -= Handle_JumpCancelled;
            // m_ActionMap.Player.Interact.performed -= Handle_InteractPerformed;
            m_ActionMap.Default.MoveVertical.performed -= Handle_VerticalPerformed;
            // m_ActionMap.Player.Crouch.performed -= Handle_Crouch;
            // m_ActionMap.Player.Crouch.canceled -= Handle_Crouch;
            // m_ActionMap.Player.Pause.performed -= Handle_PausePerformed;
        }
    }
    #endregion

    private void Handle_PausePerformed(InputAction.CallbackContext context)
    {
        OnPausePressed?.Invoke();
    }

    private void Handle_JumpPerformed(InputAction.CallbackContext context)
    {
        m_CharacterMovement.JumpPerformed();
    }

    // Detects downward input for dropping through platforms
    private void Handle_VerticalPerformed(InputAction.CallbackContext context)
    {
        float vertical = context.ReadValue<float>();
        if (vertical < 0)
        {
            OnDropThrough?.Invoke();
        }
    }

    private void Handle_Crouch(InputAction.CallbackContext context)
    {
        m_CharacterMovement.CrouchPerformed(context);
    }

    private void Handle_JumpCancelled(InputAction.CallbackContext context)
    {
        m_CharacterMovement.JumpCancelled();
    }

    private void Handle_MovePerformed(InputAction.CallbackContext context)
    {
        m_CharacterMovement.SetInMove(context.ReadValue<float>());
        Debug.Log("MOVE");
    }

    private void Handle_MoveCancelled(InputAction.CallbackContext context)
    {
        m_CharacterMovement.SetInMove(0);
    }

    // Checks for nearby interactable objects and triggers interaction
    private void Handle_InteractPerformed(InputAction.CallbackContext context)
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, 1, m_InteractLayer);

        if (col != null && col.transform.TryGetComponent<IInteractable>(out var interactable))
        {
            interactable.Interact();
        }
    }

    private void OnDestroy()
    {
        PlayerInitializer.OnPlayerReady -= PlayerReady;
    }
}
