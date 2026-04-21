using UnityEngine;

namespace MiniTransportTycoon
{
    public class WoodFactory : Facility
    {
        private void Reset()
        {
            SetResourceType(Resource.WOOD);
        }

        private void OnValidate()
        {
            SetResourceType(Resource.WOOD);
        }

        private void Awake()
        {
            SetResourceType(Resource.WOOD);
        }

        public override void produce(GameData game)
        {
            int producedAmount = GetProducedAmount();
            // TODO: Apply producedAmount to GameData wood storage.
        }
    }
}
