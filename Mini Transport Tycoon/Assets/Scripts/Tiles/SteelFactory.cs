using UnityEngine;

namespace MiniTransportTycoon
{
    public class SteelFactory : Facility
    {
        private void ConfigureFactory()
        {
            SetResourceType(Materials.Coal);
            SetProducedMaterialType(Materials.Steel);
            SetInputRequirements(Materials.Coal);
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
