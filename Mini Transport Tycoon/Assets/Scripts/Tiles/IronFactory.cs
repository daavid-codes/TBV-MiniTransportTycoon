using UnityEngine;

namespace MiniTransportTycoon
{
    public class IronFactory : Facility
    {
        private void Reset()
        {
            SetResourceType(Materials.Iron);
        }

        private void OnValidate()
        {
            SetResourceType(Materials.Iron);
        }

        private void Awake()
        {
            SetResourceType(Materials.Iron);
        }

        public override void produce(GameData game)
        {
            int producedAmount = GetProducedAmount();
            // TODO: Apply producedAmount to GameData iron storage.
        }
    }
}
