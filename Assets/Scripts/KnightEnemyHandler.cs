using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightEnemyHandler : MonoBehaviour
{
    private int left = -1, right = 1;
    private float health = 9f;
    private float speed = 1.2f;
    private int killScore = 5000;
    public float knockbackDamage = 3f;
    private const float combatDist = 0.8f, chaseDist = 5f;
    private float flipBoost;
    private const float atckRange = 1.1f, atckAniCooldown = 0.3f, deflectAniCooldown = 0.8f;
    private float attackCool = 2.5f;
    private float atckTimer = 0f;
    private int dirToMove = 0;
    private bool shouldMove = true;

    public float killDelay;
    private bool isStoppingMovement = false;
    [HideInInspector]
    public bool isBeingAttacked = false, isAttacking = false, justAttacked = false, attackBeingCool = false;
    GameObject player;
    public GameObject killFire;
    PlayerMovement pm;
    PlayerKnockback pk;
    GameManager gm;
    Animator animator;
    Rigidbody2D rb;

    enum state {
        passive, 
        chase,
        combat
    };
    private state mode = state.passive;
    SpriteRenderer sr;

    private void Awake() {
        player = GameObject.FindWithTag("Player");
        animator = GetComponent<Animator>();
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        pk = player.GetComponent<PlayerKnockback>();
        pm = player.GetComponent<PlayerMovement>();
        flipBoost = transform.localScale.x;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        // Determine mode
        getMode();

        // Select a direction to move in (1 in 25 chance)
        int luckyDay = UnityEngine.Random.Range(1, 25);
        if (luckyDay == 1) {
            slctDir();
        }

        // Start attack cooldowner if just attacked
        if (justAttacked) {
            attackCooldown();
        }

        Vector2 velocity = new Vector2(2f, 0f);
        //rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void Update() {
        // Act according to state (mode)
        if (mode == state.passive) { 
            // Stop animation if the enemy just stopped chasing player
            if (!isStoppingMovement && animator.GetBool("Knight_IsWalking")) {
                animator.SetBool("Knight_IsWalking", false);
            }
            passive();
        }
        else if (mode == state.chase) {
            chase();
        }
        else if (mode == state.combat) {
            combat();
        }
    }
    private void combat() {
        animator.SetBool("Knight_IsWalking", false);

        // Only call when not already attacking or player in Iframes
        if (isAttacking || justAttacked || isBeingAttacked || gm.hasIFrames || pk.isBeingKnockedBack) {
            return;
        }

        // Start attack animation
        attackAnimation();

        // Wait delay so animation and raycast match
        Invoke("attack", atckAniCooldown);

    }

    private void attackAnimation()
    {
        isAttacking = true;
        animator.SetBool("Knight_IsWalking", false);
        animator.SetBool("Knight_IsDeflecting", false);
        animator.SetBool("Knight_IsAttacking", true);
    }

    private void attack()
    {
        // Shoot raycast
        float rayDir;
        float diff = transform.position.x - player.transform.position.x;
        if (diff < 0) {
            rayDir = right;
        }
        else {
            rayDir = left;
        }


        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(rayDir, 0f), atckRange);
        if (hit.collider != null) {
            if (hit.collider.CompareTag("Player")) {
                gm.PlayerTakeDamage(1);
                pk.takeKnockback(new Vector2(rayDir, 0f));
            }
        }
        // Done attacking
        isAttacking = false;
        animator.SetBool("Knight_IsAttacking", false);
        justAttacked = true;

    }

    private void attackCooldown() {
        //animator.SetBool("Knight_IsAttacking", false);
        atckTimer += Time.fixedDeltaTime;
        if (atckTimer >= attackCool) {
            justAttacked = false;
            atckTimer = 0f;
        }
    }

    private void chase()
    {
        // Determine dir to chase player
        int dir = 0;
        float xDiff = transform.position.x - player.transform.position.x;
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

        animator.SetBool("Knight_IsWalking", true);
        Vector2 newPosition = new Vector2(transform.position.x + dir * speed * Time.deltaTime, transform.position.y);
        
        transform.position = newPosition;
        //rb.MovePosition(newPosition);
        //transform.Translate(new Vector3(dir, 0f, 0f) * speed * Time.deltaTime);
        //rb.AddForce(new Vector2(dir * speed, 0f));
    }

    private void flip(int dir)
    {
        // Give flip boost if dirToMove and current facing dir is different
        if (dir == left && transform.localScale.x < 0) {
            transform.position = new Vector3(transform.position.x - flipBoost, transform.position.y, transform.position.z);

            Vector3 theScale = transform.localScale;
            theScale.x = Mathf.Abs(theScale.x);
            transform.localScale = theScale;
        }
        else if (dir == right && transform.localScale.x > 0) {
            transform.position = new Vector3(transform.position.x + flipBoost, transform.position.y, transform.position.z);

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

        if (xDist < combatDist) {
            mode = state.combat;
        }
        else if (xDist < chaseDist) {
            mode = state.chase;
        }
        else {
            mode = state.passive;
        }
    }
    private void stopMovement() {
        shouldMove = false;
        isStoppingMovement = false;
        animator.SetBool("Knight_IsWalking", false);

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
        isBeingAttacked = true;
        Invoke("stopBeingAttacked", deflectAniCooldown);
        
        // Deflect if player is not crouching
        if (!pm._isCrouching && pm.currentWeapon.weapon == Weapons.whip) {
            FindObjectOfType<AudioManager>().Play("KnightDeflect");
            animator.SetBool("Knight_IsAttacking", false);
            animator.SetBool("Knight_IsWalking", false);
            animator.SetBool("Knight_IsDeflecting", true);
            Invoke("stopDeflect", deflectAniCooldown);
        }
        else {
            StartCoroutine(flashRed(killDelay));
            FindObjectOfType<AudioManager>().Play("EnemyHit");
            health -= damage;
            if (health <= 0)
            {
                Invoke("kill", killDelay);
            }
        }
        // Stop movement while being fucked by player
        stopMovement();
    }

    private IEnumerator flashRed(float dl) {
        yield return new WaitForSeconds(dl); 
        sr.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.2f);
        sr.color = new Color(1f, 1f, 1f, 1f);
    }
    private void kill() {
        // make effect?, give score?
        //PlayerPrefs.SetInt("playerScore", PlayerPrefs.GetInt("playerScore") + 2000);
        gm.SetPlayerScore(gm.GetPlayerScore() + killScore);
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
    private void stopDeflect() {
        animator.SetBool("Knight_IsDeflecting", false);
    }
}
