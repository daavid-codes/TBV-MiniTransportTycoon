using UnityEngine;

namespace MiniTransportTycoon
{
    public abstract class Facility : MonoBehaviour
    {
        [SerializeField] private int id;
        [SerializeField] private Materials resourceType;
        [SerializeField] private int resourceAmount = 300;

        private int callCount;
        private float productivityMultiplier = 1.0f;

        public int Id => id;
        public Materials ResourceType => resourceType;

        public void Initialize(int facilityId)
        {
            id = facilityId;
            callCount = 0;
            productivityMultiplier = 1.0f;
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

        public abstract void produce(GameData game);
    }
}
