using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    [Tooltip("Amount of health to remove when the player bumps the enemy.")]
        public int damage = 1;

    [Tooltip("Tag used to identify the player GameObject.")]
    public string playerTag = "Player";

    // Optional: only allow damage once per hit frame if you need a cooldown later
    // public float damageCooldown = 0.5f;
    // private float _lastDamageTime;

    private void TryDamage(GameObject other)
    {
        if (!other.CompareTag(playerTag))
            return;

        // Try to get PlayerHealth from the collided object or a parent
        var health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (health == null)
            return;

        // If you add cooldown uncomment these lines:
        // if (Time.time - _lastDamageTime < damageCooldown) return;
        // _lastDamageTime = Time.time;

        health.ChangeHealth(-damage);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other.gameObject);
    }
}
