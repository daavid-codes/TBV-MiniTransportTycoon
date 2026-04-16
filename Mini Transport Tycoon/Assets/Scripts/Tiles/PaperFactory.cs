using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperFactory : Facility
{
    public PaperFactory(int x, int y, Orientation o) : base(x, y, o)
    {
        resourceType = Resource.PAPER;
    }

    public override void produce(GameData game)
    {
        updateProductivity();
        //game.iron += (int)(resourceAmount * productivityMultiplier);
        //game.setPaper(game.getPaper() + (int)(resourceAmount * productivityMultiplier));
    }
}
