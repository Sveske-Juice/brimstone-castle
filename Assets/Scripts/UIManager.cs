using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    /* API for UIManager
     
    ----- Health and Lives -----
    * SetHearts(int _amount)            Sets the ui to display _amount of hearts.
    
    ----- Score -----
    * SetScore(int _amount)             Sets the ui to display _amount of score.
     
    ----- Potions -----
    * SetPotions(int _amount)           Sets the ui to display _amount of potions.
    
    ----- Time Left -----
    * SetTimeLeft(int _amount)          Sets the ui to display _amount of time left before termination.
    
    */


    GameManager _gm;

    TextMeshProUGUI _scoreText, _healthText, _potionsText, _timeLeftText, _livesText, _bossHealthText, _playerHealthText;
    [SerializeField] private  Sprite _fullHeartSprite, _emptyHeartSprite;

    private void Awake()
    {
        _gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        _scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
        _healthText = GameObject.Find("PlayerHealthText").GetComponent<TextMeshProUGUI>();
        _potionsText = GameObject.Find("PotionsText").GetComponent<TextMeshProUGUI>();
        _timeLeftText = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        _livesText = GameObject.Find("LivesText").GetComponent<TextMeshProUGUI>();
        _bossHealthText = GameObject.Find("EnemyHealthText").GetComponent<TextMeshProUGUI>();
        _playerHealthText = GameObject.Find("PlayerHealthText").GetComponent<TextMeshProUGUI>();

    }
    public void SetScore(int _score)
    {
        int _digits = 0;
        int n = _score;
        while (n != 0)
        {
            n = n / 10;
            ++_digits;
        }
        switch (_digits)
        {
            case 0:
                _scoreText.text = "Score-000000";
                break;
            case 1:
                _scoreText.text = $"Score-00000{_score.ToString()}";
                break;
            case 2:
                _scoreText.text = $"Score-0000{_score.ToString()}";
                break;
            case 3:
                _scoreText.text = $"Score-000{_score.ToString()}";
                break;
            case 4:
                _scoreText.text = $"Score-00{_score.ToString()}";
                break;
            case 5:
                _scoreText.text = $"Score-0{_score.ToString()}";
                break;
            default:
                _scoreText.text = $"Score-{_score.ToString()}";
                break;
        }        
    }

    public void SetHearts(int _amount)
    {
        if (_amount < 0)
            return;

        if (_amount == 0)
        {
            for (int i = 0; i < 16; i++)
            {
                //Image _currentIndex = GameObject.Find($"heart{i}").GetComponent<Image>();
                Image _currentIndex = _playerHealthText.gameObject.transform.GetChild(i).GetComponent<Image>();
                _currentIndex.sprite = _emptyHeartSprite;
            }
            _gm.KillPlayer();
        }
        else {
            for (int i = 0; i < 16; i++)
            {
                //Image _currentIndex = GameObject.Find($"heart{i}").GetComponent<Image>();
                Image _currentIndex = _playerHealthText.gameObject.transform.GetChild(i).GetComponent<Image>();
                if (i > _amount)
                {
                    // empty heart
                    _currentIndex.sprite = _emptyHeartSprite;
                    continue;
                }
                else
                {
                    _currentIndex.sprite = _fullHeartSprite;
                }
                
            }
        }
    }
    public void SetBossHearts(int _amount)
    {
        
        //Debug.Log("SETTING BOSS HEARTS-------");
        if (_amount <= 0) {
            for (int i = 0; i < 16; i++)
            {
                Image _currentIndex = _bossHealthText.gameObject.transform.GetChild(i).GetComponent<Image>();
                _currentIndex.sprite = _emptyHeartSprite;
            }
        }
        else {

            for (int i = 0; i < 16; i++)
            {
                Image _currentIndex = _bossHealthText.gameObject.transform.GetChild(i).GetComponent<Image>();

                if (i > _amount)
                {
                    // empty heart
                    _currentIndex.sprite = _emptyHeartSprite;
                    continue;
                }
                else
                {
                    _currentIndex.sprite = _fullHeartSprite;
                }
                
            }
        }
    }
    public void SetTimeLeft(int _amount)
    {
        if (_amount == 0)
            return;

        int _digits = 0;
        int n = _amount;
        while (n != 0)
        {
            n = n / 10;
            ++_digits;
        }

        switch (_digits)
        {
            case 1:
                _timeLeftText.text = $"time 000{_amount}";
                break;
            case 2:
                _timeLeftText.text = $"time 00{_amount}";
                break;
            case 3:
                _timeLeftText.text = $"time 0{_amount}";
                break;
            case 4:
                _timeLeftText.text = $"time {_amount}";
                break;

            default:
                return;

        }
        
    }
    public void SetPotions()
    {
        if(_gm._playerPotions < 10)
        {
            _potionsText.text = "0" + _gm._playerPotions.ToString();
        }
        else
        {
            _potionsText.text = _gm._playerPotions.ToString();
        }
        
    }

    public void SetLives()
    {
        if(_gm._playerLives < 10)
        {
            _livesText.text = "LIVES 0" + _gm._playerLives.ToString();
        }
        else
        {
            _livesText.text = "LIVES " + _gm._playerLives.ToString();
        }
        
    }


}
