using UnityEngine;

// Un sitio donde pueden aparecer enemigos en este nivel.
public class EnemySpawnPoint : MonoBehaviour
{
    public enum PointKind { Corner, Spark }   // Corner: esquinas de los pájaros. Spark: chispas de las momias (5B).

    [SerializeField] private PointKind kind;
    [Tooltip("Dirección inicial del enemigo al aparecer aquí.")]
    [SerializeField] private Vector2 direction = Vector2.right;

    public PointKind Kind => kind;
    public Vector2 Direction => direction;

    private void OnDrawGizmos()
    {
        Gizmos.color = kind == PointKind.Corner ? Color.red : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction.normalized * 0.2f);
    }
}