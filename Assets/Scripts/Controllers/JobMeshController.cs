using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class JobMeshController : MonoBehaviour
{
    // This bare bones controller is mostly just going to piggyback
    // on FurnitureMeshController because we dont yet fully know
    // what out job system is going to look like in the end.

    FurnitureMeshController fmc;

    public Material TransparentWallMaterial;

    Dictionary<Job, GameObject> jobGameObjectMap;

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
        if (job.jobObjectType == null)
        {
            // this job doesnt really have an associated sprite with it, so no need to render.
            return;
        }

        if (jobGameObjectMap.ContainsKey(job))
        {
            //Debug.LogError("OnJobCreated for a jobGO that already exists -- most likely a job being RE-QUEUED, as opposed to created.");
            return;
        }
        Furniture furn = World.current.furniturePrototypes[job.jobObjectType];

        //if (job.jobObjectType == "Wall" || job.jobObjectType == "Door" || job.jobObjectType == "Oxygen_Generator")
        if (furn.Is3D)
        {

            // FIXME: WE CAN only do furniture building job.
            // Create a Visual GameObject linked to this data.
            GameObject job_go = Instantiate(fmc.GetGameObjectForFurniture(job.jobObjectType), Vector3.zero, Quaternion.identity);

            // add our tile/GO pair to the dictionary
            jobGameObjectMap.Add(job, job_go);

            job_go.name = "JOB_" + job.jobObjectType + "_" + job.tile.X + "_" + job.tile.Z;
            job_go.transform.position = new Vector3(job.tile.X + ((job.furniturePrototype.Width) / 2f), 0, job.tile.Z + ((job.furniturePrototype.Height) / 2f));
            job_go.transform.SetParent(this.transform, true);

            // Get MeshRenderer component to the GameObject
            MeshRenderer meshRenderer = job_go.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = "Jobs";

            //This hardcoding is not ideal!
            if (job.jobObjectType == "Door")
            {
                job_go.transform.position = new Vector3(job.tile.X + ((job.furniturePrototype.Width - 1) / 2f), 0, job.tile.Z + ((job.furniturePrototype.Height - 1) / 2f));

                // By default, the door mesh is meant for walls to the east and west
                // check to see if we actually have a wall north/south, and if so
                // then rotate this GO by 90 degrees

                Tile northTile = World.current.GetTileAt(job.tile.X, job.tile.Z + 1);
                Tile southTile = World.current.GetTileAt(job.tile.X, job.tile.Z - 1);

                if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
                    northTile.furniture.objectType.Contains("Wall") && southTile.furniture.objectType.Contains("Wall"))
                {
                    job_go.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
            }

            // Optionally, you might want to set the material of the MeshRenderer
            meshRenderer.material = TransparentWallMaterial;
        }
        else
        {
            GameObject job_go = new GameObject();

            // Add our tile/GO pair to the dictionary.
            jobGameObjectMap.Add(job, job_go);

            job_go.name = "JOB_" + job.jobObjectType + "_" + job.tile.X + "_" + job.tile.Z;
            //go.transform.position = new Vector3(t.X + ((proto.Width) / 2f), 0, t.Z + ((proto.Height) / 2f));

            job_go.transform.position = new Vector3(job.tile.X + ((job.furniturePrototype.Width - 1) / 2f), 0, job.tile.Z + ((job.furniturePrototype.Height - 1) / 2f));
            job_go.transform.SetParent(this.transform, true);
            job_go.transform.rotation = Quaternion.Euler(90, 0, 0);
        
            SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
            sr.sprite = fmc.GetSpriteForFurniture(job.jobObjectType);
            sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
            sr.sortingLayerName = "Jobs";
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
