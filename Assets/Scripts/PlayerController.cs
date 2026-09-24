using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] runSprites;
    [SerializeField] private float animationFps = 10f;
    [SerializeField] private Sprite[] deathSprites;
    [SerializeField] private float deathAnimationFps = 10f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private BoxCollider2D col;

    private bool isGravityInverted = false;
    private bool isGrounded = false;
    private bool pendingFlip = false;
    private bool isAlive = true;
    private bool isControlsActive = false;

    private float animTimer = 0f;
    private int currentFrame = 0;

    private float deathAnimTimer = 0f;
    private int deathFrame = 0;

    public bool IsGrounded => isGrounded;
    public bool IsGravityInverted => isGravityInverted;
    public bool IsAlive => isAlive;

    public System.Action onFlipped;
    public System.Action onDied;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    public void Setup(GameConfig gameConfig)
    {
        config = gameConfig;
    }

    public void ResetPlayer()
    {
        isAlive = true;
        isControlsActive = false;
        pendingFlip = false;
        isGravityInverted = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = false;
            if (runSprites != null && runSprites.Length > 0)
            {
                spriteRenderer.sprite = runSprites[0];
            }
        }

        float initialY = config != null ? config.floorY + (config.tileThickness * 0.5f) + 0.5f : -2.5f;
        float initialX = config != null ? config.runnerX : -4.5f;
        transform.position = new Vector3(initialX, initialY, 0f);

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = true;
        isGrounded = true;
        currentFrame = 0;
        animTimer = 0f;
        deathFrame = 0;
        deathAnimTimer = 0f;
    }

    public void SetControlsActive(bool active)
    {
        isControlsActive = active;
        pendingFlip = false;
    }

    private void Update()
    {
        if (!isAlive)
        {
            UpdateDeathAnimation();
            return;
        }

        UpdateAnimation();

        if (!isControlsActive) return;

        if (IsPointerOverUI()) return;

        if (WasFlipTriggeredThisFrame() && isGrounded)
        {
            pendingFlip = true;
        }
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;

        CheckGroundStatus();

        if (pendingFlip && isGrounded)
        {
            FlipGravity();
            pendingFlip = false;
        }
        else
        {
            pendingFlip = false;
        }

        float gravityStrength = config != null ? config.gravityStrength : 20f;
        float direction = isGravityInverted ? 1.0f : -1.0f;
        rb.linearVelocity += new Vector2(0f, direction * gravityStrength * Time.fixedDeltaTime);

        float targetX = config != null ? config.runnerX : -4.5f;
        if (Mathf.Abs(transform.position.x - targetX) > 0.01f)
        {
            transform.position = new Vector3(targetX, transform.position.y, 0f);
        }

        if (config != null)
        {
            float minY = config.floorY - 2.5f;
            float maxY = config.ceilingY + 2.5f;
            if (transform.position.y < minY || transform.position.y > maxY)
            {
                Die();
            }
        }
    }

    private void FlipGravity()
    {
        isGravityInverted = !isGravityInverted;

        rb.linearVelocity = new Vector2(0f, isGravityInverted ? 1.5f : -1.5f);

        // Mirror runner sprite vertically
        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = isGravityInverted;
        }

        isGrounded = false;
        onFlipped?.Invoke();
    }

    private void CheckGroundStatus()
    {
        if (col == null) return;

        Vector2 origin = col.bounds.center;
        Vector2 size = new(col.bounds.size.x * 0.85f, 0.12f);
        Vector2 checkDir = isGravityInverted ? Vector2.up : Vector2.down;
        float checkDist = (col.bounds.size.y * 0.5f) + 0.08f;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, checkDir, checkDist, groundLayer);
        isGrounded = hit.collider != null && !hit.collider.isTrigger && !hit.collider.CompareTag("Hazard");
    }

    private void UpdateAnimation()
    {
        if (runSprites == null || runSprites.Length == 0 || spriteRenderer == null) return;

        if (isGrounded && isControlsActive)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / animationFps)
            {
                animTimer -= 1f / animationFps;
                currentFrame = (currentFrame + 1) % runSprites.Length;
                spriteRenderer.sprite = runSprites[currentFrame];
            }
        }
        else
        {
            spriteRenderer.sprite = runSprites[1 % runSprites.Length];
        }
    }

    private void UpdateDeathAnimation()
    {
        if (deathSprites == null || deathSprites.Length == 0 || spriteRenderer == null) return;

        if (deathFrame >= deathSprites.Length - 1) return;

        deathAnimTimer += Time.deltaTime;
        if (deathAnimTimer >= 1f / deathAnimationFps)
        {
            deathAnimTimer -= 1f / deathAnimationFps;
            deathFrame++;
            spriteRenderer.sprite = deathSprites[deathFrame];
        }
    }

    private bool WasFlipTriggeredThisFrame()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.upArrowKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame)
            {
                return true;
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            for (int i = 0; i < Touchscreen.current.touches.Count; i++)
            {
                int touchId = Touchscreen.current.touches[i].touchId.ReadValue();
                if (EventSystem.current.IsPointerOverGameObject(touchId)) return true;
            }
        }

        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isAlive) return;

        if (collision.gameObject.CompareTag("Hazard") || collision.collider.CompareTag("Hazard"))
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;

        if (other.CompareTag("Hazard"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        isControlsActive = false;
        rb.simulated = false;

        deathFrame = 0;
        deathAnimTimer = 0f;
        if (spriteRenderer != null && deathSprites != null && deathSprites.Length > 0)
        {
            spriteRenderer.sprite = deathSprites[0];
        }

        onDied?.Invoke();
    }
}
