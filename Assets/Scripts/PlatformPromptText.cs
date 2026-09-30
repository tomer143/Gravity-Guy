using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PlatformPromptText : MonoBehaviour
{
    [SerializeField] private string action = "TO START";

    public static string InputVerb => Application.isMobilePlatform ? "TAP SCREEN" : "PRESS SPACE";

    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text = $"{InputVerb} {action}";
    }
}
