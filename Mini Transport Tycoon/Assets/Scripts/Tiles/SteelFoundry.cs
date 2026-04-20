using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;

public class SteelFoundry : Facility
{
    public SteelFoundry(int x, int y, Orientation o) : base(x, y, o)
    {
        resourceType = Resource.STEEL;
    }

    public override void produce(GameData game)
    {
        updateProductivity();
        game.Steel += (int)(resourceAmount * productivityMultiplier);
    }
}
