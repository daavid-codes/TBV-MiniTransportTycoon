using UnityEngine;

namespace MiniTransportTycoon
{
    public class CoalFactory : Facility
    {
        private void ConfigureFactory()
        {
            SetResourceType(Materials.Iron);
            SetProducedMaterialType(Materials.Coal);
            SetInputRequirements(Materials.Iron);
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
