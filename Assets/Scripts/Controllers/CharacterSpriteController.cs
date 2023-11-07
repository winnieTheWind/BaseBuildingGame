using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterSpriteController : MonoBehaviour
{
    Dictionary<Character, GameObject> characterGameObjectMap;

    Dictionary<string, Sprite> characterSprites;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    public Material CharacterMaterial;

    // Start is called before the first frame update
    void Start()
    {
        LoadSprites();

        characterGameObjectMap = new Dictionary<Character, GameObject>();

        world.RegisterCharacterCreated(OnCharacterCreated);

        // Check for pre-existing characters, which won't do the callback.
        foreach (Character c in world.characters)
        {
            OnCharacterCreated(c);
        }
    }

    void LoadSprites()
    {
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] charSprites = Resources.LoadAll<Sprite>("Sprites/Characters");
        foreach (var item in charSprites)
        {
            //Debug.Log(item);
            characterSprites[item.name] = item;
        }
    }

    public void OnCharacterCreated(Character character)
    {
        if (character == null)
        {
            Debug.LogError("Null InstalledObject passed to OnInstalledObjectCreated");
            return;
        }

        // Create a Visual GameObject linked to this data.
        GameObject char_go = new GameObject();

        // add our tile/GO pair to the dictionary
        characterGameObjectMap.Add(character, char_go);

        char_go.name = "Character";
        char_go.transform.position = new Vector3(character.CurrTile.X, 0.904f, character.CurrTile.Z);
        char_go.transform.rotation = Quaternion.Euler(0, 0, 0);
        char_go.transform.localScale = new Vector3(1, 1, 1);
        char_go.transform.SetParent(this.transform, true);
        
        SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
        sr.sprite = characterSprites["peoplesprite1_7"];
        sr.sortingLayerName = "Characters";

        sr.material = CharacterMaterial;

        char_go.layer = 7;

        // Ensure the character is properly scaled and oriented
        char_go.transform.localScale = Vector3.one; // Set scale to (1, 1, 1)
        char_go.transform.rotation = Quaternion.identity; // Reset rotation

        character.Type = CharacterType.ConstructionWorker;

        // Add BillboardController component
        BillboardController billboardController = char_go.AddComponent<BillboardController>();

        character.RegisterOnChangedCallback(OnCharacterChanged);
    }

    void OnCharacterChanged(Character c)
    {
        //Debug.Log("OnCharacterChanged called for character at position: " + c.X + ", " + c.Z);
        if (characterGameObjectMap.ContainsKey(c) == false)
        {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map!");
        }

        GameObject char_go = characterGameObjectMap[c];

        // Ensure the billboard effect
        char_go.transform.LookAt(Camera.main.transform.position);
        char_go.transform.Rotate(0, 180, 0);
        char_go.layer = 7;

        char_go.transform.localScale = new Vector3(1, 1, 1);
        char_go.transform.position = new Vector3(c.X, 0.904f, c.Z);
        c.Type = CharacterType.ConstructionWorker;

    }
}
