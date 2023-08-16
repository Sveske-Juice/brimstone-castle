using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class MainMenuScript : MonoBehaviour
{
    bool isCool = true;
    public GameObject mainMenuFirstButton, levelSelectFirstButton, levelSelectClosedButton;
    GameObject currentObject;
    GameObject es;
    TextMeshProUGUI currentObjectText;
    GameObject lastSelection;
    AudioManager audioManager;
    GameManager gm;

    void Awake() {
        es = GameObject.FindWithTag("es");
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }
    void Start()
    {
        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(mainMenuFirstButton);

    }

    void Update()
    {
        if (lastSelection != null) {
            if (lastSelection != EventSystem.current.currentSelectedGameObject) {
                audioManager.Play("MenuScroll");
            }
        }

        if (Input.GetAxisRaw("Vertical") == -1) {
            currentObjectText.color = Color.white;
            StartCoroutine("Cooldown");
        }
        else if (Input.GetAxisRaw("Vertical") == 1) {
            currentObjectText.color = Color.white;
            StartCoroutine("Cooldown");
        }
        
        //Debug.Log(EventSystem.current.currentSelectedGameObject.name);
        currentObject = EventSystem.current.currentSelectedGameObject;
        
        if (currentObject != null) {
            currentObjectText = currentObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            try {
                currentObjectText.color = new Color32(255, 179, 0, 255);
            }
            catch (System.Exception e) {

            }
        }

        lastSelection = EventSystem.current.currentSelectedGameObject;
    }


    IEnumerator Cooldown()
    {
        isCool = false;
        yield return new WaitForSeconds(0.2f);
        isCool = true;
    }

    public void Campaign()
    {
        SceneManager.LoadScene("Level-01");
        gm.LoadPrefs();
        FindObjectOfType<AudioManager>().Play("Button");
    }
 
    public void Level2()
    {
        SceneManager.LoadScene("Level-02");
        FindObjectOfType<AudioManager>().Play("Button");
    }

    public void Level3()
    {
        SceneManager.LoadScene("Level-03");
        FindObjectOfType<AudioManager>().Play("Button");
    }

    public void Boss()
    {
        SceneManager.LoadScene("Level-Boss");
        FindObjectOfType<AudioManager>().Play("Button");
    }

    public void LevelSelect()
    {
        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(levelSelectFirstButton);
        FindObjectOfType<AudioManager>().Play("Button");
    }

    public void Back()
    {
        currentObjectText.color = Color.white;
        //clear selected object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected object
        EventSystem.current.SetSelectedGameObject(levelSelectClosedButton);
        FindObjectOfType<AudioManager>().Play("Button");
    }
}
