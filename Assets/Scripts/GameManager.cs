using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float dayDurationMinutes = 15f;
    private float timer = 0f;
    private bool isDayActive = false;

    public int money = 100;
    public int currentDay = 1;

    public GameObject dayEndWindow;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartDay();
    }

    public void StartDay()
    {
        timer = dayDurationMinutes * 60f;
        isDayActive = true;
        dayEndWindow.SetActive(false);
    }

    private void Update()
    {
        if (isDayActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                EndDay();
            }
        }
    }

    private void EndDay()
    {
        isDayActive = false;
        dayEndWindow.SetActive(true);
        // Trigger News/Events system
    }

    public void NextDay()
    {
        currentDay++;
        StartDay();
    }

    public void BuyItem(ItemType type, int cost)
    {
        if (money >= cost)
        {
            money -= cost;
            // Add to global inventory or just deduct money
            Debug.Log("Bought " + type + " for " + cost);
        }
    }
}
