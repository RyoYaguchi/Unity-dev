using UnityEngine;

public class HD2DPlayerController : MonoBehaviour
{
    public float moveSpeed = 4.5f;
    public Sprite[] sprites; // Sliced sprites: [0-2] Right, [3-5] Left, [6-8] Front (Down), [9-11] Back (Up)
    public float animFrameRate = 8f;

    private Rigidbody rb;
    private SpriteRenderer sr;
    private Vector3 moveInput;
    private int currentDirection = 2; // Default facing Front/Down (Y=2 in sheet)
    private bool isMoving = false;
    private float animTimer = 0f;
    private int animFrame = 0; // 0 = Idle, 1 = Walk 1, 2 = Walk 2

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sr = GetComponent<SpriteRenderer>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // Gather input with modern Input System compatibility
        float h = 0f;
        float v = 0f;
#if ENABLE_INPUT_SYSTEM
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) v = 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) v = -1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) h = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h = 1f;
        }
#else
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
#endif
        moveInput = new Vector3(h, 0f, v).normalized;

        isMoving = moveInput.sqrMagnitude > 0.01f;

        // Determine facing direction with diagonal deadzone to prevent visual jittering
        if (isMoving)
        {
            float absH = Mathf.Abs(h);
            float absV = Mathf.Abs(v);
            if (absH > absV + 0.15f)
            {
                currentDirection = h > 0f ? 0 : 1; // Right (Row 0) or Left (Row 1)
            }
            else if (absV > absH + 0.15f)
            {
                currentDirection = v > 0f ? 3 : 2; // Back/Up (Row 3) or Front/Down (Row 2)
            }
        }

        AnimateCharacter();
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Unity 6 modern API (rb.linearVelocity)
            rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, moveInput.z * moveSpeed);
        }
    }

    void AnimateCharacter()
    {
        if (sprites == null || sprites.Length < 12 || sr == null) return;

        if (isMoving)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / animFrameRate)
            {
                animTimer = 0f;
                // Alternating walk frames: 1 -> 2 -> 1 -> 2...
                animFrame = animFrame == 1 ? 2 : 1;
            }
        }
        else
        {
            animFrame = 0; // Idle frame
            animTimer = 0f;
        }

        // Calculate Sprite index in array
        int spriteIndex = currentDirection * 3 + animFrame;
        if (spriteIndex >= 0 && spriteIndex < sprites.Length)
        {
            sr.sprite = sprites[spriteIndex];
        }
    }
}
