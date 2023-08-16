using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

struct scCol {
        public int score;
        public string scoreName;
        public string initial;

        public scCol(int _score, string _scoreName, string _initial) {
            this.score = _score;
            this.scoreName = _scoreName;
            this.initial = _initial;
        }
};
public class GameManager : MonoBehaviour
{
    /* API for player, methods and description:
     
     ----- Player lives -----
     * SetPlayerHealth(int x)     Sets player current health to _amount.
     * GetPlayerHealth()          Returns the current health of the player.
     * PlayerTakeDamage(int x)    Damages the player by x amount of damage (default is 1), 
     *                            return 0 if player was killed in process and calls "KillPlayer()", return 1 if successfull.
     * PickupHearth(int _amount)  Increments the player's current health by _amount.
     * UsePotion()                Increments the player's current health, and decrements the amount of potions the player has.
     * KillPlayer()               Draws defeat screen, logs current score, kills player...
     

     ----- Player score -----
     * GetPlayerScore()           Gets the player's current score.
     * SetPlayerScore(int _amount)
     

     ----- Time Left -----
     * GetTimeLeft()              Gets the player's current time left before termination.
     * SetTimeLeft(int _amount)   Sets the amount of time the player's has left before termination by _amount.
     */

    UIManager _uim;
    public DeathScreen _deathScreenManager;

    public GameObject loadScreen, gameOverScreen;

    private int _playerHealth, _playerScore;
    public int _playerPotions, _playerLives;
    private const int potionHealAmount = 10, visibleScores = 26;
    private double _timeLeft;
    Rigidbody2D _playerRB;
    Vector3 respawnPoint;
    TMP_Text scoreboardText;
    TMP_Text stageText;
    AudioManager audioManager;
    BossHandler bh;
    PlayerMovement pm;
    public bool hasIFrames = false, isStoppingIFrames = false;
    bool outOfLives = false, inMain = false;
    GameObject player;
    GameObject boss;

    public AudioSource levelMusic;
    

    
    private void Awake()
    {
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Main Menu")) {
            scoreboardText = GameObject.Find("TempScoreText").GetComponent<TMP_Text>();
            inMain = true;
            /*
            PlayerPrefs.DeleteKey("score0");
            PlayerPrefs.DeleteKey("score1");
            PlayerPrefs.DeleteKey("score2");
            PlayerPrefs.DeleteKey("score4");
            PlayerPrefs.DeleteKey("initial1");
            PlayerPrefs.DeleteKey("initial2");
            PlayerPrefs.DeleteKey("scoreLength");
            */
            
            // If just compleeted campaign, then show scorepop
            if (PlayerPrefs.GetInt("finalScore") > 0) {
                Debug.Log($"FNINAL S: {PlayerPrefs.GetInt("finalScore")}");
                Debug.Log("JUST FROM CAMP");
                GameObject.Find("Scorepop").SetActive(true);
            }
            else {
                GameObject.Find("Scorepop").SetActive(false);
            }
            UpdateScoreboard();
            return;
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level-Boss")) {
            bh = GameObject.Find("Enemy_Boss").GetComponent<BossHandler>();
        }
        stageText = GameObject.Find("StageText").GetComponent<TMP_Text>();
        stageText.text = SceneManager.GetActiveScene().name;
        gameOverScreen.SetActive(false);
        pm = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        Invoke("LoadScreenStop", 2);
        _playerRB = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        _uim = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        levelMusic = Camera.main.GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        //boss = GameObject.FindGameObjectWithTag("Enemy");
        //_playerLives = 1;

