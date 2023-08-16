using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Hearts : MonoBehaviour
{
    GameManager gm;

    //small heart heals 2, large heals 5
    public int healAmount;


    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FindObjectOfType<AudioManager>().Play("Pickup");
            Destroy(this.gameObject);
            gm.PickupHeart(healAmount);
        }
    }
}
