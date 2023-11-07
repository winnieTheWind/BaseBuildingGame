using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class JobMeshController : MonoBehaviour
{
    // This bare bones controller is mostly just going to piggyback
    // on FurnitureMeshController because we dont yet fully know
    // what out job system is going to look like in the end.

    FurnitureMeshController fmc;

    public Material TransparentWallMaterial;

    Dictionary<Job, GameObject> jobGameObjectMap;

    public Material TileMaterial;

    // Start is called before the first frame update
    void Start()
    {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        fmc = GameObject.FindObjectOfType<FurnitureMeshController>();

        // FIXME: No such thing as a job queue yet!
        WorldController.Instance.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnJobCreated(Job job)
    {
        //if (job.jobObjectType == null)
        //{
        //    // this job doesnt really have an associated sprite with it, so no need to render.
        //    return;
        //}
      

        if (jobGameObjectMap.ContainsKey(job))
        {
            Debug.LogError("OnJobCreated for a jobGO that already exists -- most likely a job being RE-QUEUED, as opposed to created.");
            return;
        }

        //if (job.jobObjectType == "Wall" || job.jobObjectType == "Door" || job.jobObjectType == "Oxygen_Generator")
        if (job.Is3d)
        {
            // FIXME: WE CAN only do furniture building job.
            // Create a Visual GameObject linked to this data.
            GameObject job_go = Instantiate(fmc.GetGameObjectForFurniture(job.jobObjectType), Vector3.zero, Quaternion.identity);

            // add our tile/GO pair to the dictionary
            jobGameObjectMap.Add(job, job_go);

            job_go.name = "JOB_" + job.jobObjectType + "_" + job.tile.X + "_" + job.tile.Z;
            job_go.transform.position = new Vector3(job.tile.X + ((job.furniturePrototype.Width - 1) / 2f), 0, job.tile.Z + ((job.furniturePrototype.Height - 1) / 2f));
            job_go.transform.SetParent(this.transform, true);

            // Get MeshRenderer component to the GameObject
            MeshRenderer meshRenderer = job_go.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = "Jobs";

            //This hardcoding is not ideal!
            if (job.jobObjectType == "Door")
            {
                Tile northTile = World.current.GetTileAt(job.tile.X, job.tile.Z + 1);
                Tile southTile = World.current.GetTileAt(job.tile.X, job.tile.Z - 1);

                if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
                    northTile.furniture.objectType == "Wall" && southTile.furniture.objectType == "Wall")
                {
                    job_go.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
            }

            // Optionally, you might want to set the material of the MeshRenderer
            meshRenderer.material = MaterialManager.Instance.GetMaterial(job.jobObjectType + "TransparentMaterial");
        }
        else
        {
            if (job.isRenderingTile == false)
            {
                return;
            }

            GameObject tile_go = new GameObject();

            // Add out tile / gameobject to dictionary
            //tileGameObjectMap.Add(tile_data, tile_go);

            //tile_go.name = "Tile_" + x + "_" + z;
            tile_go.transform.position = new Vector3(job.tile.X, 0, job.tile.Z);
            tile_go.transform.rotation = Quaternion.Euler(90, 0, 0);
            tile_go.transform.SetParent(this.transform, true);
            tile_go.transform.localScale = new Vector3(2, 2, 2);

            // Add a sprite renderer
            SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer>();
            sr.material = TileMaterial;
            sr.sprite = SpriteManager.current.GetSprite("Tiles", job.tileType.ToString());
            //sr.material.SetTexture("Empty", SpriteManager.current.GetSprite("Tiles", job.tileType.ToString()).texture);

            //sr.material.SetTexture("EmptySprite", SpriteManager.current.GetSprite("Tiles", Tile.).texture);
            sr.sortingLayerName = "Jobs";

            // FIXME: This hardcoding is not ideal!  <== Understatement
            //if (job.jobObjectType == "Door")
            //{
            //    // By default, the door graphic is meant for walls to the east & west
            //    // Check to see if we actually have a wall north/south, and if so
            //    // then rotate this GO by 90 degrees

            //    Tile northTile = job.tile.world.GetTileAt(job.tile.X, job.tile.Y + 1);
            //    Tile southTile = job.tile.world.GetTileAt(job.tile.X, job.tile.Y - 1);

            //    if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
            //        northTile.furniture.objectType == "Wall" && southTile.furniture.objectType == "Wall")
            //    {
            //        job_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            //    }
            //}
            jobGameObjectMap.Add(job, tile_go);

        }


        job.RegisterJobCompletedCallback(OnJobEnded);
        job.RegisterJobStoppedCallback(OnJobEnded);
    }

    void OnJobEnded(Job job)
    {
        // FIXME: WE CAN only do furniture building job.
        GameObject job_go = jobGameObjectMap[job];
        job.UnregisterJobCompletedCallback(OnJobEnded);
        job.UnregisterJobStoppedCallback(OnJobEnded);

        Destroy(job_go);
        
    }
}
