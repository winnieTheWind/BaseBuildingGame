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

    // List to hold the tiles in this chunk.
    public List<Tile> tiles;

    private List<Inventory> inventoryItems;

    public Chunk(int startX, int startZ, int width, int height, float centerX, float centerZ)
    {
        // Initialization...
        StartX = startX;
        StartZ = startZ;
        Width = width;
        Height = height;
        CenterX = centerX;
        CenterZ = centerZ;

        tiles = new List<Tile>();
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


    // ... other parts of your Chunk class ...

    /// <summary>
    /// Checks if the chunk has enough inventory of the specified type.
    /// </summary>
    /// <param name="objectType">Type of inventory object.</param>
    /// <param name="desiredAmount">The amount of inventory needed.</param>
    /// <param name="canTakeFromStockpile">If true, inventory can be taken from stockpiles.</param>
    /// <returns>True if enough inventory is available, false otherwise.</returns>
    public bool HasEnoughInventory(string objectType, int desiredAmount, bool canTakeFromStockpile)
    {
        // Check if the dictionary has the item type
        if (!World.current.inventoryManager.inventories.ContainsKey(objectType))
        {
            return false; // No items of this type
        }

        int totalAmount = 0;

        // Access the list from the dictionary.
        List<Inventory> inventoryList = World.current.inventoryManager.inventories[objectType];

        // Go through the inventory items of the specified type.
        foreach (Inventory item in inventoryList)
        {
            // Here we need to decide how we know if an item is in a stockpile or not.
            // We're assuming that the 'Tile' class has a method or property 'IsStockpile' to check if it's a stockpile.

            bool isInStockpile = item.tile != null && item.IsInStockpile; // Hypothetical check based on your game's logic

            if (canTakeFromStockpile || !isInStockpile)
            {
                totalAmount += item.stackSize; // Or whatever property holds the quantity.
            }
        }

        // Check if we have enough inventory.
        return totalAmount >= desiredAmount;
    }



    // This is a stand-in for whatever method you have that checks stockpile amounts.
    private int GetAmountInStockpile(string objectType)
    {
        int amountInStockpile = 0;
        // Logic to determine how much of the objectType is in stockpiles.
        // This could involve checking a property of your inventory items, or a separate collection
        // that tracks stockpile amounts, etc.
        // ...

        return amountInStockpile;
    }

    // Other methods and properties...
}
