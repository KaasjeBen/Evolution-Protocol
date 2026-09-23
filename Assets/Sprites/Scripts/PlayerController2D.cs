using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Rigidbody2D rb;
    public Animator anim;

    private Vector2 movement;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    // Called by the new Input System
    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (anim != null)
        {
            anim.SetFloat("horizontal", Mathf.Abs(movement.x));
            anim.SetFloat("vertical", Mathf.Abs(movement.y));
        }

        rb.linearVelocity = movement * speed;
    }
}