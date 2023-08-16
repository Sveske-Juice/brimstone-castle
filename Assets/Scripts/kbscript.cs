using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
public class kbscript : MonoBehaviour
{
    TMP_InputField inputField;
    string initial = "";
    int curIndex = 0, aplhaLength = 30;
    const int vertJump = 11;
    float inpCool = 0f;
    bool isCool = true;
    char[] kbMap = new char[] {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'Æ', 'Ø', 'Å', 'b', 'e', 'e', 'e'};
    Color32 higlightColor = new Color32(255, 179, 0, 255);
    Color32 unhiglightColor = new Color32(255, 255, 255, 255);
    GameObject es;
    GameManager gm;
    private void Awake() {
        inputField = GameObject.Find("initialInput").GetComponent<TMP_InputField>();
        es = GameObject.Find("EventSystem");
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        higlightChar(curIndex);
        gm.UpdateScoreboard();
        
    }

    private void Update() {
        if (isCool) {
            if (Input.GetAxisRaw("Horizontal") == 1) {
                unhiglightChar(curIndex);
                inc();
                higlightChar(curIndex);
                StartCoroutine("isCoolCountdown");
            }
            else if(Input.GetAxisRaw("Horizontal") == -1) {
                unhiglightChar(curIndex);
                dec();
                higlightChar(curIndex);
                StartCoroutine("isCoolCountdown");
            }
            else if (Input.GetAxisRaw("Vertical") == -1) {
                unhiglightChar(curIndex);
                inc(vertJump);               
                higlightChar(curIndex);
                StartCoroutine("isCoolCountdown");
            }
            else if (Input.GetAxisRaw("Vertical") == 1) {
                unhiglightChar(curIndex);
                dec(vertJump);
                higlightChar(curIndex);
                StartCoroutine("isCoolCountdown");
            }
        }
        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space)) {
            updateInit();
        }
    }

    private void updateInit()
    {
        if (initial.Length == 3) {
            
            if (kbMap[curIndex] == 'e') {
                gm.SaveScore(initial);
                gm.UpdateScoreboard();
                GameObject.Find("Scorepop").SetActive(false);
            }
        }
        // backspace
        if (kbMap[curIndex] == 'b') {
            // remove last char in str
            if (initial.Length > 0) 
                initial = initial.Remove(initial.Length -1);
        }
        else if (kbMap[curIndex] == 'e') {
            if (initial.Length < 3) {
                return;
            }
        }
        else if (initial.Length < 3) {
            initial += kbMap[curIndex];
            inputField.text = initial;
            Debug.Log(initial);
        }
        

        inputField.text = initial;


        gm.UpdateScoreboard();
    }

    IEnumerator isCoolCountdown() {
        isCool = false;
        yield return new WaitForSeconds(0.15f);
        isCool = true;
    }

    private void higlightChar(int Index)
    {
        if (Index > aplhaLength) {
            return;
        }
        // backspace char
        if (kbMap[Index] == 'b' || kbMap[Index] == 'e') {
            GameObject.Find(kbMap[Index].ToString()).GetComponent<Image>().color = higlightColor;            
        }
        else {
            GameObject.Find(kbMap[Index].ToString()).transform.GetChild(0).GetComponent<TMP_Text>().color = higlightColor;
        }
    }

    private void unhiglightChar(int Index)
    {
        if (Index > aplhaLength) {
            return;
        }
        // backspace char
        if (kbMap[Index] == 'b' || kbMap[Index] == 'e') {
            GameObject.Find(kbMap[Index].ToString()).GetComponent<Image>().color = unhiglightColor;            
        }
        else {
            GameObject.Find(kbMap[Index].ToString()).transform.GetChild(0).GetComponent<TMP_Text>().color = unhiglightColor;
        }
    }

    private void OnEnable() {
        // Disable main menu event system so it doesn't interfier with current program
        es.GetComponent<StandaloneInputModule>().enabled = false;
    }
    private void OnDisable() {
        // Disable main menu event system so it doesn't interfier with current program
        es.GetComponent<StandaloneInputModule>().enabled = true;
    }

    private void inc(int val = 1) {
        // virklig hacky måde at fixe bug med at T går til enter i stedet for backspace
        if (kbMap[curIndex] == 'T' && val == vertJump) {
            curIndex = 29;
            return;
        }

        if (curIndex + val > aplhaLength) {
            // Set to enter
            curIndex = 30;
            return;
        }
        curIndex += val;
    }

    private void dec(int val = 1) {
        // virklig hacky måde at fixe bug med at T går til enter i stedet for backspace
        if (kbMap[curIndex] == 'T' && val == vertJump) {
            curIndex = 29;
            return;
        }
        if (curIndex - val < 0) {
            // Set to enter
            curIndex = 30;
            return;
        }
        curIndex -= val;
    }
}
