using UnityEngine;

public class LevelSegment : MonoBehaviour
{
    [SerializeField] private bool hasHazard = false;
    [SerializeField] private int segmentId = 0;

    public bool HasHazard => hasHazard;
    public int SegmentId => segmentId;

    public System.Action<LevelSegment> onDespawn;

    public void Init(int id, bool isHazard)
    {
        segmentId = id;
        hasHazard = isHazard;
    }

    public void Move(float distance)
    {
        transform.position += Vector3.left * distance;
    }
}
