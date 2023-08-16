using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplayer : MonoBehaviour
{
    Image displayedWeapon;
    public Sprite axe;
    public Sprite whip;
    PlayerMovement playerMovement;

    
    void Awake()
    {
        displayedWeapon = gameObject.GetComponent<Image>();
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    
    void Update()
    {
        if(playerMovement.currentWeapon.weapon == Weapons.axe)
        {
            displayedWeapon.sprite = axe;
        }
        else
        {
            displayedWeapon.sprite = whip;
        }
    }
}
