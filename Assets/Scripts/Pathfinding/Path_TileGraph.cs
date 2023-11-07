using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Path_TileGraph
{

    // This class constructs a simple pathfinding compatible graph
    // of our world. Each tile is a node. Each WALKABLE neighbour
    // from a tile is linked via an edge connection.

    public Dictionary<Tile, Path_Node<Tile>> nodes;

    public Path_TileGraph(World world)
    {
        // Loop through all the tiles of the world
        // for each tile, create a node
        // Do we create nodes for non-floor tiles? NO!
        //DO WE CREATE NODES FOR TILES THAT ARE COMPLETELY UNWALKABLE (ie walls)? NO!

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < world.Width; x++)
        {
            for(int z = 0; z < world.Height; z++)
            {
                Tile t = world.GetTileAt(x, z);

                //if (t.movementCost > 0) // Tiles with a move cost of 0 are unwalkable.
                //{
                    Path_Node<Tile> n = new Path_Node<Tile>();
                    n.data = t;
                    nodes.Add(t, n);
                //}
            }
        }

        int edgeCount = 0;

        // Now loop through all tiles again
        // Create edges for neighbours

        foreach (Tile t in nodes.Keys)
        {
            Path_Node<Tile> n = nodes[t];

            List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();

            Tile[] neighbours = t.GetNeighbours(true); 

            // NOTE: Some of the array spots could be null.
            // get list of neighbours for the tile
            // if neighbour is walkable, create an edge to the relevant node.

            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] != null && neighbours[i].movementCost > 0)
                {
                    // This neighbour exists and is walkable, so create an edge.

                    // But first, make sure we aren't clipping a diagonal or trying to squeeze inapproriately
                    if (IsClippingCorner(t, neighbours[i]))
                    {
                        continue;
                    }

                    Path_Edge<Tile> e = new Path_Edge<Tile>();
                    e.cost = neighbours[i].movementCost;
                    e.node = nodes[neighbours[i]];

                    // Add the edge to our temporary (and growable!) list
                    edges.Add(e);

                    edgeCount++;
                }
            }

            n.edges = edges.ToArray();
        }
        //Debug.Log("Path_TileGraph: Created " + nodes.Count + " nodes");
       // Debug.Log("Path_TileGraph: Created " + edgeCount + " edges");
    }

    bool IsClippingCorner(Tile curr, Tile neigh)
    {
        // If the movement from curr to neigh is diagonal (e. N-E)
        // Then check to make we aren't clipping (e.g make sure N and E are both walkable)
        if (Mathf.Abs(curr.X - neigh.X) + Mathf.Abs(curr.Z - neigh.Z) == 2)
        {
            int dX = curr.X - neigh.X;
            int dZ = curr.Z - neigh.Z;

            if (World.current.GetTileAt(curr.X - dX, curr.Z).movementCost == 0)
            {
                // East or west is unwalkable this would be a clipped movement
                return true;
            }

            if (World.current.GetTileAt(curr.X, curr.Z - dZ).movementCost == 0)
            {
                // North or south is unwalkable this would be a clipped movement
                return true;
            }
        }

        return false;
    }
}
