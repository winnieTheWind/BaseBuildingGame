using Priority_Queue;
using System.Collections.Generic;
using UnityEngine; // Assuming you're using Unity since you've used UnityEngine.Mathf.

public class Path_ChunkPath
{
    Stack<Chunk> path; // Using Stack instead of Queue for easier reconstruction.

    public List<Chunk> Chunks { get; private set; }

    public Path_ChunkPath()
    {
        Chunks = new List<Chunk>();
    }

    float HeuristicCostEstimate(Chunk a, Chunk b)
    {
        if (b == null) return 0f;

        float distanceX = a.CenterX - b.CenterX;
        float distanceZ = a.CenterZ - b.CenterZ;

        return Mathf.Sqrt(distanceX * distanceX + distanceZ * distanceZ);
    }

    public Stack<Chunk> FindPath(Chunk startChunk, Chunk targetChunk)
    {
        var openSet = new SimplePriorityQueue<Path_ChunkNode>();
        var cameFrom = new Dictionary<Chunk, Chunk>();
        var costSoFar = new Dictionary<Chunk, float>();

        Path_ChunkNode startNode = new Path_ChunkNode(startChunk, 0, HeuristicCostEstimate(startChunk, targetChunk));
        openSet.Enqueue(startNode, startNode.GCost + startNode.HCost);

        cameFrom[startChunk] = startChunk;
        costSoFar[startChunk] = 0;

        while (!openSet.IsEmpty())
        {
            Path_ChunkNode current = openSet.Dequeue();

            if (current.Chunk == targetChunk)
            {
                ReconstructPath(cameFrom, startChunk, targetChunk);
                return path; // Return path immediately after reconstruction.
            }
            
            // TODO: When ready, you'll explore neighbors here.
        }

        return null; // Return null if pathfinding didn't find a path.
    }

    private void ReconstructPath(Dictionary<Chunk, Chunk> cameFrom, Chunk startChunk, Chunk targetChunk)
    {
        path = new Stack<Chunk>();
        Chunk current = targetChunk;

        while (current != startChunk)
        {
            path.Push(current);
            current = cameFrom[current];
        }

        path.Push(startChunk); // Add the starting chunk.
    }
}
