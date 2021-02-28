using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveUtils
{
    // The directory under Resources that the dynamic objects' prefabs can be loaded from
    private static string PREFABS_PATH = "Prefabs/";
    // A dictionary of prefab guid to prefab
    public static Dictionary<string, GameObject> prefabs = LoadPrefabs(PREFABS_PATH);

    public static string SAVE_OBJECTS_PATH = Application.dataPath + "/objects.json";
    public static string SAVE_SARA_PATH = Application.dataPath + "/sara.json";

    private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };

    private static Dictionary<string, GameObject> LoadPrefabs(string prefabsPath)
    {
        Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

        GameObject[] allPrefabs = Resources.LoadAll<GameObject>(prefabsPath);
        foreach (GameObject prefab in allPrefabs)
        {
            DynamicObject dynamicObject = prefab.GetComponent<DynamicObject>();
            if (dynamicObject == null)
            {
                throw new InvalidOperationException("Prefab does not contain DynamicObject");
            }
            if (!dynamicObject.objectState.isPrefab)
            {
                throw new InvalidOperationException("Prefab's ObjectState isPrefab = false");
            }
            prefabs.Add(dynamicObject.objectState.prefabGuid, prefab);
        }

        Debug.Log("Loaded " + prefabs.Count + " saveable prefabs.");
        return prefabs;
    }

    public static void DoSave()
    {
        SaveSara(SAVE_SARA_PATH);
        SaveDynamicObjects(SAVE_OBJECTS_PATH);
    }

    public static void DoLoad()
    {
        LoadSara(SAVE_SARA_PATH);
        LoadDynamicObjects(SAVE_OBJECTS_PATH);
    }

    public static DynamicObject FindDynamicObjectByGuid(string guid)
    {
        DynamicObject[] dynamicObjects = GetRootDynamicObject().GetComponentsInChildren<DynamicObject>();
        foreach (DynamicObject dynamicObject in dynamicObjects)
        {
            if (dynamicObject.objectState.guid.Equals(guid))
            {
                return dynamicObject;
            }
        }
        return null;
    }

    private static GameObject GetSara()
    {
        return GameObject.FindGameObjectWithTag("Player");
    }

    private static GameObject GetRootDynamicObject()
    {
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("DynamicRoot"))
        {
            if (gameObject.activeSelf)
            {
                return gameObject;
            }
        }
        throw new InvalidOperationException("Cannot find root of dynamic objects");
    }

    private static void SaveSara(string path)
    {
        GameObject sara = GetSara();
        if (sara == null)
        {
            throw new InvalidOperationException("Cannot find Sara");
        }

        DynamicObject dynamicObject = sara.GetComponent<DynamicObject>();
        List<ObjectState> objectStates = dynamicObject.objectState.Save(sara);
        if (objectStates.Count != 1)
        {
            throw new InvalidOperationException("Expected only 1 object state for Sara");
        }
        WriteJson(path, objectStates[0]);
        Debug.Log("Saved Sara to: " + path);
    }

    private static void LoadSara(string path)
    {
        ObjectState objectState = ReadJson<ObjectState>(path);

        GameObject sara = GetSara();
        if (sara == null)
        {
            throw new InvalidOperationException("Cannot find Sara");
        }

        DynamicObject dynamicObject = sara.GetComponent<DynamicObject>();
        dynamicObject.Load(objectState);
        Debug.Log("Loaded Sara from: " + path);
    }

    private static void SaveDynamicObjects(string path)
    {
        List<ObjectState> objectStates = ObjectState.SaveObjects(GetRootDynamicObject());
        WriteJson(path, objectStates);
        Debug.Log("Saved objects to: " + path);
    }

    private static void LoadDynamicObjects(string path)
    {
        List<ObjectState> objectStates = ReadJson<List<ObjectState>>(path);
        ObjectState.LoadObjects(prefabs, objectStates, GetRootDynamicObject());
        Debug.Log("Loaded objects from: " + path);
    }

    private static void WriteJson<T>(string path, T obj)
    {
        string json = JsonConvert.SerializeObject(obj, Formatting.Indented, jsonSerializerSettings);
        File.WriteAllText(path, json);
    }

    private static T ReadJson<T>(string path)
    {
        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
    }

    public static float[] ConvertFromVector2(Vector2 vector2)
    {
        float[] values = { vector2.x, vector2.y };
        return values;
    }

    public static Vector2 ConvertToVector2(float[] values)
    {
        return new Vector2(values[0], values[1]);
    }

    public static float[,] ConvertFromVector2Array(Vector2[] vector2)
    {
        if (vector2 == null)
        {
            return new float[0, 2];
        }

        float[,] values = new float[vector2.Length, 2];
        for (int i = 0; i < vector2.Length; i++)
        {
            values[i, 0] = vector2[i].x;
            values[i, 1] = vector2[i].y;
        }
        return values;
    }

    public static Vector2[] ConvertToVector2Array(float[,] array)
    {
        if (array.Length == 0)
        {
            return null;
        }

        Vector2[] vector2 = new Vector2[array.GetUpperBound(0) + 1];
        for (int i = 0; i < vector2.Length; i++)
        {
            vector2[i] = new Vector2(array[i, 0], array[i, 1]);
        }
        return vector2;
    }
}
