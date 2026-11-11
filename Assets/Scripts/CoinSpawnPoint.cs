using UnityEngine;

public class CoinSpawnPoint : MonoBehaviour
{
    [SerializeField] private Vector2 direction = Vector2.right;
    public Vector2 Direction => direction;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.08f);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction.normalized * 0.2f);
    }
}