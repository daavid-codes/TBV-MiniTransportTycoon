using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;

public abstract class Site : Field
{
    protected int area;
    protected Resource resourceType;
    protected int resourceAmount;

    public Site(int x, int y, Orientation o) :  base(x, y, o)
    {
        cost = 300;//ez csak placeholder!
        this.area = 3;
        this.resourceType = Resource.MONEY;
        this.resourceAmount = 300;
    }
}
