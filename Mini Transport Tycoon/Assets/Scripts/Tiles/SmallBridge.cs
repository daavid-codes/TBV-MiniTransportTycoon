using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallBridge : MonoBehaviour
{
    public SmallBridge(int x, int y, Orientation o) : base(x, y, o)
    {
        maxDistance = 1;
    }
}
