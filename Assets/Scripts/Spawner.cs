using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private SegmentPool pool;
    [SerializeField] private GameConfig config;

    [Header("Spawn Settings")]
    [SerializeField] private float despawnX = -18.0f;
    [SerializeField] private float spawnAheadX = 35.0f;
    [SerializeField] private int initialSafeSegments = 3;

    private float nextSpawnX = 0f;
    private bool isScrolling = false;

    public void Setup(SegmentPool segmentPool, GameConfig gameConfig)
    {
        pool = segmentPool;
        config = gameConfig;
    }

    public void ResetSpawner()
    {
        if (pool == null) return;
        pool.ReturnAll();

        // Start spawning from left behind the runner (-10) up to spawnAheadX
        float startX = -10.0f;
        nextSpawnX = startX;

        // Spawn initial safe segments
        for (int i = 0; i < initialSafeSegments; i++)
        {
            SpawnSegment(isHazardAllowed: false);
        }

        // Fill remaining up to spawnAheadX
        while (nextSpawnX < spawnAheadX)
        {
            SpawnSegment(isHazardAllowed: true);
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

        float distance = config.runSpeed * Time.deltaTime;
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
            SpawnSegment(isHazardAllowed: true);
        }
    }

    private void SpawnSegment(bool isHazardAllowed)
    {
        if (pool == null || pool.PrefabCount == 0) return;

        int selectedPrefabIndex = 0; // default to clear segment

        if (isHazardAllowed && pool.PrefabCount > 1)
        {
            float roll = Random.value;
            if (roll < config.hazardDensity)
            {
                // Select a hazard prefab (index 1 to PrefabCount - 1)
                selectedPrefabIndex = Random.Range(1, pool.PrefabCount);
            }
        }

        LevelSegment seg = pool.GetSegment(selectedPrefabIndex);
        if (seg != null)
        {
            seg.transform.position = new Vector3(nextSpawnX, 0f, 0f);
            nextSpawnX += config.segmentLength;
        }
    }
}
