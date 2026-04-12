using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopperRefinery : Facility
{
    public CopperRefinery(int x, int y, Orientation o, int area) : base(x, y, o, area)
    {

    }

    public override void produce(Game game)
    {
        updateProductivity();
        //game.iron += (int)(resourceAmount * productivityMultiplier);
        game.setCopper(game.getCopper() + (int)(resourceAmount * productivityMultiplier));
    }
}
