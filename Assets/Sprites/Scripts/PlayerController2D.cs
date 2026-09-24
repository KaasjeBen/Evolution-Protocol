using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public int facingDirection = 1; // 1 for right, -1 for left

    public Rigidbody2D rb;
    public Animator anim;

    private Vector2 movement;

    // Called by the new Input System
    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (rb != null)

           
        {
            anim.SetFloat("horizontal", Mathf.Abs(movement.x));
            anim.SetFloat("vertical", Mathf.Abs(movement.y));

            if (horizontal > 0 && transform.localScale.x < 0 ||
                (horizontal < 0 && transform.localScale.x > 0))
            {
               Flip ();
            }

            rb.linearVelocity = movement * speed;
        }
    }

    
    void Flip()
    {
        facingDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}


    