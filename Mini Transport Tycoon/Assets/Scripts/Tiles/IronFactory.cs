using UnityEngine;

namespace MiniTransportTycoon
{
    public class IronFactory : Facility
    {
        private void ConfigureFactory()
        {
            SetResourceType(Materials.Steel);
            SetProducedMaterialType(Materials.Iron);
            SetInputRequirements(Materials.Steel, Materials.Coal);
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
