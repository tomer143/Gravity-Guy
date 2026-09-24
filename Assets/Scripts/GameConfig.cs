using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Gravity Guy/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Movement & Physics")]
    [Tooltip("Constant horizontal scroll speed of the level (u/s)")]
    public float runSpeed = 6.0f;

    [Tooltip("Downward/upward acceleration applied to runner (u/s²)")]
    public float gravityStrength = 20.0f;

    [Header("Level Spawning")]
    [Tooltip("Width of one pooled level segment, in world units")]
    public float segmentLength = 9.8f;

    [Tooltip("Probability weight of hazard-bearing segment vs clear segment (0 to 1)")]
    [Range(0f, 1f)]
    public float hazardDensity = 0.8f;

    [Tooltip("World units of travel per 1 point of score")]
    public float distanceUnitsPerPoint = 1.0f;

    [Header("Corridor Boundaries")]
    [Tooltip("Center Y of the corridor floor")]
    public float floorY = -3.5f;

    [Tooltip("Center Y of the corridor ceiling")]
    public float ceilingY = 3.5f;

    [Tooltip("Tile thickness in world units")]
    public float tileThickness = 0.7f;

    [Tooltip("Fixed X position of the runner")]
    public float runnerX = -4.5f;

    [Header("Timers")]
    [Tooltip("Input lockout after death in seconds")]
    public float postDeathLockout = 0.5f;
}
