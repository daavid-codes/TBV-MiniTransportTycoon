using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;
using MiniTransportTycoon;

public abstract class Facility : Site
{
    protected int callCount { get; set; }
    protected float productivityMultiplier { get; set; }

    public Facility(int x, int y, Orientation o) : base(x, y, o)
    {
        //area = 3;
        //resourceType = MONEY;
        resourceAmount = 100;
        callCount = 0;
        productivityMultiplier = 1.0f;
    }

    protected void updateProductivity()
    {
        if (callCount < 120)
        {
            productivityMultiplier = 1.0f + (callCount * 0.01f);//ezen még lehet finomítani
        }
        else
        {
            productivityMultiplier = Mathf.Max(0.5f, productivityMultiplier - 0,005f);
        }

        callCount += 1;
    }

    public abstract void produce(GameData game);
}
