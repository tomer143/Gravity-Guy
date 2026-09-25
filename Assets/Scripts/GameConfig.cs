using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Gravity Guy/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Movement & Physics")]
    [Tooltip("Constant horizontal scroll speed of the level (u/s)")]
    public float runSpeed = 7.0f;

    [Tooltip("Downward/upward acceleration applied to runner (u/s²)")]
    public float gravityStrength = 20.0f;

    [Header("Speed Progression")]
    [Tooltip("Run speed increases every time the score passes a multiple of this many points")]
    [Min(1)]
    public int pointsPerSpeedStep = 100;

    [Tooltip("How much runSpeed increases per step (u/s)")]
    public float speedIncreasePerStep = 0.5f;

    [Tooltip("Run speed never goes above this (u/s)")]
    public float maxRunSpeed = 10.0f;

    [Tooltip("How much gravity grows with speed so a flip covers the same distance. 1 = flips always cover the same distance (every obstacle stays passable), 0 = gravity never changes (flips stretch at high speed)")]
    [Range(0f, 1f)]
    public float flipDistanceCompensation = 1.0f;

    [Header("Level Spawning")]
    [Tooltip("Width of one pooled level segment, in world units")]
    public float segmentLength = 9.8f;

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
