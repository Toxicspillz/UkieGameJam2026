using UnityEngine;

public class PushableBlock : MonoBehaviour
{
    [SerializeField] private LayerMask m_BlockingMask; // obstacles + pushables
    [SerializeField] private float m_Skin = 0.001f;
    [SerializeField] private int m_MaxChain = 16;

    private Rigidbody2D rb;
    private Collider2D col;

    private readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    private ContactFilter2D filter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Recommended: set this RB to Kinematic in Inspector for stable “grid-like” pushing.
        filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(m_BlockingMask);
    }

    public bool TryPush(Vector2 dir, float distance, int depth = 0)
    {
        if (depth > m_MaxChain) return false;

        int count = col.Cast(dir, filter, hits, distance + m_Skin);

        // Find nearest hit (if any)
        RaycastHit2D nearest = default;
        float min = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider == null) continue;
            if (hits[i].distance < min)
            {
                min = hits[i].distance;
                nearest = hits[i];
            }
        }

        // No blocker ahead -> move
        if (count == 0)
        {
            rb.MovePosition(rb.position + dir * distance);
            return true;
        }

        // If hit something within our intended step, we must resolve it
        var otherBlock = nearest.collider.GetComponent<PushableBlock>();
        if (otherBlock != null)
        {
            // Try to push the next block first (chain push)
            if (!otherBlock.TryPush(dir, distance, depth + 1))
                return false;

            rb.MovePosition(rb.position + dir * distance);
            return true;
        }

        // Hit a wall/obstacle
        return false;
    }
}
