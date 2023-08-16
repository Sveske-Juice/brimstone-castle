using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    private PlatformEffector2D effector;

    private void Awake()
    {
        effector = GetComponent<PlatformEffector2D>();
        
    }

    void Update()
    {
        if (Input.GetAxisRaw("Vertical") == -1)
        {
            effector.rotationalOffset = 180;
            //pm._animator.SetBool("isCrouching", false);
        } 
        else if (Input.GetAxisRaw("Vertical") == 0)
        {
            if (effector.rotationalOffset != 0)
                effector.rotationalOffset = 0;
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        effector.rotationalOffset = 180;
    }


}
