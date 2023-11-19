using UnityEngine;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using System;

[MoonSharpUserData]

public class InventoryManager
{
    // This is a list of all "live" inventories.
    // Later on this will likely be organized by rooms instead
    // of a single master list. (Or in addition to.)
    public Dictionary<string, List<Inventory>> inventories;

    public InventoryManager Current { get; set; }

    public static Action<Furniture> cbCreatedInventory;

    public InventoryManager()
    {
        inventories = new Dictionary<string, List<Inventory>>();
    }

    void CleanupInventory(Inventory inv)
    {
        if (inv.stackSize == 0)
        {
            if (inventories.ContainsKey(inv.objectType))
            {
                inventories[inv.objectType].Remove(inv);
            }
            if (inv.tile != null)
            {
                inv.tile.Inventory = null;
                inv.tile = null;
            }

            if (inv.character != null)
            {
                inv.character.Inventory = null;
                inv.character = null;
            }
        }
    }

    public bool PlaceInventory(Tile tile, Inventory inv)
    {
        bool tileWasEmpty = tile.Inventory == null;

        if (tile.PlaceInventory(inv) == false)
        {
            // The tile did not accept the inventory for whatever reason, therefore stop.
            return false;
        }

      CleanupInventory(inv);

        // We may also created a new stack on the tile, if the tile was previously empty.
        if (tileWasEmpty)
        {
            List<Furniture> stockpiles = new List<Furniture>();
            foreach (var item in World.current.furnitures)
            {
                if (item.ObjectType == "Stockpile")
                {
                    stockpiles.Add(item);

                    if (stockpiles.Count > 0)
                    {
                        //FurnitureActions.Stockpile_Action(item);
                        //cbCreatedInventory?.Invoke(item);

                    }
                }
            }

            if (inventories.ContainsKey(tile.Inventory.objectType) == false)
            {
                inventories[tile.Inventory.objectType] = new List<Inventory>();
            }

            inventories[tile.Inventory.objectType].Add(tile.Inventory);

            World.current.OnInventoryCreated(tile.Inventory);
        }

        return true;
    }

    public bool PlaceInventory(Job job, Inventory inv)
    {

        if (job.inventoryRequirements.ContainsKey(inv.objectType) == false)
        {
            Debug.LogError("Trying to add inventory to a job that it doesnt want.");
            return false;
        }

        job.inventoryRequirements[inv.objectType].stackSize += inv.stackSize;

        if (job.inventoryRequirements[inv.objectType].maxStackSize < job.inventoryRequirements[inv.objectType].stackSize)
        {
            inv.stackSize = job.inventoryRequirements[inv.objectType].stackSize - job.inventoryRequirements[inv.objectType].maxStackSize;
            job.inventoryRequirements[inv.objectType].stackSize = job.inventoryRequirements[inv.objectType].maxStackSize;

        } else
        {
            inv.stackSize = 0;
        }

        CleanupInventory(inv);

        return true;
    }


    public bool PlaceInventory(Character character, Inventory sourceInventory, int amount = -1)
    {
        if (amount < 0)
        {
            amount = sourceInventory.stackSize;
        } else
        {
            amount = Mathf.Min(amount, sourceInventory.stackSize);
        }

        if (character.Inventory == null)
        {
            character.Inventory = sourceInventory.Clone();
            character.Inventory.stackSize = 0;
            inventories[character.Inventory.objectType].Add(character.Inventory);
        } 
        else if (character.Inventory.objectType != sourceInventory.objectType)
        {
            Debug.LogError("Character is trying to pick up a mismatched inventory object type.");
            return false;
        }

        character.Inventory.stackSize += amount;

        if (character.Inventory.maxStackSize < character.Inventory.stackSize)
        {
            sourceInventory.stackSize = character.Inventory.stackSize - character.Inventory.maxStackSize;
            character.Inventory.stackSize = character.Inventory.maxStackSize;

        }
        else
        {
            sourceInventory.stackSize -= amount;
        }

        // At this point, "inv" might be an empty stack if it was merged to another stack.
        CleanupInventory(sourceInventory);
        return true;
    }

    public Inventory GetClosestInventoryOfType(string objectType, Tile t, int desiredAmount, bool canTakeFromStockpile)
    {
        Path_AStar path = GetPathToClosestInventoryOfType(objectType, t, desiredAmount, canTakeFromStockpile);
        return path.EndTile().Inventory;
    }

    public Path_AStar GetPathToClosestInventoryOfType(string objectType, Tile t, int desiredAmount, bool canTakeFromStockpile)
    {
        if (inventories.ContainsKey(objectType) == false)
        {
            //Debug.LogError("GetClosestInventoryOfType -- no items of desired type.");
            return null;
        }

        Path_AStar path = new Path_AStar(World.current, t, null, objectType, desiredAmount, canTakeFromStockpile);

        return path;

    }
}
