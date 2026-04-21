using UnityEngine;

namespace MiniTransportTycoon
{
    public class WoodFactory : Facility
    {
        private void Reset()
        {
            SetResourceType(Materials.Wood);
        }

        private void OnValidate()
        {
            SetResourceType(Materials.Wood);
        }

        private void Awake()
        {
            SetResourceType(Materials.Wood);
        }

        public override void produce(GameData game)
        {
            int producedAmount = GetProducedAmount();
            // TODO: Apply producedAmount to GameData wood storage.
        }
    }
}
