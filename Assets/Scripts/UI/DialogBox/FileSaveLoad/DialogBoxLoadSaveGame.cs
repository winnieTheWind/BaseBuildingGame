using System.IO;
using TMPro;
using UnityEngine;
using System.Linq;

// Object -> MonoBehaviour -> DialogBox -> DialogBoxLoadSaveGame -> 
//                                                          DialogBoxSaveGame
//                                                          DialogBoxLoadGame
                                                                
public class DialogBoxLoadSaveGame : DialogBox
{
    public GameObject fileListItemPrefab;
    public Transform fileList;

    public override void CloseDialog()
    {
        ClearChildren();
        base.CloseDialog();
    }

    public void ClearChildren()
    {
        while (fileList.childCount > 0)
        {
            Transform c = fileList.GetChild(0);
            c.SetParent(null);
            Destroy(c.gameObject);
        }
    }

    public override void ShowDialog()
    {
        ClearChildren();

        base.ShowDialog();

        // Get list of files in save location
        string directoryPath = WorldController.Instance.FileSaveBasePath();

        DirectoryInfo saveDir = new DirectoryInfo(directoryPath);

        FileInfo[] saveGames = saveDir.GetFiles().OrderByDescending(f => f.CreationTime).ToArray();

        // TODO: Make sure the saves are sorted date/time with the newest
        // saves being up at the top.

        TMP_InputField inputField = gameObject.GetComponentInChildren<TMP_InputField>();

        //Build file list by instantiating prefab
        foreach (FileInfo file in saveGames)
        {

            GameObject go = (GameObject)GameObject.Instantiate(fileListItemPrefab);
            go.transform.SetParent(fileList);

            go.GetComponentInChildren<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(file.FullName);
            go.GetComponent<DialogListItem>().inputField = inputField;
        }
    }

}
