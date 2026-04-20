using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;

public class CopperRefinery : Facility
{
    public CopperRefinery(int x, int y, Orientation o) : base(x, y, o)
    {
        resourceType = Resource.COPPER;
    }

    public override void produce(GameData game)
    {
        updateProductivity();
        game.Copper += (int)(resourceAmount * productivityMultiplier);
    }
}
