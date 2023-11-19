using System.Collections.Generic;
using System;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class Job
{
    // This class holds info for a queued up job, which can include
    // things like placing furniture, moving stored inventory,
    // working at a desk, and maybe even fighting enemies.

    public Tile tile;
    public Tile tilePrototype;
    public TileType tileType;

    public bool isRenderingTile = false;

    public float jobTime
    {
        get;
        protected set;
    }

    public string jobObjectType
    {
        get; protected set;
    }

    protected float jobTimeRequired;
    protected bool jobRepeats = false;

    public Furniture furniturePrototype;
    public Furniture furniture; // the piece of furniture that owns this job, frequently will be null.

    public bool acceptsAnyInventoryItem = false;

    public Action<Job> cbJobCompleted; // We have finished the work cycle and so things should probably get built or whatever.
    public Action<Job> cbJobStopped; // The job has been stopped, either because its non-repeating or was cancelled.
    public Action<Job> cbJobWorked; // Gets called each time some work is performed -- maybe update the ui?

    public bool canTakeFromStockpile = true;

    public Dictionary<string, Inventory> inventoryRequirements;

    public bool Is3d = false;

    // Jobs for objects
    public Job(Tile tile, string jobObjectType, Action<Job> cbJobCompleted, float jobTime = 0.2f, Inventory[] inventoryRequirements = null, bool is3d = false, bool jobRepeats = false)
    {
        this.tile = tile;
        this.jobObjectType = jobObjectType;
        this.cbJobCompleted += cbJobCompleted;
        this.jobTimeRequired = this.jobTime = jobTime;
        this.Is3d = is3d;
        this.jobRepeats = jobRepeats;

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in inventoryRequirements)
            {
                this.inventoryRequirements[inv.objectType] = inv.Clone();

            }
        }
    }

    // Empty jobs
    public Job(Tile tile, Action<Job> cbJobCompleted, float jobTime = 0.2f, Inventory[] inventoryRequirements = null, bool jobRepeats = false, bool isRenderingTile = true)
    {
        this.tile = tile;
        this.cbJobCompleted += cbJobCompleted;
        this.jobTimeRequired = this.jobTime = jobTime;
        this.jobRepeats = jobRepeats;
        this.isRenderingTile = isRenderingTile;

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in inventoryRequirements)
            {
                this.inventoryRequirements[inv.objectType] = inv.Clone();

            }
        }
    }

    // Jobs For Tiles
    public Job(Tile tile, TileType tileType, Action<Job> cbJobCompleted, float jobTime = 0.2f, Inventory[] inventoryRequirements = null, bool Is3d = false, bool jobRepeats = false)
    {
        this.tile = tile;
        this.tileType = tileType;
        this.cbJobCompleted += cbJobCompleted;
        this.jobTimeRequired = this.jobTime = jobTime;
        this.Is3d = Is3d;
        this.jobRepeats = jobRepeats;

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in inventoryRequirements)
            {
                this.inventoryRequirements[inv.objectType] = inv.Clone();

            }
        }
    }

    protected Job(Job other)
    {
        this.tile = other.tile;
        this.jobObjectType = other.jobObjectType;
        this.cbJobCompleted = other.cbJobCompleted;
        this.jobTime = other.jobTime;
        this.Is3d = other.Is3d;
        this.tileType = other.tileType;
        this.jobRepeats = other.jobRepeats;

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null)
        {
            foreach (Inventory inv in other.inventoryRequirements.Values)
            {
                this.inventoryRequirements[inv.objectType] = inv.Clone();
            }
        }
    }

    virtual public Job Clone()
    {
        return new Job(this);
    }

    public void RegisterJobCompletedCallback(Action<Job> cb)
    {
        cbJobCompleted += cb;
    }

    public void RegisterJobStoppedCallback(Action<Job> cb)
    {
        cbJobStopped += cb;
    }

    public void UnregisterJobCompletedCallback(Action<Job> cb)
    {
        cbJobCompleted -= cb;
    }

    public void UnregisterJobStoppedCallback(Action<Job> cb)
    {
        cbJobStopped -= cb;
    }

    public void RegisterJobWorkedCallback(Action<Job> cb)
    {
        cbJobWorked += cb;
    }

    public void UnregisterJobWorkedCallback(Action<Job> cb)
    {
        cbJobWorked -= cb;
    }

    public void DoWork(float workTime)
    {
        // Check to make sure we actually have everything we need.
        // if not, dont register the work time
        if (HasAllMaterial() == false)
        {
            //Debug.LogError("Tried to do work on a job that doesnt have all the materials.");

            // Job cant actually be worked but still call the cb so that
            // animations and whatnot can be updated;
            if (cbJobWorked != null)
            {
                cbJobWorked(this);
            }
            return;
        }

        jobTime -= workTime;

        if (cbJobWorked != null)
        {
            cbJobWorked(this);
        }

        if (jobTime <= 0)
        {
            if (cbJobCompleted != null)
                cbJobCompleted(this);

            if (jobRepeats == false)
            {
                if (cbJobStopped != null)
                {
                    cbJobStopped(this);
                }
            } else
            {
                jobTime += jobTimeRequired;
            }
        }
    }

    public void CancelJob()
    {
        if (cbJobStopped != null)
            cbJobStopped(this);

        World.current.jobQueue.Remove(this);
    }

    public bool HasAllMaterial()
    {
        foreach (Inventory inv in inventoryRequirements.Values)
        {
            if (inv.maxStackSize > inv.stackSize)
            {
                return false;
            }
        }

        return true;
    }

    public int DesiresInventoryType(Inventory inv)
    {
        if (acceptsAnyInventoryItem)
        {
            return inv.maxStackSize;   
        }

        if (inventoryRequirements.ContainsKey(inv.objectType) == false)
        {
            return 0;
        }

        if (inventoryRequirements[inv.objectType].stackSize >= inventoryRequirements[inv.objectType].maxStackSize)
        {
            // we already have all that we need.
            return 0;
        }

        // the inventory is of a type we want, and we still need more.
        return inventoryRequirements[inv.objectType].maxStackSize - inventoryRequirements[inv.objectType].stackSize;
    }

    public Inventory GetFirstDesiredInventory()
    {
        foreach (Inventory inv in inventoryRequirements.Values)
        {
            if (inv.maxStackSize > inv.stackSize)
                return inv;            
        }

        return null;
    }
}
