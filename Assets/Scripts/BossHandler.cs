using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHandler : MonoBehaviour
{
    private int health = 15, fireBallDamage = 2, spikeDamage = 3;
    private bool isMoving = false, shouldMove = true, isAttacking = false, shouldAttack = true, lookAttack = false;
    private int dirToMove = 0, left = -1, right = 1, noDir = 0, low = -1, high = 1;
    private float passiveDist = 50f, combatDist = 10f, yThreshold = 5f, fireBallSpeed = 3f;
    private float fireBallAniDelay = 0.5f, fireBallLowYOffset = 1.2f, fireBallHighYOffset = 0.3f;
    private float spikesOffsetFromPlayer = 2f, spikesMaxLength = 14f, spikesMinLength = 8f, spikesSpeed = 4f, spikeDistToGrnd = 3f;
    private float duration = 1f;
    private int killScore = 50000;
    public float knockbackDamage = 2f, killDelay;
    public bool bossIsDead = false;
    bool iSpre = false;
    public int GetHealth() { return health; }
    private enum Facing {
        noface,
        right, 
        left
    };
    private enum State {
        idle,
        passive, 
        combat
    };

    private State state = State.idle;
    private Facing facing = Facing.noface;

    // Object references
    public GameObject smokePref;
    public GameObject spikePref;
    public GameObject fireBall;
    public GameObject skelliePref;
    public GameObject zombiePref;
    Transform player;
    SpriteRenderer spriteRenderer;

    Rigidbody2D rigidBody;
    Animator animator;
    BoxCollider2D boxCollider;
    GameManager gm;
    List<GameObject> spikes = new List<GameObject>();
    List<float> approaching = new List<float>();
    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }
    void Start() {
        // Sleep 3 seconds before doing anything
        StartCoroutine(startDelay());
    }

    IEnumerator startDelay() {
        iSpre = true;
        yield return new WaitForSeconds(3f);
        iSpre = false;
    }
    private void Update() {
        if (iSpre)
            return;

        // Perform actions depending on current state
        if (state == State.idle) {
            return;
        }
        else if (state == State.passive) {
            Passive();
        }
        else if (state == State.combat) {
            Combat();
        }
    }
    private void FixedUpdate() {
        // Poll for the current state the boss is in
        state = GetState();

        // Choose direction to walk in passive mode 
        dirToMove = ChooseDir();

        // If the boss is in combat mode, keep track of player position
        if (state == State.combat) {
            float xDist = Mathf.Abs(transform.position.x - player.transform.position.x);
            approaching.Add(xDist);
            if (approaching.Count >= 5) {
                // Pop old element
                approaching.RemoveAt(0);
            }
        }
    }

    private void Passive() {
        if (!isMoving && shouldMove) {
            // Dont move if dir is noDir
            if (dirToMove != noDir)
                Move();
                // Flip while invisible
                Invoke("Flip", 0.5f);
        }
    }

    private void Combat() {
        Flip();
        // Maybe move if player is approaching or is really close
        bool isGettingChased = GetApprState();
        bool playerClose = GetPlayerClose();
        if (isGettingChased) {
            //Debug.Log($"Trying to move away from player, movingState: {isMoving}, isAttacking: {isAttacking}");
            if (Random.Range(1, 15) == 1 && !isMoving && !lookAttack) {
                Move();
                // Flip while invisible
                Invoke("Flip", 0.5f);
            }
        }
        else if (playerClose) {
            //Debug.Log($"Trying to move away from player, movingState: {isMoving}, isAttacking: {isAttacking}");
            if (Random.Range(1, 5) == 1 && !isMoving && !lookAttack) {
                Move();
                // Flip while invisible
                Invoke("Flip", 0.5f);
            }
        }
        if (isMoving || isAttacking || !shouldAttack) {
            return;
        }
        
        shouldAttack = false;
        // Decide on attack to use
        int choice = Random.Range(1, 5);

        // Fireball low
        if (choice == 1) {
            Fireball(low);
        } // Fireball high
        else if (choice == 2) {
            Fireball(high);
        } // Spikes
        else if (choice == 3) {
            Spikes();
        }
        else if (choice == 4) {
            Summon();
        }
        // Start attackCooldown
        StartCoroutine(AttackCooldown());
    }

    private void Summon() {
        isAttacking = true;
        lookAttack = true;
        // Get values from where the skeletons will spawn
        float xMax = 3f;
        float xMin = -3f;

        // Choose how many skeltons to spawn (1 to 3)
        int count = Random.Range(2, 3);

        // Loop through and spawn the skeletons on a row with a random x offset NOTE: very bad idea of hard coded values ik, will only work in boss arena
        // lastOffset is for making the random range use left and right dir after eachother (it looks more spread out and random)
        float length = Mathf.Abs(xMax - xMin);
        float step = -length / 2f;
        float stepSize = length / count;
        float lastOffset = 0f;
        float randomOffset = 0f;
        for (int i = 0; i < count; i++) {
            if (lastOffset < 0) {
                randomOffset = Random.Range(2f, xMax);
            }
            else if (lastOffset > 0) {
                randomOffset = Random.Range(-2f, xMin);
            }
            else {
                randomOffset = Random.Range(xMin, xMax);
            }
            Vector2 spawnPos = new Vector2(step + randomOffset, transform.position.y - 1f);
            StartCoroutine(SpawnSkellie(spawnPos));
            step += stepSize;
            lastOffset = randomOffset;
        }

        // Animation and cooldown stuff (use same ani as summon spikes)
        StartCoroutine(StartAnimation("Enemy_Boss_SummonSpikes", 0f));
        StartCoroutine(DissableAttack(3f));
        StartCoroutine(DissableLookAttack(2f));
        StartCoroutine(StopAnimation("Enemy_Boss_SummonSpikes", 2f));

    }

    private IEnumerator SpawnSkellie(Vector2 spawnPos, float duration = 0.5f, float stayDuration = 0.5f, float outDuration = 1f) {
        //Debug.Log($"Spawning a skellie at {spawnPos}");
        Color transColor = new Color(1f, 1f, 1f, 0f);
        Color fullColor = new Color(1f, 1f, 1f, 1f);

        // Spawn teleportation cloud with zero opacity
        GameObject cloud = Instantiate(smokePref, spawnPos, Quaternion.identity);
        SpriteRenderer srCloud = cloud.GetComponent<SpriteRenderer>();
        srCloud.color = transColor;

        // Fade cloud in
        float time = 0f;
        while (time < duration) {
            // Linear interpolate between 0 opacity and full in 'duration' sec
            srCloud.color = Color.Lerp(transColor, fullColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // Spawn skellie
        if (Random.Range(1, 3) == 1) {
            GameObject skellie = Instantiate(skelliePref, spawnPos, Quaternion.identity);
        }
        else { // Spawn zom
            GameObject skellie = Instantiate(zombiePref, spawnPos, Quaternion.identity);
        }

        // Wait 'stayDuration' before fading cloud out
        yield return new WaitForSeconds(stayDuration);

        // Fade out
        time = 0f;
        while (time < outDuration) {
            // Linear interpolate between 0 opacity and full in 'duration' sec
            srCloud.color = Color.Lerp(fullColor, transColor, time / outDuration);
            time += Time.deltaTime;
            yield return null;
        }

        // Kill cloud
        Destroy(cloud);
    }

    private bool GetPlayerClose()
    {
        float xDiff = Mathf.Abs(transform.position.x - player.transform.position.x);
        if (xDiff <= 3f) {
            return true;
        }
        return false;
    }

    private bool GetApprState()
    {
        float curDist = approaching[0] + 0.01f;
        for (int i = 0; i < approaching.Count; i++) {
            if (!(approaching[i] < curDist)) { // not moving towards
                //Debug.Log("PLAYER NOT APPROUCHING");
                return false;
            }
            curDist = approaching[i];
        }
        //Debug.Log("Player is appr");
        return true;
    }

    private void Move() {
        // Start teleporting
        StartCoroutine("Teleport");
        // Start movement cooldown
        StartCoroutine("moveCooldown");
    }

    private void Fireball(int lvl) {
        lookAttack = true;
        isAttacking = true;
        float yLvl;
        if (lvl == low) {
            yLvl = transform.position.y - fireBallLowYOffset;
            StartCoroutine(StartAnimation("Enemy_Boss_LowFireball", 0f));
            StartCoroutine(StopAnimation("Enemy_Boss_LowFireball", 1f));
        }
        else {
            yLvl = transform.position.y - fireBallHighYOffset;
            StartCoroutine(StartAnimation("Enemy_Boss_HighFireball", 0f));
            StartCoroutine(StopAnimation("Enemy_Boss_HighFireball", 1f));
        }
        StartCoroutine(spawnFireBall(fireBallAniDelay, yLvl));
        
        StartCoroutine(DissableAttack(fireBallAniDelay + 2f));
        StartCoroutine(DissableLookAttack(fireBallAniDelay + 0.5f));
        //isAttacking = false;

    }

    private IEnumerator spawnFireBall(float delay, float yLvl) {
        yield return new WaitForSeconds(delay);
        // Shoot fire ball
        GameObject spawnedFireBall = Instantiate(fireBall, new Vector3(transform.position.x, yLvl, -5f), Quaternion.identity);

        // Give velocity
        Rigidbody2D sfrb = spawnedFireBall.GetComponent<Rigidbody2D>();

        if (facing == Facing.left) {
            sfrb.velocity = new Vector2(left, 0f) * fireBallSpeed;
            spawnedFireBall.transform.localScale = new Vector3(-spawnedFireBall.transform.localScale.x, spawnedFireBall.transform.localScale.y, spawnedFireBall.transform.localScale.z);
        }
        else if (facing == Facing.right) {
            sfrb.velocity = new Vector2(right, 0f) * fireBallSpeed;
        }
    }

    private void Spikes() {
        isAttacking = true;
        lookAttack = true;

        // Define the length of the line where spikes will spawn on, based on the players position
        float center = Random.Range(player.position.x - spikesOffsetFromPlayer, player.position.x + spikesOffsetFromPlayer);
        float length = Random.Range(spikesMinLength, spikesMaxLength);

        
        int count = (int) (length / 2f);
        //Debug.Log($"Spawning {count} spikes");
        float stepSize = length / count;
        float step = center - length / 2f;
        StartCoroutine(StartAnimation("Enemy_Boss_SummonSpikes", 0.5f));
        StartCoroutine(spawnSpikes(0f, count, stepSize, step));
        StartCoroutine(DissableAttack(1.3f + 2.5f));
        StartCoroutine(DestroySpikes(1.3f + 2.2f));
        StartCoroutine(StopAnimation("Enemy_Boss_SummonSpikes", 1.3f + 2f));
        StartCoroutine(DissableLookAttack(1.3f + 2.4f));
        //isAttacking = false;
        
    }

    private IEnumerator DestroySpikes(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Lerp color to be transparrent before destroy
        Color transColor = new Color(1f, 1f, 1f, 0.5f);
        Color fullColor = new Color(1f, 1f, 1f, 1f);
        float time = 0f;
        float duration = 1f;
        while (time < duration) {
            for (int i = 0; i < spikes.Count; i++) {
                SpriteRenderer sr = spikes[i].GetComponent<SpriteRenderer>();
                sr.color = Color.Lerp(fullColor, transColor, time / duration);
                time += Time.deltaTime;
            }
            yield return null;
        }
        for (int i = 0; i < spikes.Count; i++) {
            Destroy(spikes[i]);
        }
        spikes.Clear();
    }

    private IEnumerator spawnSpikes(float delay, int count, float stepSize, float step) {
        
        yield return new WaitForSeconds(delay);
        // Scatter an amount of 'count' spikes out on a line of length 'length' with the length between each step of 'stepSize'
        spikes.Clear();
        //Debug.Log($"SPAWNING: {count} SPIKES");
        for (int i = 0; i < count; i++) { // Spawn spikes
            float randomXOffset = Random.Range(-0.35f, 0.35f);
            //float rotationOffset = Random.Range(-2f, 2f);
            GameObject spike = Instantiate(spikePref, new Vector3(step + randomXOffset, transform.position.y - spikeDistToGrnd, 0f), Quaternion.identity);
            spike.transform.localScale = new Vector3(spike.transform.localScale.x, spike.transform.localScale.y, spike.transform.localScale.z);
            //spike.transform.eulerAngles = new Vector3(spike.transform.eulerAngles.x, spike.transform.eulerAngles.y, rotationOffset);
            spike.GetComponent<SpriteRenderer>().sortingOrder = 100;
            spikes.Add(spike);
            step += stepSize;
        }

        // Wait and let player see spikes before mving them up
        yield return new WaitForSeconds(1.3f);

        float playerStartPos = player.position.y;
        float distY = Mathf.Abs(playerStartPos + 1.33f - spikes[0].transform.position.y);
        while (distY > 0.1f) {
            for (int i = 0; i < spikes.Count; i++) {
                spikes[i].transform.Translate(new Vector3(0f, distY, 0f) * spikesSpeed * Time.deltaTime);
                
            }
            distY = Mathf.Abs(playerStartPos + 1f - spikes[0].transform.position.y);
            yield return null;
        }
        
        /*
        for (int i = 0; i < spikes.Count; i++) {
            spikes[i].GetComponent<Rigidbody2D>().MovePosition(spikes[i].transform.position.x, )
        }
        */

        //Debug.Log(spikes[0].transform.position.y);


    }

    private State GetState() {
        float xDist = Mathf.Abs(transform.position.x - player.position.x);
        State thisState;
        if (xDist > passiveDist) {
            thisState = State.idle;
        }
        else if (xDist > combatDist) {
            thisState = State.passive;
        }
        else if (xDist < combatDist) {
            thisState = State.combat;
        }
        else {
            thisState = State.idle;
        }
        return thisState;
    }

    private int ChooseDir() {
        // No need if not in passive and already in movement
        if (dirToMove != noDir) {
            if (state != State.passive && !isMoving && !isAttacking) {
                return noDir;
            }
        }

        int choice = UnityEngine.Random.Range(1, 3);
        if (choice == 1) {
            return right;
        }
        else {
            return left;
        }
    }
    private void Flip() {
        float scale = Mathf.Abs(transform.localScale.x);
        float xDiff = transform.position.x - player.position.x;
        if (xDiff < 0) {
            transform.localScale = new Vector3(scale, transform.localScale.y, transform.localScale.z);
            facing = Facing.right;
        }
        else if (xDiff > 0) {
            transform.localScale = new Vector3(-scale, transform.localScale.y, transform.localScale.z);
            facing = Facing.left;
        }
    }

    private IEnumerator Teleport() {
        float maxRight = 0f, maxLeft = 0f;
        // Get the new location of the random teleport
        float stepSize = 0.5f; // the increase between each step
        int maxDepth = 20; // The maximum step depth
        float step = transform.position.x; // The current step 
        for (int i = 0; i < maxDepth; i++) {
            // Check if there is void at current step
            Collider2D hit = Physics2D.OverlapPoint(new Vector2(step, transform.position.y));
            if (hit == null) {
                maxRight = Mathf.Abs(transform.position.x - step);
                break;
            }
            else if (i == maxDepth - 1) {
                maxRight = Mathf.Abs(transform.position.x - step);
                break;
            }
            
            step += stepSize;
        }
        step = transform.position.x;
        for (int i = 0; i < maxDepth; i++) {
            
            // Check if there is void at current step
            Collider2D hit = Physics2D.OverlapPoint(new Vector2(step, transform.position.y));
            if (hit == null) {
                maxLeft = Mathf.Abs(transform.position.x - step);
                break;
            }
            else if (i == maxDepth - 1) {
                maxLeft = Mathf.Abs(transform.position.x - step);
                break;
            }
            
            step -= stepSize;
        }

        bool dontmove = false;
        
        float xOffset = 0f;
        if (maxRight >= maxLeft) {
            xOffset = Random.Range(maxRight / 2f, maxRight - stepSize * 2f);
        }
        else if (maxLeft >= maxRight){
            xOffset = -Random.Range(maxLeft / 2f, maxLeft - stepSize * 2f);
        }
        // Dont move if the distance is NOT greater than 3f
        if (maxRight < 3f && maxLeft < 3f) {
            dontmove = true;
        }

        //Debug.Log($"Dist to right: {maxRight}, dist to left: {maxLeft}");

        Vector3 newPos = new Vector3(transform.position.x + xOffset, transform.position.y, transform.position.z);
        if (!dontmove) {
            // Disbale box collider while teleporting so player doesnt get fucked by invinsible mf
            boxCollider.enabled = false;
            rigidBody.gravityScale = 0f;

            isMoving = true;
            //Debug.Log("moving");
            // Create first smokescreen
            SpriteRenderer smokeSpriteRenderer = Instantiate(smokePref, transform.position, Quaternion.identity).GetComponent<SpriteRenderer>();
            smokeSpriteRenderer.color = new Color(smokeSpriteRenderer.color.r, smokeSpriteRenderer.color.g, smokeSpriteRenderer.color.b, 0f);
            smokeSpriteRenderer.sortingOrder = 100;
            StartCoroutine(LerpColor(0f, 15f, 0.5f, smokeSpriteRenderer, 0.5f));  

            StartCoroutine(MoveToLoc(1.5f, newPos));
            yield return new WaitForSeconds(0.4f);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);

            // Create second smokescreen
            smokeSpriteRenderer = Instantiate(smokePref, new Vector3(transform.position.x + xOffset, transform.position.y, 0f), Quaternion.identity).GetComponent<SpriteRenderer>();
            smokeSpriteRenderer.color = new Color(smokeSpriteRenderer.color.r, smokeSpriteRenderer.color.g, smokeSpriteRenderer.color.b, 0f);
            smokeSpriteRenderer.sortingOrder = 100;
            StartCoroutine(LerpColor(0.5f, 1.5f, 0.5f, smokeSpriteRenderer, 0.5f)); 
            yield return null;
        }
        
    }

    IEnumerator moveCooldown() {
        shouldMove = false;
        yield return new WaitForSeconds(Random.Range(8f, 16f));
        shouldMove = true;
    }

    IEnumerator LerpColor(float delay, float outDuration, float inDuration, SpriteRenderer sr, float stayDelay) {
        Color transColor = sr.color;
        Color fullColor = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        float time = 0f;
        yield return new WaitForSeconds(delay);
        // Fade in
        while (time < inDuration) {
            sr.color = Color.Lerp(transColor, fullColor, time / inDuration);
            time += Time.deltaTime;
            yield return null;
        }
        // Fully apply 1 transparency
        sr.color = fullColor;
        yield return new WaitForSeconds(stayDelay);
        // Fade out
        time = 0f;
        while (time < outDuration) {
            sr.color = Color.Lerp(fullColor, transColor, time / inDuration);
            time += Time.deltaTime;
            yield return null;
        }
        // Fulle apply 0 transparency
        sr.color = transColor;

        Destroy(sr.gameObject);
    }

    private IEnumerator MoveToLoc(float delay, Vector3 loc) {
        yield return new WaitForSeconds(delay);
        rigidBody.MovePosition(loc);
        StartCoroutine(DisableMovement(1.5f));

        // Apply full opacity
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        boxCollider.enabled = true;
        rigidBody.gravityScale = 1f;
    }
    private IEnumerator DisableMovement(float delay) {
        yield return new WaitForSeconds(delay);
        isMoving = false;
    }
    private IEnumerator StopAnimation(string animation, float delay) {
        yield return new WaitForSeconds(delay);
        animator.SetBool(animation, false);
    }

    private IEnumerator StartAnimation(string animation, float delay) {
        yield return new WaitForSeconds(delay);
        animator.SetBool(animation, true);
    }

    private IEnumerator DissableAttack(float delay) {
        yield return new WaitForSeconds(delay);

        
        isAttacking = false;
    }
    private IEnumerator DissableLookAttack(float delay) {
        yield return new WaitForSeconds(delay);
        lookAttack = false;
    }

    private IEnumerator AttackCooldown() {
        shouldAttack = false;
        yield return new WaitForSeconds(3f);
        shouldAttack = true;

    }

    public void TakeDamage(int damage) {
        damage = 1;
        health -= damage;
        FindObjectOfType<AudioManager>().Play("EnemyHit");
        StartCoroutine(flashRed());
        if (health <= 0) {
            kill(killDelay);
        }
    }

    public void kill(float delay) {
        // TODO Bring to main menu  + add bonus points
        gm.SetPlayerScore(gm.GetPlayerScore() + killScore);
        Invoke("die", delay);
    }

    private void die() {

        bossIsDead = true;
        Destroy(gameObject);
    }
    private IEnumerator flashRed(float dl = 0f) {
        yield return new WaitForSeconds(dl); 
        spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }
}
