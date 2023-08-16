using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    public GameObject deathScreen, gameOverScreen;

    public void DeathScreenLoad()
    {
        deathScreen.SetActive(true);
        Invoke("DeathScreenUnload", 2f);
    }

    public void DeathScreenUnload()
    {
        deathScreen.SetActive(false);
    }

    public void GameOverScreenLoad()
    {
        gameOverScreen.SetActive(true);
        Invoke("GameOverScreenUnload", 8f);
    }

    public void GameOverScreenUnload()
    {
        gameOverScreen.SetActive(false);
    }
}
