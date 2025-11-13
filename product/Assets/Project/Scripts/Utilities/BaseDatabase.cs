using System.Collections.Generic;
using System;

public class BaseDatabase<T> where T : IIdentifiable
{

    protected Dictionary<string, T> dict = new Dictionary<string, T>();
    protected string filePath;

    public virtual void ReadJson()
    {
        JsonLoader.LoadJSON<T>(dict, filePath);
    }
}