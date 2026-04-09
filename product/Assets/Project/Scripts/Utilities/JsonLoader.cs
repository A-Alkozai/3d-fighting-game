using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Utility class that reads a JSON file and populates a dictionary with deserialized items
public static class JsonLoader
{
    // Read JSON from disk, deserialize the "items" array, and add each item to the dictionary by ID
    public static void LoadJSON<T>(Dictionary<string, T> dict, string filePath) where T : IIdentifiable
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON file not found: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);

        if (wrapper == null || wrapper.items == null)
        {
            Debug.LogError($"Failed to parse JSON from {filePath}");
            return;
        }
        else if (wrapper.items.Count == 0)
        {
            Debug.LogError($"Item count is 0 in JSON {filePath}");
        }

        foreach (T item in wrapper.items)
        {
            dict.Add(item.Id, item);
        }
    }

    // Wrapper class matching the JSON structure: { "items": [ ... ] }
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }
}