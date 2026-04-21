using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MiniTransportTycoon
{
    public class GameData : MonoBehaviour
    {

    public event Action OnDataChanged;

    [SerializeField] private int money = 4242;
    [SerializeField] private string cityName = "Kaposvar";
    [SerializeField] private float timeMultiplier = 0.2f;
    [SerializeField] private bool isPaused = false;
    [SerializeField] private int wood;
    [SerializeField] private int steel;
    [SerializeField] private int paper;
    [SerializeField] private int iron;
    [SerializeField] private int coal;
    
    private DateTime currentDate;

    public int Money
    {
        get { return money; }
        set
        {
            money = value;
            OnDataChanged?.Invoke();
        }
    }

    public string CityName
    {
        get { return cityName; }
        set
        {
            cityName = value;
            OnDataChanged?.Invoke();
        }
    }

    public DateTime CurrentDate
    {
        get { return currentDate; }
        private set
        {
            currentDate = value;
            OnDataChanged?.Invoke();
        }
    }

    public float TimeMultiplier
    {
        get { return timeMultiplier; }
        set
        {
            timeMultiplier = value;
            OnDataChanged?.Invoke();
        }
    }

    public bool IsPaused
    {
        get { return isPaused; }
        set
        {
            isPaused = value;
            Time.timeScale = isPaused ? 0f : 1f;
            OnDataChanged?.Invoke();
        }
    }

    public void AddMaterial(Materials material, int amount)
    {
        if (amount == 0)
            return;

        switch (material)
        {
            case Materials.Wood:
                wood += amount;
                break;
            case Materials.Steel:
                steel += amount;
                break;
            case Materials.Paper:
                paper += amount;
                break;
            case Materials.Iron:
                iron += amount;
                break;
            case Materials.Coal:
                coal += amount;
                break;
        }

        OnDataChanged?.Invoke();
    }

    public int GetMaterialAmount(Materials material)
    {
        switch (material)
        {
            case Materials.Wood:
                return wood;
            case Materials.Steel:
                return steel;
            case Materials.Paper:
                return paper;
            case Materials.Iron:
                return iron;
            case Materials.Coal:
                return coal;
            default:
                return 0;
        }
    }

    private void Start()
    {
        currentDate = new DateTime(2026, 1, 1, 8, 0, 0);
        StartCoroutine(TimeRoutine());
        OnDataChanged?.Invoke();
    }

    private IEnumerator TimeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeMultiplier);
            CurrentDate = CurrentDate.AddMinutes(1);
        }
    }
    }
}