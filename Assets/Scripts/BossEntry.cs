using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BossEntry : MonoBehaviour
{
    GameManager gm;
    PlayerMovement pm;
    public TextMeshProUGUI pressUpText;

    void Awake() {
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        pm = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            pressUpText.text = "push up to enter!";

            if (Input.GetAxisRaw("Vertical") == 1) {
                // Save weapon to gamemanager
                if (pm.currentWeapon.weapon == Weapons.whip) {
                    Debug.Log("BOSS ENTRY SETTING WHIP");
                    PlayerPrefs.SetString("Weapon", "whip");
                }
                else {
                    Debug.Log("BOSS ENTRY SETTING AXE");
                    PlayerPrefs.SetString("Weapon", "axe");
                }
                SceneManager.LoadScene("Level-Boss");
            }
            
            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            pressUpText.text = "";
        }
    }
}
