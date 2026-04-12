using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Field
{
    private int x;
    private int y;
    private Orientation orientation;
    private int cost { get; }

    public Field(int x, int y, Orientation orientation)
    {
        this.x = x;
        this.y = y;
        this.orientation = orientation;
        this.cost = 0;
    }
}
