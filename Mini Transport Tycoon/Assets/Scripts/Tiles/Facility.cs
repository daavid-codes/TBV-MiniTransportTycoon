using System;
using UnityEngine;

namespace MiniTransportTycoon
{
    public abstract class Facility : MonoBehaviour
    {
        [SerializeField] private int id;
        [SerializeField] private Materials resourceType;
        [SerializeField] private int resourceAmount = 3000;
        [SerializeField] private int productionIntervalHours = 3;
        [SerializeField] private int storedProducedAmount;

        private int callCount;
        private float productivityMultiplier = 1.0f;
        private GameData gameData;
        private DateTime nextProductionTime;
        private bool productionScheduleInitialized;
        private bool productionStoppedLogged;

        public int Id => id;
        public Materials ResourceType => resourceType;
        public int StoredProducedAmount => storedProducedAmount;
        public int RemainingResourceAmount => resourceAmount;

        public void Initialize(int facilityId)
        {
            id = facilityId;
            callCount = 0;
            productivityMultiplier = 1.0f;
            storedProducedAmount = 0;
            productionStoppedLogged = false;
        }

        private void Start()
        {
            gameData = FindObjectOfType<GameData>();

            if (gameData == null)
            {
                Debug.LogWarning("Facility could not find GameData, production is disabled.");
                return;
            }

            gameData.OnDataChanged += HandleGameDataChanged;
            HandleGameDataChanged();
        }

        private void OnDestroy()
        {
            if (gameData != null)
            {
                gameData.OnDataChanged -= HandleGameDataChanged;
            }
        }

        protected void SetResourceType(Materials type)
        {
            resourceType = type;
        }

        protected int GetProducedAmount()
        {
            UpdateProductivity();
            return Mathf.RoundToInt(resourceAmount * productivityMultiplier);
        }

        protected void ProduceOwnMaterial(GameData game)
        {
            if (resourceAmount <= 0)
            {
                LogProductionStoppedOnce();
                return;
            }

            int plannedProducedAmount = GetProducedAmount();
            int actualProducedAmount = Mathf.Min(plannedProducedAmount, resourceAmount);

            storedProducedAmount += actualProducedAmount;
            resourceAmount -= actualProducedAmount;
            productionStoppedLogged = false;

            Debug.Log("This factory just produced " + actualProducedAmount + " of " + resourceType + " material.");

            if (resourceAmount <= 0)
            {
                LogProductionStoppedOnce();
            }
        }

        private void HandleGameDataChanged()
        {
            if (gameData == null)
                return;

            DateTime currentDate = gameData.CurrentDate;

            if (!TryInitializeProductionSchedule(currentDate))
                return;

            while (currentDate >= nextProductionTime && resourceAmount > 0)
            {
                produce(gameData);
                nextProductionTime = nextProductionTime.AddHours(productionIntervalHours);
            }

            if (resourceAmount <= 0)
            {
                LogProductionStoppedOnce();
            }
        }

        private bool TryInitializeProductionSchedule(DateTime currentDate)
        {
            if (productionScheduleInitialized)
                return true;

            if (currentDate.Year <= 1)
                return false;

            int validInterval = Mathf.Max(1, productionIntervalHours);
            nextProductionTime = currentDate.AddHours(validInterval);
            productionScheduleInitialized = true;
            return true;
        }

        protected void UpdateProductivity()
        {
            if (callCount < 120)
            {
                productivityMultiplier = 1.0f + (callCount * 0.01f);
            }
            else
            {
                productivityMultiplier = Mathf.Max(0.5f, productivityMultiplier - 0.005f);
            }

            callCount += 1;
        }

        private void LogProductionStoppedOnce()
        {
            if (productionStoppedLogged)
                return;

            Debug.Log("Factory production stopped for " + resourceType + " because no resources are available.");
            productionStoppedLogged = true;
        }

        public abstract void produce(GameData game);
    }
}
