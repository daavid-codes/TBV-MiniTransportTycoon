using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    public class VehicleManager : MonoBehaviour
    {
        public static VehicleManager Instance { get; private set; }

        private GameData gameData;

        public event Action OnVehiclesChanged;

        private int nextId = 1;
        private List<Vehicle> activeVehicles = new List<Vehicle>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                gameData = GameData.Instance;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (gameData != null)
            {
                gameData.OnHourChanged += DecraseVehicleDurability;
            }
        }

        private void OnDestroy()
        {
            if (gameData != null)
            {
                gameData.OnHourChanged -= DecraseVehicleDurability;
            }
        }

        private void DecraseVehicleDurability()
        {
            for (int i = activeVehicles.Count - 1; i >= 0; i--)
            {
                if (activeVehicles[i] != null)
                {
                    activeVehicles[i].DecreaseDurability();
                    OnVehiclesChanged?.Invoke();
                }
            }
        }

        public int RegisterVehicle(Vehicle vehicle)
        {
            activeVehicles.Add(vehicle);
            OnVehiclesChanged?.Invoke();
            return nextId++;
        }

        public void UnregisterVehicle(Vehicle vehicle)
        {
            if (activeVehicles.Contains(vehicle))
            {
                activeVehicles.Remove(vehicle);
                OnVehiclesChanged?.Invoke();
            }
        }

        public List<Vehicle> GetAllVehicles() => activeVehicles;
    }
}