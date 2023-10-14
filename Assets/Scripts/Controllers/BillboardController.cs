using UnityEngine;

public class BillboardController : MonoBehaviour
{
    void LateUpdate()
    {
        Camera mainCamera = Camera.main;
        Vector3 forwardDirection = mainCamera.transform.forward;
        Vector3 upDirection = mainCamera.transform.up;

        // Keep the sprite facing the camera
        transform.forward = forwardDirection;
        transform.up = upDirection;
    }
}
