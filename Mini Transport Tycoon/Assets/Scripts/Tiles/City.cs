using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : Site
{
    private int demand;

    public City(int x, int y, Orientation o) : base(x, y, o)
    {
        int demand = 500;//még nem tudom, hogy mi legyen ezzel
    }
}
