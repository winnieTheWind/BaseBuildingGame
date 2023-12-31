using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using static Unity.Burst.Intrinsics.X86;
using UnityEngine.TextCore.Text;

public class MouseController : MonoBehaviour
{
    public InputActionAsset cameraControls;

    private InputAction panAction;
    private InputAction rotateCaneraAction;

    public GameObject CameraPosition;
    public GameObject CameraRotation;

    private Vector2 pan;
    private Vector2 rotation;

    public float panSpeed = 20f;
    public GameObject Cursor;

    private Vector3 lastFramePosition;
    private Vector3 currFramePosition;
    private Vector3 dragStartPosition;

    List<GameObject> dragPreviewGameObjects;

    BuildModeController bmc;
    FurnitureMeshController fmc;
    CharacterSpriteController csc;

    bool isDragging = false;

    public Material TransparencyMaterial;

    enum MouseMode
    {
        SELECT,
        BUILD
    }

    MouseMode currentMode = MouseMode.SELECT;

    void Awake()
    {
        var cameraActionMap = cameraControls.FindActionMap("Camera");

        panAction = cameraActionMap.FindAction("Pan");
        rotateCaneraAction = cameraActionMap.FindAction("CameraRotate");

    }

    void OnEnable()
    {
        panAction.Enable();
        rotateCaneraAction.Enable();
    }

    void OnDisable()
    {
        panAction.Disable();
        rotateCaneraAction.Disable();
    }

    private void Start()
    {
        bmc = GameObject.FindAnyObjectByType<BuildModeController>();
        fmc = GameObject.FindAnyObjectByType<FurnitureMeshController>();
        csc = GameObject.FindAnyObjectByType<CharacterSpriteController>();

        dragPreviewGameObjects = new List<GameObject>();
    }

    public Vector3 GetMousePosition()
    {
        // Gets the mouse position in world space.
        return currFramePosition;
    }

    public Tile GetMouseOverTile() {
        return WorldController.Instance.world.GetTileAt(
            Mathf.FloorToInt(currFramePosition.x + 0.5f),
            Mathf.FloorToInt(currFramePosition.z + 0.5f)
        );
    }

    void Update()
    {
        if (WorldController.Instance.IsModal)
        {
            // Amodal dialog is open so dont process anything with the mouse.
            return;
        }

        currFramePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        currFramePosition.y = 0f; // Assuming ground is at y = 0

        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            if (currentMode == MouseMode.BUILD)
            {
                currentMode = MouseMode.SELECT;
            } else if (currentMode == MouseMode.SELECT)
            {
                Debug.Log("Show game menu?");
            }
        }

        //UpdateCursor();

        UpdateDragging();
        UpdateCameraMovement();
        UpdateSelection();


        lastFramePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());  // Store the current position for next frame
        lastFramePosition.y = 0f;
    }

    public class SelectionInfo
    {
        public Tile tile;
        public ISelectableInterface[] stuffInTile;
        public int subSelection = 0;
    }

    public SelectionInfo mySelection;

    void UpdateSelection()
    {
        // This handles us left-clicking on furniture or characters to set a selection.

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            mySelection = null;
        }

        if (currentMode != MouseMode.SELECT)
        {
            return;
        }

        // If we're over a UI element, then bail out from this.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            // We just release the mouse button, so that's our queue to update our selection.
            Tile tileUnderMouse = GetMouseOverTile();

            if (tileUnderMouse == null)
            {
                // No valid tile under mouse
                return;
            }


            if (mySelection == null || mySelection.tile != tileUnderMouse)
            {
                //Debug.Log("new tile");
                // We have just selected a brand new tile, reset the info.
                mySelection = new SelectionInfo();
                mySelection.tile = tileUnderMouse;
                RebuildSelectionStuffInTile();

                // Select the first non-null entry.
                for (int i = 0; i < mySelection.stuffInTile.Length; i++)
                {
                    if (mySelection.stuffInTile[i] != null)
                    {
                        mySelection.subSelection = i;
                        break;
                    }
                }
            }
            else
            {
                // This is the same tile we already have selected, so cycle the subSelection to the next non-null item.
                // Not that the tile sub selection can NEVER be null, so we know we'll always find something.

                // Rebuild the array of possible sub-selection in case characters moved in or out of the tile.
                RebuildSelectionStuffInTile();

                do
                {
                    mySelection.subSelection = (mySelection.subSelection + 1) % mySelection.stuffInTile.Length;
                } while (mySelection.stuffInTile[mySelection.subSelection] == null);
            }
            //Debug.Log(mySelection.subSelection);
        }
    }

    void RebuildSelectionStuffInTile()
    {

        // Make sure stuffInTile is big enough to handle all the characters, plus the 3 extra values
        mySelection.stuffInTile = new ISelectableInterface[mySelection.tile.Characters.Count + 3];

        // Copy the character references
        for (int i = 0; i < mySelection.tile.Characters.Count; i++)
        {
            mySelection.stuffInTile[i] = mySelection.tile.Characters[i];
        }

        // Now assign references to the other three sub-selections available
        mySelection.stuffInTile[mySelection.stuffInTile.Length - 3] = mySelection.tile.Furniture;
        mySelection.stuffInTile[mySelection.stuffInTile.Length - 2] = mySelection.tile.Inventory;
        mySelection.stuffInTile[mySelection.stuffInTile.Length - 1] = mySelection.tile;

    }

    void UpdateDragging()
    {
        // If we are above a UI element, bail out..
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Clean up old drag previews
        while (dragPreviewGameObjects.Count > 0)
        {
            GameObject go = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            SimplePool.Despawn(go);
        }

        Ray currRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(currRay, out RaycastHit currHit, Mathf.Infinity))
        {
            // Check the sorting layer of the hit object
            Renderer renderer = currHit.collider.gameObject.GetComponent<Renderer>();
            if (renderer != null && renderer.sortingLayerName == "InventoryUI")
            {
                // Object is on the InventoryUI sorting layer, ignore it
                return;
            }
            currFramePosition = new Vector3(currHit.point.x, currHit.point.y, currHit.point.z + 0.4f);
        }

        if (currentMode != MouseMode.BUILD)
        {
            return;
        }

        // Start Drag
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                // Check the sorting layer of the hit object
                Renderer renderer = hit.collider.gameObject.GetComponent<Renderer>();
                if (renderer != null && renderer.sortingLayerName == "InventoryUI")
                {
                    // Object is on the InventoryUI sorting layer, ignore it
                    return;
                }
                dragStartPosition = currFramePosition;
                isDragging = true;
            }
        }
        else if (isDragging == false)
        {
            dragStartPosition = currFramePosition;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // The right mouse button was released so we
            // are cancelling any dragging/build mode.
            isDragging = false;
        }

        if (bmc.IsObjectDraggable() == false)
        {
            dragStartPosition = currFramePosition;
        }

        int start_x = Mathf.FloorToInt(dragStartPosition.x + 0.5f);
        int end_x = Mathf.FloorToInt(currFramePosition.x + 0.5f);
        int start_z = Mathf.FloorToInt(dragStartPosition.z + 0.5f);
        int end_z = Mathf.FloorToInt(currFramePosition.z + 0.5f);
        // We may be dragging in the "wrong" direction, so flip things if needed.
        if (end_x < start_x)
        {
            int tmp = end_x;
            end_x = start_x;
            start_x = tmp;
        }
        if (end_z < start_z)
        {  // Changed from end_y to end_z
            int tmp = end_z;    // Changed from end_y to end_z
            end_z = start_z;    // Changed from end_y to end_z
            start_z = tmp;      // Changed from end_y to start_z
        }

        // Display a preview of the drag area
        for (int x = start_x; x <= end_x; x++)
        {
            for (int z = start_z; z <= end_z; z++)
            {  // Changed from y to z
                Tile t = WorldController.Instance.world.GetTileAt(x, z);  // Changed from y to z
                if (t != null)
                {
                    if (bmc.buildMode == BuildMode.FURNITURE)
                    {
                        ShowFurnitureObjectAtTile(bmc.buildModeObjectType, t);
                    }
                    else if (bmc.buildMode == BuildMode.CHARACTER)
                    {
                        ShowCharacterObjectAtTile(bmc.buildModeObjectType, t);
                    }
                    else
                    {
                        //Show the generic dragging visuals
                        // Display the building hint on top of this tile position
                        GameObject go = SimplePool.Spawn(Cursor, new Vector3(x, 0, z), Quaternion.Euler(90, 0, 0));  // Changed from y to z
                        go.transform.SetParent(this.transform, true);
                        dragPreviewGameObjects.Add(go);
                    }
                }
            }
        }

        // End Drag
        if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            // Loop through all the tiles
            for (int x = start_x; x <= end_x; x++)
            {
                for (int z = start_z; z <= end_z; z++)
                {  // Changed from y to z
                    Tile t = WorldController.Instance.world.GetTileAt(x, z);  // Changed from y to z

                    if (t != null)
                    {
                        bmc.DoBuild(t);
                    }
                }
            }
        }
    }


    void ShowCharacterObjectAtTile(string characterType, Tile t)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(this.transform, true);
        dragPreviewGameObjects.Add(go);
        go.name = "Character";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = csc.GetSpriteForCharacter(characterType + "Sprite_7");
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        sr.sortingLayerName = "Characters";

        Character proto = World.current.characterPrototypes[characterType];
        go.transform.rotation = Quaternion.Euler(0, 0, 0);

        go.transform.position = new Vector3(t.X + ((proto.Width) / 1f), 0.904f, t.Z + ((proto.Height) / 1f));
        go.transform.localScale = new Vector3(1, 1, 1);
        go.AddComponent<BillboardController>();

        //if (WorldController.Instance.world.IsCharacterPlacementValid(characterType, t))
        //{
        //    go.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        //}
        //else
        //{
        //    go.GetComponent<SpriteRenderer>().material.color = new Color(1f, 0.5f, 0.5f, 1f);
        //}

        go.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    void ShowLayerFloorAtTile(string layerFloorType, Tile t)
    {
        Debug.Log("ShowLayerFloorAtTile");
        GameObject go = new GameObject();
        go.transform.SetParent(this.transform, true);
        go.transform.rotation = Quaternion.Euler(90, 0, 0);
        dragPreviewGameObjects.Add(go);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = fmc.GetSpriteForFurniture(layerFloorType);
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        sr.sortingLayerName = "Jobs";

        LayerTile proto = World.current.layerTilePrototypes[layerFloorType];
        go.transform.position = new Vector3(t.X + ((proto.Width - 1) / 2f), 0, t.Z + ((proto.Height - 1) / 2f));

    }

    void ShowFurnitureObjectAtTile(string furnitureType, Tile t)
    {
        if (World.current.GetFurniturePrototype(furnitureType).Is3D)
        {
            GameObject go = Instantiate(fmc.GetGameObjectForFurniture(furnitureType), Vector3.zero, Quaternion.Euler(0, 0, 0));
            go.transform.SetParent(this.transform, true);
            dragPreviewGameObjects.Add(go);

            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = "Jobs";

            // Get MeshRenderer component to the GameObject
            // Optionally, you might want to set the material of the MeshRenderer
            meshRenderer.material = MaterialManager.Instance.GetMaterial(furnitureType + "TransparentMaterial");
            Furniture proto = World.current.furniturePrototypes[furnitureType];

            go.name = furnitureType.ToString() + "_" + t.X + "_" + t.Z;
            go.transform.position = new Vector3(t.X + ((proto.Width - 1) / 2f), 0, t.Z + ((proto.Height - 1) / 2f));

            if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t))
            {
                go.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                go.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0.5f, 0.5f);
            }

            //This hardcoding is not ideal!
            if (furnitureType == "Door")
            {
                Tile northTile = World.current.GetTileAt(t.X, t.Z + 1);
                Tile southTile = World.current.GetTileAt(t.X, t.Z - 1);

                if (northTile != null && southTile != null && northTile.Furniture != null && southTile.Furniture != null &&
                    northTile.Furniture.ObjectType == "Wall" && southTile.Furniture.ObjectType == "Wall")
                {
                    go.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
            }
        }
        else {
            GameObject go = new GameObject();
            go.transform.SetParent(this.transform, true);
            go.transform.rotation = Quaternion.Euler(90, 0, 0);
            dragPreviewGameObjects.Add(go);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = fmc.GetSpriteForFurniture(furnitureType);
            sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
            sr.sortingLayerName = "Jobs";

            Furniture proto = World.current.furniturePrototypes[furnitureType];
            go.transform.position = new Vector3(t.X + ((proto.Width - 1) / 2f), 0, t.Z + ((proto.Height - 1) / 2f));

            if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t))
            {
                go.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                go.GetComponent<SpriteRenderer>().material.color = new Color(1f, 0.5f, 0.5f, 0.5f);
            }
        }
    }

    public void StartBuildMode()
    {
        currentMode = MouseMode.BUILD;
    }

    void UpdateCameraMovement()
    {
        
        if (Keyboard.current.shiftKey.isPressed)
        {
            if (Mouse.current.rightButton.isPressed)
            {
                rotation = rotateCaneraAction.ReadValue<Vector2>();
                // Adjust these values as needed
                float rotationSpeedX = 0.4f;

                // Apply rotation
                CameraRotation.transform.Rotate(Vector3.up, -rotation.x * rotationSpeedX, Space.World);
            }
        } else
        {
            if (Mouse.current.rightButton.isPressed)
            {
                pan = panAction.ReadValue<Vector2>();
                // Create a panning vector in world space
                Vector3 panMovementWorld = new Vector3(-pan.x, 0, -pan.y);

                // Transform the panning vector to the camera's local space
                Vector3 panMovementLocal = CameraRotation.transform.TransformDirection(panMovementWorld);

                // Apply panning in local space
                CameraPosition.transform.localPosition += panMovementLocal * Time.deltaTime * panSpeed;
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                // Reset the rotation to Euler angles 50, 0, 0
                CameraRotation.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                // Reset the rotation to Euler angles 50, 0, 0
                CameraRotation.transform.rotation = Quaternion.Euler(0, 90, 0);
            }

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                // Reset the rotation to Euler angles 50, 0, 0
                CameraRotation.transform.rotation = Quaternion.Euler(0, 90 * 2, 0);
            }

            if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                // Reset the rotation to Euler angles 50, 0, 0
                CameraRotation.transform.rotation = Quaternion.Euler(0, 90 * 3, 0);
            }
        }

        // Update lastFramePosition here for more accurate movement
        lastFramePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        lastFramePosition.y = 0;  // Assuming ground is at y = 0
    }

    // oK I need to assign these camera rotations like this, but this isnt working.

}
