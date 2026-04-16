using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Field
{
    protected int x;
    protected int y;
    protected Orientation orientation;
    protected int cost { get; set; }

    public Field(int x, int y, Orientation orientation)
    {
        this.x = x;
        this.y = y;
        this.orientation = orientation;
        this.cost = 0;
    }
}
