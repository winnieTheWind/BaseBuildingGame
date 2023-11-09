using System.Collections.Generic;
using UnityEngine;

public static class PrototypeFactory
{
    [System.Serializable]
    private class PrototypeList<T> where T : IFurniturePrototype
    {
        public T[] furniture_prototypes; // This must match the JSON key exactly.
    }


    public static Dictionary<string, T> GetPrototypes<T>(string fileName) where T : IFurniturePrototype
    {
        var prototypes = new Dictionary<string, T>();

        TextAsset jsonTextAsset = Resources.Load<TextAsset>("JSON/" + fileName);

        if (jsonTextAsset == null)
        {
            Debug.LogError($"Cannot load prototypes JSON file from path: JSON/{fileName}");
            return prototypes;
        }

        PrototypeList<T> prototypeList = JsonUtility.FromJson<PrototypeList<T>>(jsonTextAsset.text);

        if (prototypeList == null || prototypeList.furniture_prototypes == null)
        {
            Debug.LogError($"Failed to parse the prototypes JSON from path: JSON/{fileName}");
            return prototypes;
        }

        foreach (T proto in prototypeList.furniture_prototypes)
        {
            if (proto == null)
            {
                Debug.LogError("A prototype in the array is null.");
                continue;
            }

            if (string.IsNullOrEmpty(proto.Type))
            {
                Debug.LogError("Prototype has a null or empty Type property.");
                continue;
            }

            //Debug.Log($"Prototype has: Type: {proto.Type} " + $"Width: {proto.Width} " + $"Height: {proto.Height} " + $"PCost: {proto.PathfindingCost}");
            //Debug.Log($"EncloseRooms: {proto.EnclosesRooms.ToString()} " + $"LTB: {proto.LinksToNeighbours.ToString()}");

            prototypes.Add(proto.Type, proto);
        }

        return prototypes;
    }
}

// Why could I just load the job_furniture_prototypes.JSON using this?