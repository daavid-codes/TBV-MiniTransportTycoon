using UnityEngine;
using System.Collections.Generic;

namespace MiniTransportTycoon
{
    public class VehicleListUI : MonoBehaviour
    {
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private Transform contentParent;

        private List<GameObject> currentEntries = new List<GameObject>();
        private bool isInitialized = false;

        private void Start()
        {
            if (VehicleManager.Instance != null)
            {
                VehicleManager.Instance.OnVehiclesChanged += RefreshList;
                RefreshList();
                isInitialized = true;
            }
        }

        private void OnEnable()
        {
            if (isInitialized)
            {
                RefreshList();
            }
        }

        private void OnDestroy()
        {
            if (VehicleManager.Instance != null)
            {
                VehicleManager.Instance.OnVehiclesChanged -= RefreshList;
            }
        }

        public void RefreshList()
        {
            if (VehicleManager.Instance == null) return;

            foreach (GameObject entry in currentEntries)
            {
                Destroy(entry);
            }
            currentEntries.Clear();

            List<Vehicle> vehicles = VehicleManager.Instance.GetAllVehicles();

            foreach (Vehicle v in vehicles)
            {
                GameObject newEntry = Instantiate(entryPrefab, contentParent);
                currentEntries.Add(newEntry);

                VehicleUIEntry uiEntry = newEntry.GetComponent<VehicleUIEntry>();
                if (uiEntry != null)
                {
                    uiEntry.Setup(v);
                }
            }
        }
    }
}