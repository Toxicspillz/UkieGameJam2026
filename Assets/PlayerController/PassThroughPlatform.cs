using UnityEngine;
using System.Collections;

/// <summary>
/// Semi-solid platform that player can drop through by pressing down + jump
/// Temporarily disables collider when player drops through
/// </summary>
public class PassThroughPlatform : MonoBehaviour
{
    private Collider2D m_Collider;
    private bool m_playerOnPlatform;
    private CharacterMovement m_currentPlayer;
    private InputHandler m_inputHandler;
    [SerializeField] private LayerMask m_PlayerMask;
    [SerializeField] private LayerMask m_SemiSolidMask;

    private void Awake()
    {
        // InputHandler found dynamically on player collision
    }

    private void Start()
    {
        m_Collider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var handler = collision.gameObject.GetComponent<InputHandler>();
        if (handler != null)
        {
            m_inputHandler = handler;
            m_inputHandler.OnDropThrough += startRoutine;
            m_playerOnPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        var handler = collision.gameObject.GetComponent<InputHandler>();
        if (handler != null && handler == m_inputHandler)
        {
            m_inputHandler.OnDropThrough -= startRoutine;
            m_inputHandler = null;
            m_currentPlayer = null;

            m_playerOnPlatform = false;
        }
    }

    private void startRoutine()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        StartCoroutine(DropThroughCollider());
    }

    // Briefly disables collider to let player fall through
    private IEnumerator DropThroughCollider()
    {
        m_Collider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        m_Collider.enabled = true;
    }
}
