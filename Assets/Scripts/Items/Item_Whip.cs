using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Whip : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Swicthing to whip");
            FindObjectOfType<AudioManager>().Play("Pickup");
            // Switch weapon to whip    
            PlayerMovement pm = collision.gameObject.GetComponent<PlayerMovement>();
            pm.currentWeapon.switchW(Weapons.whip);
            Destroy(gameObject);
        }
    }
}
