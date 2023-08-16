using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candle : MonoBehaviour
{
    private int _health = 1;
    public bool isWeaponCandle;
    public GameObject itemSmallHeart;
    public GameObject itemLargeHeart;
    public GameObject itemTreasureChest;
    public GameObject itemDiamond;
    public GameObject itemPotion;
    public GameObject axe;
    public GameObject whip;
    private PlayerMovement pm;

    public GameObject killFire;
    public float killDelay, fireDelay;

    void Awake() {
        pm = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
    }

    public void TakeDamage(int _damage = 1)
    {
        _health -= _damage;
        if (_health <= 0)
        {
            Invoke("kill", killDelay);
            
        }
    }

    void kill() {
        // Create fire and set opacity to 0
        GameObject fire = Instantiate(killFire, transform.position, Quaternion.identity);
        fire.GetComponent<SpawnItem>().weaponCandle = isWeaponCandle;
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        StartCoroutine(die());
    }

    IEnumerator die() {
        yield return new WaitForSeconds(killDelay);
        Destroy(gameObject);
    }

    /*
    private void kill() {
            if (isWeaponCandle) {
                FindObjectOfType<AudioManager>().Play("Candle");
            // Get the opposite from current weaopm
            GameObject gspawn = axe;
            Weapons currentWeapon = pm.currentWeapon.weapon;
            if (currentWeapon == Weapons.whip) {
                gspawn = axe;
            }
            else {
                gspawn = whip;
            }
            Instantiate(killFire, transform.position, Quaternion.identity);
            //StartCoroutine(DropAndKill(fireDelay, gspawn));
            return;
        }



        FindObjectOfType<AudioManager>().Play("Candle");
        if (spawn <= 35)
            {
                Instantiate(itemSmallHeart, transform.position, Quaternion.identity);
            }
            else if(spawn >= 36 && spawn <= 50)
            {
                Instantiate(itemLargeHeart, transform.position, Quaternion.identity);
            }
            else if (spawn >= 51 && spawn <= 65)
            {
                Instantiate(itemPotion, transform.position, Quaternion.identity);
            }
            else if (spawn >= 66 && spawn <= 90)
            {
                Instantiate(itemDiamond, transform.position, Quaternion.identity);
            }
            else if (spawn >= 91 && spawn <= 100)
            {
                Instantiate(itemTreasureChest, transform.position, Quaternion.identity);
            }

            Instantiate(killFire, transform.position, Quaternion.identity);
            //StartCoroutine(DropAndKill(fireDelay));
    }

    public void DropAndKill(GameObject gsp = null) {
        StartCoroutine(DropAndKill(fireDelay, gsp));
    }
    private IEnumerator DropAndKill(float dl, GameObject gspawn = null)
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        yield return new WaitForSeconds(dl);
        if (gspawn)
            Instantiate(gspawn, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    */
}
