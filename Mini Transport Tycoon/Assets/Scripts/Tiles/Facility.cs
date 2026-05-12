using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    [Serializable]
    public class FacilityInputRequirement
    {
        public Materials material;
        public int amountPerUnit = 1;

        public FacilityInputRequirement(Materials material, int amountPerUnit = 1)
        {
            this.material = material;
            this.amountPerUnit = Mathf.Max(1, amountPerUnit);
        }
    }

    public abstract class Facility : MonoBehaviour
    {
        private const int DefaultRequiredInputAmount = 3000;

        [SerializeField] private int id;
        [SerializeField] private Materials resourceType;
        [SerializeField] private Materials producedMaterialType;
        [SerializeField] private int resourceAmount = 3000;
        [SerializeField] private int productionIntervalHours = 3;
        [SerializeField] private int storedProducedAmount;
        [SerializeField] private List<FacilityInputRequirement> inputRequirements = new List<FacilityInputRequirement>();
        [SerializeField] private List<MaterialAmount> inputInventory = new List<MaterialAmount>();

        private int callCount;
        private float productivityMultiplier = 1.0f;
        private GameData gameData;
        private DateTime nextProductionTime;
        private bool productionScheduleInitialized;
        private bool productionStoppedLogged;

        public int Id => id;
        public Materials ResourceType => resourceType;
        public Materials ProducedMaterialType => producedMaterialType;
        public int StoredProducedAmount => storedProducedAmount;
        public int RemainingResourceAmount => resourceAmount;
        public IReadOnlyList<FacilityInputRequirement> InputRequirements => inputRequirements;

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

        protected void SetProducedMaterialType(Materials type)
        {
            producedMaterialType = type;
        }

        protected void SetInputRequirements(params Materials[] requiredMaterials)
        {
            inputRequirements = new List<FacilityInputRequirement>();

            if (requiredMaterials == null)
                return;

            for (int i = 0; i < requiredMaterials.Length; i++)
            {
                inputRequirements.Add(new FacilityInputRequirement(requiredMaterials[i], 1));
            }

            EnsureDefaultInputInventory();
        }

        private void EnsureDefaultInputInventory()
        {
            if (inputInventory == null)
            {
                inputInventory = new List<MaterialAmount>();
            }

            for (int i = 0; i < inputRequirements.Count; i++)
            {
                FacilityInputRequirement requirement = inputRequirements[i];

                if (requirement == null)
                    continue;

                bool hasInventoryEntry = false;

                for (int j = 0; j < inputInventory.Count; j++)
                {
                    if (inputInventory[j].material == requirement.material)
                    {
                        hasInventoryEntry = true;
                        break;
                    }
                }

                if (!hasInventoryEntry)
                {
                    inputInventory.Add(new MaterialAmount(requirement.material, DefaultRequiredInputAmount));
                }
            }
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
            int maxByInputs = GetMaxProducibleByInputs();
            int actualProducedAmount = Mathf.Min(plannedProducedAmount, resourceAmount, maxByInputs);

            if (actualProducedAmount <= 0)
            {
                LogProductionStoppedOnce();
                return;
            }

            ConsumeInputs(actualProducedAmount);

            storedProducedAmount += actualProducedAmount;
            resourceAmount -= actualProducedAmount;
            productionStoppedLogged = false;

            Debug.Log("This factory just produced " + actualProducedAmount + " of " + producedMaterialType + " material.");

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

        public void AddInputMaterial(Materials material, int amount)
        {
            if (amount <= 0)
                return;

            for (int i = 0; i < inputInventory.Count; i++)
            {
                if (inputInventory[i].material != material)
                    continue;

                inputInventory[i].amount += amount;
                return;
            }

            inputInventory.Add(new MaterialAmount(material, amount));
        }

        public int GetInputMaterialAmount(Materials material)
        {
            for (int i = 0; i < inputInventory.Count; i++)
            {
                if (inputInventory[i].material == material)
                    return inputInventory[i].amount;
            }

            return 0;
        }

        public bool RequiresInputMaterial(Materials material)
        {
            if (inputRequirements == null)
                return false;

            for (int i = 0; i < inputRequirements.Count; i++)
            {
                FacilityInputRequirement requirement = inputRequirements[i];

                if (requirement == null)
                    continue;

                if (requirement.material == material)
                    return true;
            }

            return false;
        }

        public int TakeProducedMaterial(int amount)
        {
            if (amount <= 0)
                return 0;

            int takenAmount = Mathf.Min(amount, storedProducedAmount);
            storedProducedAmount -= takenAmount;
            return takenAmount;
        }

        private int GetMaxProducibleByInputs()
        {
            if (inputRequirements == null || inputRequirements.Count == 0)
                return int.MaxValue;

            int maxProducible = int.MaxValue;

            for (int i = 0; i < inputRequirements.Count; i++)
            {
                FacilityInputRequirement requirement = inputRequirements[i];

                if (requirement == null)
                    continue;

                int requiredPerUnit = Mathf.Max(1, requirement.amountPerUnit);
                int availableAmount = GetInputMaterialAmount(requirement.material);
                int producibleFromThisInput = availableAmount / requiredPerUnit;

                maxProducible = Mathf.Min(maxProducible, producibleFromThisInput);
            }

            return maxProducible;
        }

        private void ConsumeInputs(int producedAmount)
        {
            if (producedAmount <= 0 || inputRequirements == null)
                return;

            for (int i = 0; i < inputRequirements.Count; i++)
            {
                FacilityInputRequirement requirement = inputRequirements[i];

                if (requirement == null)
                    continue;

                int requiredAmount = producedAmount * Mathf.Max(1, requirement.amountPerUnit);
                RemoveInputMaterial(requirement.material, requiredAmount);
            }
        }

        private void RemoveInputMaterial(Materials material, int amount)
        {
            if (amount <= 0)
                return;

            for (int i = 0; i < inputInventory.Count; i++)
            {
                if (inputInventory[i].material != material)
                    continue;

                inputInventory[i].amount = Mathf.Max(0, inputInventory[i].amount - amount);
                return;
            }
        }

        private void LogProductionStoppedOnce()
        {
            if (productionStoppedLogged)
                return;

            Debug.Log("Factory production stopped for " + producedMaterialType + " because no resources are available.");
            productionStoppedLogged = true;
        }

        public abstract void produce(GameData game);
    }
}
