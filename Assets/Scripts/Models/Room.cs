using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

public enum EdgeOrientation
{
    North,
    South,
    East,
    West,
    NorthWest,
    SouthWest,
    SouthEast,
    NorthEast,
    Unknown // Use this if the orientation cannot be determined
}

[MoonSharpUserData]
public class Room
{
    Dictionary<string, float> atmosphericGasses;

    List<Tile> tiles;

    World world;

    public Room(World world)
    {
        this.world = world;
        tiles = new List<Tile>();
        atmosphericGasses = new Dictionary<string, float>();
    }

    public void AssignTile(Tile t)
    {
        if (tiles.Contains(t))
        {
            // This tile already in this room.
            return;
        }

        if (t.Room != null)
        {
            // Belongs to some other room
            t.Room.tiles.Remove(t);
        }

        t.Room = this;
        tiles.Add(t);
    }

    public void ReturnTilesToOutsideRoom()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].Room = World.current.GetOutsideRoom();    // Assign to outside
        }
        tiles = new List<Tile>();
    }

    public bool IsOutsideRoom()
    {
        return this == world.GetOutsideRoom();
    }

    public void ChangeGas(string name, float amount)
    {
        if (IsOutsideRoom())
            return;

        if (atmosphericGasses.ContainsKey(name))
        {
            atmosphericGasses[name] += amount;
        }
        else
        {
            atmosphericGasses[name] = amount;
        }

        if (atmosphericGasses[name] < 0)
            atmosphericGasses[name] = 0;

    }

    public float GetGasAmount(string name)
    {
        if (atmosphericGasses.ContainsKey(name))
        {
            return atmosphericGasses[name];
        }

        return 0;
    }

    public float GetGasPercentage(string name)
    {
        if (atmosphericGasses.ContainsKey(name) == false)
        {
            return 0;
        }

        float t = 0;

        foreach (string n in atmosphericGasses.Keys)
        {
            t += atmosphericGasses[n];
        }

        return atmosphericGasses[name] / t;
    }

    public string[] GetGasNames()
    {
        return atmosphericGasses.Keys.ToArray();
    }

    public static void DoRoomFloodFill(Tile sourceTile, bool onlyIfNull)
    {
        // sourceFurniture is the piece of furniture that may be
        // splitting two existing rooms, or may be the final 
        // enclosing piece to form a new room.
        // Check the NESW neighbours of the furniture's tile
        // and do flood fill from them

        World world = World.current;

        Room oldRoom = sourceTile.Room;

        if (oldRoom != null)
        {
            // The source tile had a room, so this must be a new piece of furniture
            // that is potentially dividing this old room into as many as four new rooms
            
            // Try building new rooms for each of our NESW directions
            foreach (Tile t in sourceTile.GetNeighbours())
            {
                if (t.Room != null && (onlyIfNull == false || t.Room.IsOutsideRoom()))
                {
                    ActualFloodFill(t, oldRoom);
                }
            }

            sourceTile.Room = null;

            oldRoom.tiles.Remove(sourceTile);

            // If this piece of furniture was added to an existing room
            // (which should always be true assuming with consider "outside" to be a big room)
            // delete that room and assign all tiles within to be "outside" for now

            if (oldRoom.IsOutsideRoom() == false)
            {
                // At this point, oldRoom shouldn't have any more tiles left in it,
                // so in practice this "DeleteRoom" should mostly only need
                // to remove the room from the world's list.

                

                world.DeleteRoom(oldRoom);

                if (oldRoom.tiles.Count > 0)
                {
                    Debug.LogError("'oldRoom' still has tiles assigned to it. This is clearly wrong.");
                }
            }
        } else
        {
            // oldRoom is null, which means the source tile was probably a wall.
            // Though this may not be the case any longer ie the wall was probably
            // deconstructed. The only thing we have to try is to spawn one new room
            // starting from the tile in question.

            ActualFloodFill(sourceTile, null);
        }
    }

    protected static void ActualFloodFill(Tile tile, Room oldRoom)
    {
        if (tile == null)
        {
            // We are trying to flood fill off the map, so just return
            // without doing anything.
            return;
        }

        if (tile.Room != oldRoom)
        {
            // This tile was already assigned to another "new" room, which means
            // that the direction picked isn't isolated. So we can just return
            // without creating a new room.
            return;
        }

        if (tile.Furniture != null && tile.Furniture.RoomEnclosure)
        {
            // This tile has a wall/door/whatever in it, so clearly
            // we can't do a room here.
            return;
        }

        if (tile.Type == TileType.Empty)
        {
            // This tile is empty space and must remain part of the outside.
            return;
        }


        // If we get to this point, then we know that we need to create a new room.

        Room newRoom = new Room(World.current);
        Queue<Tile> tilesToCheck = new Queue<Tile>();
        tilesToCheck.Enqueue(tile);

        bool isConnectedToSpace = false;
        int processedTiles = 0;

        while (tilesToCheck.Count > 0)
        {
            Tile t = tilesToCheck.Dequeue();

            processedTiles++;

            if (t.Room != newRoom)
            {
                newRoom.AssignTile(t);

                Tile[] ns = t.GetNeighbours();
                foreach (Tile t2 in ns)
                {
                    if (t2 == null || t2.Type == TileType.Empty)
                    {
                        // We have hit open space (either by being the edge of the map or being an empty tile)
                        // so this "room" we're building is actually part of the Outside.
                        // Therefore, we can immediately end the flood fill (which otherwise would take ages)
                        // and more importantly, we need to delete this "newRoom" and re-assign
                        // all the tiles to Outside.

                        isConnectedToSpace = true;
                        if (oldRoom != null)
                        {
                            newRoom.ReturnTilesToOutsideRoom();
                            return;
                        }
                    } else
                    {
                        // We know t2 is not null nor is it an empty tile, so just make sure it
                        // hasn't already been processed and isn't a "wall" type tile.
                        if (t2.Room != newRoom && (t2.Furniture == null || t2.Furniture.RoomEnclosure == false))
                        {
                            tilesToCheck.Enqueue(t2);
                        }
                    }
                }
            }
        }

        //Debug.Log("ActualFloodFill -- Processed Tiles: " + processedTiles);

        if (isConnectedToSpace)
        {
            // All tiles that were found by this floodfil should actually be assigned
            // to outside.
            newRoom.ReturnTilesToOutsideRoom();
            return;
        } else
        {
            newRoom.AddCeiling();
        }

        // Copy data from the old room into the new room.
        if (oldRoom != null)
        {
            // In this case we are splitting one room into two or more, 
            // so we can just copy the old gas ratios.
            newRoom.CopyGas(oldRoom);
        } else
        {
            // In this case, we are merging one or more rooms together,
            // so we need to actually figure out the total volume of gas
            // in the old room vs the new room and correctly adjust
            // atmospheric contributions.
            // TODO
        }



        // Tell the world that a new room has been formed.
        World.current.AddRoom(newRoom);


    }

    public void AddCeiling()
    {
        foreach (Tile t in tiles)
        {
            this.world.PlaceFurniture("Ceiling", t, false);
        }

        foreach (Tile t in GetBoundaryTiles())
        {
            this.world.PlaceFurniture("Ceiling", t, false);
        }

        //foreach (Tile t in GetSecondLevelBoundaryTiles())
        //{
        //    this.world.PlaceFurniture("Ceiling", t, false);
        //}
    }

    public HashSet<Tile> GetBoundaryTiles()
    {
        HashSet<Tile> boundaryTiles = new HashSet<Tile>();

        foreach (Tile roomTile in tiles)
        {
            foreach (Tile neighbor in roomTile.GetNeighbours(true))
            {
                // If neighbor is not in the room and not already in the set, it's a boundary tile
                if (neighbor != null && !tiles.Contains(neighbor) && !boundaryTiles.Contains(neighbor))
                {
                    boundaryTiles.Add(neighbor);
                }
            }
        }

        return boundaryTiles;
    }

    public HashSet<Tile> GetSecondLevelBoundaryTiles()
    {
        HashSet<Tile> firstLevelBoundary = GetBoundaryTiles();
        HashSet<Tile> secondLevelBoundary = new HashSet<Tile>();

        foreach (Tile boundaryTile in firstLevelBoundary)
        {
            foreach (Tile neighbor in boundaryTile.GetNeighbours(true))
            {
                // Exclude tiles that are part of the room or the first boundary
                if (neighbor != null && !tiles.Contains(neighbor) && !firstLevelBoundary.Contains(neighbor) && !secondLevelBoundary.Contains(neighbor))
                {
                    secondLevelBoundary.Add(neighbor);
                }
            }
        }

        return secondLevelBoundary;
    }

    // Ok this is good. So I'm spawning edge objects on the outside of the room, which is great.

    // I've seperated them between Ceiling and Edge. I need to check the neighbours..
    // if there is a neighbour to the N, then do something..
    // and so on

    void CopyGas(Room other)
    {
        foreach (string n in other.atmosphericGasses.Keys)
        {
            this.atmosphericGasses[n] = other.atmosphericGasses[n];
        }
    }

}
