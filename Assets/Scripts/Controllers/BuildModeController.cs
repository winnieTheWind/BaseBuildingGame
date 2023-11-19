using UnityEngine;

public enum BuildMode
{
    CHARACTER,
    FLOOR,
    LAYERFLOOR,
    FURNITURE,
    DESCONSTRUCT
}

public class BuildModeController : MonoBehaviour
{
    public BuildMode buildMode = BuildMode.FLOOR;
    TileType buildModeTile = TileType.Grass;
    public string buildModeObjectType;

    public Material TransparenyMaterial;

    FurnitureMeshController fmc;

    public string TileName = "";

    private void Start()
    {
        fmc = GameObject.FindObjectOfType<FurnitureMeshController>();
    }

    public bool IsObjectDraggable()
    {
        if (buildMode == BuildMode.FLOOR || buildMode == BuildMode.DESCONSTRUCT ||
            buildMode == BuildMode.LAYERFLOOR)
        {
            // floors are draggable
            return true;
        }

        if (buildMode == BuildMode.CHARACTER)
        {
            return false;
        }

        Furniture proto = WorldController.Instance.world.furniturePrototypes[buildModeObjectType];
        return proto.Width == 1 && proto.Height == 1;
    }

    public void SetMode_BuildCharacter(string characterType)
    {
        buildMode = BuildMode.CHARACTER;

        buildModeObjectType = characterType;

        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }

    public void SetMode_BuildFloor(TileType type)
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = type;

        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }

    public void SetMode_BuildLayerFloor(TileType type)
    {

        buildMode = BuildMode.LAYERFLOOR;
        buildModeTile = type;

        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }

    public void SetMode_RemoveFloor()
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = TileType.Empty;
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();

    }

    public void SetMode_BuildFurniture(string objectType)
    {
        // Wall is not a tile, wall is an "InstalledObject" that exists on top of a tile.
        buildMode = BuildMode.FURNITURE;
        buildModeObjectType = objectType;

        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }


    public void SetMode_Deconstruct(string objectType)
    {
        // Wall is not a tile, wall is an "InstalledObject" that exists on top of a tile.
        buildMode = BuildMode.DESCONSTRUCT;
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }

    public void DoBuild(Tile t)
    {
        if (buildMode == BuildMode.FURNITURE)
        {
            if (WorldController.Instance.world.currentUserState == World.UserState.FREE_EDIT)
            {
                // Place furniture immediately without creating jobs
                WorldController.Instance.world.PlaceFurniture(buildModeObjectType, t);
                return; // Exit early to prevent further processing
            }
            // Create the Furniture and assign it to the tile
            // Can we build the furniture in the selected tile?
            // Run the ValidPlacement function!
            string furnitureType = buildModeObjectType;

            if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t) &&
                t.PendingFurnitureJob == null)
            {
                // This tile is valid for this furniture
                // Create a job for it to be built!
                //Job j = new Job(t, (theJob) => {

                Job j;

                if (WorldController.Instance.world.furnitureJobPrototypes.ContainsKey(furnitureType))
                {
                    // Make a clone
                    j = WorldController.Instance.world.furnitureJobPrototypes[furnitureType].Clone();
                    j.tile = t;
                    j.Is3d = true;
                } else
                {
                    //Debug.LogError("There is no furniture job prototype for " + furnitureType);
                    j = new Job(t, furnitureType, FurnitureActions.JobComplete_FurnitureBuilding, 0.1f, null, true, false);
                }

                j.furniturePrototype = WorldController.Instance.world.furniturePrototypes[furnitureType];

                // FIXME: I dont like having to manually and explicity set flags
                // that prevent conflicts, its too easy to forget to set/clear them!
                t.PendingFurnitureJob = j;
                j.RegisterJobStoppedCallback((theJob) => { theJob.tile.PendingFurnitureJob = null; });

                WorldController.Instance.world.jobQueue.Enqueue(j);
                //Debug.Log("Job Queue Size: " + WorldController.Instance.world.jobQueue.Count);
            }
        }
        else if (buildMode == BuildMode.FLOOR)
        {
            if (WorldController.Instance.world.currentUserState == World.UserState.FREE_EDIT)
            {
                if (t.Furniture != null)
                {
                    return;
                }

                if (t.LayerTile != null)
                {
                    t.LayerTile.Deconstruct();
                }

                t.Type = buildModeTile;
                return; // Exit early to prevent further processing
            }

            if (t.PendingTileJob == null)
            {
                // If the tile change is valid, proceed with creating and queueing the job.
                Job tileJob = new Job(
                    t,
                    buildModeTile, // Assuming you use the tile type as the jobObjectType.
                    cbTileJobCallback,
                    1f, // Assume it takes 1 time unit to perform this job, adjust as needed.
                    null,
                    false,
                    false
                );
                tileJob.isRenderingTile = true;

                t.PendingTileJob = tileJob;

                // Enqueue the job in your job system.
                WorldController.Instance.world.jobQueue.Enqueue(tileJob);
            }

        
        }
        else if (buildMode == BuildMode.DESCONSTRUCT)
        {
            // TODO:
            if (t.Furniture != null)
            {
                t.Furniture.Deconstruct();
            }
        }
        else if (buildMode == BuildMode.LAYERFLOOR)
        {
            if(WorldController.Instance.world.currentUserState == World.UserState.FREE_EDIT)
            {
                if (t.LayerTile == null)
                {
                    // Place furniture immediately without creating jobs
                    WorldController.Instance.world.PlaceLayerTile(buildModeTile.ToString(), t);
                    return; // Exit early to prevent further processing
                }
            }
        }
        else if (buildMode == BuildMode.CHARACTER)
        {
            if (t.Characters.Count == 0)
            {
                string type = buildModeObjectType;

                Character c = WorldController.Instance.world.characterPrototypes[type].Clone();
                c.Tile = t;

                WorldController.Instance.world.CreateCharacter(WorldController.Instance.world.GetTileAt(c.Tile.X, c.Tile.Z), type);
            } else
            {
                Debug.Log("Character already in place.");
            }
        }
        else
        {
            Debug.LogError("Unimplemented Build Mode!");
        }

    }

    void cbTileJobCallback(Job j)
    {
        j.tile.Type = j.tileType;
    }

}

