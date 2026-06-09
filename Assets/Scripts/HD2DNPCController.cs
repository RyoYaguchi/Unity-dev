using UnityEngine;

public class HD2DNPCController : MonoBehaviour
{
    public Sprite[] sprites; // [0-2] Right, [3-5] Left, [6-8] Front (Down), [9-11] Back (Up)
    public Vector3[] patrolWaypoints;
    public float moveSpeed = 1.4f;
    public float idleTimeAtWaypoint = 2.0f;
    public float animFrameRate = 6f;

    private SpriteRenderer sr;
    private Rigidbody rb;
    private NPCDialogue dialogue;

    private int currentWaypointIndex = 0;
    private bool isIdle = false;
    private float idleTimer = 0f;
    private Vector3 currentVelocity = Vector3.zero;
    
    private int currentDirection = 2; // Default Front/Down
    private bool isMoving = false;
    private float animTimer = 0f;
    private int animFrame = 0;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
        dialogue = GetComponent<NPCDialogue>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.isKinematic = true; // Use kinematic so NPCs glide nicely and don't knock physics out
        }

        // Snapping starting position to the first waypoint if available
        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            transform.position = patrolWaypoints[0];
        }
    }

    void Update()
    {
        // 1. Check if talking to player - if so, freeze and face the player!
        if (dialogue != null && dialogue.isTalking)
        {
            isMoving = false;
            FacePlayer();
            AnimateCharacter();
            return;
        }

        // 2. Normal Patrol AI
        if (patrolWaypoints == null || patrolWaypoints.Length < 2)
        {
            isMoving = false;
            AnimateCharacter();
            return;
        }

        if (isIdle)
        {
            isMoving = false;
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTimeAtWaypoint)
            {
                isIdle = false;
                idleTimer = 0f;
                // Move to next waypoint
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            }
        }
        else
        {
            Vector3 targetPos = patrolWaypoints[currentWaypointIndex];
            // Match NPC's current Y height to terrain
            targetPos.y = transform.position.y; 

            Vector3 diff = targetPos - transform.position;
            float dist = diff.magnitude;

            if (dist < 0.15f)
            {
                // Reached waypoint - pause here
                isIdle = true;
                isMoving = false;
                idleTimer = 0f;
            }
            else
            {
                isMoving = true;
                Vector3 dir = diff.normalized;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

                // Set direction facing
                float absH = Mathf.Abs(dir.x);
                float absV = Mathf.Abs(dir.z);
                if (absH > absV + 0.1f)
                {
                    currentDirection = dir.x > 0f ? 0 : 1; // Right or Left
                }
                else if (absV > absH + 0.1f)
                {
                    currentDirection = dir.z > 0f ? 3 : 2; // Back or Front
                }
            }
        }

        AnimateCharacter();
    }

    void FacePlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Vector3 dir = player.transform.position - transform.position;
            float absH = Mathf.Abs(dir.x);
            float absV = Mathf.Abs(dir.z);
            if (absH > absV + 0.15f)
            {
                currentDirection = dir.x > 0f ? 0 : 1; // Face Right or Left
            }
            else
            {
                currentDirection = dir.z > 0f ? 3 : 2; // Face Back or Front
            }
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
                animFrame = animFrame == 1 ? 2 : 1; // alternate walks
            }
        }
        else
        {
            animFrame = 0; // Idle frame
            animTimer = 0f;
        }

        int index = currentDirection * 3 + animFrame;
        if (index >= 0 && index < sprites.Length)
        {
            sr.sprite = sprites[index];
        }
    }
}
