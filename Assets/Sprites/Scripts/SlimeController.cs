using UnityEngine;

public class SlimeController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float minActionDuration = 1f;
    public float maxActionDuration = 3f;
    [Range(0f, 1f)] public float idleChance = 0.5f;
    public float detectionRange = 5f;
    public float closeDistance = 1f;
    public float runSpeedMultiplier = 2f;
    public float closeAttackInterval = 1f;

    public Transform player;

    public Rigidbody2D rb;
    public Animator anim;

    private Vector2 movement;
    private float actionTimer;
    private float closeAttackTimer;
    private bool wasCloseToPlayer;

    private void Start()
    {
        if (player == null)
        {
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null)
            {
                player = playerMovement.transform;
            }
        }

        ChooseNextAction();
    }

    private void FixedUpdate()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            FollowPlayer();
        }
        else
        {
            SetSeeingPlayer(false);
            if (anim != null)
            {
                anim.SetBool("closeToPlayer", false);
            }
            wasCloseToPlayer = false;
        }

        actionTimer -= Time.fixedDeltaTime;

        if (actionTimer <= 0f && !IsSeeingPlayer())
        {
            ChooseNextAction();
        }

        if (rb != null)
        {
            float speedMultiplier = IsSeeingPlayer() ? runSpeedMultiplier : 1f;
            rb.linearVelocity = movement * moveSpeed * speedMultiplier;
        }

        if (anim != null)
        {
            anim.SetFloat("horizontal", Mathf.Abs(movement.x));
            anim.SetFloat("vertical", Mathf.Abs(movement.y));
        }
    }

    private void ChooseNextAction()
    {
        float minDuration = Mathf.Max(0.1f, minActionDuration);
        float maxDuration = Mathf.Max(minDuration, maxActionDuration);
        actionTimer = Random.Range(minDuration, maxDuration);

        if (Random.value < idleChance)
        {
            movement = Vector2.zero;
            return;
        }

        movement = Random.insideUnitCircle.normalized;

        if (movement.x != 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(movement.x);
            transform.localScale = scale;
        }
    }

    private void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isCloseToPlayer = distanceToPlayer <= closeDistance;

        SetSeeingPlayer(true);

        if (anim != null)
        {
            anim.SetBool("closeToPlayer", isCloseToPlayer);
        }

        if (isCloseToPlayer)
        {
            movement = Vector2.zero;
            closeAttackTimer -= Time.fixedDeltaTime;

            if ((!wasCloseToPlayer || closeAttackTimer <= 0f) && anim != null)
            {
                anim.SetTrigger("attack");
                closeAttackTimer = Mathf.Max(0.1f, closeAttackInterval);
            }
        }
        else
        {
            movement = ((Vector2)player.position - (Vector2)transform.position).normalized;
            closeAttackTimer = 0f;
        }

        wasCloseToPlayer = isCloseToPlayer;

        if (movement.x != 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(movement.x);
            transform.localScale = scale;
        }
    }

    private void SetSeeingPlayer(bool isSeeingPlayer)
    {
        if (anim != null)
        {
            anim.SetBool("seePlayer", isSeeingPlayer);
            anim.speed = isSeeingPlayer ? runSpeedMultiplier : 1f;
        }
    }

    private bool IsSeeingPlayer()
    {
        return player != null && Vector2.Distance(transform.position, player.position) <= detectionRange;
    }
}
