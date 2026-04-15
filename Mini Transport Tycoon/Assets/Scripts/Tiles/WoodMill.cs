using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodMIll : Facility
{
    public WoodMill(int x, int y, Orientation o, int area) : base(x, y, o, area)
    {
        resourceType = WOOD;
    }

    public override void produce(Game game)
    {
        updateProductivity();
        //game.iron += (int)(resourceAmount * productivityMultiplier);
        game.setWood(game.getWood() + (int)(resourceAmount * productivityMultiplier));
    }
}
