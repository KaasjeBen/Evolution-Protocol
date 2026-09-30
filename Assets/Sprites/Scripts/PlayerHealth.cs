using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth <= 0)

        {
            gameObject.SetActive(false);
        }
    }
}