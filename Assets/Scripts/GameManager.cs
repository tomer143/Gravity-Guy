using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    MainMenu,
    GetReady,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const string BEST_SCORE_KEY = "GravityGuy_BestScore";

    [Header("References")]
    [SerializeField] private GameConfig config;
    [SerializeField] private PlayerController player;
    [SerializeField] private Spawner spawner;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Camera mainCamera;

    private GameState currentState = GameState.MainMenu;
    private float distanceTraveled = 0f;
    private int currentScore = 0;
    private int speedLevel = 0;
    private float currentSpeed = 0f;
    private int bestScore = 0;
    private bool isPaused = false;
    private bool canRetry = false;
    private Coroutine lockoutCoroutine;

    public GameState CurrentState => currentState;
    public int CurrentScore => currentScore;
    public float CurrentSpeed => currentSpeed > 0f ? currentSpeed : (config != null ? config.runSpeed : 0f);
    public int SpeedLevel => speedLevel;
    public int BestScore => bestScore;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
    }

    private void Start()
    {
        if (player != null)
        {
            player.onFlipped += HandlePlayerFlipped;
            player.onDied += HandlePlayerDied;
        }

        EnterMainMenu();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.onFlipped -= HandlePlayerFlipped;
            player.onDied -= HandlePlayerDied;
        }
    }

    public void EnterMainMenu()
    {
        currentState = GameState.MainMenu;
        Time.timeScale = 1f;
        isPaused = false;
        canRetry = false;
        SetSpeedLevel(0);

        if (spawner != null)
        {
            spawner.ResetSpawner();
            spawner.SetScrolling(false);
        }

        if (player != null)
        {
            player.ResetPlayer();
            player.SetControlsActive(false);
        }

        if (uiManager != null)
        {
            uiManager.ShowMainMenu(bestScore);
        }
    }

    public void EnterGetReady()
    {
        currentState = GameState.GetReady;
        Time.timeScale = 1f;
        isPaused = false;
        canRetry = false;
        distanceTraveled = 0f;
        currentScore = 0;
        SetSpeedLevel(0);

        if (spawner != null)
        {
            spawner.ResetSpawner();
            spawner.SetScrolling(false);
        }

        if (player != null)
        {
            player.ResetPlayer();
            player.SetControlsActive(false);
        }

        if (uiManager != null)
        {
            uiManager.ShowGetReady(bestScore);
        }
    }

    public void StartPlaying()
    {
        if (currentState != GameState.GetReady) return;

        currentState = GameState.Playing;

        if (spawner != null)
        {
            spawner.SetScrolling(true);
        }

        if (player != null)
        {
            player.SetControlsActive(true);
        }

        if (uiManager != null)
        {
            uiManager.ShowPlaying();
        }
    }

    private void Update()
    {
        // Global Pause Toggle via Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentState == GameState.Playing || currentState == GameState.GetReady)
            {
                TogglePause();
            }
        }

        if (isPaused) return;

        // State-specific input
        if (currentState == GameState.MainMenu)
        {
            if (WasAnyTapOrSpacePressed())
            {
                EnterGetReady();
            }
        }
        else if (currentState == GameState.GetReady)
        {
            if (WasAnyTapOrSpacePressed())
            {
                StartPlaying();
            }
        }
        else if (currentState == GameState.GameOver)
        {
            if (canRetry && WasAnyTapOrSpacePressed())
            {
                OnRetryClicked();
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentState != GameState.Playing || isPaused) return;

        if (config != null)
        {
            float dt = Time.fixedDeltaTime;
            distanceTraveled += CurrentSpeed * dt;
            int newScore = Mathf.FloorToInt(distanceTraveled / Mathf.Max(0.01f, config.distanceUnitsPerPoint));
            if (newScore != currentScore)
            {
                currentScore = newScore;
                if (uiManager != null)
                {
                    uiManager.UpdateScore(currentScore);
                }

                int newSpeedLevel = currentScore / Mathf.Max(1, config.pointsPerSpeedStep);
                if (newSpeedLevel != speedLevel)
                {
                    SetSpeedLevel(newSpeedLevel);
                }
            }
        }
    }

    private void SetSpeedLevel(int level)
    {
        bool increased = level > speedLevel;
        speedLevel = level;

        if (config != null)
        {
            float maxSpeed = Mathf.Max(config.runSpeed, config.maxRunSpeed);
            currentSpeed = Mathf.Min(config.runSpeed + level * config.speedIncreasePerStep, maxSpeed);
        }

        if (uiManager != null)
        {
            uiManager.UpdateSpeed(speedLevel + 1, increased);
        }
    }

    private void HandlePlayerFlipped()
    {
        if (audioManager != null)
        {
            audioManager.PlayFlip();
        }
    }

    private void HandlePlayerDied()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;

        if (spawner != null)
        {
            spawner.SetScrolling(false);
        }

        if (audioManager != null)
        {
            audioManager.PlayDeath();
        }

        if (mainCamera != null)
        {
            CameraShake shake = mainCamera.GetComponent<CameraShake>();
            if (shake != null)
            {
                shake.Shake(0.3f, 0.35f);
            }
        }

        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, bestScore);
            PlayerPrefs.Save();
        }

        canRetry = false;
        if (uiManager != null)
        {
            uiManager.ShowGameOver(currentScore, bestScore, false);
        }

        if (lockoutCoroutine != null) StopCoroutine(lockoutCoroutine);
        lockoutCoroutine = StartCoroutine(PostDeathLockoutCoroutine());
    }

    private IEnumerator PostDeathLockoutCoroutine()
    {
        float lockout = config != null ? config.postDeathLockout : 0.5f;
        yield return new WaitForSecondsRealtime(lockout);

        canRetry = true;
        if (uiManager != null)
        {
            uiManager.SetRetryPromptVisible(true);
        }
    }

    public void OnRetryClicked()
    {
        if (currentState != GameState.GameOver || !canRetry) return;

        EnterGetReady();
    }

    public void TogglePause()
    {
        if (currentState != GameState.Playing && currentState != GameState.GetReady) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (uiManager != null)
        {
            uiManager.ShowPause(isPaused);
        }
    }

    private bool WasAnyTapOrSpacePressed()
    {
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            return true;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        return false;
    }
}
