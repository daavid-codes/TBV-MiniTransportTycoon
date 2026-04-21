using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;

public class PaperFactory : Facility
{
    public PaperFactory(int x, int y, Orientation o) : base(x, y, o)
    {
        resourceType = Resource.PAPER;
    }

    public override void produce(GameData game)
    {
        updateProductivity();
        game.Paper += (int)(resourceAmount * productivityMultiplier);
    }
}
