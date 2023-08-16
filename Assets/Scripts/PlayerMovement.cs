using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Weapons {
        whip,
        axe
    };
public class PlayerMovement : MonoBehaviour
{
    public float _speed, _jumpForce, _ladderClimbSpeed;
    private static float _movement, _prevMovement;
    private bool hasCheckedYLvl = false, farToGround = false;
    public bool _facingRight = true, _isGrounded = true;
    
    [HideInInspector]
    public bool _isCrouching = false, isJumping = false;
    private float _defaultSizeX = 1.0625f, _defaultSizeY = 1.875f, _crouchSizeX = 0.875f, _crouchSizeY = 1.35f;

    private float axeSpawnDelay = 0.35f, axeXOffset, axeYOffset = 0.55f, axeCrouchXOffset, axeCrouchYOffset = -0.2f;
    GameObject curLadder;
    public GameObject axePref; 
    

    public struct WhipWeapon {
        public const float range = 2f;
        public const float damage = 3f;
        public float cooldown, crouchCooldown;
        public const float yOffset = 0.25f;
        public const float timeBHit = 0.35f;
    }

    public struct AxeWeapon {
        public const float range = Mathf.Infinity;
        public const float damage = 4f;
        public float cooldown, crouchCooldown;
        public const float yOffset = 0.25f;
        public const float timeBHit = 0f;
        public const float speed = 12f;
    }

    public struct CurrentWeapon {
        public Weapons weapon;
        public float range;
        public float damage;
        public float coolDown, crouchCooldown;
        public float yOffset;
        public float timeBhit;
        public float speed;

        public void switchW(Weapons _weapon) {
            if (_weapon == Weapons.whip) {
                this.weapon = Weapons.whip;
                this.range = WhipWeapon.range;
                this.damage = WhipWeapon.damage;
                this.coolDown = 0f;
                this.crouchCooldown = 0f;
                this.yOffset = WhipWeapon.yOffset;
                this.timeBhit = WhipWeapon.timeBHit;
            }
            else if (_weapon == Weapons.axe) {
                this.weapon = Weapons.axe;
                this.range = AxeWeapon.range;
                this.damage = AxeWeapon.damage;
                this.coolDown = 0f;
                this.crouchCooldown = 0f;
                this.yOffset = AxeWeapon.yOffset;
                this.timeBhit = AxeWeapon.timeBHit;
                this.speed = AxeWeapon.speed;
            }
        }
    };

    public CurrentWeapon currentWeapon;
    
    List<string> alreadyHits = new List<string>();

    Rigidbody2D _rb;
    public Animator _animator;
    GameManager _gm;
    BoxCollider2D _bc;
    BoxCollider2D _pickupBox;
    PlayerKnockback pk;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _bc = GetComponent<BoxCollider2D>();
        _gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        pk = GetComponent<PlayerKnockback>();
        _pickupBox = GetComponents<BoxCollider2D>()[1];

