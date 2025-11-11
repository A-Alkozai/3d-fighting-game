using System.Collections.Generic;
using System;

public class BaseDatabase<T>
{

    protected List<T> list = new List<T>();
    protected string filePath;

    public void ReadJson()
    {
        JsonLoader.LoadJSON<T>(list, filePath);
    }

    public void Add(T item)
    {
        list.Add(item);
    }

    public void Remove(T item)
    {
        list.Remove(item);
    }

    public List<T> GetList()
    { return list; }
}