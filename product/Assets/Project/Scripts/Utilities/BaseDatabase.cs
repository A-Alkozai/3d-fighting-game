using System.Collections.Generic;
using System;

// Generic base class for JSON-loaded databases - stores items by their ID
// Subclasses set filePath and can override ReadJson for post-load processing
public class BaseDatabase<T> where T : IIdentifiable
{
    protected Dictionary<string, T> dict = new Dictionary<string, T>();
    protected string filePath;

    // Load JSON from filePath and populate the dictionary keyed by each item's Id
    public virtual void ReadJson()
    {
        JsonLoader.LoadJSON<T>(dict, filePath);
    }
}