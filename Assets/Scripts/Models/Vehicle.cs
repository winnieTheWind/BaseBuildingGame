using UnityEngine;

public class Vehicle
{
    private Vector3 targetPosition; // The destination waypoint.
    private float speed; // Speed at which the vehicle moves.

    public Vector3 CurrentPosition { get; private set; } // Current position of the vehicle.

    public GameObject GameObjectReference { get; }

    public Vehicle(Vector3 startPosition, Vector3 targetPosition, float speed, GameObject gameObjectReference)
    {
        this.CurrentPosition = startPosition;
        this.targetPosition = targetPosition;
        this.speed = speed;

        // Store the reference to the associated game object.
        GameObjectReference = gameObjectReference;
        GameObjectReference.transform.position = startPosition;
    }

    // Indicates whether the vehicle has reached its destination.
    public bool HasArrived { get; private set; } = false;

    public void MoveTowardsWaypoint()
    {
        // Calculate the step size for one frame.
        float step = speed * Time.deltaTime;

        // Move our position a step closer to the target.
        // Note: You're moving the GameObject here, not just changing a value in your class.
        GameObjectReference.transform.position = Vector3.MoveTowards(GameObjectReference.transform.position, targetPosition, step);

        // Check if the vehicle has arrived at the target position.
        if (GameObjectReference.transform.position == targetPosition)
        {
            HasArrived = true;
        }
    }


    // ... other methods and logic ...
}
