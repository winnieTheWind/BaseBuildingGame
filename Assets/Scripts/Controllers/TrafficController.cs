using System.Collections.Generic;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    private List<Vector3> waypoints; // The list of waypoints that vehicles will move towards.
    private List<Vehicle> vehicles; // The list of active vehicles.
    private List<GameObject> vehicleObjects;
    private Traffic traffic; // Reference to our traffic logic class.
    bool hasSpawned = false;
    public LayerMask RenderInFrontLayer;

    void Start()
    {
        traffic = new Traffic();
        vehicles = new List<Vehicle>();
        InitializeWaypoints(); // Initialize your waypoints here.
    }

    private void Update()
    {
        // Handle vehicle spawning.
        if (!hasSpawned)
        {
            SpawnVehicle();
            hasSpawned = true;
        }

        // Here we will handle the movement of each vehicle every frame.
        for (int i = 0; i < vehicles.Count; i++)
        {
            Vehicle vehicle = vehicles[i];

            // We move the vehicle towards its waypoint if it hasn't arrived yet.
            if (!vehicle.HasArrived)
            {
                vehicle.MoveTowardsWaypoint();
            }
            else
            {
                // Optional: If the vehicle has arrived, you might want to do something, like removing it.
                // Remember to handle the list change appropriately if you do this.
            }
        }
    }

    void InitializeWaypoints()
    {
        // Here, you should set up your waypoints.
        // This is just a placeholder; your actual waypoints will depend on your game's layout.
        waypoints = new List<Vector3>
        {
            new Vector3(13.2f, 0, -8.78f),
            new Vector3(13.2f, 0, 33.2f), // etc.
        };
    }

    void SpawnVehicle()
    {
        // Determine the starting waypoint and target waypoint for the new vehicle.
        Vector3 start = waypoints[0]; // For simplicity, start at the first waypoint.
        Vector3 end = waypoints[1]; // And target the second one.

        //GameObject vehicleObject = Instantiate(vehiclePrefab, start, Quaternion.identity);

        GameObject vehicleObject = new GameObject();
        // You set its layer like this:
        vehicleObject.layer = LayerMask.NameToLayer("RenderInFront");
        SpriteRenderer sr = vehicleObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.current.GetSprite("Vehicles", "truckSprite");
        vehicleObject.AddComponent<BillboardController>();
        vehicleObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        vehicleObject.transform.localScale = new Vector3(2, 2, 2);



        // Create the new Vehicle instance, now passing the game object reference as well.
        Vehicle newVehicle = new Vehicle(start, end, 6, vehicleObject);

        // Add the new vehicle to our active list.
        vehicles.Add(newVehicle);
        Debug.Log("Vehicle spawned. Total vehicles: " + vehicles.Count);

        // Inform the Traffic logic that a new vehicle has been created so it can update its count.
        //traffic.RegisterVehicle();
    }
}
