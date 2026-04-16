using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronFoundry : Facility
{
    public IronFoundry(int x, int y, Orientation o) : base(x, y, o)
    {
        resourceType = Resource.IRON;
    }

    public override void produce(GameData game)
    {
        updateProductivity();
        //game.iron += (int)(resourceAmount * productivityMultiplier);
        //game.setIron(game.getIron() + (int)(resourceAmount * productivityMultiplier));
    }
}
