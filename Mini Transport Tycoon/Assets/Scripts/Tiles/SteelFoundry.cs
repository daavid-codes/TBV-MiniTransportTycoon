using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteelFoundry : Facility
{
    public SteelFoundry(int x, int y, Orientation o, int area) : base(x, y, o, area)
    {
        resourceType = STEEL;
    }

    public override void produce(Game game)
    {
        updateProductivity();
        //game.iron += (int)(resourceAmount * productivityMultiplier);
        game.setSteel(game.getSteel() + (int)(resourceAmount * productivityMultiplier));
    }
}
