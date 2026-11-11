using UnityEngine;

[CreateAssetMenu(menuName = "BombJack/Flight Config")]
public class FlightConfig : ScriptableObject
{
    [Header("Saltos (altura en unidades de mundo)")]
    public float jumpHeight = 3f;
    public float powerJumpHeight = 8f;
    public float powerJumpWindow = 0.1f;

    [Header("Gravedad (multiplicador sobre Physics2D.gravity)")]
    public float gravityScale = 3f;
    public float glideGravityScale = 0.3f;
    public float fastFallGravityScale = 6f;

    [Header("Velocidad máxima de caída")]
    public float maxFallSpeed = 12f;
    public float glideMaxFallSpeed = 1.5f;
    public float fastFallMaxSpeed = 20f;

    public float SpeedForHeight(float height) =>
        Mathf.Sqrt(2f * height * Physics2D.gravity.magnitude * gravityScale);
}