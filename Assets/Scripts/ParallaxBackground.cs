using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private float scrollRatio = 0.25f;
    [SerializeField] private float width = 20.0f;
    [SerializeField] private Transform[] backgroundParts;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        float speed = config != null ? config.runSpeed * scrollRatio : 1.5f;
        float moveDistance = speed * Time.deltaTime;

        if (backgroundParts == null || backgroundParts.Length == 0) return;

        for (int i = 0; i < backgroundParts.Length; i++)
        {
            Transform part = backgroundParts[i];
            part.position += Vector3.left * moveDistance;

            if (part.position.x <= -width)
            {
                // Wrap around to right
                part.position += Vector3.right * (width * backgroundParts.Length);
            }
        }
    }
}
