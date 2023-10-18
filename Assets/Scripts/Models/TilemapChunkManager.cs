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

    public ChunkDebugger ChunkDebugger { get; set; }

    private int index = 0;

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

                // Calculate the center of the chunk. Make sure to cast to float to avoid integer division.
                float centerX = startX + (float)ChunkWidth / 2.0f;
                float centerZ = startZ + (float)ChunkHeight / 2.0f;

                // Here, you could create a new chunk object with its boundaries and center point.
                Chunk newChunk = new Chunk(this, startX, startZ, ChunkWidth, ChunkHeight, centerX, centerZ);
                Chunks.Add(newChunk);
            }
        }
    }

    // Okay the way I'm calculating startX and Z is correct.
    // But the center is wrong. Each chunk is 5 in width and 5 in height. Each time i log centerX and Z, I get ints instead of floats..
    // because if you divide 5 you should get 2.5. Instead I get 2, which is wrong.

    public Chunk GetChunkAtGridPosition(int gridX, int gridZ)
    {
        // Here, you need to translate from grid coordinates (gridX, gridZ) to your chunk list index.
        // This translation depends on how you're storing your chunks (e.g., row-major, column-major).

        // For example, if you're storing in row-major order:
        int index = gridZ * ChunkColumns + gridX; // Calculate index based on grid position.

        // Check if the index is within the bounds of the list.
        if (index >= 0 && index < Chunks.Count)
        {
            return Chunks[index];
        }
        else
        {
            return null; // Out of bounds.
        }
    }

    public List<Chunk> GetAllChunks()
    {
        return Chunks;
    }
}

// There might something wrong with this code.
// If I select a tile in chunk x 1 z 2 or is it 3 not sure..
// it will then print out a console log showing that its 
// constructed a path betw