using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxLoadGame : DialogBoxLoadSaveGame
{
    public void OkayWasClicked()
    {
        // check to see if the file already exists
        // if so, ask for overwrite confirmation
        string fileName = gameObject.GetComponentInChildren<TMP_InputField>().text;

        // Right now fileName is just what was in the dialog box.
        // We need to pad this out to the full path plus an extension.
        // In the end were looking for something thats going
        // to be similar to this (depending on OS)

        string filePath = System.IO.Path.Combine(WorldController.Instance.FileSaveBasePath(), fileName + ".sav");

        if (File.Exists(filePath) == false)
        {
            // TODO: Do file overwrite dialog box.

            Debug.LogError("File doesn't exist.  What?");
            CloseDialog();
            return;
        }

        CloseDialog();

        LoadWorld(filePath);
    }

    public void LoadWorld(string filepath)
    {
        // This function gets called when the user confirms a filename
        // from the save dialog box.
        WorldController.Instance.LoadWorld(filepath);
    }
}
