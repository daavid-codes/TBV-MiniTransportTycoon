using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    public class VehicleManager : MonoBehaviour
    {
        public static VehicleManager Instance { get; private set; }

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
            return nextId++;
        }

        public void UnregisterVehicle(Vehicle vehicle)
        {
            if (activeVehicles.Contains(vehicle))
            {
                activeVehicles.Remove(vehicle);
            }
        }

        public void GetAllVehicles()
        {
            foreach (var vehicle in activeVehicles)
            {
                Debug.Log($"Vehicle ID: {vehicle.Id}, Type: {vehicle.Type}");
            } 
            
                      
        } 
    }
}