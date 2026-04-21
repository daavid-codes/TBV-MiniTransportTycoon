using UnityEngine;

namespace MiniTransportTycoon
{
    public class PaperFactory : Facility
    {
        private void Reset()
        {
            SetResourceType(Materials.Paper);
        }

        private void OnValidate()
        {
            SetResourceType(Materials.Paper);
        }

        private void Awake()
        {
            SetResourceType(Materials.Paper);
        }

        public override void produce(GameData game)
        {
            int producedAmount = GetProducedAmount();
            // TODO: Apply producedAmount to GameData paper storage.
        }
    }
}
