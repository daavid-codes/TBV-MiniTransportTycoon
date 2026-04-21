using UnityEngine;

namespace MiniTransportTycoon
{
    public class SteelFactory : Facility
    {
        private void Reset()
        {
            SetResourceType(Resource.STEEL);
        }

        private void OnValidate()
        {
            SetResourceType(Resource.STEEL);
        }

        private void Awake()
        {
            SetResourceType(Resource.STEEL);
        }

        public override void produce(GameData game)
        {
            int producedAmount = GetProducedAmount();
            // TODO: Apply producedAmount to GameData steel storage.
        }
    }
}
