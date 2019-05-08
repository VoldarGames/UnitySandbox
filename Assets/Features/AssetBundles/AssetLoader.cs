using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AssetLoader : MonoBehaviour
{
    static List<string> assetBundleList = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        InitAssetBundleList();
        InstantiateRandomAssetBundleGameObject("Shape");
        InstantiateRandomAssetBundleGameObject("Ornament");
        InstantiateRandomAssetBundleGameObject("Bottom");
        InstantiateRandomAssetBundleGameObject("Structure");
    }

    static void InstantiateRandomAssetBundleGameObject(string prefixLocation)
    {
        prefixLocation = prefixLocation.ToLower();
        var prefixAssetList = assetBundleList.Where(s => s.StartsWith(prefixLocation)).ToArray();
        var prefixAssetListCount = prefixAssetList.Count();
        int randomIndex = 0;
        if(prefixAssetListCount > 1)
        {
            randomIndex = UnityEngine.Random.Range(0, prefixAssetListCount);
        }
        
        var loadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, prefixAssetList[randomIndex]));
        if (loadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }
        var names = loadedAssetBundle.GetAllAssetNames();
        var prefab = loadedAssetBundle.LoadAsset<GameObject>(names[0]);
        Instantiate(prefab, new Vector3(2, 0, 0), Quaternion.identity);
    }

    void InitAssetBundleList()
    {
        var assetBundleNamesCSV = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "AssetBundleNames.csv"));

        if(string.IsNullOrEmpty(assetBundleNamesCSV))
        {
            Debug.LogError("AssetBundleNames.csv can't be found on StrimgAssets folder");
        }

        foreach (var name in assetBundleNamesCSV.Split(','))
        {
            assetBundleList.Add(name);
        }
    }
}
