using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class garageField : Field
{
    private List<Vehicle> vehicles;

    public GarageField(int x, int y, Orientation o) : base(x, y, o)
    {
        cost = 300;
        vehicles = new List<Vehicle>();
    }
}