        // Set scene display

    }
    public void SetPlayerHealth(int _amount) { _playerHealth = _amount; }
    public void SetPlayerScore(int _amount) { _playerScore = _amount; }
    public void SetPlayerLives(int _amount) { _playerLives = _amount; }
    public void SetPlayerPotions(int _amount) { _playerPotions = _amount; }
    public int GetPlayerHealth() { return _playerHealth; }
    public int GetPlayerScore() { return _playerScore; }
    public int GetPlayerLives() {return _playerLives; }
    public int GetPlayerPotions() { return _playerPotions; }
    public void PickupHeart(int _amount) { if (_playerHealth >= 15) { return; } _playerHealth += _amount; }
    public void PickupPotion() { _playerPotions++; }
    public void PickupOneUp() { _playerLives++; }
    public void UsePotion() { if (_playerHealth <= 14) { FindObjectOfType<AudioManager>().Play("Potion"); _playerHealth += potionHealAmount; _playerPotions--; } }
    public double GetTimeLeft() { return _timeLeft; }
    private void SetTimeLeft(int _amount) { if (_amount == 0) return; _timeLeft = _amount; }
    
    public void SetPlayerWeapon(string weapon) {
        
        if (weapon == "whip") {
            SetPlayerScore(GetPlayerScore() + 1000);
            PlayerMovement pm = GameObject.Find("Player").GetComponent<PlayerMovement>();
            pm.currentWeapon.switchW(Weapons.whip);
            Debug.Log($"Setting player weapon to: {pm.currentWeapon.weapon}");
        }
        else if (weapon == "axe") {
            SetPlayerScore(GetPlayerScore() + 1000);
            PlayerMovement pm = GameObject.Find("Player").GetComponent<PlayerMovement>();
            pm.currentWeapon.switchW(Weapons.axe);
            Debug.Log($"Setting player weapon to: {pm.currentWeapon.weapon}");
        }
        
    }

    public string GetWeapon() {
        string name = GameObject.Find("Player").GetComponent<PlayerMovement>().currentWeapon.weapon.ToString();
        Debug.Log($"Saving {name} to PF");
        return name;
    }
    
    private void Start()
    {
        //LoadPrefs();
        LoadToRam();
 
    }

    private void Update()
    {
       
        if(_playerLives > 0)
        {
            outOfLives = false;
        }
        else
        {
            outOfLives = true;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            KillPlayer();
        }

    }
    private void FixedUpdate()
    {
        // Don't try do anything if in gm is in main menu
        if (inMain) {
            return;
        }

        // Update the time left
        _timeLeft -= Time.fixedDeltaTime;

        _uim.SetHearts(_playerHealth);
        _uim.SetScore(_playerScore);
        _uim.SetPotions();
        _uim.SetTimeLeft((int) GetTimeLeft());
        _uim.SetLives();
        try {
            _uim.SetBossHearts(bh.GetHealth());
        } catch{}
    }
    public int PlayerTakeDamage(int _damage = 3) 
    {
        
        if (_playerHealth - _damage <= 0) 
        { 
            // Reset weapon
            //PlayerPrefs.SetString("Weapon", "whip");
            KillPlayer();
            return 0; 
        } 
        else 
        {
            //KnockbaDK
            //ck is in another script on the player
            audioManager.Play("PlayerDamage");
            SetPlayerHealth(_playerHealth - _damage);
            StartCoroutine(pm.damageAni());
            hasIFrames = true;
            if (!isStoppingIFrames) {
                isStoppingIFrames = true;
                Invoke("TurnOffIFrames", 1.8f);
            }
        } 
        return 1; 
    }
    public void KillPlayer()
    {
        SetPlayerHealth(0);
        player.SetActive(false);
        //boss.SetActive(false);
        _playerLives--;

        //FindObjectOfType<AudioManager>().Play("PlayerDeath");
        // TODO change respawn to happen when death animation is done but for now, its just on a timer
        if (outOfLives)
        {
            levelMusic.Stop();
            PlayerPrefs.SetString("Weapon", "whip");
            SetPlayerScore(GetPlayerScore() - 5000);
            _deathScreenManager.GameOverScreenLoad();
            Invoke("GameOver", 6);
        }
        else
        {
            _deathScreenManager.DeathScreenLoad();
            Invoke("Respawn", .7f);
        }
        

        // TODO Log player's score

        // TODO kill player
    }

    private void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);


    }

    private void TurnOffIFrames()
    {
        isStoppingIFrames = false;
        hasIFrames = false;
    }

    private void LoadScreenStop()
    {
        loadScreen.SetActive(false);
    }

    private void GameOver()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void LoadPrefs() {
        
            // Level 1
            PlayerPrefs.SetInt("playerScore", 0);
            PlayerPrefs.SetInt("playerHealth", 15);
            PlayerPrefs.SetInt("playerLives", 0);
            PlayerPrefs.SetInt("playerPotions", 0);
            PlayerPrefs.SetFloat("timeLeft", 500f);
            PlayerPrefs.SetString("Weapon", "whip");
            LoadToRam();

    }

    private void LoadToRam() {
        Debug.Log("loading playerPrefs....");
        // Load values from playerPrefs to ram
        SetPlayerScore(PlayerPrefs.GetInt("playerScore"));
        SetPlayerHealth(PlayerPrefs.GetInt("playerHealth"));
        SetTimeLeft((int) PlayerPrefs.GetFloat("timeLeft"));
        SetPlayerLives(PlayerPrefs.GetInt("playerLives"));
        SetPlayerPotions(PlayerPrefs.GetInt("playerPotions"));
        SetPlayerWeapon(PlayerPrefs.GetString("Weapon"));
    }
    
    public void SaveToPF() {
        // Update playerPrefs with ram values
        PlayerPrefs.SetInt("playerScore", GetPlayerScore());
        PlayerPrefs.SetInt("playerHealth", GetPlayerHealth());
        PlayerPrefs.SetInt("playerLives", GetPlayerLives());
        PlayerPrefs.SetInt("playerPotions", GetPlayerPotions());
        PlayerPrefs.SetFloat("timeLeft", (float) GetTimeLeft());
        PlayerPrefs.SetString("Weapon", GetWeapon());
    }

    public void SaveScore(string initial) {
        int finalScore = CalcScore();

        // Increment length of "array" in playerPrefs
        if (PlayerPrefs.HasKey("scoreLength")) {
            PlayerPrefs.SetInt("scoreLength", PlayerPrefs.GetInt("scoreLength") + 1);
        }
        else {
            PlayerPrefs.SetInt("scoreLength", 0);
        }
        // Write new score to playerPrefs at scoreLength
        string sHolder = $"score{PlayerPrefs.GetInt("scoreLength")}";
        string iHolder = $"initial{PlayerPrefs.GetInt("scoreLength")}";
        PlayerPrefs.SetInt(sHolder, finalScore);
        PlayerPrefs.SetString(iHolder, initial);

        // Update changes to scorebaord
        UpdateScoreboard();
    }

    private int CalcScore()
    {
        int fs = PlayerPrefs.GetInt("finalScore");
        // maybe set to 0
        PlayerPrefs.DeleteKey("finalScore");
        return fs;
    }

    public void UpdateScoreboard() {
        // clear text
        scoreboardText.text = "";
        // Sort scores
        int size = PlayerPrefs.GetInt("scoreLength");
        scCol[] sortedScores = new scCol[size + 1];
        // Add all scores to list and store them in a struct
        for (int i = 0; i <= size; i++) {
            sortedScores[i] = new scCol(PlayerPrefs.GetInt($"score{i}"), $"score{i}", $"initial{i}");
        }

        Array.Sort<scCol>(sortedScores, (x,y) => x.score.CompareTo(y.score));
        Array.Reverse<scCol>(sortedScores);
        

        // traverse through scoreLength and update board
        for (int i = 0; i < sortedScores.Length; ++i) {
            // return if past vissible barier
            if (i == visibleScores) {
                return;
            }
            scoreboardText.text += $"{PlayerPrefs.GetString(sortedScores[i].initial)}: {sortedScores[i].score}\n";
        }
    }

    public void ResetStats() {
        PlayerPrefs.SetInt("playerScore", 0);
        PlayerPrefs.SetInt("playerHealth", 15);
        PlayerPrefs.SetInt("playerLives", 0);
        PlayerPrefs.SetInt("playerPotions", 0);
        PlayerPrefs.SetFloat("timeLeft", 500f);
        PlayerPrefs.SetString("Weapon", "whip");
        LoadToRam();
    }
}
