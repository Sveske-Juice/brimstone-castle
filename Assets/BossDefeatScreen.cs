using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDefeatScreen : MonoBehaviour
{
    public GameObject bossDefeatScreen;
    public BossHandler bossHandler;
    public AudioSource levelMusic;
    public GameObject player;
    public GameObject boss;

    // Start is called before the first frame update
    void Awake()
    {
        bossDefeatScreen.SetActive(false);
        levelMusic = Camera.main.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bossHandler.bossIsDead)
        {
            boss.SetActive(false);
            player.SetActive(false);
            levelMusic.Stop();
            bossDefeatScreen.SetActive(true);
            Invoke("BackToMenu", 8);
        }
    }


    public void BackToMenu()
    {
        SceneManager.LoadScene("Main Menu");

    }
}
