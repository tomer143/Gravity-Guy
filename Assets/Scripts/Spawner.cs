using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private SegmentPool pool;
    [SerializeField] private GameConfig config;

    [Header("Spawn Settings")]
    [SerializeField] private float despawnX = -18.0f;
    [SerializeField] private float spawnAheadX = 35.0f;
    [SerializeField] private int initialSafeSegments = 3;

    [Header("Segment Groups")]
    [Tooltip("Groups of segments spawned back to back. When empty, random single hazard segments are spawned instead.")]
    [SerializeField] private SegmentGroup[] segmentGroups;

    private float nextSpawnX = 0f;
    private bool isScrolling = false;
    private readonly Queue<LevelSegment> pendingGroupSegments = new();

    public void Setup(SegmentPool segmentPool, GameConfig gameConfig)
    {
        pool = segmentPool;
        config = gameConfig;
    }

    public void ResetSpawner()
    {
        if (pool == null) return;
        pool.ReturnAll();
        pendingGroupSegments.Clear();

        // Start spawning from left behind the runner (-10) up to spawnAheadX
        float startX = -10.0f;
        nextSpawnX = startX;

        // Spawn initial safe segments
        for (int i = 0; i < initialSafeSegments; i++)
        {
            SpawnSafeSegment();
        }

        // Fill remaining up to spawnAheadX
        while (nextSpawnX < spawnAheadX)
        {
            SpawnHazardSegment();
        }

        isScrolling = false;
    }

    public void SetScrolling(bool scroll)
    {
        isScrolling = scroll;
    }

    private void Update()
    {
        if (!isScrolling || pool == null || config == null) return;

        float speed = GameManager.Instance != null ? GameManager.Instance.CurrentSpeed : config.runSpeed;
        float distance = speed * Time.deltaTime;
        nextSpawnX -= distance;

        // Move all active segments
        var active = pool.ActiveSegments;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            LevelSegment seg = active[i];
            seg.Move(distance);

            // Check if segment is fully off-screen to the left
            if (seg.transform.position.x + config.segmentLength < despawnX)
            {
                pool.ReturnSegment(seg);
            }
        }

        // Spawn new segments on the right as needed
        while (nextSpawnX < spawnAheadX)
        {
            SpawnHazardSegment();
        }
    }

    private void SpawnSafeSegment()
    {
        if (pool == null || pool.PrefabCount == 0) return;

        PlaceSegment(pool.GetSegment(0));
    }

    private void SpawnHazardSegment()
    {
        if (pool == null) return;

        if (pendingGroupSegments.Count == 0)
        {
            SegmentGroup group = PickGroup();
            if (group != null)
            {
                foreach (LevelSegment prefab in group.segments)
                {
                    if (prefab != null) pendingGroupSegments.Enqueue(prefab);
                }
            }
        }

        if (pendingGroupSegments.Count > 0)
        {
            PlaceSegment(pool.GetSegment(pendingGroupSegments.Dequeue()));
            return;
        }

        if (pool.PrefabCount == 0) return;
        int selectedPrefabIndex = pool.PrefabCount > 1 ? Random.Range(1, pool.PrefabCount) : 0;
        PlaceSegment(pool.GetSegment(selectedPrefabIndex));
    }

    private SegmentGroup PickGroup()
    {
        if (segmentGroups == null) return null;

        float totalWeight = 0f;
        foreach (SegmentGroup group in segmentGroups)
        {
            if (group != null && group.IsValid) totalWeight += group.weight;
        }

        if (totalWeight <= 0f) return null;

        float roll = Random.value * totalWeight;
        SegmentGroup picked = null;
        foreach (SegmentGroup group in segmentGroups)
        {
            if (group == null || !group.IsValid) continue;
            picked = group;
            roll -= group.weight;
            if (roll < 0f) break;
        }
        return picked;
    }

    private void PlaceSegment(LevelSegment seg)
    {
        if (seg == null)
        {
            nextSpawnX += config.segmentLength;
            return;
        }

        seg.transform.position = new Vector3(nextSpawnX, 0f, 0f);
        nextSpawnX += config.segmentLength;
    }
}
