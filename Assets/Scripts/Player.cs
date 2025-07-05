using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    //Things idk what to call
    public Rigidbody2D rb;
    public Transform Tr;
    public Vector3 startpos;
    public Vector2 hvmovement;
    public SpriteRenderer spr;

    //Animations
    public Animator Playeranimator;
    public Animator ConsumableAnimator;
    public Animator CheckpointAnimator;
    private bool isAttacking;
    private bool isShooting;
    public int tempSpeed;

    //Variables
    public int speed = 5;
    //public int Direction = 1; using spr.flipX now but keep it bc i might need it

    //Dash variables cuz why not
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 5f;
    public float dashTimeRemaining = 0f;
    public float lastDashTime = -Mathf.Infinity;
    public float currentSpeed;

    //Consumables
    //Health potion
    public int ammo = 10; //Ammo

    //Healthbar;
    public int maxHealth = 100;
    public int currentHealth;
    public Healthbar healthbar;

    //Melee Attack
    public int swordDamage = 20;
    [SerializeField]
    private float swordRange = 0.75f;
    public GameObject hitbox;
    private ContactFilter2D enemyFilter;
    private Collider2D[] Enemies;

    //Ranged Attack
    public GameObject energyBlast;  // game object to spawn
    public Transform shotPoint;  // position to shoot at / aim

    public float FadeTime = 1f;
    public Image img;

    void Start()
    {
        //Enemy
        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        enemyFilter.useLayerMask = true;

        //Position
        startpos = Tr.position;
        StartCoroutine(blink());

        //Healthbar
        currentHealth = maxHealth;
        healthbar.SetMaxHealth(currentHealth);
    }

    void Update()
    {
        //Horizontal and vertical movement (hvmovement)
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        hvmovement = new Vector2(x,y).normalized;

        //Flip character so it looks good
        if (x > 0) 
            spr.flipX = false;
        if (x < 0) 
            spr.flipX = true;

        //Animations
        if (x != 0 || y != 0)
            Playeranimator.SetBool("Moving", true); //Moving animation
        else 
            Playeranimator.SetBool("Moving", false); //Still animation

        //Dash (i will shoot the person who told me to do this bc i mean what is this code bro)
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown) //checks if time passed from last dash is more than the cooldown
        {
            dashTimeRemaining = dashDuration;
            lastDashTime = Time.time;
        }

        if (dashTimeRemaining > 0f) //decrease dashTimeRemaining as time passes
            dashTimeRemaining -= Time.deltaTime;

        if (dashTimeRemaining > 0f) //if there is still dashTimeRemaining, then currentSpeed = dashSpeed. after dashtime ends, currentspeed = normal speed
            currentSpeed = dashSpeed;
        else
            currentSpeed = speed;

        //rb.velocity = hvmovement * currentSpeed; works but next one is better because if i remove .normalized
        rb.velocity = new Vector2(x*currentSpeed, y*currentSpeed); //Final movement


        //Healthbar update
        healthbar.SetHealth(currentHealth);

        //Melee Attack (kill me please)
        if (Input.GetKeyDown(KeyCode.Z) && !isAttacking)
        {
            Playeranimator.SetTrigger("Attack");
            Attack();
        }

        //Ranged Attack (why didnt you kill me yet)
        if (Input.GetKeyDown(KeyCode.X) && !isShooting)
        {
            Playeranimator.SetTrigger("Shoot");
            Vector2 target = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y)); //cursor direction

            Vector2 charPos = new Vector2(Tr.position.x, Tr.position.y);

            //get direction from player to target
            Vector2 direction = target - charPos;

            //normalize vector
            direction.Normalize(); // yippieee

            //stuff that do stuff idk they dont pay me enough for this
            Quaternion rotation =
                Quaternion.Euler(0, 0,
                (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f);

            GameObject projectile =
                (GameObject)Instantiate(energyBlast, shotPoint.position, rotation);
        }
    }

    //Checkpoint and consumables
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Checkpoint in breakroom
        if (collision.gameObject.tag == "Checkpoint")
        {
            CheckpointAnimator = collision.gameObject.GetComponent<Animator>();
            if (CheckpointAnimator != null)
            {
                CheckpointAnimator.SetBool("Checkpoint", true);
            }
            startpos = Tr.position;
            //audio.playSFX(audio.checkpt);
        }

        //Consumables
        if (collision.gameObject.tag == "HP") //Health potion
        {
            currentHealth = maxHealth;
            ConsumableAnimator = collision.gameObject.GetComponent<Animator>();
            if (ConsumableAnimator != null)
            {
                ConsumableAnimator.SetBool("isCollected", true);
                StartCoroutine(DestroyConsumableAfterAnimation(collision.gameObject, ConsumableAnimator));
            }
            //audio.playSFX(audio.collected);
        }
        
        if (collision.gameObject.tag == "Ammo") //AMMO GO BRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR
        {
            ammo += 10;
            ConsumableAnimator = collision.gameObject.GetComponent<Animator>();
            if (ConsumableAnimator != null)
            {
                ConsumableAnimator.SetBool("isCollected", true);
                StartCoroutine(DestroyConsumableAfterAnimation(collision.gameObject, ConsumableAnimator));
            }
            //audio.playSFX(audio.collected);
        }
    }

    public void ShootStart()  //Character not allowed to move while shooting
    {
        Playeranimator.SetBool("isShooting", true);
        tempSpeed = speed;
        speed = 0;
        isShooting = true;
    }

    public void ShootEnd()
    {
        Playeranimator.SetBool("isShooting", false);
        speed = tempSpeed;
        isShooting = false;
    }

    public void Attack()
    {
        //Enemies = Physics2D.OverlapCircleAll(hitbox.transform.position,ATKRange , enemyFilter.layerMask); AOE GO BRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR
        Enemies = new Collider2D[10];
        Physics2D.OverlapCollider(hitbox.GetComponent<Collider2D>(), enemyFilter, Enemies);
        foreach (Collider2D Enemy in Enemies)
        {
            if (Enemy != null)
            {
                EnemyPath Villain = Enemy.GetComponent<EnemyPath>();
                if (Villain != null)
                {
                    Debug.Log("Damaged Enemy on a Path: - 20");
                    Villain.DamageEnemy(swordDamage);
                }
            }

        }
    }

    public void AttackStart()
    {
        Playeranimator.SetBool("isAttacking", true);
        tempSpeed = speed;
        speed = 0;
        isAttacking = true;
        hitbox.GetComponent<Collider2D>().enabled = true;
    }
    public void AttackEnd()
    {
        Playeranimator.SetBool("isAttacking", false);
        isAttacking = false;
        speed = tempSpeed;
        hitbox.GetComponent<Collider2D>().enabled = false;

    }



    //Player take damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
            SceneManager.LoadScene("MainMenu");
    }








    // Coroutines
    IEnumerator blink()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.1f);
            spr.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spr.enabled = true;
        }
    }

    private IEnumerator DestroyConsumableAfterAnimation(GameObject consumable, Animator consumableAnimator)
    {

        yield return new WaitForSeconds(consumableAnimator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(consumable);
    }
}
