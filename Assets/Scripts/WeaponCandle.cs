using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCandle : MonoBehaviour
{
    private int _health = 1;
    public GameObject Axe;
    public GameObject Whip;

    private int spawn;
    public float killDelay;

    private void Start()
    {
        spawn = Random.Range(1, 100);
    }

    public void TakeDamage(int _damage = 1)
    {


        _health -= _damage;
        if (_health <= 0)
        {
            Invoke("kill", killDelay);

        }
    }

    private void kill()
    {
        FindObjectOfType<AudioManager>().Play("Candle");
        if (spawn <= 50)
        {
            Instantiate(Axe, transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(Whip, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
