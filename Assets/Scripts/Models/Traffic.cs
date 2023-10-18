using System;

public class Traffic
{
    private int vehicleCount; // The current number of active vehicles.

    public Traffic()
    {
        vehicleCount = 0;
    }

    public bool ShouldSpawnVehicle()
    {
        // Logic to decide whether a new vehicle should be spawned.
        // This could be based on various conditions - for simplicity, we're using vehicle count here.
        return vehicleCount < 10; // for example, a maximum of 10 vehicles
    }

    public void RegisterVehicle()
    {
        // Increment the count of active vehicles.
        vehicleCount++;
    }

    public bool ShouldDespawnVehicle()
    {
        // Logic for determining if a vehicle should be despawned.
        // This can be as simple or complex as needed for your game's logic.
        return vehicleCount > 0; // if there's at least one vehicle
    }

    public void DeregisterVehicle()
    {
        // Decrement the count of active vehicles.
        if (vehicleCount > 0)
        {
            vehicleCount--;
        }
    }
}
