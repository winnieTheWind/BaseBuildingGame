using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class SpriteManager : MonoBehaviour
{

    // Sprite Manager isn't responsible for actually creating GameObjects.
    // That is going to be the job of the individual ________SpriteController scripts.
    // Our job is simply to load all sprites from disk and keep the organized.

    static public SpriteManager current;

    Dictionary<string, Sprite> sprites;

    // Use this for initialization
    void OnEnable()
    {
        current = this;

        LoadSprites();
    }

    void LoadSprites()
    {
        sprites = new Dictionary<string, Sprite>();

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Images");
        //filePath = System.IO.Path.Combine( Application.streamingAssetsPath, "CursorCircle.png" );

        //LoadSprite("CursorCircle", filePath);

        LoadSpritesFromDirectory(filePath);

    }

    void LoadSpritesFromDirectory(string filePath)
    {
        // First, we're going to see if we have any more sub-directories,
        // if so -- call LoadSpritesFromDirectory on that.

        string[] subDirs = Directory.GetDirectories(filePath);
        foreach (string sd in subDirs)
        {
            LoadSpritesFromDirectory(sd);
        }

        string[] filesInDir = Directory.GetFiles(filePath);
        foreach (string fn in filesInDir)
        {
            // Is this an image file?
            // Unity's LoadImage seems to support only png and jpg
            // NOTE: We **could** try to check file extensions, but why not just
            // have Unity **attemp** to load the image, and if it doesn't work,
            // then I guess it wasn't an image! An advantage of this, is that we
            // don't have to worry about oddball filenames, nor do we have to worry
            // about what happens if Unity adds support for more image format
            // or drops support for existing ones.

            string spriteCategory = new DirectoryInfo(filePath).Name;

            LoadImage(spriteCategory, fn);
        }

    }

    void LoadImage(string spriteCategory, string filePath)
    {
        //Debug.Log("LoadImage: " + filePath);

        // TODO:  LoadImage is returning TRUE for things like .meta and .xml files.  What??!
        //		So as a temporary fix, let's just bail if we have something we KNOW should not
        //  	be an image.
        if (filePath.Contains(".xml") || filePath.Contains(".meta"))
        {
            return;
        }

        // Load the file into a texture
        byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);

        Texture2D imageTexture = new Texture2D(1, 1);   // Create some kind of dummy instance of Texture2D
                                                        // LoadImage will correctly resize the texture based on the image file
        imageTexture.filterMode = FilterMode.Point;  // Set the filter mode

        if (imageTexture.LoadImage(imageBytes))
        {

            // Image was successfully loaded.
            // So let's see if there's a matching XML file for this image.
            string baseSpriteName = Path.GetFileNameWithoutExtension(filePath);
            string basePath = Path.GetDirectoryName(filePath);

            // NOTE: The extension must be in lower case!
            string xmlPath = System.IO.Path.Combine(basePath, baseSpriteName + ".xml");

            if (System.IO.File.Exists(xmlPath))
            {
                string xmlText = System.IO.File.ReadAllText(xmlPath);
                // TODO: Loop through the xml file finding all the <sprite> tags
                // and calling LoadSprite once for each of them.

                XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));

                // Set our cursor on the first Sprite we find.
                if (reader.ReadToDescendant("Sprites") && reader.ReadToDescendant("Sprite"))
                {
                    do
                    {
                        ReadSpriteFromXml(spriteCategory, reader, imageTexture);
                    } while (reader.ReadToNextSibling("Sprite"));
                }
                else
                {
                    Debug.LogError("Could not find a <Sprites> tag.");
                    return;
                }

            }
            else
            {
                // File couldn't be read, probably because it doesn't exist
                // so we'll just assume the whole image is one sprite with pixelPerUnit = 32
                LoadSprite(spriteCategory, baseSpriteName, imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), 32);

            }

            // Attempt to load/parse the XML file to get information on the sprite(s)

        }

        // else, the file wasn't actually a image file, so just move on.

    }

    void ReadSpriteFromXml(string spriteCategory, XmlReader reader, Texture2D imageTexture)
    {
        //Debug.Log("ReadSpriteFromXml");
        string name = reader.GetAttribute("name");
        int x = int.Parse(reader.GetAttribute("x"));
        int y = int.Parse(reader.GetAttribute("y"));
        int w = int.Parse(reader.GetAttribute("w"));
        int h = int.Parse(reader.GetAttribute("h"));
        int pixelPerUnit = int.Parse(reader.GetAttribute("pixelPerUnit"));

        LoadSprite(spriteCategory, name, imageTexture, new Rect(x, y, w, h), pixelPerUnit);
    }

    void LoadSprite(string spriteCategory, string spriteName, Texture2D imageTexture, Rect spriteCoordinates, int pixelsPerUnit)
    {
        spriteName = spriteCategory + "/" + spriteName;
        //Debug.Log("LoadSprite: " + spriteName);
        Vector2 pivotPoint = new Vector2(0.5f, 0.5f);   // Ranges from 0..1 -- so 0.5f == center

        Sprite s = Sprite.Create(imageTexture, spriteCoordinates, pivotPoint, pixelsPerUnit);

        sprites[spriteName] = s;
    }

    public Sprite GetSprite(string categoryName, string spriteName)
    {
        //Debug.Log(spriteName);


        spriteName = categoryName + "/" + spriteName;

        if (sprites.ContainsKey(spriteName) == false)
        {
            //Debug.LogError("No sprite with name: " + spriteName);
            return null;    // TODO: What if we return a "dummy" sprite, like a purple square?
        }

        return sprites[spriteName];
    }
}

// Ok this class SpriteManager handles game assets that are in the StreamingAssets folder.
// I need to rewrite this code so that it looks in the Resources folder instead.
