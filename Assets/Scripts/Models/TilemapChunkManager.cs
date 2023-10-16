using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapChunkManager
{
    public int TotalWidth { get; private set; }
    public int TotalHeight { get; private set; }
    public int ChunkRows { get; private set; }
    public int ChunkColumns { get; private set; }

    // These properties are calculated once and exposed for easy access.
    public int ChunkWidth { get; private set; }
    public int ChunkHeight { get; private set; }

    // This could be a list of chunk information, including boundaries and center points.
    public List<Chunk> Chunks { get; private set; }

    static public TilemapChunkManager current { get; protected set; }

    public TilemapChunkManager(int totalWidth, int totalHeight, int chunkRows, int chunkColumns)
    {
        // Set the current TileChunkManager to be this TileChunkManager.
        current = this;

        TotalWidth = totalWidth;
        TotalHeight = totalHeight;
        ChunkRows = chunkRows;
        ChunkColumns = chunkColumns;

        GenerateChunks();
    }

    private void GenerateChunks()
    {
        Chunks = new List<Chunk>();

        // Calculate the dimensions of each chunk
        ChunkWidth = TotalWidth / ChunkColumns;
        ChunkHeight = TotalHeight / ChunkRows;

        for (int i = 0; i < ChunkRows; i++)
        {
            for (int j = 0; j < ChunkColumns; j++)
            {
                // Calculate the lower-left corner of the chunk
                int startX = j * ChunkWidth;
                int startZ = i * ChunkHeight;

                // Calculate the center of the chunk
                float centerX = startX + (ChunkWidth / 2.0f);
                float centerZ = startZ + (ChunkHeight / 2.0f);

                // Here, you could create a new chunk object with its boundaries and center point.
                Chunk newChunk = new Chunk(startX, startZ, ChunkWidth, ChunkHeight, centerX, centerZ);
                Chunks.Add(newChunk);
            }
        }
      

        //Chunks[0].ChangeTilesColor(Color.red);

    }

    // Method to get a chunk by its position in the chunk grid
    public Chunk GetChunk(int chunkColumn, int chunkRow)
    {
        // This assumes chunks are stored row by row. Adjust if your Chunks list is organized differently.
        int index = chunkRow * ChunkColumns + chunkColumn;
        if (index >= 0 && index < Chunks.Count)
        {
            return Chunks[index];
        }
        Debug.LogError("GetChunk: Invalid chunk position");
        return null;
    }

    public List<Chunk> GetAllChunks()
    {
        return Chunks;
    }
}

// i'M TRYing to set one chunk of tiles to red.