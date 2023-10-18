using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; protected set; }

    public World world { get; protected set; }

    static string loadWorldFromFile = null;

    private bool _isPaused = false;
    public bool IsPaused
    {
        get
        {
            return _isPaused || IsModal;
        }
        set
        {
            _isPaused = value;
        }

    }

    public bool IsModal; // if true, a modal dialog box is open so normal inputs should be ignored.

    void OnEnable()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be two world controllers.");
        }
        Instance = this;

        if (loadWorldFromFile != null)
        {
            CreateWorldFromSaveFile();
            loadWorldFromFile = null;
        }
        else
        {
            CreateEmptyWorld();
        }
    }

    void Update()
    {
        if (IsPaused == false)
        {
            // TODO: Add pause/unpause, speed controls, etc
            world.Update(Time.deltaTime);
        }
      
    }

    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x);
        int z = Mathf.FloorToInt(coord.z);
        return world.GetTileAt(x, z);

    }

    public void NewWorld()
    {
        Debug.Log("NewWorld button was clicked.");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);


    }

    public string FileSaveBasePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "Saves");
    }


    public void LoadWorld(string fileName)
    {
        //Debug.Log("LoadWorld button was clicked.");

        // Reload the scene to reset all data (and purge old references)
        loadWorldFromFile = fileName;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    void CreateEmptyWorld()
    {
        // Create a world with Empty tiles
        world = new World(20, 20);

    }

    void CreateWorldFromSaveFile()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(World));

        // This can throw an exception.
        // TODO: Show a error message to the user.
        string saveGameText = File.ReadAllText(loadWorldFromFile);

        TextReader reader = new StringReader(saveGameText);

        world = (World)serializer.Deserialize(reader);
        reader.Close();
    }
}

