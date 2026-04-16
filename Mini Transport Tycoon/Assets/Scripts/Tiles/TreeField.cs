using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniTransportTycoon;

public class TreeField : Field
{
    private int treeCount { get; set; }

    public TreeField(int x, int y, Orientation o) : base(x, y, o)
    {
        treeCount = 0;
    }

    public TreeField(int x, int y, Orientation o, int cost) : base(x, y, o)
    {
        this.treeCount = treeCount;
    }
}
