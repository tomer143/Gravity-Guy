using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject getReadyOverlay;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Text Displays")]
    [SerializeField] private TextMeshProUGUI menuBestScoreText;
    [SerializeField] private TextMeshProUGUI hudScoreText;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverBestScoreText;
    [SerializeField] private TextMeshProUGUI retryPromptText;

    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => GameManager.Instance?.TogglePause());

        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => GameManager.Instance?.TogglePause());

        if (retryButton != null)
            retryButton.onClick.AddListener(() => GameManager.Instance?.OnRetryClicked());
    }

    public void ShowMainMenu(int bestScore)
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (hudPanel) hudPanel.SetActive(false);
        if (getReadyOverlay) getReadyOverlay.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);

        if (menuBestScoreText) menuBestScoreText.text = $"BEST: {bestScore}";
    }

    public void ShowGetReady(int bestScore)
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(true);
        if (getReadyOverlay) getReadyOverlay.SetActive(true);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);

        UpdateScore(0);
    }

    public void ShowPlaying()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(true);
        if (getReadyOverlay) getReadyOverlay.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
    }

    public void UpdateScore(int currentScore)
    {
        if (hudScoreText) hudScoreText.text = currentScore.ToString();
    }

    public void ShowGameOver(int finalScore, int bestScore, bool allowRetry)
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (getReadyOverlay) getReadyOverlay.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);

        if (gameOverScoreText) gameOverScoreText.text = $"SCORE: {finalScore}";
        if (gameOverBestScoreText) gameOverBestScoreText.text = $"BEST: {bestScore}";

        if (retryPromptText)
        {
            retryPromptText.gameObject.SetActive(allowRetry);
            retryPromptText.text = "TAP OR SPACE TO RETRY";
        }
    }

    public void SetRetryPromptVisible(bool visible)
    {
        if (retryPromptText) retryPromptText.gameObject.SetActive(visible);
    }

    public void ShowPause(bool isPaused)
    {
        if (pausePanel) pausePanel.SetActive(isPaused);
        if (pauseButton) pauseButton.gameObject.SetActive(!isPaused);
    }
}
