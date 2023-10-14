using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

public enum BuildMode
{
    SPAWNCHARACTER,
    FLOOR,
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

    private void Start()
    {
        fmc = GameObject.FindObjectOfType<FurnitureMeshController>();
    }

    public bool IsObjectDraggable()
    {
        if (buildMode == BuildMode.FLOOR || buildMode == BuildMode.DESCONSTRUCT)
        {
            // floors are draggable
            return true;
        }

        if (buildMode == BuildMode.SPAWNCHARACTER)
        {
            return false;
        }

        Furniture proto = WorldController.Instance.world.furniturePrototypes[buildModeObjectType];
        return proto.Width == 1 && proto.Height == 1;
    }

    public void DoPathfindingTest()
    {
        WorldController.Instance.world.SetupPathfindingExample();

    }

    public void SetMode_BuildFloor()
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = TileType.Grass;


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

    public void SetMode_SpawnCharacter(string objectType)
    {
        buildMode = BuildMode.SPAWNCHARACTER;
        buildModeObjectType = objectType;

        //Debug.Log("SetMode_SpawnCharacter -- " + buildModeObjectType);
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
            // FIXME: This instantly builds the furniture
            //WorldController.Instance.world.PlaceFurniture();

            // Can we build the furniture in the selected tile?
            // Run the ValidPlacement function!
            string furnitureType = buildModeObjectType;

            if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t) &&
                t.pendingFurnitureJob == null)
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
                } else
                {
                    //Debug.LogError("There is no furniture job prototype for " + furnitureType);
                    j = new Job(t, furnitureType, FurnitureActions.JobComplete_FurnitureBuilding, 0.1f, null);
                }

                j.furniturePrototype = WorldController.Instance.world.furniturePrototypes[furnitureType];

                // FIXME: I dont like having to manually and explicity set flags
                // that prevent conflicts, its too easy to forget to set/clear them!
                t.pendingFurnitureJob = j;
                j.RegisterJobStoppedCallback((theJob) => { theJob.tile.pendingFurnitureJob = null; });

                WorldController.Instance.world.jobQueue.Enqueue(j);
            }
        }
        else if (buildMode == BuildMode.FLOOR)
        {
            // We are in tile-changing mode
            t.Type = buildModeTile;
        } else if (buildMode == BuildMode.DESCONSTRUCT)
        {
            // TODO:
            if (t.furniture != null)
            {
                t.furniture.Deconstruct();
            }
        }
        else if (buildMode == BuildMode.SPAWNCHARACTER)
        {
            string charType = buildModeObjectType;

            Character c = WorldController.Instance.world.characterPrototypes[charType].Clone();
            c.tile = t;

            WorldController.Instance.world.CreateCharacter(WorldController.Instance.world.GetTileAt(c.tile.X, c.tile.Z));
        }
        else
        {
            Debug.LogError("Unimplemented Build Mode!");
        }

    }
}

// Okay only one mesh is spawning, and is moving with the mouse.
// However, you can also choose which type of mesh you wish to spawn.
// When I press the Build Oxygen Generator button, after pressing
// the Build Wall button, I need it to destroy the current preview
// object and instantiate the new preview mesh when switch
// to a different selection of mesh.