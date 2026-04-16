using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteelFoundry : Facility
{
    public SteelFoundry(int x, int y, Orientation o) : base(x, y, o)
    {
        resourceType = Resource.STEEL;
    }

    public override void produce(GameData game)
    {
        updateProductivity();
        //game.iron += (int)(resourceAmount * productivityMultiplier);
        //game.setSteel(game.getSteel() + (int)(resourceAmount * productivityMultiplier));
    }
}
