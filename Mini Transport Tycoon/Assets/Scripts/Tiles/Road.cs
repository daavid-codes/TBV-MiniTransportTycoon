using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;

public abstract class Road : Field
{
    private bool leftSideOccupied { get; set; }
    private bool rightSideOccupied { get; set; }

    public Road(int x, int y, Orientation o) : base(x, y, o)
    {
        cost = 50;
        leftSideOccupied = false;
        rightSideOccupied = false;
    }
}
