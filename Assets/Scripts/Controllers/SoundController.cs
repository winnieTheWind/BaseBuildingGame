using UnityEngine;

public class SoundController : MonoBehaviour
{
    float soundCoolDown = 0;
    // Start is called before the first frame update
    void Start()
    {
        World.current.cbFurnitureCreated += OnFurnitureCreated;
        World.current.cbTileChanged += OnTileChanged;
        World.current.cbLayerTileCreated += OnLayerTileCreated;
        World.current.cbLayerTileRemoved += OnLayerTileRemoved;
    }

    void Update()
    {
        soundCoolDown -= Time.deltaTime;
    }

    void OnLayerTileCreated(LayerTile tile_data)
    {
        //FIXME
        if (soundCoolDown > 0)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCoolDown = 0.1f;
    }

    void OnLayerTileRemoved(LayerTile tile_data)
    {
        //FIXME
        if (soundCoolDown > 0)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCoolDown = 0.1f;
    }

    void OnTileChanged(Tile tile_data)
    {
        //FIXME
        if (soundCoolDown > 0)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCoolDown = 0.1f;
    }

    void OnFurnitureCreated(Furniture furn)
    {
        //FIXME
        if (soundCoolDown > 0)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>("Sounds/" + furn.ObjectType + "_OnCreated");

        if (ac == null)
        {
            ac = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
        }

        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCoolDown = 0.1f;
    }
}
