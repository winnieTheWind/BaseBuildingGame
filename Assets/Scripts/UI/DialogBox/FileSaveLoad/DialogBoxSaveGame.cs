using System.IO;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;


public class DialogBoxSaveGame : DialogBoxLoadSaveGame
{


    public void OkayWasClicked()
    {
        // TODO:
        // check to see if the file already exists
        // if so, ask for overwrite confirmation.

        //GameObject dialogBgObj = gameObject.transform.Find("Dialog Background").gameObject;
        //GameObject fileNameObj = dialogBgObj.transform.Find("File Name").gameObject;
        //GameObject textAreaObj = fileNameObj.transform.Find("Text Area").gameObject;
        //GameObject textObj = textAreaObj.transform.Find("Text").gameObject;
        //string fileName = textObj.GetComponent<TextMeshProUGUI>().text;

        string fileName = gameObject.GetComponentInChildren<TMP_InputField>().text;

        // TODO: Is the filename valid?  I.E. we may want to ban path-delimiters (/ \ or :) and 
        // maybe periods?      ../../some_important_file

        // Right now fileName is just what was in the dialog box.  We need to pad this out to the full
        // path, plus an extension!
        // In the end, we're looking for something that's going to be similar to this (depending on OS)
        //    C:\Users\Quill18\ApplicationData\MyCompanyName\MyGameName\Saves\SaveGameName123.sav

        // Application.persistentDataPath == C:\Users\<username>\ApplicationData\MyCompanyName\MyGameName\

        string filePath = System.IO.Path.Combine(WorldController.Instance.FileSaveBasePath(), fileName + ".sav");

        // At this point, filePath should look very much like
        //     C:\Users\Quill18\ApplicationData\MyCompanyName\MyGameName\Saves\SaveGameName123.sav

        if (File.Exists(filePath) == true)
        {
            // TODO: Do file overwrite dialog box.
            //Debug.LogError("File already exists -- overwriting the file for now.");

        }

        CloseDialog();

        SaveWorld(filePath);
    }

    public void SaveWorld(string filePath)
    {
        // This function gets called when the user confirms a filename
        // from the save dialog box.

        // Get the file name from the save file dialog box

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, WorldController.Instance.world);
        writer.Close();

        // Create/overwrite the save file with the xml text.

        // Make sure the save folder exists.
        if (Directory.Exists(WorldController.Instance.FileSaveBasePath()) == false)
        {
            // NOTE: This can throw an exception if we can't create the folder,
            // but why would this ever happen? We should, by definition, have the ability
            // to write to our persistent data folder unless something is REALLY broken
            // with the computer/device we're running on.
            Directory.CreateDirectory(WorldController.Instance.FileSaveBasePath());
        }

        File.WriteAllText(filePath, writer.ToString());
    }
}
