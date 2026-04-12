using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Facility : Site
{
    private int callCount;
    private float productivityMultiplier;

    public Facility(int x, int y, Orientation o) : base(x, y, o)
    {
        //area = 3;
        //resourceType = MONEY;
        //resourceAmount = 300;
        callCount = 0;
        productivityMultiplier = 1.0f;
    }

    private void updateProductivity()
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

    public abstract void produce(Game game);
}
