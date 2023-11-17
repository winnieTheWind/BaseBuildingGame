using System.Collections.Generic;
using UnityEngine;

public static class PrototypeFactory
{
    [System.Serializable]
    private class PrototypeList<T> where T : IPrototype
    {
        public T[] prototypes; // This must match the JSON key exactly.
    }
    public static Dictionary<string, T> GetPrototypes<T>(string fileName, string key) where T : IPrototype
    {
        var prototypes = new Dictionary<string, T>();

        TextAsset jsonTextAsset = Resources.Load<TextAsset>("JSON/" + fileName);

        if (jsonTextAsset == null)
        {
            Debug.LogError($"Cannot load prototypes JSON file from path: JSON/{fileName}");
            return prototypes;
        }

        // Replace the JSON key with a generic key to match the dynamic nature
        string jsonToParse = jsonTextAsset.text.Replace(fileName, key);

        PrototypeList<T> prototypeList = JsonUtility.FromJson<PrototypeList<T>>(jsonToParse);

        if (prototypeList == null || prototypeList.prototypes == null) // Use the generic field name here
        {
            Debug.LogError($"Failed to parse the prototypes JSON from path: JSON/{fileName}");
            return prototypes;
        }

        foreach (T proto in prototypeList.prototypes) // Use the generic field name here
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

            prototypes.Add(proto.Type, proto);
        }

        return prototypes;
    }


}

