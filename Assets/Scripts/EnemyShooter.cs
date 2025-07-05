using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject enemyBullet;
    public Transform firePoint;
    public float fireInterval = 2f;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating("Shoot", 1f, fireInterval);
    }

    void Shoot()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        GameObject bullet = Instantiate(enemyBullet, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = dir * 1f; //Bullet speed low so player can dash
    }
}

