using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 initialPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        initialPosition = transform.position;
    }

    public void Shake(float duration = 0.25f, float magnitude = 0.25f)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.position = new Vector3(initialPosition.x + x, initialPosition.y + y, initialPosition.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.position = initialPosition;
        shakeCoroutine = null;
    }
}
