using System.Collections.Generic;
using UnityEngine; // If you're using Unity's Color class, otherwise, you might have a custom one.

public class Chunk
{
    // Properties...
    public int StartX { get; }
    public int StartZ { get; }
    public int Width { get; }
    public int Height { get; }
    public float CenterX { get; }
    public float CenterZ { get; }

    public Vector3 CenterPosition { get; set; }
    // List to hold the tiles in this chunk.
    public List<Tile> tiles;

    public float movementCost = 1;

    private List<Inventory> inventoryItems;

    TilemapChunkManager tilemapChunkManager;

    public List<Character> characters;


    public Chunk(TilemapChunkManager tcm, int startX, int startZ, int width, int height, float centerX, float centerZ)
    {
        tilemapChunkManager = tcm;
        // Initialization...
        StartX = startX;
        StartZ = startZ;
        Width = width;
        Height = height;
        CenterX = centerX;
        CenterZ = centerZ;
        CenterPosition = new Vector3(CenterX, 0, CenterZ);



        tiles = new List<Tile>();
        characters = new List<Character>();
    }

    // Add a tile to the chunk.
    public void AddTile(Tile tile)
    {
        tiles.Add(tile);
    }

    // Method to change the color of all tiles in the chunk.
    public void ChangeTilesColor(Color newColor)
    {
        foreach (Tile tile in tiles)
        {
            tile.ChangeColor(newColor); // Calls the ChangeColor method on your Tile class.
        }
    }
    public Chunk[] GetNeighbors()
    {
        List<Chunk> neighbors = new List<Chunk>();

        // These are the four directions: up, down, left, and right.
        (int, int)[] directions = new (int, int)[] { (0, 1), (0, -1), (-1, 0), (1, 0) };

        int currentChunkGridX = StartX / tilemapChunkManager.ChunkWidth; // Assuming StartX is the world position.
        int currentChunkGridZ = StartZ / tilemapChunkManager.ChunkHeight; // Assuming StartZ is the world position.

        foreach (var (xOffset, zOffset) in directions)
        {
            int neighborChunkGridX = currentChunkGridX + xOffset;
            int neighborChunkGridZ = currentChunkGridZ + zOffset;

            // Validate the calculated grid position before attempting to access the chunk.
            if (neighborChunkGridX >= 0 && neighborChunkGridX < tilemapChunkManager.ChunkColumns &&
                neighborChunkGridZ >= 0 && neighborChunkGridZ < tilemapChunkManager.ChunkRows)
            {
                Chunk neighbor = tilemapChunkManager.GetChunkAtGridPosition(neighborChunkGridX, neighborChunkGridZ);
                if (neighbor != null) neighbors.Add(neighbor);
            }
        }

        return neighbors.ToArray();
    }



}

// Heres the chunk class. cAN YOU rewrite the ChunkPathfinder class to make sure it can connect to this class.