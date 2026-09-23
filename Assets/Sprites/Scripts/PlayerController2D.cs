using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    // Public variables
    public float speed = 5f; // The speed at which the player moves
    public bool canMoveDiagonally = true; // Controls whether the player can move diagonally

    public InputActionReference moveAction;
    public InputActionReference attackAction;

    [Header("Attack")]
    public int attackDamage = 1;
    public float attackRange = 0.75f;
    public float attackRadius = 0.35f;
    public float attackCooldown = 0.35f;
    public float attackAnimationDuration = 0.12f;
    public float attackAnimationScale = 1.2f;
    public LayerMask attackLayers = ~0;

    [Header("Moving Sprites")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    [Header("Idle Sprites")]
    public Sprite idleUp;
    public Sprite idleDown;
    public Sprite idleLeft;
    public Sprite idleRight;

    // Private variables 
    private Rigidbody2D rb; // Reference to the Rigidbody2D component attached to the player
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private Vector2 movement; // Stores the direction of player movement
    private bool isMovingHorizontally = true; // Flag to track if the player is moving horizontally
    private string lastDirection = "Down"; // Tracks last look direction for idle sprite
    private float nextAttackTime;
    private Coroutine attackAnimation;

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }

        if (attackAction != null)
        {
            attackAction.action.Enable();
        }
    }

    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Prevent the player from rotating
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // Get player input from keyboard or controller
        Vector2 moveInput = moveAction != null
            ? moveAction.action.ReadValue<Vector2>()
            : Vector2.zero;

        if (moveInput == Vector2.zero && Keyboard.current != null)
        {
            moveInput = new Vector2(
                (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f),
                (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f));
        }

        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        // Check if diagonal movement is allowed
        if (canMoveDiagonally)
        {
            // Set movement direction based on input
            movement = new Vector2(horizontalInput, verticalInput);
        }
        else
        {
            // Determine the priority of movement based on input
            if (horizontalInput != 0)
            {
                isMovingHorizontally = true;
            }
            else if (verticalInput != 0)
            {
                isMovingHorizontally = false;
            }

            // Set movement direction
            if (isMovingHorizontally)
            {
                movement = new Vector2(horizontalInput, 0);
            }
            else
            {
                movement = new Vector2(0, verticalInput);
            }
        }

        // Handle swapping sprites based on current input and last direction
        UpdateSprite();

        bool attackPressed = attackAction != null && attackAction.action.WasPressedThisFrame();
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            attackPressed = true;
        }

        if (attackPressed)
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        // Normalize movement vector so diagonal isn't faster, then apply velocity
        rb.linearVelocity = movement.normalized * speed;
    }

    void UpdateSprite()
    {
        // Make sure we have a SpriteRenderer attached so it doesn't throw a NullReferenceException
        if (spriteRenderer == null) return;

        // Check if player is giving movement input
        if (movement.sqrMagnitude > 0.01f)
        {
            // Priority given to X axis when moving diagonally
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                if (movement.x > 0)
                {
                    spriteRenderer.sprite = spriteRight;
                    lastDirection = "Right";
                }
                else
                {
                    spriteRenderer.sprite = spriteLeft;
                    lastDirection = "Left";
                }
            }
            else
            {
                if (movement.y > 0)
                {
                    spriteRenderer.sprite = spriteUp;
                    lastDirection = "Up";
                }
                else
                {
                    spriteRenderer.sprite = spriteDown;
                    lastDirection = "Down";
                }
            }
        }
        else // Player stopped moving -> apply matching idle sprite
        {
            switch (lastDirection)
            {
                case "Up":
                    if (idleUp != null) spriteRenderer.sprite = idleUp;
                    break;
                case "Down":
                    if (idleDown != null) spriteRenderer.sprite = idleDown;
                    break;
                case "Left":
                    if (idleLeft != null) spriteRenderer.sprite = idleLeft;
                    break;
                case "Right":
                    if (idleRight != null) spriteRenderer.sprite = idleRight;
                    break;
            }
        }
    }

    private void Attack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        if (attackAnimation != null)
        {
            StopCoroutine(attackAnimation);
        }

        attackAnimation = StartCoroutine(PlayAttackAnimation());

        Vector2 attackPosition = (Vector2)transform.position + GetFacingDirection() * attackRange;
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPosition, attackRadius, attackLayers);

        foreach (Collider2D target in targets)
        {
            if (target.transform.root == transform.root)
            {
                continue;
            }

            target.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator PlayAttackAnimation()
    {
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < attackAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / attackAnimationDuration);
            float punch = Mathf.Sin(progress * Mathf.PI);
            transform.localScale = originalScale * Mathf.Lerp(1f, attackAnimationScale, punch);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, Color.white, punch);
            }

            yield return null;
        }

        transform.localScale = originalScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        attackAnimation = null;
    }

    private Vector2 GetFacingDirection()
    {
        switch (lastDirection)
        {
            case "Up":
                return Vector2.up;
            case "Left":
                return Vector2.left;
            case "Right":
                return Vector2.right;
            default:
                return Vector2.down;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }

        if (attackAction != null)
        {
            attackAction.action.Disable();
        }
    }
}