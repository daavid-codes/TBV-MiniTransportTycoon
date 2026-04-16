using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;

public class GarageField : Field
{
    protected List<Vehicle> vehicles;

    public GarageField(int x, int y, Orientation o) : base(x, y, o)
    {
        this.cost = 300;
        vehicles = new List<Vehicle>();
    }
}
