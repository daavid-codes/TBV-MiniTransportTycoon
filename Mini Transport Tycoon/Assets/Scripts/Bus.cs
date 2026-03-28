public class Bus : Vehicle
{
    private void Awake()
    {
        type = CarType.Bus;
    }

    private void Reset()
    {
        type = CarType.Bus;
    }

    private void OnValidate()
    {
        type = CarType.Bus;
    }
}
