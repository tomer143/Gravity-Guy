using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SegmentPool : MonoBehaviour
{
    [SerializeField] private LevelSegment[] segmentPrefabs;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 30;

    private Dictionary<int, ObjectPool<LevelSegment>> pools = new Dictionary<int, ObjectPool<LevelSegment>>();
    private readonly List<LevelSegment> activeSegments = new();

    public IReadOnlyList<LevelSegment> ActiveSegments => activeSegments;
    public int PrefabCount => segmentPrefabs != null ? segmentPrefabs.Length : 0;

    private void Awake()
    {
        if (segmentPrefabs != null && segmentPrefabs.Length > 0)
        {
            Initialize(segmentPrefabs);
        }
    }

    public void Initialize(LevelSegment[] prefabs)
    {
        segmentPrefabs = prefabs;
        pools.Clear();

        for (int index = 0; index < segmentPrefabs.Length; index++)
        {
            LevelSegment prefab = segmentPrefabs[index];
            prefab.Init(index, prefab.HasHazard);

            ObjectPool<LevelSegment> pool = new(
                createFunc: () =>
                {
                    LevelSegment seg = Instantiate(prefab, transform);
                    seg.Init(index, prefab.HasHazard);
                    return seg;
                },
                actionOnGet: seg =>
                {
                    seg.gameObject.SetActive(true);
                },
                actionOnRelease: seg =>
                {
                    seg.gameObject.SetActive(false);
                },
                actionOnDestroy: seg =>
                {
                    if (seg != null) Destroy(seg.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxPoolSize
            );

            pools[index] = pool;
        }
    }

    public LevelSegment GetSegment(int prefabIndex)
    {
        if (!pools.TryGetValue(prefabIndex, out var pool))
        {
            Debug.LogError($"[SegmentPool] No pool for index {prefabIndex}");
            return null;
        }

        LevelSegment seg = pool.Get();
        activeSegments.Add(seg);
        return seg;
    }

    public void ReturnSegment(LevelSegment seg)
    {
        if (seg == null) return;

        activeSegments.Remove(seg);
        if (pools.TryGetValue(seg.SegmentId, out var pool))
        {
            pool.Release(seg);
        }
        else
        {
            Destroy(seg.gameObject);
        }
    }

    public void ReturnAll()
    {
        for (int i = activeSegments.Count - 1; i >= 0; i--)
        {
            LevelSegment seg = activeSegments[i];
            if (seg != null && pools.TryGetValue(seg.SegmentId, out var pool))
            {
                pool.Release(seg);
            }
        }
        activeSegments.Clear();
    }
}
