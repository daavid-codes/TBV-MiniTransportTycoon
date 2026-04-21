using UnityEngine;

namespace MiniTransportTycoon
{
    public class SteelFactory : Facility
    {
        private void Reset()
        {
            SetResourceType(Materials.Steel);
        }

        private void OnValidate()
        {
            SetResourceType(Materials.Steel);
        }

        private void Awake()
        {
            SetResourceType(Materials.Steel);
        }

        public override void produce(GameData game)
        {
            int producedAmount = GetProducedAmount();
            // TODO: Apply producedAmount to GameData steel storage.
        }
    }
}
