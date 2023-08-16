using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayTextScript : MonoBehaviour
{
    float transparencyLevel = 0f;
    float timer;


    void FixedUpdate()
    {
        timer += Time.deltaTime;


        if (timer >= .1f && timer < .25f)
        {
            transparencyLevel += .006f;
        }
        else if (timer > .35f && timer < .50f)
        {
            transparencyLevel -= .006f;
        }
        else if (timer > .5f)
        {
            timer = 0;
        }

        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, transparencyLevel);
    }
}
