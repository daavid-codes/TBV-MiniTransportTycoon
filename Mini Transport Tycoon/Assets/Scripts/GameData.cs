using UnityEngine;
using System;
using System.Collections;

public class GameData : MonoBehaviour
{
    public event Action OnDataChanged;

    [SerializeField] private int money;
    [SerializeField] private string cityName = "Kezdő Város";
    
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
            yield return new WaitForSeconds(0.2f);
            CurrentDate = CurrentDate.AddMinutes(1);
        }
    }
}