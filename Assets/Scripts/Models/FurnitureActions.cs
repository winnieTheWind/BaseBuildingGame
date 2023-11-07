using UnityEngine;

public static class FurnitureActions
{
    // This file contains code which will likely be completely moved to
    // some LUA files later on and will be parsed at run-time.

    public static void Door_UpdateAction(Furniture furn, float deltaTime)
    {
        //Debug.Log("Door_UpdateAction: " + furn.furnParameters["openness"]);

        if (furn.GetParameter("is_opening") >= 1)
        {
            furn.ChangeParameter("openness", deltaTime * 4);    // FIXME: Maybe a door open speed parameter?
            if (furn.GetParameter("openness") >= 1)
            {
                furn.SetParameter("is_opening", 0);
            }
        }
        else
        {
            furn.ChangeParameter("openness", deltaTime * -4);
        }

        furn.SetParameter("openness", Mathf.Clamp01(furn.GetParameter("openness")));

        if (furn.cbOnChanged != null)
        {
            furn.cbOnChanged(furn);
        }
    }

    public static ENTERABILITY Door_IsEnterable(Furniture furn)
    {
        //Debug.Log("Door_IsEnterable");
        furn.SetParameter("is_opening", 1);

        if (furn.GetParameter("openness") >= 1)
        {
            return ENTERABILITY.Yes;
        }

        return ENTERABILITY.Soon;
    }

    public static void JobComplete_FurnitureBuilding(Job theJob)
    {
        WorldController.Instance.world.PlaceFurniture(theJob.jobObjectType, theJob.tile);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        theJob.tile.pendingFurnitureJob = null;
    }

    public static Inventory[] Stockpile_GetItemsFromFilter()
    {
        // TODO: This should be reading from some kind of UI for this
        // particular stockpile

        // Since jobs copy arrays automatically, we could already have
        // an Inventory[] prepared and just return that (as a sort of example filter)

        return new Inventory[1] { new Inventory("Steel_Plate", 50, 0) };
    }

    public static void Stockpile_UpdateAction(Furniture furn, float deltaTime)
    {
        // We need to ensure that we have a job on the queue
        // asking for either:
        //  (if we are empty): That ANY loose inventory be brought to us.
        //  (if we have something): Then IF we are still below the max stack size,
        //						    that more of the same should be brought to us.

        // TODO: This function doesn't need to run each update.  Once we get a lot
        // of furniture in a running game, this will run a LOT more than required.
        // Instead, it only really needs to run whenever:
        //		-- It gets created
        //		-- A good gets delivered (at which point we reset the job)
        //		-- A good gets picked up (at which point we reset the job)
        //		-- The UI's filter of allowed items gets changed

        if (furn.tile.inventory != null && furn.tile.inventory.stackSize >= furn.tile.inventory.maxStackSize)
        {
            // We are full!
          furn.CancelJobs();
            return;
        }

        // Maybe we already have a job queued up?
        if (furn.JobCount() > 0)
        {
            // Cool, all done.
            return;
        }

        // We currently are NOT full, but we don't have a job either.
        // Two possibilities: Either we have SOME inventory, or we have NO inventory.

        // Third possibility: Something is WHACK
        if (furn.tile.inventory != null && furn.tile.inventory.stackSize == 0)
        {
            Debug.LogError("Stockpile has a zero-size stack. This is clearly WRONG!");
            furn.CancelJobs();
            return;
        }

        // TODO: In the future, stockpiles -- rather than being a bunch of individual
        // 1x1 tiles -- should manifest themselves as single, large objects.  This
        // would respresent our first and probably only VARIABLE sized "furniture" --
        // at what happenes if there's a "hole" in our stockpile because we have an
        // actual piece of furniture (like a cooking stating) installed in the middle
        // of our stockpile?
        // In any case, once we implement "mega stockpiles", then the job-creation system
        // could be a lot smarter, in that even if the stockpile has some stuff in it, it
        // can also still be requestion different object types in its job creation.

        Inventory[] itemsDesired;

        if (furn.tile.inventory == null)
        {
            itemsDesired = Stockpile_GetItemsFromFilter();
        }
        else
        {
            Inventory desInv = furn.tile.inventory.Clone();
            desInv.maxStackSize -= desInv.stackSize;
            desInv.stackSize = 0;

            itemsDesired = new Inventory[] { desInv };
        }

        Job j = new Job(
            furn.tile,
            null,
            0,
            itemsDesired,
            false,
            false
        );

        // TODO: Later on, add stockpile priorities, so that we can take from a lower
        // priority stockpile for a higher priority one.
        j.canTakeFromStockpile = false;

        j.RegisterJobWorkedCallback(Stockpile_JobWorked);
        furn.AddJob(j);
    }

    static void Stockpile_JobWorked(Job j)
    {
        j.CancelJob();

        // TODO: Change this when we figure out what we're doing for the all/any pickup job.
        foreach (Inventory inv in j.inventoryRequirements.Values)
        {
            if (inv.stackSize > 0)
            {
                World.current.inventoryManager.PlaceInventory(j.tile, inv);

                return;  // There should be no way that we ever end up with more than on inventory requirement with stackSize > 0
            }
        }
    }


    public static void OxygenGenerator_UpdateAction(Furniture furn, float deltaTime)
    {
        if (furn.tile.room.GetGasAmount("O2") < 0.20f)
        {
            // TODO: Change the gas contribution based on the volume of the room
            furn.tile.room.ChangeGas("O2", 0.01f * deltaTime);  // TODO: Replace hardcoded value!
                                                                // TODO: Consume electricity while running!
        }
        else
        {
            // TODO: Stand-by electric usage?
        }
    }

    public static void MiningDroneStation_UpdateAction(Furniture furn, float deltaTime)
    {
        Tile spawnSpot = furn.GetSpawnSpotTile();

        if (furn.JobCount() > 0)
        {
            // We already have a job, so nothing to do.
            if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize)
            {
                // We should stop this job, because its impossible to make any more items.
                furn.CancelJobs();
            }
            return;
        }

        // If we get here, then have no current job. Check to see if your destination is full.
        if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize)
        {
            // We are full, dont make jo
            return;
        }

        // If we get here, we need to create a new job.
        Tile jobSpot = furn.GetJobSpotTile();

        if (jobSpot.inventory != null && (jobSpot.inventory.stackSize >= jobSpot.inventory.maxStackSize))
        {
            // Our drop spot is already full, so don't create a job.
            return;
        }

        Job j = new Job(
           jobSpot,
           MiningDroneStation_JobComplete,
           1f,
           null,
           true, 
           false

       );

        furn.AddJob(j);
    }

    public static void MiningDroneStation_JobComplete(Job j)
    {
        World.current.inventoryManager.PlaceInventory(j.furniture.GetSpawnSpotTile(), new Inventory("Steel_Plate", 50, 2));
    }

}