        // Start out with whip
        //currentWeapon.switchW(Weapons.whip);
        //Debug.Log($"Current w range : {currentWeapon.range}");
        
    }
    void Update()
    {
        _isGrounded = Mathf.Abs(_rb.velocity.y) < 0.0001f;
        Move();
        Jump();
        Combat();
        UsePotion();

        Debug.DrawRay(transform.position, Vector2.right * 0.1f);

    }

    void FixedUpdate() {
        // Ignore collision between enemy and player so player can pass through enemy when player has I frames
        if (_gm.hasIFrames) {
            Physics2D.IgnoreLayerCollision(8, 0, true);
        }
        else {
            Physics2D.IgnoreLayerCollision(8, 0, false);
        }

    }
    void Move()
    {
        // no movement if being knockbacked
        if (pk.isBeingKnockedBack) {
            return;
        }

        _movement = Input.GetAxisRaw("Horizontal");

        if (isJumping && !_isGrounded)
        {
            // Restrict user to not being able to move in mid air (keep velocity)
            _movement = _prevMovement;
        }
        else if (!_isGrounded && _rb.velocity.y < 0 && !isJumping) {
            if (!hasCheckedYLvl) {
                farToGround = isFarToGround();
            }
            
            if (farToGround) {
                _movement = 0f;
            }
            else {
                _movement = _prevMovement;
            }
  
        }
        else
        {
            // Turn jump animation of
            _animator.SetBool("isJumpingCrouching", false);
            isJumping = false;
            hasCheckedYLvl = false;
        }
        if (!_animator.GetBool("isJumpingCrouching") && !_animator.GetBool("isCrouching"))
        {
            // Reset Boxcollider
            _bc.size = new Vector2(_defaultSizeX, _defaultSizeY);
            _pickupBox.size = new Vector2(_defaultSizeX + 0.05f, _defaultSizeY + 0.05f);
        }        

        // Restrict the user to not being able to move/change velocity while whipping, axing or crouching
        if ((_animator.GetBool("isWhipping") || _animator.GetBool("isCrouchWhipping") || _animator.GetBool("isCrouching") || _animator.GetBool("isAxing") || _animator.GetBool("isCrouchAxing"))  && _isGrounded)
            _movement = 0f;

        // Let the animator decide if the character is running based on _movement
        _animator.SetFloat("Speed", Mathf.Abs(_movement));
        

        // Apply the x-axis movement relative to world space on the player
        if (curLadder != null) {
            if (Input.GetAxisRaw("Vertical") == -1) {
                curLadder.transform.GetChild(0).GetComponent<BoxCollider2D>().isTrigger = true;
            }
            else if (Input.GetAxisRaw("Vertical") == 1) {
                curLadder.transform.GetChild(0).GetComponent<BoxCollider2D>().isTrigger = false;
            }
        }
        transform.Translate(new Vector2(_movement, 0f) * _speed * Time.deltaTime);
         

        // Flip the player, according to orientation and movement
        if(_movement > 0 && !_facingRight)
        {
            Flip();
        }
        if (_movement < 0 && _facingRight)
        {
            Flip();
        }
        if (_movement == 0)
        {
            if (Input.GetAxisRaw("Vertical") == -1 && !_animator.GetBool("isWhipping") && !_animator.GetBool("isAxing") && !_animator.GetBool("isCrouching") && !_animator.GetBool("isCrouchAxing") && !isOnLadder(2f))
            {
                _animator.SetBool("isCrouching", true);
                _isCrouching = true;

                // Resize BoxCollider
                _bc.size = new Vector2(_crouchSizeX, _crouchSizeY);
                _pickupBox.size = new Vector2(_crouchSizeX, _crouchSizeY);
                transform.position = new Vector3(transform.position.x, transform.position.y - 0.21f, 0f);
            }
            else
            {
                if (Input.GetAxisRaw("Vertical") == -1 || _animator.GetBool("isCrouchWhipping") || _animator.GetBool("isCrouchAxing"))
                    return;
                
                _isCrouching = false;
                _animator.SetBool("isCrouching", false);

                
            }
        }

        _prevMovement = _movement;
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded && !_animator.GetBool("isWhipping") && !_animator.GetBool("isCrouchWhipping") && !_animator.GetBool("isAxing") && !_animator.GetBool("isCrouchAxing"))
        {
            if (!isOnLadder()) {
                FindObjectOfType<AudioManager>().Play("Jump");
                // Resize BoxCollider
                _bc.size = new Vector2(_crouchSizeX, _crouchSizeY);
                _pickupBox.size = new Vector2(_crouchSizeX, _crouchSizeY);

                _animator.SetBool("isJumpingCrouching", true);   
                _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
                isJumping = true;
            }
        }
    }

    void Flip()
    {
        Vector3 _scale = transform.localScale;
        transform.localScale = new Vector3(-_scale.x, _scale.y, _scale.z);
        _facingRight = !_facingRight;
    }

    void Combat()
    {
        
        //stop whip animation
        if (_animator.GetBool("isWhipping"))
        {            
            currentWeapon.coolDown += Time.deltaTime;
        }
        if (currentWeapon.coolDown >= 0.6f && currentWeapon.weapon == Weapons.whip)
        {
            _animator.SetBool("isWhipping", false);
            currentWeapon.coolDown = 0;
        }

        //stop crouching whip animation
        if (_animator.GetBool("isCrouchWhipping"))
        {
            
            currentWeapon.crouchCooldown += Time.deltaTime;
        }
        if (currentWeapon.crouchCooldown > 0.6f && currentWeapon.weapon == Weapons.whip)
        {
            _animator.SetBool("isCrouchWhipping", false);
            currentWeapon.crouchCooldown = 0;
        }

        // Stop axe ani
        if (_animator.GetBool("isAxing"))
        {
            currentWeapon.coolDown += Time.deltaTime;
        }
        if (currentWeapon.coolDown > 0.5f && currentWeapon.weapon == Weapons.axe)
        {
            _animator.SetBool("isAxing", false);
            _animator.SetBool("isCrouchWhipping", false);
            _animator.SetBool("isWhipping", false);
            currentWeapon.coolDown = 0;
        }
        // Stop crouch axe ani
        if (_animator.GetBool("isCrouchAxing"))
        {
            _animator.SetBool("isCrouching", false);
            _bc.size = new Vector2(_defaultSizeX, _defaultSizeY - 0.05f);
            currentWeapon.crouchCooldown += Time.deltaTime;
        }
        if (currentWeapon.crouchCooldown > 0.5f && currentWeapon.weapon == Weapons.axe)
        {
            _animator.SetBool("isCrouchAxing", false);
            _animator.SetBool("isCrouchWhipping", false);
            _animator.SetBool("isWhipping", false);
            currentWeapon.crouchCooldown = 0;
            _bc.size = new Vector2(_defaultSizeX, _defaultSizeY);
        }


        if (_animator.GetBool("isWhipping") || _animator.GetBool("isCrouchWhipping") || _animator.GetBool("isAxing") || _animator.GetBool("isCrouchAxing"))
            return;

        if (currentWeapon.weapon == Weapons.whip) {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!_animator.GetBool("isCrouching"))
                {
                    // signal enemy that its going to be attacked so it can allow the player to attack and stopping the enemy from attacking
                    signalAttack();
                    StartCoroutine("rayKill");
                    Invoke("WhipSound", .4f);

                    _animator.SetBool("isJumpingCrouching", false);
                    StartCoroutine(StartAttackAnimation(0f, false));
                }
                else
                {
                    // signal enemy that its going to be attacked so it can allow the player to attack and stopping the enemy from attacking
                    signalAttack();
                    StartCoroutine("rayKill");
                    Invoke("WhipSound", .4f);
                    StartCoroutine(StartAttackAnimation(0f, true));
                }
            }
        }
        else if (currentWeapon.weapon == Weapons.axe) {
            if (Input.GetButtonDown("Fire1")) {
                if (!_animator.GetBool("isCrouching")) {
                    // Start axe normal animation
                    StartCoroutine(StartAttackAnimation(0f, false));
                    
                    KillWithAxe();
                }
                else {
                    _animator.SetBool("isCrouching", false);
                    // Start axe crouch animation
                    StartCoroutine(StartAttackAnimation(0f, true));
                    
                    KillWithAxe();
                }
            }
        }
    }

    private void KillWithAxe() {
        // Spawn axe pref in direction player is facing
        float dir;
        if (_facingRight)
            dir = 1;
        else
            dir = -1;
        _animator.SetBool("isJumpingCrouching", false);
        StartCoroutine(SpawnAxe(axeSpawnDelay, dir));
    }


    private IEnumerator SpawnAxe(float delay, float dir) {
        yield return new WaitForSeconds(delay);
        GameObject axe;
        if (_isCrouching) {
            Vector3 location = new Vector3(transform.position.x + axeCrouchXOffset, transform.position.y + axeCrouchYOffset, transform.position.z);
            axe = Instantiate(axePref, location, Quaternion.identity);
        }
        else {
            Vector3 location = new Vector3(transform.position.x + axeXOffset, transform.position.y + axeYOffset, transform.position.z);
            axe = Instantiate(axePref, location, Quaternion.identity);            
        }
        // Give velocity
        //axe.GetComponent<Rigidbody2D>().velocity = new Vector2(dir, 0f) * currentWeapon.speed;
        if (dir == 1) {
            axe.GetComponent<Rigidbody2D>().AddForce(new Vector2(0.4f * (currentWeapon.speed - 4f), 0.7f * (currentWeapon.speed - 2f)), ForceMode2D.Impulse);
        }
        else {
            axe.GetComponent<Rigidbody2D>().AddForce(new Vector2(-0.4f * (currentWeapon.speed - 4f), 0.7f * (currentWeapon.speed - 2f)), ForceMode2D.Impulse);
        }
        // Give damage var to instance
        axe.GetComponent<axeProj>().damage = (int) currentWeapon.damage;
    }

    private void KillWithRay(float delay = 0f)
    {

        // Get the direction of the player orientation
        Vector2 _dir;
        if (_facingRight)
            _dir = Vector2.right;
        else
            _dir = Vector2.left;

        float xOffset;
        if (_dir.x == -1)  {
            xOffset = 1f;
        }
        else {
            xOffset = -1f;
        }
        Vector2 origin = new Vector2(transform.position.x + xOffset, transform.position.y + currentWeapon.yOffset);
        Vector2 size = new Vector2(currentWeapon.range, 0.3f);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, transform.eulerAngles.z, _dir, currentWeapon.range);
        for (int i = 0; i < hits.Length; i++) {
            if (hits[i].collider == null) {
                return;
            }
            if (hits[i].collider.CompareTag("Enemy")) {
                // if already hit enemy; don't
                if (alreadyHits.Contains(hits[i].collider.name)) { 
                    
                    return; 
                }
                // pls look away when reading code below -_-
                try { // normal knight
                    KnightEnemyHandler _keh = hits[i].collider.gameObject.GetComponent<KnightEnemyHandler>();
                    _keh.killDelay = delay;
                    _keh.TakeDamage((int) currentWeapon.damage);
                }
                catch (System.Exception err) { // pumpkin
                    try {
                        PumpkinEnemyHandler _peh = hits[i].collider.gameObject.GetComponent<PumpkinEnemyHandler>();
                        _peh.killDelay = delay;
                        _peh.TakeDamage((int) currentWeapon.damage);
                    }
                    catch(System.Exception err1) { // zombie
                        try {
                            ZombieEnemyHandler _zeh = hits[i].collider.gameObject.GetComponent<ZombieEnemyHandler>();
                            _zeh.killDelay = delay;
                            _zeh.TakeDamage((int) currentWeapon.damage);
                        }
                        catch (System.Exception err2){
                            try {
                                BossHandler bh = hits[i].collider.gameObject.GetComponent<BossHandler>();
                                bh.killDelay = delay;
                                bh.TakeDamage((int) currentWeapon.damage);
                            }
                            catch {
                                SkeletonHandler bh = hits[i].collider.gameObject.GetComponent<SkeletonHandler>();
                                bh.killDelay = delay;
                                bh.TakeDamage((int) currentWeapon.damage);
                            }
                        }
                    }
                }

                
            }
            else if (hits[i].collider.CompareTag("Candle"))
            {
                // if already hit enemy; don't
                if (alreadyHits.Contains(hits[i].collider.name)) { 
                    
                    return; 
                }
                Candle candle = hits[i].collider.gameObject.GetComponent<Candle>();
                // Set delay so candle destruction match whip collision
                candle.killDelay = delay;
                candle.TakeDamage(1);
            }
            else if (hits[i].collider.CompareTag("Projectile")) {
                item_fireball ball = hits[i].collider.GetComponent<item_fireball>();
                ball.killDelay = delay;
                ball.kill();
            }
            else if (hits[i].collider.CompareTag("Bone")){
                boneProj bone = hits[i].collider.GetComponent<boneProj>();
                bone.killDelay = delay;
                bone.kill();
            }
            alreadyHits.Add(hits[i].collider.name);
            
        }

    }

    private void signalAttack() {
        
        // Get the direction of the player orientation
        Vector2 _dir;
        if (_facingRight)
            _dir = Vector2.right;
        else
            _dir = Vector2.left;


        RaycastHit2D hit = Physics2D.BoxCast(new Vector2(transform.position.x, transform.position.y +currentWeapon.yOffset), new Vector2(currentWeapon.yOffset, 0.3f), transform.eulerAngles.z, _dir, currentWeapon.range - 0.2f);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                KnightEnemyHandler _eh = hit.collider.gameObject.GetComponent<KnightEnemyHandler>();
                try {
                    //_eh.isBeingAttacked = true;
                    _eh.TakeDamage(0);
                }
                catch (System.Exception e) {
                    
                }
            }

        }
    }

    private void UsePotion()
    {
        if(_gm._playerPotions > 0)
        {
            if (Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.E))
            {
                _gm.UsePotion();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            curLadder = collision.gameObject;
            if (Input.GetAxisRaw("Vertical") == 1)
            {
                _rb.gravityScale = 0;
                transform.position += new Vector3(0, 1, 0) * _ladderClimbSpeed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                _rb.gravityScale = 0;
                transform.position -= new Vector3(0, 1, 0) * _ladderClimbSpeed * Time.deltaTime;
            }
            else
            {
                transform.position -= new Vector3(0, 1, 0) * _ladderClimbSpeed * Time.deltaTime;
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            _rb.gravityScale = 2;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 _dir;
        if (_facingRight)
            _dir = Vector3.right;
        else
            _dir = Vector3.left;
        Gizmos.DrawWireCube(new Vector3(transform.position.x + _dir.x, transform.position.y + currentWeapon.yOffset, 0f), new Vector3(currentWeapon.range, 0.3f, 0f));
    }

    IEnumerator rayKill() {
        // First ray
        yield return new WaitForSeconds(currentWeapon.timeBhit - 0.2f);
        KillWithRay(0.2f);
        // snd raytransform.position, Vector2.right * 0.001f
        yield return new WaitForSeconds(0.1f);
        KillWithRay(0.1f);
        //thrd ray
        yield return new WaitForSeconds(0.1f);
        KillWithRay(0f);

        alreadyHits.Clear();
        
    }

    private bool isFarToGround() {
        hasCheckedYLvl = true;
        Vector2 ori;
        if (_facingRight) {
            ori = new Vector2(transform.position.x + 1f, transform.position.y);
        }
        else {
            ori = new Vector2(transform.position.x - 1f, transform.position.y);
        }
        RaycastHit2D hit = Physics2D.Raycast(ori, Vector2.down, 3f);
        if (hit.collider != null) {
            if (!hit.collider.CompareTag("CameraBond")) {

                return false;
            }
        }

        return true;
    }

    private bool isCloseToWall(int dir) {
        RaycastHit2D hit;
        if (dir == 1) {
            hit = Physics2D.Raycast(transform.position, Vector2.right * 0.001f);
        }
        else {
            hit = Physics2D.Raycast(transform.position, Vector2.left * 0.001f);
        }
        
        if (hit.collider != null) {
            if (hit.collider.CompareTag("Platform")) {
                Debug.Log(hit.collider.name);
                return true;
            }
        }
        return false;
    }

    private GameObject getLadder() {
        Collider2D[] hits = Physics2D.OverlapPointAll(new Vector2(transform.position.x, transform.position.y + _bc.offset.y - _bc.size.y / 2f - 1f), LayerMask.GetMask("Default"));
        if (hits[0] != null) {
            
            for (int i = 0; i < hits.Length; i++) {
                Debug.Log(hits[i].name);
                if (hits[i].CompareTag("Ladder")) {
                    return hits[i].gameObject;
                }
            }
        }
        return curLadder;
    }

    

    void WhipSound()
    {
        FindObjectOfType<AudioManager>().Play("Whip");
    }

    private IEnumerator StartAttackAnimation(float delay, bool crouch = false) {
        yield return new WaitForSeconds(delay);

        // Whip animations
        if (currentWeapon.weapon == Weapons.whip && crouch) {
            _animator.SetBool("isCrouchWhipping", true);
            _animator.SetBool("isWhipping", false);
        }
        else if (currentWeapon.weapon == Weapons.whip && !crouch) {
            _animator.SetBool("isCrouchWhipping", false);
            _animator.SetBool("isWhipping", true);
        }
        else if (currentWeapon.weapon == Weapons.axe && !crouch) {
            _animator.SetBool("isCrouchAxing", false);
            _animator.SetBool("isAxing", true);
        }
        else if (currentWeapon.weapon == Weapons.axe && crouch) {
            _animator.SetBool("isAxing", false);
            _animator.SetBool("isCrouchAxing", true);
        }
        
    }
    bool isOnLadder(float _yOffset = 0f) {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y - _yOffset, transform.position.z);
        Collider2D[] hits = Physics2D.OverlapPointAll(pos);
        for (int i = 0; i < hits.Length; i++) {
            if (hits[i].CompareTag("Ladder")) {
                return true;
            }
        }
        return false;
    }

    public IEnumerator damageAni() {
        Debug.Log("KKJKOWNOO");
        _animator.SetBool("isDamaged", true);
        yield return new WaitForSeconds(0.2f);
        _animator.SetBool("isDamaged", false);
    }


}

