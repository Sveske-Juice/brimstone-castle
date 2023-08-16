using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Axe : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FindObjectOfType<AudioManager>().Play("Pickup");
            // Switch weapon to Axe
            PlayerMovement pm = collision.gameObject.GetComponent<PlayerMovement>();
            pm.currentWeapon.switchW(Weapons.axe);
            Destroy(gameObject);
        }
    }
}


