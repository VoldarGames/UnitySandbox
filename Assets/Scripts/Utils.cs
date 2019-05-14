using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            Debug.LogError(errorMsg);
            throw new FileNotFoundException(errorMsg);
        }
        return assetBundle;
    }

    public static GameObject InstantiateAssetBundle(AssetBundle assetBundle, Vector3 position, Transform parent)
    {
        var go = assetBundle.LoadAsset<GameObject>(assetBundle.GetAllAssetNames()[0]);
        return Object.Instantiate(go, position, Quaternion.identity, parent);
    }
}
