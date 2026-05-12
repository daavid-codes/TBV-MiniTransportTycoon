using System;
using System.Collections.Generic;
using PlasticGui.WorkspaceWindow;
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

        public int NextId => nextId;

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
                gameData.OnDayChanged += AutoRepairVehicles;
            }
        }

        private void OnDestroy()
        {
            if (gameData != null)
            {
                gameData.OnHourChanged -= DecraseVehicleDurability;
                gameData.OnDayChanged -= AutoRepairVehicles;
            }
        }

        private void DecraseVehicleDurability()
        {
            for (int i = activeVehicles.Count - 1; i >= 0; i--)
            {
                if (activeVehicles[i] != null)
                {
                    activeVehicles[i].DecreaseDurability();
                    activeVehicles[i].SetMaintenanceCost((100 - activeVehicles[i].GetDurability()) * 10);
                    OnVehiclesChanged?.Invoke();
                }
            }
        }

        private void AutoRepairVehicles()
        {
            if (gameData == null) return;

            bool vehiclesChanged = false;
            for (int i = activeVehicles.Count - 1; i >= 0; i--)
            {
                Vehicle v = activeVehicles[i];
                if (v != null && v.HasGarageInRoute() && v.GetDurability() < 100)
                {
                    int repairCost = ((100 - v.GetDurability()) * 10) / 2; // Féláron történő javítás
                    gameData.Money -= repairCost;
                    v.SetDurability(100);
                    v.SetMaintenanceCost(0);
                    vehiclesChanged = true;
                }
            }

            if (vehiclesChanged)
            {
                OnVehiclesChanged?.Invoke();
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

        public void RemoveVehicleById(int id)
        {
            var vehicle = activeVehicles.Find(v => v.Id == id);
            if (vehicle != null)
            {
                activeVehicles.Remove(vehicle);
                OnVehiclesChanged?.Invoke();
            }
        }

        public void RepairVehicle(Vehicle vehicle)
        {
            vehicle.SetDurability(100);
            vehicle.SetMaintenanceCost(0);
            OnVehiclesChanged?.Invoke();
        }

        public void SetNextId(int id)
        {
            nextId = id;
        }
    }
}