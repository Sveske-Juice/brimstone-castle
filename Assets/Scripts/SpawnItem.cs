using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    int spawn;
    public bool weaponCandle;
    public bool isMonsterDrop;
    public GameObject axe;
    public GameObject whip;
    public GameObject itemSmallHeart;
    public GameObject itemLargeHeart;
    public GameObject itemTreasureChest;
    public GameObject itemDiamond;
    public GameObject itemPotion;
    PlayerMovement pm;
    void Awake() {
        spawn = Random.Range(1, 100);
        pm = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
    }

    public void DropAndKill() {
        FindObjectOfType<AudioManager>().Play("Candle");
        if (weaponCandle) { // Drop a gameobject (whip | axe)

            // Get the opposite from current weaopm
            GameObject gspawn = axe;
            Weapons currentWeapon = pm.currentWeapon.weapon;
            if (currentWeapon == Weapons.whip) {
                gspawn = axe;
            }
            else {
                gspawn = whip;
            }
            Instantiate(gspawn, transform.position, Quaternion.identity);
        }
        else { // Drop a pickup item
            // Only drop 40% of the time if it is a monster drop
            if (isMonsterDrop) {
                int luckyday = Random.Range(1, 7);
                if (luckyday == 1 || luckyday == 2 || luckyday == 3 || luckyday == 4) {
                    Destroy(gameObject);
                    return;
                } 
            }
            
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
            
            
        }

        Destroy(gameObject);
    }

}
