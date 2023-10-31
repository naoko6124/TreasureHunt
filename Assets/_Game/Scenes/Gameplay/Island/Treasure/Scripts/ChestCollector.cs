using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ChestCollector : MonoBehaviour
{
    [Header("Chest Counter")]
    public int chestCounter = 0;
    public float timer = 5 * 60;
    [SerializeField] private TMP_Text chestCounterText;
    [SerializeField] private TMP_Text timerText;

    public void IncreaseChest()
    {
        chestCounter++;
        chestCounterText.text = chestCounter + "";
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            PlayerPrefs.SetInt("ChestsCollected", chestCounter);
            SceneManager.LoadScene("GameOver");
        }
        int min = Mathf.FloorToInt(timer / 60.0f);
        int sec = Mathf.FloorToInt(timer % 60.0f);
        string timeText = min + ":";
        if (sec < 10) timeText += "0";
        timeText += sec;
        timerText.text = timeText;
    }
}
