using UnityEngine;

[CreateAssetMenu(fileName = "SegmentGroup", menuName = "Gravity Guy/Segment Group")]
public class SegmentGroup : ScriptableObject
{
    [Tooltip("Segment prefabs spawned one after the other, in this order")]
    public LevelSegment[] segments;

    [Tooltip("Relative chance of this group being picked compared to other groups")]
    [Min(0f)]
    public float weight = 1f;

    public bool IsValid => segments != null && segments.Length > 0 && weight > 0f;
}
