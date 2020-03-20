using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [Header("Set in Inspector:  Set player health.")]
    public int health;

	public void TakeDamage(int amount)
    {
        //Debug.Log("Health at: " + health);
        health -= amount;
        if (health <= 0)
        {
            GameManager.singleton.GameOver();
        }
    }
}
