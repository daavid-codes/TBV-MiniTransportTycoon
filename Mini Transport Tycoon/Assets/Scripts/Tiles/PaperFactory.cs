using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperFactory : Facility
{
    public PaperFactory(int x, int y, Orientation o, int area) : base(x, y, o, area)
    {

    }

    public override void produce(Game game)
    {
        updateProductivity();
        //game.iron += (int)(resourceAmount * productivityMultiplier);
        game.setPaper(game.getPaper() + (int)(resourceAmount * productivityMultiplier));
    }
}
