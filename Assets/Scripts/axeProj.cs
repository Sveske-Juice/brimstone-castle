using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class axeProj : MonoBehaviour
{
    public int damage = 0;
    float killDelay = 5f;

    void Awake() {
        StartCoroutine(kill(killDelay));
    }
    
    IEnumerator kill(float dl) {
        yield return new WaitForSeconds(dl);
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider) {
        if (collider.CompareTag("Enemy")) {
            // pls look away when reading code below -_-
            try { // normal knight
                KnightEnemyHandler _keh = collider.GetComponent<KnightEnemyHandler>();
                _keh.killDelay = 0f;
                _keh.TakeDamage(damage);
            }
            catch (System.Exception err) { // pumpkin
                try {
                    PumpkinEnemyHandler _peh = collider.GetComponent<PumpkinEnemyHandler>();
                    _peh.killDelay = 0f;
                    _peh.TakeDamage(damage);
                }
                catch(System.Exception err1) { // zombie
                    try {
                        ZombieEnemyHandler _zeh = collider.GetComponent<ZombieEnemyHandler>();
                        _zeh.killDelay = 0f;
                        _zeh.TakeDamage(damage);
                    }
                    catch (System.Exception err2){
                        try {
                            BossHandler bh = collider.GetComponent<BossHandler>();
                            bh.killDelay = 0f;
                            bh.TakeDamage(damage);
                        }
                        catch {
                            SkeletonHandler bh = collider.GetComponent<SkeletonHandler>();
                            bh.killDelay = 0f;
                            bh.TakeDamage(damage);
                        }
                    }
                }
            }
            Destroy(gameObject);
        }
        else if (collider.CompareTag("Candle")) {
            Candle candle = collider.GetComponent<Candle>();
            // Set delay so candle destruction match whip collision
            candle.killDelay = 0f;
            candle.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}
