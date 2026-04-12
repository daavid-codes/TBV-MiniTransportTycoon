using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediumBridge : MonoBehaviour
{
    public MediumBridge(int x, int y, Orientation o) : base(x, y, o)
    {
        maxDistance = 2;
    }
}
