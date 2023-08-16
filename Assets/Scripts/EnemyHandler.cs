using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHandler : MonoBehaviour
{
    private float health = 10f;
    private float speed = 1.2f;
    private float damage = 1f;
    private bool shouldMove = true, shouldAttack = false, isStoppingMovement = false, moveCooldown = false, hasBoosted = false, changedDir = false, isAttacking = false;
    private int directionToMove = -1;
    private const int right = 1;
    private const int left = -1;
    public float attackCooldown = 0.85f, movementCooldown = 5f, deflectAniCooldown = 1f;
    private float attackAniCooldown = 0.3f;
    public float flipBoost;
    private float atckCool = 0f, mvCool = 0f;
    private const float combatDist = 0.8f, chaseDist = 5f;
    private const float range = 1.1f;
    public bool isBeingAttacked = false;
    GameObject player;
    Animator animator;
    BoxCollider2D bc;
    GameManager gm;
    PlayerKnockback pk;
    PlayerMovement pm;
    [SerializeField] private float sizeX, sizeY;

    /* 0: passive, 1: chase */
    enum state{
        passive,
        chase,
        combat
    };

    enum facing {
        right, 
        left
    };
    private state mode = state.passive;
    private facing orientation = facing.right;
    private facing oldOri;
    public void TakeDamage(int _damage = 1)
    {
        isBeingAttacked = true;
        Invoke("stopBeingAttacked", deflectAniCooldown);
        // Deflect if player is not crouching
        if (!pm._isCrouching) {
            animator.SetBool("Knight_IsAttacking", false);
            animator.SetBool("Knight_IsWalking", false);
            animator.SetBool("Knight_IsDeflecting", true);
            Invoke("stopDeflect", deflectAniCooldown);
        }
        else { 
            health -= _damage;
            if (health <= 0)
            {
                Destroy(gameObject);
            }

            // Stop movement while being fucked by player
            stopMovement();

        }
    }
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        animator = GetComponent<Animator>();
        bc = GetComponent<BoxCollider2D>();
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        pk = player.GetComponent<PlayerKnockback>();
        pm = player.GetComponent<PlayerMovement>();
    }
    private void Update()
    {
        // If enemy should move while in passive state
        if (shouldMove && mode == state.passive && directionToMove != 0)
        {
            move(directionToMove);
            if (!isStoppingMovement) {
                Invoke("stopMovement", Random.Range(1.5f, 2.5f));
                isStoppingMovement = true;
            }
        } // In chase state
        else if (mode == state.chase)
        {
            chase(player.transform.position);
        } // In combat state
        else if (mode == state.combat && shouldAttack) 
        {
            playAttackAni();

            Invoke("attack", attackAniCooldown);
            shouldAttack = false;
        }
    }
    private void FixedUpdate()
    {
        // Identify mode to be in
        getMode(player.transform.position);
        //animator.SetBool("Knight_IsDeflecting", true);
        // attackCooldowner
        attackCooldowner();

        // move cooldowner
        if (moveCooldown) {
            MoveCooldown();
        }

        // Should do idle movement
        if (Random.Range(1, 25) == 1 && mode == state.passive && !shouldMove)
        {
            oldOri = orientation;
            slctDir();
        }

    }
    private void slctDir()
    {
        directionToMove = 0;
        // 50 % percent chance of moving to the left
        if (Random.Range(1, 3) == 1) {
            // Set dir to left if no void to the left
            if (checkVoid(facing.left) == 0) {
                directionToMove = left;
                orientation = facing.left;
                shouldMove = true;
            }
        }
        else { // Set dir to right if no void to the right
            if (checkVoid(facing.right) == 0) {
                directionToMove = right;
                orientation = facing.right;
                shouldMove = true;
            }
        }
        
    }

    private void chase(Vector2 playerPos)
    {
        // Distance from this enemy to the player on
        float distX = Mathf.Abs(transform.position.x - playerPos.x);
        // Move right
        if (transform.position.x < playerPos.x)
        {
            move(right);
        }
        // move left
        else
        {
            move(left);
        }

        //BENJAMIN hvis du vil gøre det selv, s� hedder attack animationen "Knight_IsAttacking"... - til n�r du g�r s� den kan sl� spilleren. OLIVER: NICE
    }

    /* dir:1 = right, dir:-1 = left */
    private void move(int dir)
    {
        if (isBeingAttacked || isAttacking)
            return;
        
        // Flip enemy according to walk dir 
        if (dir == -1) { // give boost when the enemy is changing dir
            if (transform.localScale.x < 0) {
                transform.position = new Vector3(transform.position.x - flipBoost, transform.position.y, transform.position.z);
            }

            Vector3 theScale = transform.localScale;
            theScale.x = Mathf.Abs(theScale.x);
            transform.localScale = theScale;
        }
        else { // give boost when the enemy is changing dir
            if (transform.localScale.x > 0) {
                transform.position = new Vector3(transform.position.x + flipBoost, transform.position.y, transform.position.z);
            }
            Vector3 theScale = transform.localScale;
            theScale.x = -Mathf.Abs(theScale.x);
            transform.localScale = theScale;
        }

        
        animator.SetBool("Knight_IsWalking", true);
        transform.Translate(new Vector3(dir, 0f, 0f) * speed * Time.deltaTime);
    }

    private void getMode(Vector2 playerPos) 
    {
        float distY = Mathf.Abs(transform.position.y - playerPos.y);
        float distX = Mathf.Abs(transform.position.x - playerPos.x);
        if (distY > 5f) {
            // Stop movement from other state ex. chase
            //stopMovement();
            mode = state.passive;
            return;
        }
        if (distX < combatDist) 
        {
            //stopMovement();
            mode = state.combat;
        }
        else if (distX < chaseDist) 
        {
            //stopMovement();
            mode = state.chase;
        }
        else
        {
            //stopMovement();
            mode = state.passive;
        }
    }

    private void attack() {
        // Play and stop animations
        if (isBeingAttacked)
            return;



        // Shoot raycast
        float rayDir;
        float diff = transform.position.x - player.transform.position.x;
        if (diff < 0) {
            rayDir = 1;
        }
        else {
            rayDir = -1;
        }


        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(rayDir, 0f), range);
        if (hit.collider != null) {
            // Hit player
            if (hit.collider.CompareTag("Player")) {
                gm.PlayerTakeDamage(1);
                pk.takeKnockback(new Vector2(rayDir, 0f));
            }
        } 
    }

    private void stopAttackAnimation() {
        animator.SetBool("Knight_IsAttacking", false);
        isAttacking = false;
    }

    private void attackCooldowner() {
        atckCool += Time.fixedDeltaTime;
        if (atckCool >= attackCooldown + 1.5f) {
            atckCool = 0f;
            shouldAttack = true;
        }
    }

    private void stopMovement() {
        animator.SetBool("Knight_IsWalking", false);
        isStoppingMovement = false;
        shouldMove = false;
        moveCooldown = true;
        movementCooldown = Random.Range(1f, 4.5f);
    }

    private void MoveCooldown() {
        mvCool += Time.fixedDeltaTime;
        if (mvCool >= movementCooldown) {
            mvCool = 0f;
            shouldMove = true;
            hasBoosted = false;
            oldOri = orientation;
            moveCooldown = false;
        }
    }

    private int checkVoid(facing _dir) {

        // choose dir which doesn't lead to void
        int dir;
        if (_dir == facing.left) {
            dir = -1;
        }
        else { dir = 1; }
        
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

            return 0;
        }
        else { // void to dir of enemy

            return 1;
        }     
    }

    private void stopDeflect() {
        animator.SetBool("Knight_IsDeflecting", false);
    }

    private void stopBeingAttacked() {
        isBeingAttacked = false;
    }

    private void playAttackAni() {
        isAttacking = true;
        animator.SetBool("Knight_IsAttacking", true);
        Invoke("stopAttackAnimation", attackCooldown);
        animator.SetBool("Knight_IsWalking", false);
    }
}
