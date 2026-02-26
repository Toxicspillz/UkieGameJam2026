using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float m_MoveSpeed = 12f;


    // Layers which will stop player slide 
    [SerializeField] private LayerMask m_ObstacleMask;

    private Rigidbody2D m_Rigidbody;

    private Vector2 moveDir;
    [SerializeField] private bool isMoving;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Rigidbody.gravityScale = 0f;
        m_Rigidbody.freezeRotation = true;
    }



    private void FixedUpdate()
    {

        m_Rigidbody.linearVelocity = isMoving ? moveDir * m_MoveSpeed : Vector2.zero;
    }

    /// <summary>
    /// Start a new slide in the pressed direction (cardinal only)
    /// Computes a targetPos using a raycast, then FixedUpdate handles motion
    /// </summary>
    public void SetMove(Vector2 inputDirection)
    {
        if (isMoving)
        {
            return;
        }

        // Collapse to 4-way.
        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))
            moveDir = new Vector2(Mathf.Sign(inputDirection.x), 0f);
        else
            moveDir = new Vector2(0f, Mathf.Sign(inputDirection.y));

        if (moveDir == Vector2.zero) return;

        isMoving = true;
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        int otherLayer = collision.gameObject.layer;
        bool isObstacle = (m_ObstacleMask.value & (1 << otherLayer)) != 0;
        if (!isObstacle)
        {
            return ;
        }
        Debug.Log("made its");
        if (moveDir == Vector2.zero) return;

        isMoving = false;


    }
}

