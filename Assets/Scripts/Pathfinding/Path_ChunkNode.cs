using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_ChunkNode
{
    public Chunk Chunk { get; private set; }
    public float GCost { get; set; } // Cost from the start point to this chunk.
    public float HCost { get; set; } // Estimated cost from this chunk to the target.

    public float FCost
    {
        get { return GCost + HCost; } // Total cost (used for priority in the queue).
    }

    public Path_ChunkNode(Chunk chunk, float gCost, float hCost)
    {

        this.Chunk = chunk;
        this.GCost = gCost;
        this.HCost = hCost;
    }
}