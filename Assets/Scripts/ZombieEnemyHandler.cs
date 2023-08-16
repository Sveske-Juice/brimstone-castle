using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieEnemyHandler : MonoBehaviour
{
    private int left = -1, right = 1;
    private float health = 2f;
    private float speed = 1.4f;
    public float knockbackDamage = 4f;
    private const float chaseDist = 5f;
    private int dirToMove = 0;
    private bool shouldMove = true;
    public GameObject killFire;
    private bool isStoppingMovement = false;
    [HideInInspector]
    public bool isBeingAttacked = false, isAttacking = false, justAttacked = false, attackBeingCool = false;
    public float killDelay;
    GameObject player;
    PlayerMovement pm;
    PlayerKnockback pk;
    GameManager gm;
    Animator animator;
    Rigidbody2D rb;
    AudioManager audioManager;

    enum state {
        passive, 
        chase
    };
    private state mode = state.passive;
    SpriteRenderer sr;
    

    private void Awake() {
        player = GameObject.FindWithTag("Player");
        animator = GetComponent<Animator>();
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        pk = player.GetComponent<PlayerKnockback>();
        pm = player.GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate() {
        // Determine mode
        getMode();

        // Select a direction to move in (1 in 25 chance)
        int luckyDay = UnityEngine.Random.Range(1, 25);
        if (luckyDay == 1) {
            slctDir();
        }
    }

    private void Update() {
        // Act according to state (mode)
        if (mode == state.passive) { 
            // Stop animation if the enemy just stopped chasing player
            if (!isStoppingMovement && animator.GetBool("Enemy_Zombie_IsWalking")) {
                animator.SetBool("Enemy_Zombie_IsWalking", false);
            }
            passive();
        }
        else if (mode == state.chase) {
            chase();
        }

    }

    private void chase()
    {
        // Determine dir to chase player
        int dir = 0;
        float xDiff = transform.position.x - player.transform.position.x;
        float yDiff = Mathf.Abs(transform.position.y - player.transform.position.y);
        if (yDiff > 1f) {
            return;
        }
        if (xDiff < 0) {
            dir = right;
        }
        else {
            dir = left;
        }

        // Move towards player
        move(dir);

    }

    private void passive()
    {
        if (shouldMove) {
            move(dirToMove);
            moveStopCooldown();
            isStoppingMovement = true;
        }
    }

    private void moveStopCooldown()
    {
        if (isStoppingMovement) {
            return;
        }
        // Let enemy walk for around 1.5 to 2.5s before stopping movement and start cooldown
        Invoke("stopMovement", UnityEngine.Random.Range(1.5f, 2.5f));
    }

    private void move(int dir)
    {
        if (dir == 0 || isBeingAttacked) {
            return;
        }
        // Flip sprite according to dir
        flip(dir);

        animator.SetBool("Enemy_Zombie_IsWalking", true);
        Vector2 newPosition = new Vector2(transform.position.x + dir * speed * Time.deltaTime, transform.position.y);
        //rb.MovePosition(newPosition);
        transform.position = newPosition;
        //transform.Translate(new Vector3(dir, 0f, 0f) * speed * Time.deltaTime);
        //rb.AddForce(new Vector2(dir * speed, 0f));
    }

    private void flip(int dir)
    {
        // Give flip boost if dirToMove and current facing dir is different
        if (dir == left) {
            Vector3 theScale = transform.localScale;
            theScale.x = Mathf.Abs(theScale.x);
            transform.localScale = theScale;
        }
        else if (dir == right) {
            Vector3 theScale = transform.localScale;
            theScale.x = -Mathf.Abs(theScale.x);
            transform.localScale = theScale;
        }
    
    }

    private void slctDir()
    {
        // Not necsesary to change dir
        if (mode != state.passive || shouldMove) {
            return;
        }

        // 50 50 chance
        int choice = UnityEngine.Random.Range(1, 3);
        if (choice == 1) {
            // check if there is void to the left
            if (isVoidToDir(left)) {
                dirToMove = right;
            }
            else {
                dirToMove = left;
            }
        }
        else {
            // check if there is void to the right
            if (isVoidToDir(right)) {
                dirToMove = left;
            }
            else {
                dirToMove = right;
            }
        }
    }

    private void getMode() {
        Vector3 playerPos = player.transform.position;
        float xDist = Mathf.Abs(transform.position.x - playerPos.x);
        float yDist = Mathf.Abs(transform.position.y - playerPos.y);
        // To far away on y-axis
        if (yDist >= 1.5f) {
            mode = state.passive;
            return;
        }
        if (xDist < chaseDist) {
            mode = state.chase;
        }
        else {
            mode = state.passive;
        }
    }
    private void stopMovement() {
        shouldMove = false;
        isStoppingMovement = false;
        animator.SetBool("Enemy_Zombie_IsWalking", false);

        // Let enemy walk again after 1-4.5s
        Invoke("moveCooldown", UnityEngine.Random.Range(1f, 4.5f));
    }
    private void moveCooldown() {
        shouldMove = true;
    }
    private bool isVoidToDir(int dir) {
        if (dir == 0)
            return false;
        // Move the origin of the raycast forward so the raycast will predict where the enemy will be in the future
        Vector2 origin = new Vector2(transform.position.x + dir, transform.position.y);
        Vector2 origin1 = new Vector2(transform.position.x + dir * 2f, transform.position.y);
        Vector2 origin2 = new Vector2(transform.position.x + dir * 3f, transform.position.y);
        Vector2 origin3 = new Vector2(transform.position.x + dir * 4f, transform.position.y);
        Vector2 origin4 = new Vector2(transform.position.x + dir * 5f, transform.position.y);
        // Draw raycasts split over 5 units
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 5f);
        RaycastHit2D hit1 = Physics2D.Raycast(origin1, Vector2.down, 5f);
        RaycastHit2D hit2 = Physics2D.Raycast(origin2, Vector2.down, 5f);
        RaycastHit2D hit3 = Physics2D.Raycast(origin3, Vector2.down, 5f);
        RaycastHit2D hit4 = Physics2D.Raycast(origin4, Vector2.down, 5f);
        
        if (hit.collider != null && hit1.collider != null && hit2.collider != null && hit3.collider != null && hit4.collider != null) {
            
            return false;
        }
        else { // void to dir of enemy
            return true;
        }     
    }

    public void TakeDamage(int damage = 1) {
        StartCoroutine(flashRed(killDelay));
        health -= damage;
        if (health <= 0)
        {
            // hacky way of making death time with whip impact
            Invoke("kill", killDelay);
        }
        // Stop movement while being fucked by player
        stopMovement();
    }

    private void kill() {
        // make effect?, give score?
        PlayerPrefs.SetInt("playerScore", PlayerPrefs.GetInt("playerScore") + 2000);
        FindObjectOfType<AudioManager>().Play("EnemyKill");

        // Spawn firekill to drop items
        GameObject fire = Instantiate(killFire, transform.position, Quaternion.identity);
        fire.GetComponent<SpawnItem>().weaponCandle = false;
        fire.GetComponent<SpawnItem>().isMonsterDrop = true;
        Destroy(gameObject);
    }
    private void stopBeingAttacked() {
        isBeingAttacked = false;
    }
    private IEnumerator flashRed(float dl) {
        yield return new WaitForSeconds(dl); 
        sr.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.2f);
        sr.color = new Color(1f, 1f, 1f, 1f);
    }
}
