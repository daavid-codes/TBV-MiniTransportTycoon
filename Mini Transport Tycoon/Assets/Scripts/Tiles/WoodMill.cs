using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;

public class WoodMill : Facility
{
    public WoodMill(int x, int y, Orientation o) : base(x, y, o)
    {
        resourceType = Resource.WOOD;
    }

    public override void produce(GameData game)
    {
        updateProductivity();
        //game.iron += (int)(resourceAmount * productivityMultiplier);
        //game.setWood(game.getWood() + (int)(resourceAmount * productivityMultiplier));
    }
}
