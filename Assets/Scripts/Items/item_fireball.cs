using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class item_fireball : MonoBehaviour
{
    public float killDelay;
    public int damage = 2;

    public void kill() {
        StartCoroutine(die(killDelay));
    }

    private IEnumerator die(float dl) {
        yield return new WaitForSeconds(dl);
        Destroy(gameObject);
    }
}
