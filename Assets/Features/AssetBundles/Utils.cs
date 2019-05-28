using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public static class Utils
{
    public static string StringArrayToCSV(string[] stringArray)
    {
        var result = string.Empty;
        foreach (var str in stringArray)
        {
            result += string.Format("{0},", str);
        }
        return result.TrimEnd(',');
    }

    public static void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static AssetBundle LoadAssetBundleFromFile(string file)
    {
        var assetBundle = AssetBundle.LoadFromFile(Application.persistentDataPath + "/" + file);
        if (assetBundle == null)
        {
            var errorMsg = file + "doesn't exist on Persistent Data Path";
            Logger.Log(errorMsg, Logger.LogLevel.Error);            
            throw new FileNotFoundException(errorMsg);
        }
        return assetBundle;
    }

    public static GameObject InstantiateAssetBundle(AssetBundle assetBundle, Vector3 position, Transform parent)
    {
        var go = assetBundle.LoadAsset<GameObject>(assetBundle.GetAllAssetNames()[0]);
        return UnityEngine.Object.Instantiate(go, position, Quaternion.identity, parent);
    }

    public static void Debounce(Action action, ref int lastDebounce, int milliseconds = 100)
    {
        var current = Interlocked.Increment(ref lastDebounce);
        Logger.Log($"Debounce--> current: {current} , last: {lastDebounce}", Logger.LogLevel.Debug);        

        Thread.Sleep(milliseconds);
        if (current == lastDebounce)
        {
            Thread.Sleep(milliseconds);
            action();
        }
    }

    public static readonly string ProjectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
}
