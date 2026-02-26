using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

/// <summary>
/// Detects when player is grounded and broadcasts ground state events
/// Tracks multiple ground contacts and void triggers
/// </summary>
public class GroundCheck : MonoBehaviour
{
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] private LayerMask m_SemiSolidLayer;
    [SerializeField] private LayerMask m_voidLayer;

    public bool m_IsGrounded { get; private set; }

    public event Action OnGrounded;
    public event Action OnLeftGround;
    public event Action PlayerEnteredVoid;

    private int groundContacts = 0;
    private Collider2D m_LastGroundCollider;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsVoidLayer(other))
        {
            StartCoroutine(DelayedVoidInvoke());
        }

        if (IsGroundedLayer(other))
        {
            m_LastGroundCollider = other;
            groundContacts++;
            if (groundContacts == 1)
            {
                m_IsGrounded = true;
                OnGrounded?.Invoke();
            }
        }
    }

    // Waits one frame to ensure checkpoint event processes first
    private IEnumerator DelayedVoidInvoke()
    {
        yield return null;
        PlayerEnteredVoid?.Invoke();
    }

    public Collider2D GetCurrentSemiSolid()
    {
        return m_LastGroundCollider;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsGroundedLayer(other))
        {
            groundContacts--;
            if (groundContacts <= 0)
            {
                groundContacts = 0;
                m_IsGrounded = false;
                OnLeftGround?.Invoke();
            }
        }
    }

    private bool IsGroundedLayer(Collider2D col)
    {
        int layerMask = (m_GroundLayer | m_SemiSolidLayer);
        return (layerMask & (1 << col.gameObject.layer)) != 0;
    }

    private bool IsVoidLayer(Collider2D col)
    {
        int layerMask = m_voidLayer;
        return (layerMask & (1 << col.gameObject.layer)) != 0;
    }
}
