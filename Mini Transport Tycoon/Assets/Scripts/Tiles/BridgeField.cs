using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;

public class BridgeField : Field
{
    protected int maxDistance;
    protected int maxSpeed { get; }

    public BridgeField(int x, int y, Orientation o) : base(x, y, o)
    {
        maxDistance = 0;
        maxSpeed = 60;
    }
}
