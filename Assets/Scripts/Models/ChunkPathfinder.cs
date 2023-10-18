using System.Collections.Generic;
using UnityEngine; // Necessary if you're using Unity's Vector3, etc.

public class ChunkPathfinder
{
    private List<Chunk> path;
    public List<Chunk> FindPath(Chunk start, Chunk end)
    {
        // A list to store the path from start to end.
        path = new List<Chunk>();

        // Used for BFS algorithm, containing chunks to be inspected.
        Queue<Chunk> frontier = new Queue<Chunk>();
        frontier.Enqueue(start);

        // To avoid revisiting the same chunk.
        HashSet<Chunk> visited = new HashSet<Chunk>();
        visited.Add(start);

        // To reconstruct the path, we remember where we came from.
        Dictionary<Chunk, Chunk> cameFrom = new Dictionary<Chunk, Chunk>();
        cameFrom[start] = null;

        while (frontier.Count > 0)
        {
            Chunk current = frontier.Dequeue();

            if (current.Equals(end))
            {
                // Reconstruct the path
                while (current != null)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }

                // The path is currently from end to start, reverse it.
                path.Reverse();
                return path;
            }

            // Checking the neighbors of the current chunk
            Chunk[] neighbors = current.GetNeighbors(); // Assuming non-diagonal neighbors; change the argument if otherwise.

            foreach (Chunk next in neighbors)
            {
                if (!visited.Contains(next))
                {
                    frontier.Enqueue(next);
                    visited.Add(next);
                    cameFrom[next] = current;
                }
            }
        }

        // If here, then no path was found.
        return null;
    }

    // Provides a method to clear the path.
    public void ClearPath()
    {
        if (path != null)
        {
            path.Clear();
        }
    }

    // Provides a method to get the path. Encapsulation is important to prevent external manipulation.
    public List<Chunk> GetPath()
    {
        return path; // You might return a copy if you want to prevent external changes.
    }

    public List<Chunk> GetCurrentPath()
    {
        return path; // This returns the current path without altering it.
    }
}

// This aint working. I'm initializing this class in a function that is being updated every frame.
// Because I want to find a path with the chunks at any point when the user tells the worker
// to start a job.