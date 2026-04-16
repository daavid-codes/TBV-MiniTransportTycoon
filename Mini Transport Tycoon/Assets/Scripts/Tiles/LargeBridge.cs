using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeBridge : BridgeField
{
    public LargeBridge(int x, int y, Orientation o) : base(x, y, o)
    {
        maxDistance = 3;
    }
}
