using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    public float knockbackForce = 4;
    Rigidbody2D _rb;
    [HideInInspector] public bool isBeingKnockedBack = false;
    GameManager _gm;
    SpriteRenderer sprite;


    private void Awake()
    {
        _gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        _rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            if (!_gm.hasIFrames && !isBeingKnockedBack)
            {
                // Try get the knockbackDamage that the specific enemy deals
                int damage = 1;
                // I know, very bad practice... but no time
                try {
                    KnightEnemyHandler handler = collision.GetComponent<KnightEnemyHandler>();
                    damage = (int) handler.knockbackDamage;
                }
                catch {
                    try {
                        PumpkinEnemyHandler handler = collision.GetComponent<PumpkinEnemyHandler>();
                        damage = (int) handler.knockbackDamage;
                    }
                    catch {
                        try {
                            ZombieEnemyHandler handler = collision.GetComponent<ZombieEnemyHandler>();
                            damage = (int) handler.knockbackDamage;
                        }
                        catch {
                            try {
                                BossHandler handler = collision.GetComponent<BossHandler>();
                                damage = (int) handler.knockbackDamage;
                            }
                            catch {
                                try {
                                    // SKeleton
                                    SkeletonHandler handler = collision.GetComponent<SkeletonHandler>();
                                    damage = (int) handler.knockbackDamage;
                                }
                                catch {}
                            }
                        }
                    }
                }
                // Knigh: 3
                // zombie : 4
                // pump: 2
                // Boss: 2, 3 spikes, 2 fireball
                // skel: 2 proj, 3 collision
                takeKnockback(_rb.transform.position - collision.transform.position, damage);
            
            }
        }
        else if (collision.CompareTag("Projectile")) {
            // try get the damage that specific projectile deals
            try {
                int damage = collision.gameObject.GetComponent<item_fireball>().damage;
                if (!_gm.hasIFrames && !isBeingKnockedBack) {
                    takeKnockback(_rb.transform.position - collision.transform.position, (int) damage);
                    // Destroy fireball
                    Destroy(collision.gameObject);
                }
            }
            catch 
            {

            }
        }
        else if (collision.CompareTag("Spike")) {
            if (!_gm.hasIFrames && !isBeingKnockedBack) {
                takeKnockback(_rb.transform.position - collision.transform.position, 3);
            }
        }
        else if (collision.CompareTag("Bone")) {
            if (!_gm.hasIFrames && !isBeingKnockedBack) {
                takeKnockback(_rb.transform.position - collision.transform.position, 3);
                Destroy(collision.gameObject);
            }
        }
    }

    void FixedUpdate() {
        bool _isGrounded = Mathf.Abs(_rb.velocity.y) < 0.0001f;
        if (_isGrounded) {
            isBeingKnockedBack = false;
        }
    }

    void TurnOffGrayColor()
    {
        sprite.color = Color.white;
    }

    public void takeKnockback(Vector2 dir, int damage = 1) {
        _gm.PlayerTakeDamage(damage);

        sprite.color = Color.gray;

        //xxxxKNOCKBACKxxxx
        _rb.velocity = Vector2.zero;
        isBeingKnockedBack = true;
        Vector2 difference = dir;
        difference = difference.normalized * knockbackForce;
        difference.y = difference.y + 5;
        difference.x = difference.x + difference.x;
        _rb.AddForce(difference, ForceMode2D.Impulse);
        //xxxxxxxxxxxxxxxxx
        Invoke("TurnOffGrayColor", 1.5f);
    }
}
