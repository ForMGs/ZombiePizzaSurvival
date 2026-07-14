using System;
using UnityEngine;

[Serializable]
public class DropEntry
{
    public ItemData item;

    [Min(0f)]
    public float weight = 1f;

    public int minAmount = 1;
    public int maxAmount = 1;

}