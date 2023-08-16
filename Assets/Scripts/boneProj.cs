using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boneProj : MonoBehaviour
{
    public float killDelay;
    void Awake() {
        StartCoroutine(Die(3f));
    }
    
    public void kill() {
        StartCoroutine(Die(killDelay));
    }
    IEnumerator Die(float dl) {
        yield return new WaitForSeconds(dl);
        Destroy(gameObject);
    }
}
