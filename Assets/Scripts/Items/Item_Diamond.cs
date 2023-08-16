using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Diamond : MonoBehaviour
{
    public int itemValue;
    GameManager gm;

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
            gm.SetPlayerScore(gm.GetPlayerScore() + itemValue);
        }
    }
}
