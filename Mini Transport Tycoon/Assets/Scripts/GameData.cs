using UnityEngine;
using System;
using System.Collections;

namespace MiniTransportTycoon
{
    public class GameData : MonoBehaviour
    {
    public event Action OnDataChanged;

    [SerializeField] private int money = 4242;
    [SerializeField] private string cityName = "Kaposvar";
    [SerializeField] private float timeMultiplier = 0.2f;
    
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
            yield return new WaitForSeconds(timeMultiplier);
            CurrentDate = CurrentDate.AddMinutes(1);
        }
    }
    }
}