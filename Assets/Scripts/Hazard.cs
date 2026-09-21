using UnityEngine;

[DisallowMultipleComponent]
public class Hazard : MonoBehaviour
{
    private void Reset()
    {
        gameObject.tag = "Hazard";
    }
}
