using UnityEngine;

public class SoundController : MonoBehaviour
{
    float soundCoolDown = 0;
    // Start is called before the first frame update
    void Start()
    {
        WorldController.Instance.world.RegisterFurnitureCreated( OnFurnitureCreated );
        WorldController.Instance.world.RegisterTileChanged(OnTileChanged);
        WorldController.Instance.world.RegisterLayerTileCreated(OnLayerTileCreated);
        WorldController.Instance.world.RegisterLayerTileRemoved(OnLayerTileRemoved);
    }

    // Update is called once per frame
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

        AudioClip ac = Resources.Load<AudioClip>("Sounds/" + furn.objectType + "_OnCreated");

        if (ac == null)
        {
            ac = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
        }

        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCoolDown = 0.1f;
    }
}
