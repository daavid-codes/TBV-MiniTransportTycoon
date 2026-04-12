using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronFoundry : Facility
{
    public IronFoundry(int x, int y, Orientation o, int area) : base(x, y, o, area)
    {
        
    }

    public override void produce(Game game)
    {
        updateProductivity();
        //game.iron += (int)(resourceAmount * productivityMultiplier);
        game.setIron(game.getIron() + (int)(resourceAmount * productivityMultiplier));
    }
}
