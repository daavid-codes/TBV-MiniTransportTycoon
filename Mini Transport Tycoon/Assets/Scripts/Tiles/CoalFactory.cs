using UnityEngine;

namespace MiniTransportTycoon
{
    public class CoalFactory : Facility
    {
        private void Reset()
        {
            SetResourceType(Materials.Coal);
        }

        private void OnValidate()
        {
            SetResourceType(Materials.Coal);
        }

        private void Awake()
        {
            SetResourceType(Materials.Coal);
        }

        public override void produce(GameData game)
        {
            int producedAmount = GetProducedAmount();
            // TODO: Apply producedAmount to GameData coal storage.
        }
    }
}
