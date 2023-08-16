using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonHandler : MonoBehaviour
{
    int killScore = 4000;
    public float killDelay = 0f;
    public float knockbackDamage = 3f, projectileDamage = 2f, atckAniCooldown = 1f;
    int health = 6;
    private float combatDist = 4f, chaseDist = 10f;
    private static int right = 1, left = -1, noDir = 0;
    private int dirToMove = noDir;
    private float speed = 1f;

    private float atckTimer = 0f, attackCool = 3f;

    private bool shouldMove = true, justAttacked = false, isBeingAttacked = false, isStoppingMovement = false, isAttacking = false;
    GameObject player;
    public GameObject killFire;
    public GameObject bonePref;
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
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }
    private void Update() {
        // Act according to state (mode)
        if (mode == state.passive) { 
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
        animator.SetBool("Skeleton_isWalking", false);

        // Only call when not already attacking or player in Iframes
        if (isAttacking || justAttacked || isBeingAttacked) {
            return;
        }

        // Start attack animation
        attackAnimation();
        
        // Throw bone
        attack();

        // Attack cooldown
        StartCoroutine(attackCooldoown());
    }

    private IEnumerator attackCooldoown() {
        isAttacking = true;

        yield return new WaitForSeconds(2f);
        isAttacking = false;
    }
    private void attack() {
        float xDiff = transform.position.x - player.transform.position.x;
        int dir = noDir;
        if (xDiff < 0) {
            dir = left;
        }
        else {
            dir = right;
        }
        flip(-dir);

        // Spawn bone and give velocity in 'dir'
        GameObject bone = Instantiate(bonePref, transform.position, Quaternion.identity);
        bone.GetComponent<Rigidbody2D>().velocity = new Vector2(-dir, 0f) * 2.3f;
        // Rotate if dir is left
        if (dir == left) {
            bone.transform.eulerAngles = new Vector3(bone.transform.eulerAngles.x, 180f, bone.transform.eulerAngles.z);
        }
    }

    
    private void attackAnimation()
    {
        isAttacking = true;
        animator.SetBool("Skeleton_isWalking", false);
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
    private void move(int dir)
    {
        
        if (dir == noDir || isBeingAttacked) {
            return;
        }

        //Debug.Log($"moving in {dir}, with speed {speed * Time.deltaTime} units pr. s");

        // Flip sprite according to dir
        flip(dir);

        animator.SetBool("Skeleton_isWalking", true);
        Vector2 newPosition = new Vector2(transform.position.x + dir * speed * Time.deltaTime, transform.position.y);
        //Debug.Log(newPosition);

        transform.position = newPosition;
        // idk y rb... doesnt work but its super scuffed, so is just setting pos
        //rb.MovePosition(newPosition);
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
    private void attackCooldown() {
        //animator.SetBool("Knight_IsAttacking", false);
        atckTimer += Time.fixedDeltaTime;
        if (atckTimer >= attackCool) {
            justAttacked = false;
            atckTimer = 0f;
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

    private void stopMovement() {
        shouldMove = false;
        isStoppingMovement = false;
        animator.SetBool("Skeleton_isWalking", false);

        // Let enemy walk again after 1-4.5s
        Invoke("moveCooldown", UnityEngine.Random.Range(1f, 4.5f));
    }
    private void moveCooldown() {
        shouldMove = true;
    }

    public void TakeDamage(int damage) {
        health -= damage;
        GameObject.FindObjectOfType<AudioManager>().Play("EnemyHit");
        StartCoroutine(flashRed(killDelay));
        //Debug.Log(health);
        if (health <= 0) {
            StartCoroutine(kill(killDelay));
        }
        isBeingAttacked = true;
        StartCoroutine(stopBeingAttacked());
    }
    private IEnumerator stopBeingAttacked(float dl = 2f) {
        animator.SetBool("Skeleton_isWalking", false);
        yield return new WaitForSeconds(dl);
        isBeingAttacked = false;
    }
    private IEnumerator kill(float dl) {
        yield return new WaitForSeconds(dl);

        gm.SetPlayerScore(gm.GetPlayerScore() + killScore);
        FindObjectOfType<AudioManager>().Play("EnemyKill");

        // Spawn firekill to drop items
        GameObject fire = Instantiate(killFire, transform.position, Quaternion.identity);
        fire.GetComponent<SpawnItem>().weaponCandle = false;
        fire.GetComponent<SpawnItem>().isMonsterDrop = true;
        
        Destroy(gameObject);
    }
    private IEnumerator flashRed(float dl) {
        yield return new WaitForSeconds(dl); 
        sr.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.2f);
        sr.color = new Color(1f, 1f, 1f, 1f);
    }

}
