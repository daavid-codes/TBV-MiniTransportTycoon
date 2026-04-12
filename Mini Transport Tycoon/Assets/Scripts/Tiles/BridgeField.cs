using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeField : MonoBehaviour
{
    private int maxDistance;
    private int maxSpeed { get; }

    public BridgeField(int x, int y, Orientation o) : base(x, y, o)
    {
        maxDistance = 0;
        maxSpeed = 60;
    }
}
