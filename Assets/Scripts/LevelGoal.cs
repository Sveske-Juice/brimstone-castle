using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    GameManager gm;

    void Awake() {
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Saving to playerPrefs"); 
            gm.SaveToPF();

            // Add the score of this level
            addScore();

            // if it is the boss level set dummy var in playerprefs so gm can check in main menu and display scorepop LATER CHANGE TO MAIN MENU
            if (SceneManager.GetSceneByBuildIndex(SceneManager.GetActiveScene().buildIndex + 1) == SceneManager.GetSceneByName("Boss")) {
                Debug.Log("NEXT BOSS");
                PlayerPrefs.SetInt("doneCamp", 1);
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void addScore() {
        // Score
        PlayerPrefs.SetInt("finalScore", PlayerPrefs.GetInt("finalScore") + gm.GetPlayerScore());

        // time + time multiplier
        PlayerPrefs.SetInt("finalScore", PlayerPrefs.GetInt("finalScore") + (int) gm.GetTimeLeft() * 5);

        Debug.Log($"FINAL SCORE: {PlayerPrefs.GetInt("finalScore")}");
    }
}
