using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    public class VehicleManager : MonoBehaviour
    {
        public static VehicleManager Instance { get; private set; }

        public event Action OnVehiclesChanged;

        private int nextId = 1;
        private List<Vehicle> activeVehicles = new List<Vehicle>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
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