using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Site : Field
{
    private int area;
    private Resource resourceType;
    private int resourceAmount;

    public Site(int x, int y, Orientation o) :  base(x, y, o)
    {
        cost = 300;//ez csak placeholder!
        this.area = 3;
        this.resourceType = MONEY;
        this.resourceAmount = 300;
    }
}
