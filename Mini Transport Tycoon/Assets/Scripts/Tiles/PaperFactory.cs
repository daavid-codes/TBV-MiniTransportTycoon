using UnityEngine;

namespace MiniTransportTycoon
{
    public class PaperFactory : Facility
    {
        private void ConfigureFactory()
        {
            SetResourceType(Materials.Wood);
            SetProducedMaterialType(Materials.Paper);
            SetInputRequirements(Materials.Wood, Materials.Coal);
        }

        private void Reset()
        {
            ConfigureFactory();
        }

        private void OnValidate()
        {
            ConfigureFactory();
        }

        private void Awake()
        {
            ConfigureFactory();
        }

        public override void produce(GameData game)
        {
            ProduceOwnMaterial(game);
        }
    }
}
