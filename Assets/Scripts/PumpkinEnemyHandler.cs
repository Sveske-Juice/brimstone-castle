using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpkinEnemyHandler : MonoBehaviour
{
    private float health = 1f;
    private float speed = 1.2f;
    private int killScore = 2000;
    public float knockbackDamage = 2f;
    private float combatDist = 0f, chaseDist = 6f, chaseYDist = 3f;
    private float passiveLevel;
    private int dirToMove = 0, left = -1, right = 1;
    private bool shouldMove = true, isStoppingMovement = false;
    public float killDelay;
    GameObject player;
    public GameObject killFire;
    GameManager gm;
    CameraFollower cf;
    Rigidbody2D rb;

    enum state {
        passive,
        chase,
        combat
    };

    private state mode = state.passive;
    SpriteRenderer sr;
    void Awake() {
        player = GameObject.FindWithTag("Player");
        passiveLevel = transform.position.y;
        cf = Camera.main.gameObject.GetComponent<CameraFollower>();
        rb = GetComponent<Rigidbody2D>();
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update() {
        // Act on the current player state (mode)
        if (mode == state.passive) {
            passive();
        }
        else if (mode == state.chase) {
            chase();
        }
        else if (mode == state.combat) {
            combat();
        }

        /* Dbg ray
        Vector2 dir = transform.position - player.transform.position;
        dir = dir.normalized;
        Debug.DrawRay(transform.position, -dir * chaseYDist * 5f); */
    }

    void FixedUpdate() {
        // Get current state in which the pumpkin should be in
        GetMode();

        // Select a direction to move in (1 in 25 chance)
        int luckyDay = UnityEngine.Random.Range(1, 25);
        if (luckyDay == 1) {
            slctDir();
        }
    }

    private void GetMode() {
        Vector3 playerPos = player.transform.position;
        float distance = Mathf.Abs(transform.position.x - playerPos.x);
        if (distance < combatDist) {
            mode = state.combat;
        }
        else if (distance < chaseDist && isInYdis() == true) {
            // Check if there is a clear passage to player (no blocks in the way)
            if (isClearPassage() == true)
                mode = state.chase;
        }
        else {
            mode = state.passive;
        }
    }

    private void passive()
    {
        if (shouldMove) {
            move(dirToMove);
            moveStopCooldown();
            isStoppingMovement = true;
        }

        // If not on passive y level move torwards it, usage of yDiff instead of speed gives a cool interpolation of its speed
        float yDiff = transform.position.y - passiveLevel;
        if (yDiff > 0.1f) { // down
            transform.Translate(new Vector3(0f, -Mathf.Abs(yDiff), 0f) * speed * Time.deltaTime);
        }
        else if (yDiff < -0.1f) { // up
            transform.Translate(new Vector3(0f, Mathf.Abs(yDiff), 0f) * speed * Time.deltaTime);
        }

    }
    private void chase()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 translation;
        float yDiff = transform.position.y - playerPos.y;
        float xDiff = transform.position.x - playerPos.x;

        // Determine direction in which the player is
        if (xDiff > 0) {
            dirToMove = left;
        }
        else {
            dirToMove = right;
        }

        // Move down or up to player level
        if (yDiff > 0.1f) { // down
            translation = new Vector3(dirToMove, -yDiff, 0f);
        }
        else if (yDiff < -0.1f) { // up
            translation = new Vector3(dirToMove, Mathf.Abs(yDiff), 0f);
        }
        else { // Move towards player on x-axis
            translation = new Vector3(dirToMove, 0f, 0f);
        }

        transform.Translate(translation * speed * Time.deltaTime);
    }
    private void combat()
    {
        throw new NotImplementedException();
    }

    private void slctDir()
    {
        if (mode != state.passive || shouldMove) {
            return;
        }

        // 50 50 chance
        int choice = UnityEngine.Random.Range(1, 3);
        if (choice == 1) {
            dirToMove = left;
        }
        else {
            dirToMove = right;
        }
    }

    private void move(int dir) {
        if (dir == 0) {
            return;
        }
        // Flip sprite according to dir
        flip(dir);

        Vector2 newPos = new Vector2(transform.position.x + dir * speed * Time.deltaTime, transform.position.y);
        transform.position = newPos;
        //rb.MovePosition(newPos);
        //transform.Translate(new Vector3(dir, 0f, 0f) * speed * Time.deltaTime);
    }

    private void flip(int dir)
    {
        if (dir == 0) {
            return;
        }
        if (dir == right) {
            Vector3 theScale = transform.localScale;
            theScale.x = -Mathf.Abs(theScale.x);
            transform.localScale = theScale;
        }
        else {
            Vector3 theScale = transform.localScale;
            theScale.x = Mathf.Abs(theScale.x);
            transform.localScale = theScale;
        }
    }

    private void moveStopCooldown() {
        if (isStoppingMovement) {
            return;
        }
        // Let enemy walk for around 1.5 to 2.5s before stopping movement and start cooldown
        Invoke("stopMovement", UnityEngine.Random.Range(1.5f, 2.5f));

    }
    private void stopMovement() {
        shouldMove = false;
        isStoppingMovement = false;

        // Let enemy walk again after 1-4.5s
        Invoke("moveCooldown", UnityEngine.Random.Range(1f, 4.5f));
    }
    private void moveCooldown() {
        shouldMove = true;
    }

    /* works but we use y distance instead
    private bool isInsideCMBound(BoxCollider2D bound) {
        Vector3 pos = bound.gameObject.transform.position;
        float boundLeftEdge = bound.transform.position.x + bound.offset.x - bound.size.x / 2f;
        float boundRightEdge = bound.transform.position.x + bound.offset.x + bound.size.x / 2f;
        float boundTopEdge = bound.transform.position.y + bound.offset.y + bound.size.y / 2f;
        float boundBottomEdge = bound.transform.position.y + bound.offset.y - bound.size.y / 2f;

        if ((transform.position.x >= boundLeftEdge && transform.position.x <= boundRightEdge) && (transform.position.y >= boundBottomEdge && transform.position.y <= boundTopEdge)) {
            Debug.Log("Inside same bound");
            return true;
        }
        return false;
    }*/

    private bool isInYdis() {
        float yDist = Mathf.Abs(transform.position.y - player.transform.position.y);
        if (yDist < chaseYDist) {
            return true;
        }
        
        return false;
    }

    private bool isClearPassage() {
        // Shoot ray in dir of player to see if any blocks in the way
        Vector2 dir = transform.position - player.transform.position;
        dir = dir.normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, -dir * chaseYDist * 5f);
        if (hit.collider != null) {
            if (hit.collider.CompareTag("Player")) {
                return true;
            }
        }
        return false;
    }

    public void TakeDamage(int value) {
        health -= value;
        StartCoroutine(flashRed(killDelay));
        if (health <= 0) {
            Invoke("kill", killDelay);
        }
    }

    private void kill() {
        //PlayerPrefs.SetInt("playerScore", PlayerPrefs.GetInt("playerScore") + 2000);
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
