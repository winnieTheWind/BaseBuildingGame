using UnityEngine;
using System.Collections.Generic;

public class ChunkDebugger : MonoBehaviour
{
    public TilemapChunkManager tilemapChunkManager;
    public Color chunkColor = Color.white;
    public Color iconColor = Color.green;  // The color for the test icon
    public float iconSize = 0.5f;          // The size of the test icon
    bool HasBeenDrawn =false;
    public GameObject MarkerPrefab;

    private void Start()
    {
        Debug.Log("ChunkDebugger");
        // Initialize your tilemapChunkManager here.
        // This code assumes you're getting the reference from another component like 'WorldController'.
        tilemapChunkManager = WorldController.Instance.world.tilemapChunkManager;
        tilemapChunkManager.ChunkDebugger = this;

        Chunk chunk = tilemapChunkManager.Chunks[0];

        Instantiate(MarkerPrefab, new Vector3(chunk.CenterX, 0, chunk.CenterZ), Quaternion.identity);
        Instantiate(MarkerPrefab, new Vector3(chunk.StartX, 0, chunk.StartZ), Quaternion.identity);
        Instantiate(MarkerPrefab, new Vector3(chunk.StartX + chunk.Width, 0, chunk.StartZ), Quaternion.identity);
        Instantiate(MarkerPrefab, new Vector3(chunk.StartX, 0, chunk.StartZ + chunk.Height), Quaternion.identity);
        Instantiate(MarkerPrefab, new Vector3(chunk.StartX + chunk.Width, 0, chunk.StartZ + chunk.Height), Quaternion.identity);




    }
}
