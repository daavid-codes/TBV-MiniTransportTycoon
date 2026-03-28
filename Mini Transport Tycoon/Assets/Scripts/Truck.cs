public class Truck : Vehicle
{
    private void Awake()
    {
        type = CarType.Truck;
    }

    private void Reset()
    {
        type = CarType.Truck;
    }

    private void OnValidate()
    {
        type = CarType.Truck;
    }
}
