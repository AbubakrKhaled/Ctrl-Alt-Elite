using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPath : MonoBehaviour
{
    public int maxHealth = 100;
    private int currHealth;
    public float speed = 4f;
    public Rigidbody2D rb;
    public SpriteRenderer sr;

    private Transform player;

    void Start()
    {
        currHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * speed;
    }

    public void DamageEnemy(int dmg)
    {
        currHealth -= dmg;
        StartCoroutine(blinkred(sr));
        if (currHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator blinkred(SpriteRenderer sr)
    {
        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }
}

